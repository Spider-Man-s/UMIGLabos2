using UnityEngine;
using Fusion;
using Fusion.Sockets;
using System.Collections.Generic;
using System;
using SpellFlinger.PlayScene;
using SpellFlinger.Enum;
using SpellFlinger.LoginScene;
using Unity.VisualScripting;
using UnityEngine.SceneManagement;
using WebSocketSharp;

namespace SpellSlinger.Networking
{
    public class FusionConnection : SingletonPersistent<FusionConnection>, INetworkRunnerCallbacks
    {
        private static string _playerName = null;
        [SerializeField] private PlayerCharacterController _playerPrefab = null;
        [SerializeField] private GameManager _gameManagerPrefab = null;
        [SerializeField] private NetworkRunner _networkRunnerPrefab = null;
        [SerializeField] private int _playerCount = 10;
        private NetworkRunner _runner = null;
        private NetworkSceneManagerDefault _networkSceneManager= null;
        private Dictionary<PlayerRef, NetworkObject> _spawnedCharacters = new Dictionary<PlayerRef, NetworkObject>();
        private List<SessionInfo> _sessions = new List<SessionInfo>();
        private static GameModeType _gameModeType;

        public PlayerCharacterController LocalCharacterController { get; set; }
        public List<SessionInfo> Sessions => _sessions;
        public static GameModeType GameModeType => _gameModeType;

        public string PlayerName => _playerName;

        private void Awake()
        {
            base.Awake();
            _networkSceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>();
            _runner = gameObject.AddComponent<NetworkRunner>();
        }

        public void ConnectToLobby(String playerName = null)
        {
            if(!playerName.IsNullOrEmpty()) _playerName = playerName;
            _runner.JoinSessionLobby(SessionLobby.ClientServer);
        }

        public async void CreateSession(string sessionName, GameModeType gameMode, LevelType level)
        {
            
            /* U ovoj metodi potrebno je lokalno cache-irati odabrani nacin igre, te pozvati metodu StartGame NetworkRunner instance koja igraca spaja u sobu.
             * StartGame metoda prima argument tipa StartGameArgs strukture. Potrebno je napraviti novu instancu strukture, te joj inicijalizirati vrijednosti.

             * Potrebno je postaviti nacin igre na Host ili AutoHostOrClient, proslijediti ime sesije, scenu koja se treba ucitat nakon spajanja u sobu (parametar se 
             * predaje u obliku SceneRef.FromIndex()), maksimalni broj igraca, scene manager iz lokalne reference i SessionProperties. 


             * U SessionProperties ulaze custom svojstva, u ovom slucaju su to tip igre i level koji se treba ucitati. Proucite kojeg tipa je 
             * SessionPropeties, te mu proslijedite sva potrebna svojstva.
              (tip: za pretvaranje custom svojstva u pogodan oblik moze se koristiti SessionProperty.Convert() metoda.
             */


            _gameModeType=gameMode;
            StartGameArgs args = new StartGameArgs(){
                GameMode = GameMode.Host,
                SessionName = sessionName,
                Scene = SceneRef.FromIndex((int)level),
                PlayerCount = _playerCount,
                SceneManager = _networkSceneManager,
                SessionProperties = new Dictionary<string, SessionProperty>() {
                    {
                        "GameModeType", (int)gameMode
                    },
                    {
                        "LevelType", (int)level
                    }
                }
            };
            _runner.StartGame(args);
        }

        public async void JoinSession(string sessionName, GameModeType gameMode)
        {
            _runner.ProvideInput = true;
            _gameModeType = gameMode;

            await _runner.StartGame(new StartGameArgs()
            {
                GameMode = GameMode.Client,
                SessionName = sessionName,
            });
        }

        public void LeaveSession() // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        {
            /*
             * U ovoj metodi je potrebno pozvati Shutdown metodu instance NetworkRunner klase,
              ucitati pocetni ekran 
             * i otkljucati cursor korisnika !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
             */

             if (_runner != null && !_runner.IsDestroyed()) _runner.Shutdown();
             _runner.LoadScene(SceneRef.FromIndex(3), LoadSceneMode.Additive);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;         
            // _runner.ProvideInput = false;   //??????????
        }

        public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
        {
            _sessions=sessionList;
            SessionView.Instance.UpdateSessionList();
            /*
             * U ovoj metodi je potrebno lokalno spremiti osvjezenu listu soba, te osvjeziti prikaz liste soba pozivom 
             * pripadne metode klase SessionView.
             */
        }

        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
        {
            Debug.Log("On Player Joined");
            if (runner.IsServer)
            {
                if (player == runner.LocalPlayer)
                {
                    HealingPointSpawner.Instance.SpawnHealingPoints(runner);
                    runner.Spawn(_gameManagerPrefab);
                }

                NetworkObject playerObject = runner.Spawn(_playerPrefab.gameObject, inputAuthority: player);
                _spawnedCharacters.Add(player, playerObject);

                PlayerStats stats = playerObject.GetComponent<PlayerCharacterController>().PlayerStats;
                if (_gameModeType == GameModeType.TDM) stats.Team = PlayerManager.Instance.GetTeamWithLessPlayers();
            }
        }


        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
        {
            Debug.Log("On Player Left");
            if (_spawnedCharacters.TryGetValue(player, out NetworkObject networkObject))
            {
                runner.Despawn(networkObject);
                _spawnedCharacters.Remove(player);
            }
        }

        private void OnApplicationQuit()
        {
            base.OnApplicationQuit();

            if (_runner != null && !_runner.IsDestroyed()) _runner.Shutdown();
        }

        #region UnusedCallbacks
        public void OnInput(NetworkRunner runner, NetworkInput input)
        {
            Debug.Log("On Input");
        }

        public void OnConnectedToServer(NetworkRunner runner)
        {
            Debug.Log("On Connected to server");
        }

        public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
        {
            Debug.Log("On Connect Failed");
        }

        public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
        {
            Debug.Log("On Connect Request");
        }

        public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
        {
            Debug.Log("On Custom Authentication Response");
        }

        public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
        {
            Debug.Log("On Disconnected From Server");
        }

        public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
        {
            Debug.Log("On Host Migration");
        }

        public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
        {
            Debug.Log("On Input Missing");
        }

        public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
        {
            Debug.Log("On Object Enter AOI");
        }

        public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
        {
            Debug.Log("OnO bject Exit AOI");
        }

        public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress)
        {
            Debug.Log("On Reliable Data Progress");
        }

        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data)
        {
            Debug.Log("On Reliable Data Received");
        }

        public void OnSceneLoadDone(NetworkRunner runner)
        {
            Debug.Log("On Scene Load Done");
        }

        public void OnSceneLoadStart(NetworkRunner runner)
        {
            Debug.Log("On Scene Load Start");
        }

        public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
        {
            Debug.Log("On Shut down, reason: " + shutdownReason.ToString());
            LeaveSession();
        }

        public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
        {
            Debug.Log("On User Simulation Message");
        }
        #endregion
    }
}