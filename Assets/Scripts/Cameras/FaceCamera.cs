using System;
using UnityEngine;

namespace Cameras
{
    public class FaceCamera : MonoBehaviour
    {
        private Transform mainCameraTransform;

        private void Start()
        {
            if (!Camera.main) return;
            mainCameraTransform = Camera.main.transform;
        }

        private void LateUpdate()
        {
            Quaternion cameraRotation = mainCameraTransform.rotation;
            transform.LookAt(
                transform.position + cameraRotation * Vector3.forward,
                cameraRotation * Vector3.up
            );
        }
    }
}