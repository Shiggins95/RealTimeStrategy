using Mirror;
using UnityEngine;

namespace Combat
{
    public class Targeter : NetworkBehaviour
    {

        private Targetable target;

        public Targetable GetTarget()
        {
            return target;
        }

        #region Server

        [Command]
        public void CmdSetTarget(GameObject targetGameObject)
        {
            if (!targetGameObject.TryGetComponent<Targetable>(out Targetable newTarget)) return;
            target = newTarget;
        }

        [Command]
        public void ClearTarget()
        {
            target = null;
        }

        #endregion
        
    }
    
}