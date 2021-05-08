using System;
using Mirror;
using Networking;
using TMPro;
using UnityEngine;

namespace Resources
{
    public class ResourcesDisplay : MonoBehaviour
    {
        [SerializeField] private TMP_Text resourceText;
        
        private RtsPlayer rtsPlayer;

        private void Update()
        {
            if (rtsPlayer) return;
            rtsPlayer = NetworkClient.connection.identity.GetComponent<RtsPlayer>();
            if (!rtsPlayer) return;
            ClientHandleResourcesUpdated(rtsPlayer.GetResources());
            rtsPlayer.ClientOnResourcesUpdated += ClientHandleResourcesUpdated;
        }

        private void OnDestroy()
        {
            rtsPlayer.ClientOnResourcesUpdated -= ClientHandleResourcesUpdated;
        }

        private void ClientHandleResourcesUpdated(int newResources)
        {
            resourceText.text = $"Resources: {newResources}";
        }
    }
}