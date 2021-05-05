using System;
using System.Collections.Generic;
using Buildings;
using Mirror;
using UnityEngine;

namespace Networking
{
    public class RtsPlayer : NetworkBehaviour
    {
        [SerializeField] private List<Unit.Unit> myUnits = new List<Unit.Unit>();

        public List<Unit.Unit> GetMyUnits()
        {
            return myUnits;
        }
        
        #region Server

        // When player is initialised
        public override void OnStartServer()
        {
            // subscribe to server associated events in the Unit script as we want logic to be performed when
            // a new unit is spawned
            Unit.Unit.ServerOnUnitSpawned += ServerHandleUnitSpawned;
            Unit.Unit.ServerOnUnitDeSpawned += ServerHandleUnitDeSpawned;
        }

        // When player exists / server is stopped
        public override void OnStopServer()
        {
            // unsubscribe from server associated events in the Unit script
            Unit.Unit.ServerOnUnitSpawned -= ServerHandleUnitSpawned;
            Unit.Unit.ServerOnUnitDeSpawned -= ServerHandleUnitDeSpawned;
        }

        // callback function for event, contains the logic we want to be executed when a unit is despawned
        private void ServerHandleUnitDeSpawned(Unit.Unit unit)
        {
            // only perform logic if the request if for the current client i.e me
            if (unit.connectionToClient.connectionId != connectionToClient.connectionId) return;
            // remove despawned unit from current list of myUnits
            myUnits.Remove(unit);
        }

        private void ServerHandleUnitSpawned(Unit.Unit unit)
        {
            // only perform logic if the request if for the current client i.e me
            if (unit.connectionToClient.connectionId != connectionToClient.connectionId) return;
            // add spawned unit to current list of myUnits
            myUnits.Add(unit);
        }

        #endregion

        #region Client

        // when client starts for current player
        public override void OnStartAuthority()
        {
            // only perform logic if not a server to avoid duplicate units being added to above list
            if (NetworkServer.active) return;
            // subscribe to events to perform custom logic when unit is spawned/despawned
            Unit.Unit.AuthorityOnUnitSpawned += AuthorityHandleUnitSpawned;
            Unit.Unit.AuthorityOnUnitDeSpawned += AuthorityHandleUnitDeSpawned;
        }
        
        public override void OnStopClient()
        {
            // only perform logic if not a server to avoid duplciate commands
            if (!isClientOnly || !hasAuthority) return;
            // unsubscribe from events
            Unit.Unit.AuthorityOnUnitSpawned -= AuthorityHandleUnitSpawned;
            Unit.Unit.AuthorityOnUnitDeSpawned -= AuthorityHandleUnitDeSpawned;
        }
        
        private void AuthorityHandleUnitSpawned(Unit.Unit unit)
        {
            myUnits.Add(unit);
        }
        
        private void AuthorityHandleUnitDeSpawned(Unit.Unit unit)
        {
            myUnits.Remove(unit);
        }

        #endregion
        
    }
}