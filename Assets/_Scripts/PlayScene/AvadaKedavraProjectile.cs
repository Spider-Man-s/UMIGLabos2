using Fusion;
using SpellFlinger.Enum;
using SpellSlinger.Networking;
using System.Linq;
using UnityEngine;

namespace SpellFlinger.PlayScene
{
    public class AvadaKedavraProjectile : Projectile
    {
        [SerializeField] private float _range = 0f;
        [SerializeField] private GameObject _projectileModel = null;

        [Networked] public bool ProjectileHit { get; private set; }

        public override void Throw(Vector3 direction, PlayerStats ownerPlayerStats)
        {
            Direction = direction.normalized * _movementSpeed;
            OwnerPlayerStats = ownerPlayerStats;
            transform.rotation = Quaternion.FromToRotation(transform.forward, Direction.normalized);
        }

        public override void FixedUpdateNetwork()
        {
            if (ProjectileHit)
                return;

            transform.position += (Direction * Runner.DeltaTime);

            if (!HasStateAuthority)
                return;

            Collider[] hitColliders = Physics.OverlapSphere(transform.position, _range);

            foreach (Collider collider in hitColliders)
            {
                if (collider.CompareTag("Player"))
                {
                    PlayerStats player = collider.GetComponent<PlayerStats>();

                    if (player.Object.InputAuthority == OwnerPlayerStats.Object.InputAuthority) continue;
                    if (FusionConnection.GameModeType == GameModeType.TDM && player.Team == OwnerPlayerStats.Team) continue;

                    player.DealDamage(player.Health, OwnerPlayerStats);

                    ProjectileHit = true;
                    RPC_Disappear();
                    Runner.Despawn(Object);
                    return;
                }
            }
            if (hitColliders.Any(collider => collider.CompareTag("Ground")))
            {
                ProjectileHit = true;
                RPC_Disappear();
                Runner.Despawn(Object);
            }
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All, HostMode = RpcHostMode.SourceIsServer)]
        private void RPC_Disappear()
        {
            if (_projectileModel != null)
                _projectileModel.SetActive(false);
        }
    }
}
