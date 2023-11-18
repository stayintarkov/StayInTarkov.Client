using JetBrains.Annotations;
using UnityEngine;

namespace SIT.Core.Coop.FreeCamera
{
    /// <summary>
    /// A simple free camera to be added to a Unity game object.
    /// 
    /// Full credit to Ashley Davis on GitHub for the inital code:
    /// https://gist.github.com/ashleydavis/f025c03a9221bc840a2b
    /// 
    /// This is HEAVILY based on Terkoiz's work found here. Thanks for your work Terkoiz! 
    /// https://dev.sp-tarkov.com/Terkoiz/Freecam/raw/branch/master/project/Terkoiz.Freecam/FreecamController.cs
    /// </summary>
    public class FreeCamera : MonoBehaviour
    {
        public bool IsActive = false;

        [UsedImplicitly]
        public void Update()
        {
            if (!IsActive)
            {
                return;
            }

            var fastMode = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            var movementSpeed = fastMode ? 10f : 2f;

            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            {
                transform.position += (-transform.right * (movementSpeed * Time.deltaTime));
            }

            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            {
                transform.position += (transform.right * (movementSpeed * Time.deltaTime));
            }

            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
            {
                transform.position += (transform.forward * (movementSpeed * Time.deltaTime));
            }

            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
            {
                transform.position += (-transform.forward * (movementSpeed * Time.deltaTime));
            }

            if (true)
            {
                if (Input.GetKey(KeyCode.Q))
                {
                    transform.position += (transform.up * (movementSpeed * Time.deltaTime));
                }

                if (Input.GetKey(KeyCode.E))
                {
                    transform.position += (-transform.up * (movementSpeed * Time.deltaTime));
                }

                if (Input.GetKey(KeyCode.R) || Input.GetKey(KeyCode.PageUp))
                {
                    transform.position += (Vector3.up * (movementSpeed * Time.deltaTime));
                }

                if (Input.GetKey(KeyCode.F) || Input.GetKey(KeyCode.PageDown))
                {
                    transform.position += (-Vector3.up * (movementSpeed * Time.deltaTime));
                }
            }

            float newRotationX = transform.localEulerAngles.y + Input.GetAxis("Mouse X") * 3f;
            float newRotationY = transform.localEulerAngles.x - Input.GetAxis("Mouse Y") * 3f;
            transform.localEulerAngles = new Vector3(newRotationY, newRotationX, 0f);

            //if (FreecamPlugin.CameraMousewheelZoom.Value)
            //{
            //    float axis = Input.GetAxis("Mouse ScrollWheel");
            //    if (axis != 0)
            //    {
            //        var zoomSensitivity = fastMode ? FreecamPlugin.CameraFastZoomSpeed.Value : FreecamPlugin.CameraZoomSpeed.Value;
            //        transform.position += transform.forward * (axis * zoomSensitivity);
            //    }
            //}
        }

        [UsedImplicitly]
        private void OnDestroy()
        {
            Destroy(this);
        }
    }
}