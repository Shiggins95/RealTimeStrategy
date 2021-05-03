using Mirror;
using UnityEngine;

namespace Networking
{
    public class RtsNetworkManager : NetworkManager
    {
        [SerializeField] private GameObject unitSpawnerPrefab;

        public override void OnServerAddPlayer(NetworkConnection conn)
        {
            base.OnServerAddPlayer(conn);
            var spawnedPlayerTransform = conn.identity.transform;
            // create new game object using the spawnable prefab (prefab must be set it Netowrk Manager
            // from Mirror as well
            GameObject unitSpawnerInstance = Instantiate(
                unitSpawnerPrefab,
                spawnedPlayerTransform.position,
                spawnedPlayerTransform.rotation
            );
            // spawn on server using and assign which user this player is for ß(conn)
            NetworkServer.Spawn(unitSpawnerInstance, conn);
        }
    }
}