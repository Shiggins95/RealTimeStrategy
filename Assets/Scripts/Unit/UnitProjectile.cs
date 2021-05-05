using System;
using Combat;
using Mirror;
using UnityEngine;

namespace Unit
{
    public class UnitProjectile : NetworkBehaviour
    {
        [SerializeField] private Rigidbody rb;
        [SerializeField] private float destroyTimer = 5f;
        [SerializeField] private float launchForce = 5f;
        [SerializeField] private int damage;

        private void Start()
        {
            rb.velocity = transform.forward * launchForce;
        }

        public override void OnStartServer()
        {
            // call method after x seconds
            Invoke(nameof(DestroySelf), destroyTimer);
        }

        [ServerCallback]
        private void OnTriggerEnter(Collider other)
        {
            if (!other.TryGetComponent<NetworkIdentity>(out NetworkIdentity networkIdentity)) return;
            if (networkIdentity.connectionToClient == connectionToClient) return;
            if (!other.TryGetComponent<Health>(out Health health)) return;
            health.DealDamage(damage);
            DestroySelf();
        }

        [Server]
        private void DestroySelf()
        {
            NetworkServer.Destroy(gameObject);
        }
    }
}