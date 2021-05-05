using System;
using Combat;
using Mirror;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Buildings
{
    public class UnitSpawner : NetworkBehaviour, IPointerClickHandler
    {
        // get references to what we need
        [SerializeField] private Health health;
        [SerializeField] private GameObject unitPrefab;
        [SerializeField] private Transform unitSpawnPoint;
        
        #region Server

        public override void OnStartServer()
        {
            health.ServerOnDie += ServerHandleDie;   
        }
        
        public override void OnStopServer()
        {
            health.ServerOnDie -= ServerHandleDie;
        }

        [Server]
        private void ServerHandleDie()
        {
            // NetworkServer.Destroy(gameObject);
        }

        // server command
        [Command]
        private void CmdSpawnUnit()
        {
            // instantiate unit spawner - prefab must be set in NetworkManager provided by Mirror
            GameObject unitInstance = Instantiate(unitPrefab, unitSpawnPoint.position, unitSpawnPoint.rotation);
            // spawn on server and link correct client to the unit
            NetworkServer.Spawn(unitInstance, connectionToClient);
        }

        #endregion

        #region Client

        // handle click logic
        public void OnPointerClick(PointerEventData eventData)
        {
            // only perform for left button click
            if (eventData.button != PointerEventData.InputButton.Left) return;

            // return if we don't own the unit
            if (!hasAuthority) return;
            
            // Spawn unit
            CmdSpawnUnit();
        }

        #endregion
    }
}