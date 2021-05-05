using System;
using Combat;
using Mirror;
using UnityEngine;
using UnityEngine.Events;

namespace Unit
{
    public class Unit : NetworkBehaviour
    {
        #region Declarables

        [SerializeField] private UnityEvent onSelected;
        [SerializeField] private UnityEvent onDeSelected;
        [SerializeField] private UnitMovement unitMovement;
        [SerializeField] private Targeter targeter;
        [SerializeField] private Health health;

        #endregion

        #region Events

        public static event Action<Unit> ServerOnUnitDeSpawned;
        public static event Action<Unit> ServerOnUnitSpawned;
        public static event Action<Unit> AuthorityOnUnitSpawned;
        public static event Action<Unit> AuthorityOnUnitDeSpawned;

        #endregion
        
        #region Getters

        public UnitMovement GetUnitMovement()
        {
            return unitMovement;
        }

        public Targeter GetTargeter()
        {
            return targeter;
        }

        #endregion
        
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
        
        public override void OnStartAuthority()
        {
            // trigger event which will invoke the callbacks in the RtsPlayer script
            AuthorityOnUnitSpawned?.Invoke(this);
        }

        public override void OnStopClient()
        {
            // return if we don't own the current unit or if we are a server also
            if (!hasAuthority) return;
            // trigger event which will invoke the callbacks in the RtsPlayer script
            AuthorityOnUnitDeSpawned?.Invoke(this);
        }

        #endregion

        #region Server

        public override void OnStartServer()
        {
            // trigger event which will invoke the callbacks in the RtsPlayer script
            ServerOnUnitSpawned?.Invoke(this);
            health.ServerOnDie += ServerHandleOnDie;
        }

        public override void OnStopServer()
        {
            // trigger event which will invoke the callbacks in the RtsPlayer script
            ServerOnUnitDeSpawned?.Invoke(this);
            health.ServerOnDie -= ServerHandleOnDie;
        }

        [Server]
        private void ServerHandleOnDie()
        {
            NetworkServer.Destroy(gameObject);
        }

        #endregion
    }
}