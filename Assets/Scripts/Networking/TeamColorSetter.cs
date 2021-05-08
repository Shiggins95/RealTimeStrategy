using Mirror;
using UnityEngine;

namespace Networking
{
    public class TeamColorSetter : NetworkBehaviour
    {
        [SerializeField] private Renderer[] colorRenderers = new Renderer[0];
        [SyncVar(hook = nameof(HandleTeamColorUpdated))] private Color teamColor = new Color();

        #region Server

        public override void OnStartServer()
        {
            RtsPlayer player = connectionToClient.identity.GetComponent<RtsPlayer>();
            teamColor = player.GetTeamColor();
        }

        #endregion

        #region Client

        private void HandleTeamColorUpdated(Color oldColor, Color newColor)
        {
            foreach (Renderer colorRenderer in colorRenderers)
            {
                colorRenderer.material.SetColor("_BaseColor", teamColor);
            }
        }

        #endregion
    }
}