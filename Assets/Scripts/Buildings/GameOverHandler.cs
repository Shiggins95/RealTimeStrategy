using System;
using System.Collections.Generic;
using Mirror;
using Networking;
using UnityEngine;

namespace Buildings
{
    public class GameOverHandler : NetworkBehaviour
    {

        public static event Action<string> ClientOnGameOver;
        public static event Action ServerOnGameOver;
        
        private List<UnitBase> bases = new List<UnitBase>();
        
        #region Server

        public override void OnStartServer()
        {
            UnitBase.ServerOnBaseSpawn += ServerHandleBaseSpawned;
            UnitBase.ServerOnBaseDeSpawn += ServerHandleBaseDeSpawned;
        }

        public override void OnStopServer()
        {
            UnitBase.ServerOnBaseSpawn -= ServerHandleBaseSpawned;
            UnitBase.ServerOnBaseDeSpawn -= ServerHandleBaseDeSpawned;
        }

        [Server]    
        private void ServerHandleBaseSpawned(UnitBase unitBase)
        {
            bases.Add(unitBase);
        }
        
        [Server]            
        private void ServerHandleBaseDeSpawned(UnitBase unitBase)
        {
            bases.Remove(unitBase);
            if (bases.Count > 1) return;
            int playerId = bases[0].connectionToClient.connectionId;
            RpcGameOver($"Player {playerId}");
            ServerOnGameOver?.Invoke();
        }


        #endregion

        #region Client

        [ClientRpc]
        private void RpcGameOver(string winner)
        {
            ClientOnGameOver?.Invoke(winner);
        }

        #endregion
    }
}