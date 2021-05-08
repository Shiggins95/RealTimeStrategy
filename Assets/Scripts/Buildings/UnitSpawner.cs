using System;
using System.Net.Mime;
using Combat;
using Mirror;
using Networking;
using TMPro;
using Unit;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Buildings
{
    public class UnitSpawner : NetworkBehaviour, IPointerClickHandler
    {
        // get references to what we need
        [SerializeField] private Health health;
        [SerializeField] private Unit.Unit unitPrefab;
        [SerializeField] private Transform unitSpawnPoint;
        [SerializeField] private TMP_Text remainingUnitsText;
        [SerializeField] private Image UnitProgressImage;
        [SerializeField] private int maxUnitQueue = 5;
        [SerializeField] private float spawnMoveRange = 7f;
        [SerializeField] private float unitSpawnDuration = 5f;
        [SerializeField] private float progressImageVelocity;

        [SyncVar(hook = nameof(ClientHandleQueuedUnitsUpdated))]
        private int queuedUnits;

        [SyncVar] private float unitTimer;

        private RtsPlayer rtsPlayer;

        private void Start()
        {
            if (isClient)
            {
                remainingUnitsText.text = "0";
            }
        }

        private void Update()
        {
            if (isServer)
            {
                ProduceUnits();
            }

            if (isClient)
            {
                UpdateUI();
            }
        }

        #region Server

        public override void OnStartServer()
        {
            health.ServerOnDie += ServerHandleDie;
        }

        public override void OnStopServer()
        {
            health.ServerOnDie -= ServerHandleDie;
        }

        [Server]
        private void ServerHandleDie()
        {
            NetworkServer.Destroy(gameObject);
        }

        // server command
        [Command]
        private void CmdSpawnUnit()
        {
            if (queuedUnits == maxUnitQueue) return;
            rtsPlayer = connectionToClient.identity.GetComponent<RtsPlayer>();
            int resources = rtsPlayer.GetResources();
            if (resources < unitPrefab.GetResourceCost()) return;
            queuedUnits++;
            rtsPlayer.SetResources(rtsPlayer.GetResources() - unitPrefab.GetResourceCost());
        }

        [Server]
        private void ProduceUnits()
        {
            if (queuedUnits == 0) return;
            unitTimer += Time.deltaTime;
            if (unitTimer < unitSpawnDuration) return;
            Vector3 spawnPosition = unitSpawnPoint.position;
            GameObject unitInstance =
                Instantiate(unitPrefab.gameObject, spawnPosition, unitSpawnPoint.rotation);
            NetworkServer.Spawn(unitInstance, connectionToClient);

            Vector3 spawnOffset = Random.insideUnitSphere * spawnMoveRange;
            spawnOffset.y = spawnPosition.y;

            UnitMovement unitMovement = unitInstance.GetComponent<UnitMovement>();
            unitMovement.ServerMove(spawnPosition + spawnOffset);
            queuedUnits--;
            unitTimer = 0;
        }

        #endregion

        #region Client

        // handle click logic
        public void OnPointerClick(PointerEventData eventData)
        {
            // only perform for left button click
            if (eventData.button != PointerEventData.InputButton.Left) return;

            // return if we don't own the unit
            if (!hasAuthority) return;

            // Spawn unit
            CmdSpawnUnit();
        }

        private void ClientHandleQueuedUnitsUpdated(int oldUnits, int newUnits)
        {
            remainingUnitsText.text = newUnits.ToString();
        }

        private void UpdateUI()
        {
            float newProgress = unitTimer / unitSpawnDuration;

            if (newProgress < UnitProgressImage.fillAmount)
            {
                UnitProgressImage.fillAmount = newProgress;
            }
            else
            {
                UnitProgressImage.fillAmount = Mathf.SmoothDamp(
                    UnitProgressImage.fillAmount,
                    newProgress,
                    ref progressImageVelocity,
                    0.1f
                );
            }
        }

        #endregion
    }
}