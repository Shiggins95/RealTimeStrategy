using System;
using Mirror;
using UnityEngine;

namespace Unit
{
    public class UnitProjectile : NetworkBehaviour
    {
        [SerializeField] private Rigidbody rb;
        [SerializeField] private float destroyTimer = 5f;
        [SerializeField] private float launchForce = 5f;

        private void Start()
        {
            rb.velocity = transform.forward * launchForce;
        }

        public override void OnStartServer()
        {
            // call method after x seconds
            Invoke(nameof(DestroySelf), destroyTimer);
        }

        [Server]
        private void DestroySelf()
        {
            NetworkServer.Destroy(gameObject);
        }
    }
}