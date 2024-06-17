#nullable enable

using BepInEx.Logging;
using BSG.CameraEffects;
using Comfort.Common;
using EFT;
using EFT.CameraControl;
using EFT.UI;
using StayInTarkov.Configuration;
using StayInTarkov.Coop.Components.CoopGameComponents;
using StayInTarkov.Coop.Players;
using System;
using System.Collections;
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
        private FreeCamera? _freeCamScript;

        private BattleUIScreen? _playerUi;
        private bool _uiHidden;

        private GamePlayerOwner? _gamePlayerOwner;
        private DateTime _lastTime = DateTime.MinValue;

        private ManualLogSource Logger { get; } = BepInEx.Logging.Logger.CreateLogSource("FreeCameraController");
        private CoopPlayer Player => (CoopPlayer) Singleton<GameWorld>.Instance.MainPlayer;

        public GameObject? CameraParent { get; set; }
        public Camera? CameraFreeCamera { get; private set; }
        public Camera? CameraMain { get; private set; }

        protected void Awake()
        {
            CameraParent = new GameObject("CameraParent");
            Camera FCamera = CameraParent.GetOrAddComponent<Camera>();
            FCamera.enabled = false;
        }

        public void Start()
        {
            // Find Main Camera
            CameraMain = CameraClass.Instance.Camera;
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
            _gamePlayerOwner = GetLocalPlayerFromWorld()?.GetComponentInChildren<GamePlayerOwner>();
            if (_gamePlayerOwner == null)
            {
                return;
            }

            Player.OnPlayerDead += Player_OnPlayerDead;
        }

        private IEnumerator PlayerDeathRoutine()
        {
            yield return new WaitForSeconds(PluginConfigSettings.Instance?.CoopSettings.BlackScreenOnDeathTime ?? 5);

            var fpsCamInstance = CameraClass.Instance;
            if (fpsCamInstance == null)
            {
                Logger.LogDebug("fpsCamInstance for camera is null");
                yield break;
            }

            // Reset FOV after died
            if (fpsCamInstance.Camera != null)
                fpsCamInstance.Camera.fieldOfView = Singleton<SharedGameSettingsClass>.Instance.Game.Settings.FieldOfView;

            EffectsController effectsController = fpsCamInstance.EffectsController;
            if (effectsController == null)
            {
                Logger.LogDebug("effects controller for camera is null");
                yield break;
            }

            DisableAndDestroyEffect(effectsController.GetComponent<DeathFade>());
            DisableAndDestroyEffect(effectsController.GetComponent<FastBlur>());
            DisableAndDestroyEffect(effectsController.GetComponent<EyeBurn>());
            DisableAndDestroyEffect(effectsController.GetComponent<TextureMask>());
            DisableAndDestroyEffect(effectsController.GetComponent<CC_Wiggle>());
            DisableAndDestroyEffect(effectsController.GetComponent<CC_RadialBlur>());
            DisableAndDestroyEffect(effectsController.GetComponent<MotionBlur>());
            DisableAndDestroyEffect(effectsController.GetComponent<BloodOnScreen>());
            DisableAndDestroyEffect(effectsController.GetComponent<GrenadeFlashScreenEffect>());
            DisableAndDestroyEffect(effectsController.GetComponent<DepthOfField>());
            //DisableAndDestroyEffect(effectsController.GetComponent<RainScreenDrops>());

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

        private void Player_OnPlayerDead(EFT.Player player, IPlayer lastAggressor, DamageInfo damageInfo, EBodyPart part)
        {
            Player.OnPlayerDead -= Player_OnPlayerDead;
            StartCoroutine(PlayerDeathRoutine());
        }

        public void Update()
        {
            if (_gamePlayerOwner == null)
                return;

            if (Player == null)
                return;

            if (Player.PlayerHealthController == null)
                return;

            if (!SITGameComponent.TryGetCoopGameComponent(out SITGameComponent coopGC))
                return;


            var quitState = coopGC.GetQuitState();
            if (Player.PlayerHealthController.IsAlive && 
                (Input.GetKey(KeyCode.F9) || (quitState != SITGameComponent.EQuitState.NONE && _freeCamScript?.IsActive == false)) && 
                _lastTime < DateTime.Now.AddSeconds(-3))
            {
                _lastTime = DateTime.Now;
                ToggleCamera();
                ToggleUi();
            }            
        }

        /// <summary>
        /// Toggles the Freecam mode
        /// </summary>
        public void ToggleCamera()
        {
            // Get our own Player instance. Null means we're not in a raid
            if (Player == null)
                return;

            if (_freeCamScript?.IsActive == false)
            {
                GameObject[] allGameObject = Resources.FindObjectsOfTypeAll<GameObject>();
                foreach (GameObject gobj in allGameObject)
                {
                    gobj.GetComponent<DisablerCullingObject>()?.ForceEnable(true);
                }
                SetPlayerToFreecamMode(Player);
            }
            else
            {
                SetPlayerToFirstPersonMode(Player);
            }
        }

        /// <summary>
        /// Hides the main UI (health, stamina, stance, hotbar, etc.)
        /// </summary>
        public void ToggleUi()
        {
            // Check if we're currently in a raid
            if (Player == null)
                return;

            // If we don't have the UI Component cached, go look for it in the scene
            if (_playerUi == null)
            {
                GameObject gameObject = GameObject.Find("BattleUIScreen");
                if (gameObject == null)
                    return;

                _playerUi = gameObject.GetComponent<BattleUIScreen>();
                if (_playerUi == null)
                {
                    Logger.LogError("Failed to locate player UI");
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

            if (localPlayer.PlayerBody != null)
            {
                localPlayer.PlayerBody.PointOfView.Value = EPointOfView.FreeCamera;
                localPlayer.GetComponent<PlayerCameraController>().UpdatePointOfView();
            }

            if (_gamePlayerOwner != null)
            {
                _gamePlayerOwner.enabled = false;
            }
            if (_freeCamScript != null)
            {
                _freeCamScript.IsActive = true;
            }
        }

        /// <summary>
        /// A helper method to reset the player view back to First Person
        /// </summary>
        /// <param name="localPlayer"></param>
        private void SetPlayerToFirstPersonMode(EFT.Player localPlayer)
        {
            if (_freeCamScript != null)
            {
                _freeCamScript.IsActive = true;
            }

            // re-enable _gamePlayerOwner
            if (_gamePlayerOwner != null)
            {
                _gamePlayerOwner.enabled = false;
            }

            localPlayer.PointOfView = EPointOfView.FirstPerson;
            CameraClass.Instance.SetOcclusionCullingEnabled(true);

        }

        /// <summary>
        /// Gets the current <see cref="Player"/> instance if it's available
        /// </summary>
        /// <returns>Local <see cref="Player"/> instance; returns null if the game is not in raid</returns>
        private EFT.Player? GetLocalPlayerFromWorld()
        {
            // If the GameWorld instance is null or has no RegisteredPlayers, it most likely means we're not in a raid
            GameWorld gameWorld = Singleton<GameWorld>.Instance;
            if (gameWorld == null || gameWorld.MainPlayer == null)
            {
                return null;
            }

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
            Destroy(CameraParent);

            // Destroy FreeCamScript before FreeCamController if exists
            Destroy(_freeCamScript);
            Destroy(this);
        }
    }
}
