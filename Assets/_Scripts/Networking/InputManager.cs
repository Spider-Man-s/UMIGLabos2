using UnityEngine;
using Fusion;
using Fusion.Sockets;
using System.Collections.Generic;
using System;
using SpellFlinger.PlayScene;
using SpellFlinger.Scriptables;

namespace SpellSlinger.Networking
{
    public class InputManager : SimulationBehaviour, IBeforeUpdate, INetworkRunnerCallbacks
    {
        private NetworkInputData _accumulatedInput;
        private bool _reset = false;

        public void BeforeUpdate()
        {
            if (_reset)
            {
                _reset = false;
                _accumulatedInput = default;
            }

            if (CameraController.Instance && !CameraController.Instance.CameraEnabled)
            {
                return;
            }

            Vector2 direction = Vector2.zero;
            NetworkButtons buttons = default;

            /*
             * Ovu metodu je potrebno nadopuniti kodom za skupljanje korisničkih naredbi.
             * Potrebno je skupiti pritisak tipki WASD kao komande za gretanje u naprijed, lijevo, nazad i desno,
             * te rotaciju lika u x osi. 
             
             Rotacija lika se očitava pomakom miša u x osi i osjetljivošću okretanja iz 
             * SensitivitySettingsScriptable scriptable objekta.


             * Očitane vrijednosti je potrebno spremiti u varijablu za akumulaciju naredbi između poziva metode OnInput.


             * Također je potrebno očitati naredbu za skok pritiskom tipke space. Njeno spremanje se postiže na način sličan
             * kao spremanje naredbe za pucanje.
             */
            if (Input.GetKey(KeyCode.W)) direction += Vector2.up;
            if (Input.GetKey(KeyCode.S)) direction += Vector2.down;
            if (Input.GetKey(KeyCode.A)) direction += Vector2.left;
            if (Input.GetKey(KeyCode.D)) direction += Vector2.right;

            if (Input.GetKey(KeyCode.UpArrow)) direction += Vector2.up;
            if (Input.GetKey(KeyCode.DownArrow)) direction += Vector2.down;
            if (Input.GetKey(KeyCode.LeftArrow)) direction += Vector2.left;
            if (Input.GetKey(KeyCode.RightArrow)) direction += Vector2.right;

            if (Input.GetAxis("Mouse X") != 0)
            {
                _accumulatedInput.YRotation += Input.GetAxis("Mouse X") * SensitivitySettingsScriptable.Instance.LeftRightMultiplier;
            }
            _accumulatedInput.Direction += direction;

            buttons.Set(NetworkInputData.SHOOT, Input.GetMouseButton(0));

            if (Input.GetMouseButton(0)) _accumulatedInput.ShootTarget = FusionConnection.Instance.LocalCharacterController.GetShootDirection();

            buttons.Set(NetworkInputData.JUMP, Input.GetKey(KeyCode.Space));


            _accumulatedInput.Buttons = new NetworkButtons(_accumulatedInput.Buttons.Bits | buttons.Bits);

        }

        public void OnInput(NetworkRunner runner, NetworkInput input)
        {
            _accumulatedInput.Direction.Normalize();
            input.Set(_accumulatedInput);

            _reset = true;
            _accumulatedInput.YRotation = 0f;
        }

        #region UnusedCallbacks
        public void OnConnectedToServer(NetworkRunner runner) { }

        public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }

        public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }

        public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }

        public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }

        public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }

        public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }

        public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }

        public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }

        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) { }

        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }

        public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }

        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }

        public void OnSceneLoadDone(NetworkRunner runner) { }

        public void OnSceneLoadStart(NetworkRunner runner) { }

        public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }

        public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }

        public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    }
    #endregion
}