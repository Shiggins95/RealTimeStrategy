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
        [SerializeField] private List<Building> myBuildings = new List<Building>();
        [SerializeField] private Building[] allBuildings = new Building[0];
        [SerializeField] private LayerMask buildingLayerMask;
        [SerializeField] private float buildingRangeLimit = 5f;

        [SyncVar(hook = nameof(ClientHandleResourcesUpdated))]
        private int resources = 500;

        private Color teamColor;

        public event Action<int> ClientOnResourcesUpdated;

        public List<Unit.Unit> GetMyUnits()
        {
            return myUnits;
        }

        public List<Building> GetMyBuildings()
        {
            return myBuildings;
        }

        public int GetResources()
        {
            return resources;
        }

        public Color GetTeamColor()
        {
            return teamColor;
        }

        public bool CanPlaceBuilding(BoxCollider buildingCollider, Vector3 position)
        {
            if (Physics.CheckBox(position + buildingCollider.center, buildingCollider.size / 2, Quaternion.identity,
                buildingLayerMask))
            {
                return false;
            }

            foreach (Building building in myBuildings)
            {
                if ((position - building.transform.position).sqrMagnitude < buildingRangeLimit * buildingRangeLimit)
                {
                    return true;
                }
            }

            return false;
        }

        #region Server

        // When player is initialised
        public override void OnStartServer()
        {
            // subscribe to server associated events in the Unit script as we want logic to be performed when
            // a new unit is spawned
            Unit.Unit.ServerOnUnitSpawned += ServerHandleUnitSpawned;
            Unit.Unit.ServerOnUnitDeSpawned += ServerHandleUnitDeSpawned;
            Building.ServerOnBuildingSpawned += ServerHandleBuildingSpawned;
            Building.ServerOnBuildingDeSpawned += ServerHandleBuildingDeSpawned;
        }

        // When player exists / server is stopped
        public override void OnStopServer()
        {
            // unsubscribe from server associated events in the Unit script
            Unit.Unit.ServerOnUnitSpawned -= ServerHandleUnitSpawned;
            Unit.Unit.ServerOnUnitDeSpawned -= ServerHandleUnitDeSpawned;
            Building.ServerOnBuildingSpawned -= ServerHandleBuildingSpawned;
            Building.ServerOnBuildingDeSpawned -= ServerHandleBuildingDeSpawned;
        }
        
        [Server]
        public void SetResources(int newResources)
        {
            resources = newResources;
        }

        [Server]
        public void SetTeamColor(Color newColor)
        {
            teamColor = newColor;
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

        private void ServerHandleBuildingDeSpawned(Building building)
        {
            // only perform logic if the request if for the current client i.e me
            if (building.connectionToClient.connectionId != connectionToClient.connectionId) return;
            // remove despawned unit from current list of myUnits
            myBuildings.Remove(building);
        }

        private void ServerHandleBuildingSpawned(Building building)
        {
            // only perform logic if the request if for the current client i.e me
            if (building.connectionToClient.connectionId != connectionToClient.connectionId) return;
            // add spawned unit to current list of myUnits
            myBuildings.Add(building);
        }

        [Command]
        public void CmdTryPlaceBuilding(int buildingId, Vector3 position)
        {
            Building foundBuilding = null;
            foreach (Building currentBuilding in allBuildings)
            {
                if (currentBuilding.GetId() != buildingId) continue;
                foundBuilding = currentBuilding;
                break;
            }

            if (!foundBuilding) return;
            position.y = 0;

            if (resources < foundBuilding.GetPrice()) return;

            BoxCollider buildingCollider = foundBuilding.GetComponent<BoxCollider>();

            if (!CanPlaceBuilding(buildingCollider, position)) return;

            GameObject buildingInstance =
                Instantiate(foundBuilding.gameObject, position, foundBuilding.transform.rotation);
            NetworkServer.Spawn(buildingInstance, connectionToClient);
            SetResources(resources - foundBuilding.GetPrice());
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
            Building.AuthorityOnBuildingSpawned += AuthorityHandleBuildingSpawned;
            Building.AuthorityOnBuildingDeSpawned += AuthorityHandleBuildingDeSpawned;
        }

        public override void OnStopClient()
        {
            // only perform logic if not a server to avoid duplciate commands
            if (!isClientOnly || !hasAuthority) return;
            // unsubscribe from events
            Unit.Unit.AuthorityOnUnitSpawned -= AuthorityHandleUnitSpawned;
            Unit.Unit.AuthorityOnUnitDeSpawned -= AuthorityHandleUnitDeSpawned;
            Building.AuthorityOnBuildingSpawned -= AuthorityHandleBuildingSpawned;
            Building.AuthorityOnBuildingDeSpawned -= AuthorityHandleBuildingDeSpawned;
        }

        private void AuthorityHandleUnitSpawned(Unit.Unit unit)
        {
            myUnits.Add(unit);
        }

        private void AuthorityHandleUnitDeSpawned(Unit.Unit unit)
        {
            myUnits.Remove(unit);
        }

        private void AuthorityHandleBuildingSpawned(Building building)
        {
            myBuildings.Add(building);
        }

        private void AuthorityHandleBuildingDeSpawned(Building building)
        {
            myBuildings.Remove(building);
        }

        private void ClientHandleResourcesUpdated(int oldResources, int newResources)
        {
            ClientOnResourcesUpdated?.Invoke(newResources);
        }

        #endregion
    }
}