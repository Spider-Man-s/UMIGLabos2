namespace Fusion {
    using System;
    using System.Runtime.CompilerServices;
  using System.Runtime.InteropServices;
  using UnityEngine;

  [StructLayout(LayoutKind.Explicit)]
  [NetworkStructWeaved(WORDS + 4)]
  public unsafe struct NetworkCCData : INetworkStruct {
    public const int WORDS = NetworkTRSPData.WORDS + 4;
    public const int SIZE  = WORDS * 4;

    [FieldOffset(0)]
    public NetworkTRSPData TRSPData;

    [FieldOffset((NetworkTRSPData.WORDS + 0) * Allocator.REPLICATE_WORD_SIZE)]
    int _grounded;

    [FieldOffset((NetworkTRSPData.WORDS + 1) * Allocator.REPLICATE_WORD_SIZE)]
    Vector3Compressed _velocityData;

    public bool Grounded {
      get => _grounded == 1;
      set => _grounded = (value ? 1 : 0);
    }

    public Vector3 Velocity {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get => _velocityData;
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      set => _velocityData = value;
    }
  }

  [DisallowMultipleComponent]
  [RequireComponent(typeof(CharacterController))]
  [NetworkBehaviourWeaved(NetworkCCData.WORDS)]
  // ReSharper disable once CheckNamespace
  public sealed unsafe class NetworkCharacterController : NetworkTRSP, INetworkTRSPTeleport, IBeforeAllTicks, IAfterAllTicks, IBeforeCopyPreviousState {
    new ref NetworkCCData Data => ref ReinterpretState<NetworkCCData>();

    [Header("Character Controller Settings")]
    public float gravity = -20.0f;
    public float jumpImpulse   = 8.0f;
    public float rotationSpeed = 15.0f;
    public float moveSpeed = 0f;
    public float slowAmount = 0f;
    public float doubleJumpBoost = 0f;
    public float slopeRaycastDistance = 0f;

    private float _squareOfTwo = Mathf.Sqrt(2);

    Tick                _initial;
    CharacterController _controller;

    public Vector3 Velocity {
      get => Data.Velocity;
      set => Data.Velocity = value;
    }

    public bool Grounded {
      get => Data.Grounded;
      set => Data.Grounded = value;
    }

    public void Teleport(Vector3? position = null, Quaternion? rotation = null) {
      _controller.enabled = false;
      NetworkTRSP.Teleport(this, transform, position, rotation);
      _controller.enabled = true;
    }


    public void Jump(bool ignoreGrounded = false, bool doubleJump = false) {
      if (Data.Grounded || ignoreGrounded) {
        var newVel = Data.Velocity;
        newVel.y = doubleJump ? doubleJumpBoost : jumpImpulse;
        Data.Velocity =  newVel;
      }
    }

    public void Move(Vector2 direction, bool isSlowed, float rotation, bool isGrounded)
    {
        var deltaTime = Runner.DeltaTime;
        var moveVelocity = Data.Velocity;

        if (Data.Grounded && moveVelocity.y < 0)
        {
            moveVelocity.y = 0f;
        }

        Vector3 _moveDirection = transform.right * direction.x + transform.forward * direction.y;
        _moveDirection *= moveSpeed;
        if (isSlowed) _moveDirection *= slowAmount;
        if (direction.x != 0 && direction.y != 0) _moveDirection /= _squareOfTwo;

        moveVelocity.y += gravity * deltaTime;

        _moveDirection = AdjustVelocityToSlope(_moveDirection);
        _moveDirection.y += moveVelocity.y;
        _controller.Move(_moveDirection * deltaTime);

        transform.eulerAngles = new Vector3(0, transform.eulerAngles.y + rotation, 0);

        moveVelocity = _moveDirection;
        Data.Velocity = moveVelocity;
        Data.Grounded = isGrounded;
    }

    private Vector3 AdjustVelocityToSlope(Vector3 velocity)
    {
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, slopeRaycastDistance))
        {
            if (hit.collider.tag == "Ground")
            {
                Quaternion slopeRotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
                Vector3 adjustedVelocity = slopeRotation * velocity;

                if (adjustedVelocity.y < 0) return adjustedVelocity;
            }
        }

        return velocity;
    }

    public override void Spawned() {
      _initial = default;
      TryGetComponent(out _controller);
      CopyToBuffer();
    }

    public override void Render() {
      NetworkTRSP.Render(this, transform, false, false, false, ref _initial);
    }

    void IBeforeAllTicks.BeforeAllTicks(bool resimulation, int tickCount) {
      CopyToEngine();
    }

    void IAfterAllTicks.AfterAllTicks(bool resimulation, int tickCount) {
      CopyToBuffer();
    }

    void IBeforeCopyPreviousState.BeforeCopyPreviousState() {
      CopyToBuffer();
    }
    
    void Awake() {
      TryGetComponent(out _controller);
    }

    void CopyToBuffer() {
      Data.TRSPData.Position = transform.position;
      Data.TRSPData.Rotation = transform.rotation;
    }

    void CopyToEngine() {
      // CC must be disabled before resetting the transform state
      _controller.enabled = false;

      // set position and rotation
      transform.SetPositionAndRotation(Data.TRSPData.Position, Data.TRSPData.Rotation);

      // Re-enable CC
      _controller.enabled = true;
    }
  }
}