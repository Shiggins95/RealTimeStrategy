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

        #region Server

        // Server command
        [Command]
        public void CmdMove(Vector3 position)
        {
            // return if the position is out of bounds of the nav mesh
            if (!NavMesh.SamplePosition(position, out NavMeshHit hit, 1f, NavMesh.AllAreas)) return;

            // update position of unit (nav mesh agent)
            agent.SetDestination(hit.position);
        }

        #endregion
    }
}