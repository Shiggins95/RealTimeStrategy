using System;
using Mirror;
using UnityEngine;
using UnityEngine.Events;

namespace Unit
{
    public class Unit : NetworkBehaviour
    {
        [SerializeField] private UnityEvent onSelected;
        [SerializeField] private UnityEvent onDeSelected;
        [SerializeField] private UnitMovement unitMovement;

        public static event Action<Unit> ServerOnUnitSpawned;
        public static event Action<Unit> ServerOnUnitDeSpawned;
        public static event Action<Unit> AuthorityOnUnitDeSpawned;
        public static event Action<Unit> AuthorityOnUnitSpawned;
        public UnitMovement GetUnitMovement()
        {
            return unitMovement;
        }

        #region Client

        // client command
        [Client]
        public void SelectUnit()
        {
            // return if we don't own the current unit
            if (!hasAuthority) return;
            // trigger event
            onSelected?.Invoke();
        }

        [Client]
        public void DeSelectUnit()
        {
            // return if we don't own the current unit
            if (!hasAuthority) return;
            // trigger event
            onDeSelected?.Invoke();
        }
        
        public override void OnStartClient()
        {
            // return if we don't own the current unit or if we are a server also
            if (!hasAuthority || !isClientOnly) return;
            base.OnStartClient();
            // trigger event which will invoke the callbacks in the RtsPlayer script
            AuthorityOnUnitSpawned?.Invoke(this);
        }

        public override void OnStopClient()
        {
            // return if we don't own the current unit or if we are a server also
            if (!hasAuthority || !isClientOnly) return;
            base.OnStopClient();
            // trigger event which will invoke the callbacks in the RtsPlayer script
            AuthorityOnUnitDeSpawned?.Invoke(this);
        }

        #endregion

        #region Server

        public override void OnStartServer()
        {
            base.OnStartServer();
            // trigger event which will invoke the callbacks in the RtsPlayer script
            ServerOnUnitSpawned?.Invoke(this);
        }

        public override void OnStopServer()
        {
            base.OnStopServer();
            // trigger event which will invoke the callbacks in the RtsPlayer script
            ServerOnUnitDeSpawned?.Invoke(this);
        }

        #endregion
    }
}