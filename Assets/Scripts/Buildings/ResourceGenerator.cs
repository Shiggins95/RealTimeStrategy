using System;
using Combat;
using Mirror;
using Networking;
using UnityEngine;

namespace Buildings
{
    public class ResourceGenerator : NetworkBehaviour
    {
        [SerializeField] private Health health;
        [SerializeField] private int resourcesPerInterval = 10;
        [SerializeField] private float interval = 2f;

        private float timer;
        private RtsPlayer rtsPlayer;

        public override void OnStartServer()
        {
            timer = interval;
            rtsPlayer = connectionToClient.identity.GetComponent<RtsPlayer>();

            health.ServerOnDie += ServerHandleDie;
            GameOverHandler.ServerOnGameOver += ServerHandleGameOver;
        }

        public override void OnStopServer()
        {
            health.ServerOnDie -= ServerHandleDie;
            GameOverHandler.ServerOnGameOver -= ServerHandleGameOver;
        }

        [ServerCallback]
        private void Update()
        {
            timer -= Time.deltaTime;
            if (timer > 0) return;
            timer = interval;
            rtsPlayer.SetResources(rtsPlayer.GetResources() + resourcesPerInterval);
        }

        private void ServerHandleDie()
        {
            NetworkServer.Destroy(gameObject);
        }

        private void ServerHandleGameOver()
        {
            enabled = false;
        }
    }
}