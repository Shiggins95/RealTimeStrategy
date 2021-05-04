using Mirror;
using UnityEngine;

namespace Combat
{
    public class Targetable : NetworkBehaviour 
    {
        [SerializeField] private Transform targetPoint;

        public Transform GetTargetPoint()
        {
            return targetPoint;
        }
    }
}