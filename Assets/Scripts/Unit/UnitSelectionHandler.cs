using System;
using System.Collections.Generic;
using Buildings;
using Mirror;
using Networking;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Unit
{
    public class UnitSelectionHandler : MonoBehaviour
    {
        [SerializeField] private LayerMask layerMask;
        [SerializeField] private RectTransform unitSelectionArea;
        private RtsPlayer rtsPlayer;
        private Vector2 startPosition;
        private Camera mainCamera;
        public List<Unit> SelectedUnits { get; } = new List<Unit>(); 

        private void Start()
        {
            // assign reference to camera
            mainCamera = Camera.main;
            Unit.AuthorityOnUnitDeSpawned += AuthorityHandleUnitDespawned;
            GameOverHandler.ClientOnGameOver += ClientHandleGameOver;
        }

        private void OnDestroy()
        {
            
            Unit.AuthorityOnUnitDeSpawned -= AuthorityHandleUnitDespawned;
            GameOverHandler.ClientOnGameOver -= ClientHandleGameOver;
        }

        private void Update()
        {
            if (!rtsPlayer)
            {
                rtsPlayer = NetworkClient.connection.identity.GetComponent<RtsPlayer>();
            }
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                // reset units to be ready for reselecting when the left button is raised
                StartSelectionArea();
            }
            else if (Mouse.current.leftButton.IsPressed())
            {
                UpdateSelectionArea();
            }
            else if (Mouse.current.leftButton.wasReleasedThisFrame)
            {
                // get units and add to list of selectedUnits
                ClearSelectionArea();
            }
        }

        private void UpdateSelectionArea()
        {
            Vector2 mousePosition = Mouse.current.position.ReadValue();
            float areaWidth = mousePosition.x - startPosition.x;
            float areaHeight = mousePosition.y - startPosition.y;

            unitSelectionArea.sizeDelta = new Vector2(Mathf.Abs(areaWidth), Mathf.Abs(areaHeight));
            unitSelectionArea.anchoredPosition = startPosition + new Vector2(areaWidth / 2, areaHeight / 2);
        }

        private void StartSelectionArea()
        {
            // allow multiple selections if holding left shift
            if (!Keyboard.current.leftShiftKey.IsPressed())
            {
                DeselectUnits();
            }
            // set image active and update start position
            unitSelectionArea.gameObject.SetActive(true);
            startPosition = Mouse.current.position.ReadValue();
            // trigger first tick of update selection area
            UpdateSelectionArea();
        }

        private void DeselectUnits()
        {
            // loop through each unit and deselect it
            foreach (Unit unit in SelectedUnits)
            {
                unit.DeSelectUnit();
            }
            // clear all units
            SelectedUnits.Clear();
        }

        private void ClearSelectionArea()
        {
            // deactivate selection box
            unitSelectionArea.gameObject.SetActive(false);
            if (unitSelectionArea.sizeDelta.magnitude == 0)
            {
                HandleSingleSelection();
                return;
            }

            HandleMultipleSelction();
        }

        private void HandleMultipleSelction()
        {
            Vector2 min = unitSelectionArea.anchoredPosition - (unitSelectionArea.sizeDelta / 2);
            Vector2 max = unitSelectionArea.anchoredPosition + (unitSelectionArea.sizeDelta / 2);

            foreach (Unit unit in rtsPlayer.GetMyUnits())
            {
                if (SelectedUnits.Contains(unit)) continue;
                Vector3 screenPosition = mainCamera.WorldToScreenPoint(unit.transform.position);
                if (screenPosition.x < min.x) return;
                if (screenPosition.x > max.x) return;
                if (screenPosition.y < min.y) return;
                if (screenPosition.y > max.y) return;
                
                SelectedUnits.Add(unit);
                unit.SelectUnit();
            }
        }

        private void HandleSingleSelection()
        {
            // get ray cast
            Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
            // return if we havent clicked anywhere clickable
            if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layerMask)) return;
            // check if we have clicked on a unit
            if (!hit.collider.TryGetComponent<Unit>(out Unit unit)) return;
            // return if we don't own the unit
            if (!unit.hasAuthority) return;
            if (SelectedUnits.Contains(unit)) return;
            // add to selectedUnits
            SelectedUnits.Add(unit);
            // update all selected units to be selected
            foreach (Unit selectedUnit in SelectedUnits)
            {
                selectedUnit.SelectUnit();
            }
        }

        private void AuthorityHandleUnitDespawned(Unit unit)
        {
            SelectedUnits.Remove(unit);
        }

        private void ClientHandleGameOver(string winnerName)
        {
            enabled = false;
        }
    }
}