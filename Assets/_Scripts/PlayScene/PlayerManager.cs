using SpellFlinger.Enum;
using SpellSlinger.Networking;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SpellFlinger.PlayScene
{
    public class PlayerManager : Singleton<PlayerManager>
    {
        [SerializeField] private Material _friendlyMaterial;
        [SerializeField] private Material _enemyMaterial;
        [SerializeField] private Color _friendlyColor;
        [SerializeField] private Color _enemyColor;
        private List<PlayerStats> _players = new();
        private TeamType _friendlyTeam;

        public Action OnPlayerTeamTypeSet = null;
        public TeamType FriendlyTeam => _friendlyTeam;
        public Color FriendlyColor => _friendlyColor;
        public Color EnemyColor => _enemyColor;

        public void RegisterPlayer(PlayerStats player) => _players.Add(player);

        public void UnregisterPlayer(PlayerStats player) => _players.Remove(player);

        public TeamType GetTeamWithLessPlayers()
        {
            /*
             * Potrebno je zamijeniti liniju return (TeamType)(-1); tako da metoda vraća
             * TeamType s timom koji ima manje igrača.
             */

            return (TeamType)(-1);
        }

        public void SetFriendlyTeam(TeamType friendlyTeam)
        {
            _friendlyTeam = friendlyTeam;
            _players.ForEach((player) => SetPlayerColor(player));
            OnPlayerTeamTypeSet?.Invoke();
        }

        public void SetPlayerColor(PlayerStats player)
        {
            if (FusionConnection.GameModeType == GameModeType.DM || player.Team != _friendlyTeam) player.SetTeamMaterial(_enemyMaterial, _enemyColor);
            else player.SetTeamMaterial(_friendlyMaterial, _friendlyColor);
        }

        public void SendGameEndRpc() => _players.ForEach((player) =>
        {
            player.PlayerCharacterController.RPC_DisableController();
            player.PlayerCharacterController.RPC_GameEnd();
            player.PlayerCharacterController.StopRespawnCoroutine();
        });

        public void SendGameStartRpc() => _players.ForEach((player) =>
        {
            player.PlayerCharacterController.RPC_EnableController();
            player.PlayerCharacterController.RPC_GameStart();
            player.PlayerCharacterController.SetGameStartPosition();
            player.ResetGameInfo();
        });
    }
}
