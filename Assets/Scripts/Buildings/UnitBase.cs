using System;
using Combat;
using Mirror;
using UnityEngine;

namespace Buildings
{
    public class UnitBase : NetworkBehaviour
    {
        [SerializeField] private Health health;

        public static event Action<UnitBase> ServerOnBaseSpawn;
        public static event Action<UnitBase> ServerOnBaseDeSpawn;

        #region Client

        #endregion

        #region Server

        public override void OnStartServer()
        {
            health.ServerOnDie += ServerHandleOnDie;
            ServerOnBaseSpawn?.Invoke(this);
        }

        public override void OnStopServer()
        {
            health.ServerOnDie -= ServerHandleOnDie;
            ServerOnBaseDeSpawn?.Invoke(this);
        }

        [Server]
        private void ServerHandleOnDie()
        {
            NetworkServer.Destroy(gameObject);
        }

        #endregion
    }
}