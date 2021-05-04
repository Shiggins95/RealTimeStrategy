using System;
using Combat;
using Mirror;
using Unity.Mathematics;
using UnityEngine;

namespace Unit
{
    public class UnitFiring : NetworkBehaviour
    {
        [SerializeField] private Targeter targeter;
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private float fireRange = 5f;
        [SerializeField] private float fireRate = 1f;
        [SerializeField] private float rotationSpeed = 20f;

        private float lastFireTime;

        [ServerCallback]
        private void Update()
        {
            Targetable target = targeter.GetTarget();
            if (!target) return;
            if (!CanFireAtTarget()) return;
            Quaternion targetRotation =
                Quaternion.LookRotation(target.transform.position - transform.position);
            transform.rotation =
                Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            if (Time.time <= (1 / fireRate) + lastFireTime) return;
            Quaternion projectileRotation =
                Quaternion.LookRotation(target.GetTargetPoint().position - spawnPoint.position);
            GameObject projectileInstance = Instantiate(projectilePrefab, spawnPoint.position, projectileRotation);
            NetworkServer.Spawn(projectileInstance, connectionToClient);
            lastFireTime = Time.time;
        }

        [Server]
        private bool CanFireAtTarget()
        {
            return ((targeter.GetTarget().transform.position - transform.position).sqrMagnitude <=
                    (fireRange * fireRange));
        }
    }
}