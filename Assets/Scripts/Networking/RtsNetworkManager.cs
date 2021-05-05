using Buildings;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Networking
{
    public class RtsNetworkManager : NetworkManager
    {
        [SerializeField] private GameObject unitSpawnerPrefab;
        [SerializeField] private GameOverHandler gameOverHandler;
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

        public override void OnServerSceneChanged(string sceneName)
        {
            Debug.Log($"SCENE NAME: {sceneName}");
            if (!SceneManager.GetActiveScene().name.StartsWith("Scene_Map")) return;
            GameOverHandler gameOverHandlerInstance = Instantiate(gameOverHandler);
            NetworkServer.Spawn(gameOverHandlerInstance.gameObject);
        }
    }
}