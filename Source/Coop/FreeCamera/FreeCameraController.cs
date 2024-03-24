using BSG.CameraEffects;
using Comfort.Common;
using EFT;
using EFT.CameraControl;
using EFT.UI;
using HarmonyLib;
using StayInTarkov.Configuration;
using StayInTarkov.Coop.Components.CoopGameComponents;
using StayInTarkov.Coop.SITGameModes;
using System;
using UnityEngine;
using UnityStandardAssets.ImageEffects;

namespace StayInTarkov.Coop.FreeCamera
{
    /// <summary>
    /// This is HEAVILY based on Terkoiz's work found here. Thanks for your work Terkoiz! 
    /// https://dev.sp-tarkov.com/Terkoiz/Freecam/raw/branch/master/project/Terkoiz.Freecam/FreecamController.cs
    /// </summary>

    public class FreeCameraController : MonoBehaviour
    {
        //private GameObject _mainCamera;
        private FreeCamera _freeCamScript;

        private BattleUIScreen _playerUi;
        private bool _uiHidden;

        private GamePlayerOwner _gamePlayerOwner;

        public GameObject CameraParent { get; set; }
        public Camera CameraFreeCamera { get; private set; }
        public Camera CameraMain { get; private set; }

        void Awake()
        {
            CameraParent = new GameObject("CameraParent");
            var FCamera = CameraParent.GetOrAddComponent<Camera>();
            FCamera.enabled = false;
        }

        public void Start()
        {
            // Find Main Camera
            CameraMain = FPSCamera.Instance.Camera;
            if (CameraMain == null)
            {
                return;
            }

            // Add Freecam script to main camera in scene
            _freeCamScript = CameraMain.gameObject.AddComponent<FreeCamera>();
            if (_freeCamScript == null)
            {
                return;
            }

            // Get GamePlayerOwner component
            _gamePlayerOwner = GetLocalPlayerFromWorld().GetComponentInChildren<GamePlayerOwner>();
            if (_gamePlayerOwner == null)
            {
                return;
            }
        }

        private DateTime _lastTime = DateTime.MinValue;

        int DeadTime = 0;

        public void Update()
        {
            if (_gamePlayerOwner == null)
                return;

            if (_gamePlayerOwner.Player == null)
                return;

            if (_gamePlayerOwner.Player.PlayerHealthController == null)
                return;

            if (!SITGameComponent.TryGetCoopGameComponent(out var coopGC))
                return;

            var coopGame = coopGC.LocalGameInstance as CoopSITGame;
            if (coopGame == null)
                return;

            var quitState = coopGC.GetQuitState();

            if (_gamePlayerOwner.Player.PlayerHealthController.IsAlive
                && (Input.GetKey(KeyCode.F9) || (quitState != SITGameComponent.EQuitState.NONE && !_freeCamScript.IsActive))
                && _lastTime < DateTime.Now.AddSeconds(-3))
            {
                _lastTime = DateTime.Now;
                ToggleCamera();
                ToggleUi();
            }

            if (!_gamePlayerOwner.Player.PlayerHealthController.IsAlive)
            {
                // This is to make sure the screen effect remove code only get executed once, instead of running every frame.
                if (DeadTime == -1)
                    return;

                if (DeadTime < PluginConfigSettings.Instance.CoopSettings.BlackScreenOnDeathTime)
                {
                    DeadTime++;
                }
                else
                {
                    DeadTime = -1;

                    var fpsCamInstance = FPSCamera.Instance;
                    if (fpsCamInstance == null)
                        return;

                    // Reset FOV after died
                    if (fpsCamInstance.Camera != null)
                        fpsCamInstance.Camera.fieldOfView = Singleton<SettingsManager>.Instance.Game.Settings.FieldOfView;

                    var effectsController = fpsCamInstance.EffectsController;
                    if (effectsController == null)
                        return;

                    DisableAndDestroyEffect(effectsController.GetComponent<DeathFade>());
                    DisableAndDestroyEffect(effectsController.GetComponent<FastBlur>());
                    DisableAndDestroyEffect(effectsController.GetComponent<EyeBurn>());
                    DisableAndDestroyEffect(effectsController.GetComponent<TextureMask>());
                    DisableAndDestroyEffect(effectsController.GetComponent<CC_Wiggle>());
                    DisableAndDestroyEffect(effectsController.GetComponent<CC_RadialBlur>());
                    DisableAndDestroyEffect(effectsController.GetComponent<MotionBlur>());

                    var ccBlends = fpsCamInstance.EffectsController.GetComponents<CC_Blend>();
                    if (ccBlends != null)
                        foreach (var ccBlend in ccBlends)
                            DisableAndDestroyEffect(ccBlend);

                    DisableAndDestroyEffect(fpsCamInstance.VisorEffect);
                    DisableAndDestroyEffect(fpsCamInstance.NightVision);
                    DisableAndDestroyEffect(fpsCamInstance.ThermalVision);

                    // Go to free camera mode
                    ToggleCamera();
                    ToggleUi();
                }
            }
        }

        //DateTime? _lastOcclusionCullCheck = null;
        //Vector3? _playerDeathOrExitPosition;
        //bool showAtDeathOrExitPosition;

        /// <summary>
        /// Toggles the Freecam mode
        /// </summary>
        public void ToggleCamera()
        {
            // Get our own Player instance. Null means we're not in a raid
            var localPlayer = GetLocalPlayerFromWorld();
            if (localPlayer == null)
                return;

            if (!_freeCamScript.IsActive)
            {
                SetPlayerToFreecamMode(localPlayer);
            }
            else
            {
                SetPlayerToFirstPersonMode(localPlayer);
            }
        }

        /// <summary>
        /// Hides the main UI (health, stamina, stance, hotbar, etc.)
        /// </summary>
        private void ToggleUi()
        {
            // Check if we're currently in a raid
            if (GetLocalPlayerFromWorld() == null)
                return;

            // If we don't have the UI Component cached, go look for it in the scene
            if (_playerUi == null)
            {
                var gameObject = GameObject.Find("BattleUIScreen");
                if (gameObject == null)
                    return;

                _playerUi = gameObject.GetComponent<BattleUIScreen>();

                if (_playerUi == null)
                {
                    //FreecamPlugin.Logger.LogError("Failed to locate player UI");
                    return;
                }
            }

            if (_playerUi == null || _playerUi.gameObject == null)
                return;

            _playerUi.gameObject.SetActive(_uiHidden);
            _uiHidden = !_uiHidden;
        }

        /// <summary>
        /// A helper method to set the Player into Freecam mode
        /// </summary>
        /// <param name="localPlayer"></param>
        private void SetPlayerToFreecamMode(EFT.Player localPlayer)
        {
            // We set the player to third person mode
            // This means our character will be fully visible, while letting the camera move freely
            localPlayer.PointOfView = EPointOfView.ThirdPerson;

            // Get the PlayerBody reference. It's a protected field, so we have to use traverse to fetch it
            var playerBody = Traverse.Create(localPlayer).Field<PlayerBody>("_playerBody").Value;
            if (playerBody != null)
            {
                playerBody.PointOfView.Value = EPointOfView.FreeCamera;
                localPlayer.GetComponent<PlayerCameraController>().UpdatePointOfView();
            }

            _gamePlayerOwner.enabled = false;
            _freeCamScript.IsActive = true;
        }

        /// <summary>
        /// A helper method to reset the player view back to First Person
        /// </summary>
        /// <param name="localPlayer"></param>
        private void SetPlayerToFirstPersonMode(EFT.Player localPlayer)
        {
            _freeCamScript.IsActive = false;

            //if (FreecamPlugin.CameraRememberLastPosition.Value)
            //{
            //    _lastPosition = _mainCamera.transform.position;
            //    _lastRotation = _mainCamera.transform.rotation;
            //}

            // re-enable _gamePlayerOwner
            _gamePlayerOwner.enabled = true;

            localPlayer.PointOfView = EPointOfView.FirstPerson;
            FPSCamera.Instance.SetOcclusionCullingEnabled(true);

        }

        /// <summary>
        /// Gets the current <see cref="Player"/> instance if it's available
        /// </summary>
        /// <returns>Local <see cref="Player"/> instance; returns null if the game is not in raid</returns>
        private EFT.Player GetLocalPlayerFromWorld()
        {
            // If the GameWorld instance is null or has no RegisteredPlayers, it most likely means we're not in a raid
            var gameWorld = Singleton<GameWorld>.Instance;
            if (gameWorld == null || gameWorld.MainPlayer == null)
                return null;

            // One of the RegisteredPlayers will have the IsYourPlayer flag set, which will be our own Player instance
            return gameWorld.MainPlayer;
        }

        public void DisableAndDestroyEffect(MonoBehaviour effect)
        {
            if (effect != null)
            {
                effect.enabled = false;
                Destroy(effect);
            }
        }

        public void OnDestroy()
        {
            GameObject.Destroy(CameraParent);

            // Destroy FreeCamScript before FreeCamController if exists
            Destroy(_freeCamScript);
            Destroy(this);
        }
    }
}
