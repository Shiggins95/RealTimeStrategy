using System;
using Buildings;
using Combat;
using Mirror;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

namespace Unit
{
    public class UnitMovement : NetworkBehaviour
    {
        [SerializeField] private NavMeshAgent agent = null;
        [SerializeField] private Camera mainCamera;
        [SerializeField] private Targeter targeter;
        [SerializeField] private float chaseRange = 10;

        #region Server

        public override void OnStartServer()
        {
            GameOverHandler.ServerOnGameOver += ServerHandleGameOver;

        }

        public override void OnStopServer()
        {
            GameOverHandler.ServerOnGameOver -= ServerHandleGameOver;

        }

        [ServerCallback]
        private void Update()
        {
            Targetable target = targeter.GetTarget();
            if (target)
            {
                if ((target.transform.position - transform.position).sqrMagnitude > (chaseRange * chaseRange))
                {
                    agent.SetDestination(target.transform.position);
                    return;
                }
                if (agent.hasPath)
                {
                    agent.ResetPath();
                }

                return;
            }

            if (!agent.hasPath) return;
            if (agent.remainingDistance > agent.stoppingDistance) return;
            agent.ResetPath();
        }

        // Server command
        [Command]
        public void CmdMove(Vector3 position)
        {
            targeter.ClearTarget();
            // return if the position is out of bounds of the nav mesh
            if (!NavMesh.SamplePosition(position, out NavMeshHit hit, 1f, NavMesh.AllAreas)) return;

            // update position of unit (nav mesh agent)
            agent.SetDestination(hit.position);
        }

        [Server]
        private void ServerHandleGameOver()
        {
            agent.ResetPath();
        }

        #endregion
    }
}