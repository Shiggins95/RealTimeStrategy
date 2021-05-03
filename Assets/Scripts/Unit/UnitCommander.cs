using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Unit
{
    public class UnitCommander : MonoBehaviour
    {
        [SerializeField] private UnitSelectionHandler unitSelectionHandler;
        [SerializeField] private LayerMask layerMask;
        private Camera mainCamera;
        private void Start()
        {
            // assign reference to camera
            mainCamera = Camera.main;
        }

        private void Update()
        {
            // return if no right button press
            if (!Mouse.current.rightButton.wasPressedThisFrame) return;
            // get ray cast of mouse position
            Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
            // return if we don't hit anywhere that we can move to
            if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layerMask)) return;
            // move
            TryMove(hit.point);
        }

        private void TryMove(Vector3 point)
        {
            // loop through each unit and move it to selected point
            foreach (Unit unit in unitSelectionHandler.SelectedUnits)
            {
                unit.GetUnitMovement().CmdMove(point);
            }
        }
    }
}