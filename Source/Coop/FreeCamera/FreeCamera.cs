#nullable enable

using StayInTarkov.Coop.Components.CoopGameComponents;
using StayInTarkov.Coop.Players;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace StayInTarkov.Coop.FreeCamera
{
    /// <summary>
    /// A simple free camera to be added to a Unity game object.
    /// 
    /// Full credit to Ashley Davis on GitHub for the inital code:
    /// https://gist.github.com/ashleydavis/f025c03a9221bc840a2b
    /// 
    /// This is HEAVILY based on Terkoiz's work found here. Thanks for your work Terkoiz! 
    /// https://dev.sp-tarkov.com/Terkoiz/Freecam/raw/branch/master/project/Terkoiz.Freecam/Freecam.cs
    /// </summary>
    public class FreeCamera : MonoBehaviour
    {
        private CoopPlayer? _playerSpectating;
        private bool _isSpectatingPlayer = false;

        public bool IsActive = false;

        private void StopSpectatingPlayer()
        {
            if (_playerSpectating != null)
            {
                _playerSpectating = null;
            }
            if (_isSpectatingPlayer)
            {
                _isSpectatingPlayer = false;
            }
            transform.parent = null;
        }

        private void SpectateNextPlayer()
        {
            UpdatePlayerSpectator(true);
        }

        private void SpectatePreviousPlayer()
        {
            UpdatePlayerSpectator(false);
        }

        /// <summary>
        /// Updates the player beign followed by the camera
        /// </summary>
        /// <param name="nextPlayer">True for the next player and false for the previous player</param>
        private void UpdatePlayerSpectator(bool nextPlayer)
        {
            SITGameComponent coopGameComponent = SITGameComponent.GetCoopGameComponent();
            List<CoopPlayer> players = [.. coopGameComponent
                .Players
                .Values
                .Where(x => !x.IsYourPlayer && x.HealthController.IsAlive && x.GroupId?.Contains("SIT") == true)
            ];

            if (players.Count > 0)
            {
                if (_playerSpectating == null)
                {
                    if (players[0] != null)
                    {
                        _playerSpectating = players[0];
                    }
                }
                else
                {
                    // We want to look for the next player
                    if (nextPlayer)
                    {
                        int playerIndex = players.IndexOf(_playerSpectating) + 1;
                        if (players.Count - 1 >= playerIndex)
                        {
                            _playerSpectating = players[playerIndex];
                        }
                        else
                        {
                            _playerSpectating = players[0];
                        }
                    }
                    // We are going backwards looking for the previous player
                    else
                    {
                        int playerIndex = players.IndexOf(_playerSpectating) - 1;
                        if (playerIndex >= 0)
                        {
                            _playerSpectating = players[playerIndex];
                        }
                        else
                        {
                            _playerSpectating = players[players.Count - 1];
                        }
                    }

                }

                if (_playerSpectating != null)
                {
                    // Attach the camera to the player we are spectating;
                    transform.parent = _playerSpectating?.PlayerBones.Head.Original;
                    transform.localPosition = new Vector3(-0.05f, 0.17f, -0.05f);
                    transform.localEulerAngles = new Vector3(260, 80, 0);
                    _isSpectatingPlayer = true;
                }
            }
            else
            {
                StopSpectatingPlayer();
            }
        }

        private void MoveAndRotateCamera()
        {
            bool fastMode = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            float movementSpeed = fastMode ? 20f : 3f;

            // Strafe Right
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            {
                transform.position += (-transform.right * (movementSpeed * Time.deltaTime));
            }

            // Strafe Left
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            {
                transform.position += (transform.right * (movementSpeed * Time.deltaTime));
            }

            // Forwards
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
            {
                transform.position += (transform.forward * (movementSpeed * Time.deltaTime));
            }

            // Backwards
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
            {
                transform.position += (-transform.forward * (movementSpeed * Time.deltaTime));
            }

            // Up
            if (Input.GetKey(KeyCode.Q))
            {
                transform.position += (transform.up * (movementSpeed * Time.deltaTime));
            }

            // Down
            if (Input.GetKey(KeyCode.E))
            {
                transform.position += (-transform.up * (movementSpeed * Time.deltaTime));
            }

            // Up
            if (Input.GetKey(KeyCode.R) || Input.GetKey(KeyCode.PageUp))
            {
                transform.position += (Vector3.up * (movementSpeed * Time.deltaTime));
            }

            // Down
            if (Input.GetKey(KeyCode.F) || Input.GetKey(KeyCode.PageDown))
            {
                transform.position += (-Vector3.up * (movementSpeed * Time.deltaTime));
            }

            float newRotationX = transform.localEulerAngles.y + Input.GetAxis("Mouse X") * 3f;
            float newRotationY = transform.localEulerAngles.x - Input.GetAxis("Mouse Y") * 3f;
            transform.localEulerAngles = new Vector3(newRotationY, newRotationX, 0f);
        }

        protected void OnDestroy()
        {
            Destroy(this);
        }

        protected void Update()
        {
            if (!IsActive)
            {
                return;
            }

            // Spectate the next player
            if (Input.GetKey(KeyCode.Mouse0))
            {
                SpectateNextPlayer();
            }
            // Spectate the previous player
            else if (Input.GetKey(KeyCode.Mouse1))
            {
                SpectatePreviousPlayer();
            }
            // Stop following the currently selected player
            else if (Input.GetKey(KeyCode.End))
            {
                StopSpectatingPlayer();
            }

            // If we aren't spectating anyone then just update the camera normally
            if (!_isSpectatingPlayer)
            {
                MoveAndRotateCamera();
            }
        }
    }
}