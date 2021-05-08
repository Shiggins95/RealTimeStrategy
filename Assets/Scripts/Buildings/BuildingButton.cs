using Mirror;
using Networking;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Buildings
{
    public class BuildingButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private Building building;
        [SerializeField] private Image iconImage;
        [SerializeField] private TMP_Text priceText;
        [SerializeField] private LayerMask floorMask;

        private Camera mainCamera;
        private RtsPlayer rtsPlayer;
        private GameObject buildingPreviewInstance;
        private Renderer buildingRendererInstance;
        private BoxCollider buildingCollider;
        private void Start()
        {
            mainCamera = Camera.main;
            iconImage.sprite = building.GetIcon();
            priceText.text = building.GetPrice().ToString();
            buildingCollider = building.GetComponent<BoxCollider>();
        }

        private void Update()
        {
            if (!rtsPlayer)
            {
                rtsPlayer = NetworkClient.connection.identity.GetComponent<RtsPlayer>();
            }

            if (!buildingPreviewInstance) return;
            UpdateBuildingPreview();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left) return;
            if (rtsPlayer.GetResources() < building.GetPrice()) return;
            buildingPreviewInstance = Instantiate(building.GetBuildingPreview());
            buildingRendererInstance = buildingPreviewInstance.GetComponentInChildren<Renderer>();
            buildingPreviewInstance.SetActive(false);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!buildingPreviewInstance) return;
            Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, floorMask))
            {
                // place building
                rtsPlayer.CmdTryPlaceBuilding(building.GetId(), hit.point);
            }

            Destroy(buildingPreviewInstance);
        }

        private void UpdateBuildingPreview()
        {
            Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, floorMask)) return;
            buildingPreviewInstance.transform.position = hit.point;
            if (!buildingPreviewInstance.activeSelf)
            {
                buildingPreviewInstance.SetActive(true);
            }

            Color color = rtsPlayer.CanPlaceBuilding(buildingCollider, hit.point) ? Color.green : Color.red;

            buildingRendererInstance.material.SetColor("_BaseColor", color);
        }
    }
}