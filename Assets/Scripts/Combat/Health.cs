using System;
using Mirror;
using UnityEngine;

namespace Combat
{
    public class Health : NetworkBehaviour
    {
        [SerializeField] private int maxHealth = 100;
        [SyncVar(hook = nameof(HandleHealthUpdated))] private int currentHealth;

        public event Action ServerOnDie;
        public event Action<int, int> ClientOnHealthUpdated;


        #region Server

        public override void OnStartServer()
        {
            currentHealth = maxHealth;
        }

        [Server]
        public void DealDamage(int damageAmount)
        {
            if (currentHealth == 0) return;
            // removes the damage amount from currentHealth but if it is less than 0, it returns 0
            currentHealth = Mathf.Max(currentHealth - damageAmount, 0);
            if (currentHealth != 0) return;
            ServerOnDie?.Invoke();
            Debug.Log($"we ded");
        }

        #endregion

        #region Client

        private void HandleHealthUpdated(int oldHealth, int newHealth)
        {
            ClientOnHealthUpdated?.Invoke(newHealth, maxHealth);
        }
        
        #endregion
    }
}