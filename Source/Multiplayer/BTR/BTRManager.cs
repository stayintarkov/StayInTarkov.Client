using BepInEx.Logging;
using Comfort.Common;
using EFT;
using EFT.AssetsManager;
using EFT.InventoryLogic;
using EFT.UI;
using EFT.Vehicle;
using HarmonyLib;
using StayInTarkov.AkiSupport.Singleplayer.Utils;
using StayInTarkov.Coop.Components.CoopGameComponents;
using StayInTarkov.Coop.Matchmaker;
using StayInTarkov.Coop.NetworkPacket.BTR;
using StayInTarkov.Coop.Players;
using StayInTarkov.Multiplayer.BTR.Utils;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Random = UnityEngine.Random;

namespace StayInTarkov.Multiplayer.BTR
{
    public sealed class BTRManager : MonoBehaviour
    {
        private GameWorld gameWorld;
        private BotEventHandler botEventHandler;

        private BotBTRService btrBotService;
        private BTRControllerClass btrController;
        private BTRVehicle btrServerSide;
        private BTRView btrClientSide;
        private BotOwner btrBotShooter;
        private BTRDataPacket btrDataPacket = default;
        private bool btrBotShooterInitialized = false;
        private string botShooterId;
        private float coverFireTime = 90f;
        private Coroutine _coverFireTimerCoroutine;

        private BTRSide lastInteractedBtrSide;
        public BTRSide LastInteractedBtrSide => lastInteractedBtrSide;

        private Coroutine _shootingTargetCoroutine;
        private BTRTurretServer btrTurretServer;
        private bool isTurretInDefaultRotation;
        private EnemyInfo currentTarget = null;
        private bool isShooting = false;
        private float machineGunAimDelay = 0.4f;
        private Vector2 machineGunBurstCount;
        private Vector2 machineGunRecoveryTime;
        private BulletClass btrMachineGunAmmo;
        private Item btrMachineGunWeapon;
        private Player.FirearmController firearmController;
        private WeaponSoundPlayer weaponSoundPlayer;

        private MethodInfo _updateTaxiPriceMethod;

        private float originalDamageCoeff;

        private ManualLogSource Logger { get; set; }

        public Vector3 Position
        {
            get
            {

                return btrClientSide.transform.position;

            }
        }

        BTRManager()
        {
            Type btrControllerType = typeof(BTRControllerClass);
            _updateTaxiPriceMethod = AccessTools.GetDeclaredMethods(btrControllerType).Single(IsUpdateTaxiPriceMethod);
        }

        #region Unity

        private void Awake()
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(nameof(BTRManager));
            Logger.LogDebug($"{nameof(Awake)}");

            try
            {
                gameWorld = Singleton<GameWorld>.Instance;
                if (gameWorld == null)
                {
                    Logger.LogError($"{nameof(Awake)} Unable to Spawn BTR, GameWorld has not been instantiated!");
                    Destroy(this);
                    return;
                }

                if (gameWorld.BtrController == null)
                {
                    gameWorld.BtrController = new BTRControllerClass();
                }

                btrController = gameWorld.BtrController;

                InitBtr();

                Comfort.Common.Singleton<BTRManager>.Create(this);
            }
            catch (Exception ex)
            {
                Logger.LogError($"{nameof(Awake)} Unable to Spawn BTR {ex}");
                ConsoleScreen.LogError("[AKI-BTR] Unable to spawn BTR");
                Destroy(this);
                throw;
            }
        }

        private void Update()
        {
            UpdateHost();
            UpdateClient();
        }

        void UpdateHost()
        {
            if (SITMatchmaking.IsClient)
                return;

            btrController.SyncBTRVehicleFromServer(UpdateDataPacket());

            if (btrController.BotShooterBtr == null) return;

            // BotShooterBtr doesn't get assigned to BtrController immediately so we check this in Update
            if (!btrBotShooterInitialized)
            {
                InitBtrBotService();
                btrBotShooterInitialized = true;
            }

            UpdateTarget();

            if (HasTarget())
            {
                SetAim();

                if (!isShooting && CanShoot())
                {
                    StartShooting();
                }
            }
            else if (!isTurretInDefaultRotation)
            {
                btrTurretServer.DisableAiming();
            }
        }

        public ConcurrentQueue<BTRPacket> BTRPacketsOnClient { get; } = new();

        void UpdateClient()
        {
            if (!SITMatchmaking.IsClient)
                return;

            if (BTRPacketsOnClient.Any())
            {
                if (BTRPacketsOnClient.TryDequeue(out var packet))
                {
                    if (!string.IsNullOrEmpty(packet.BotProfileId))
                    {
                        AttachBot(packet.BotProfileId);
                    }
                    if (packet.HasShot)
                    {
                        ReplicatedShot(packet.ShotPosition.Value, packet.ShotDirection.Value);
                    }
                    btrDataPacket = packet.DataPacket;
                }
            }

            if (!btrBotShooterInitialized)
            {
                InitBtrBotService();
                btrBotShooterInitialized = true;
            }

            btrController.SyncBTRVehicleFromServer(btrDataPacket);

            if (!isTurretInDefaultRotation)
            {
                btrTurretServer.DisableAiming();
            }
        }

        private void ReplicatedShot(Vector3 position, Vector3 direction)
        {
            gameWorld.SharedBallisticsCalculator.Shoot(btrMachineGunAmmo, position, direction, botShooterId, btrMachineGunWeapon, 1f, 0);
            firearmController.method_54(weaponSoundPlayer, btrMachineGunAmmo, position, direction, false);
        }

        public void PlayerInteractWithDoor(Player player, PlayerInteractPacket interactPacket)
        {
            bool playerGoIn = interactPacket.InteractionType == EInteractionType.GoIn;
            bool playerGoOut = interactPacket.InteractionType == EInteractionType.GoOut;

            player.BtrInteractionSide = btrClientSide.method_9(interactPacket.SideId);
            lastInteractedBtrSide = player.BtrInteractionSide;

            if (!player.IsYourPlayer)
            {
                HandleBtrDoorState(player.BtrState);
            }

            if (interactPacket.SideId == 0 && playerGoIn)
            {
                if (interactPacket.SlotId == 0)
                {
                    btrServerSide.LeftSlot0State = 1;
                }
                else if (interactPacket.SlotId == 1)
                {
                    btrServerSide.LeftSlot1State = 1;
                }
            }
            else if (interactPacket.SideId == 0 && playerGoOut)
            {
                if (interactPacket.SlotId == 0)
                {
                    btrServerSide.LeftSlot0State = 0;
                }
                else if (interactPacket.SlotId == 1)
                {
                    btrServerSide.LeftSlot1State = 0;
                }
            }
            else if (interactPacket.SideId == 1 && playerGoIn)
            {
                if (interactPacket.SlotId == 0)
                {
                    btrServerSide.RightSlot0State = 1;
                }
                else if (interactPacket.SlotId == 1)
                {
                    btrServerSide.RightSlot1State = 1;
                }
            }
            else if (interactPacket.SideId == 1 && playerGoOut)
            {
                if (interactPacket.SlotId == 0)
                {
                    btrServerSide.RightSlot0State = 0;
                }
                else if (interactPacket.SlotId == 1)
                {
                    btrServerSide.RightSlot1State = 0;
                }
            }

            GameWorld gameWorld = Singleton<GameWorld>.Instance;
            gameWorld.BtrController.BtrView.Interaction(player, interactPacket);

        }

        public void AttachBot(string profileId)
        {
            if (!SITGameComponent.TryGetCoopGameComponent(out var sitGameComponent))
                return;

            if (!sitGameComponent.Players.TryGetValue(profileId, out CoopPlayer player))
                return;

            BTRTurretView turretView = btrClientSide.turret;

            player.Transform.Original.position = turretView.BotRoot.position;
            player.PlayerBones.Weapon_Root_Anim.SetPositionAndRotation(turretView.GunRoot.position, turretView.GunRoot.rotation);

            WeaponPrefab weaponPrefab = player.HandsController.ControllerGameObject.GetComponent<WeaponPrefab>();
            if (weaponPrefab != null)
            {
                weaponSoundPlayer = weaponPrefab.GetComponent<WeaponSoundPlayer>();

                Transform weaponTransform = weaponPrefab.Hierarchy.GetTransform(ECharacterWeaponBones.weapon);
                if (weaponTransform != null)
                {
                    weaponPrefab.transform.SetPositionAndRotation(turretView.GunRoot.position, turretView.GunRoot.rotation);
                    weaponTransform.SetPositionAndRotation(turretView.GunRoot.position, turretView.GunRoot.rotation);

                    string[] gunModsToDisable = Traverse.Create(turretView).Field("_gunModsToDisable").GetValue<string[]>();
                    if (gunModsToDisable != null)
                    {
                        foreach (Transform child in weaponTransform)
                        {
                            if (gunModsToDisable.Contains(child.name))
                            {
                                child.gameObject.SetActive(false);
                            }
                        }
                    }
                }
            }

            if (player.HealthController.IsAlive)
            {
                player.BodyAnimatorCommon.enabled = false;
                if (player.HandsController.FirearmsAnimator != null)
                {
                    player.HandsController.FirearmsAnimator.Animator.enabled = false;
                }

                PlayerPoolObject component = player.gameObject.GetComponent<PlayerPoolObject>();
                foreach (Collider collider in component.Colliders)
                {
                    collider.enabled = false;
                }

                List<Renderer> rendererList = new(256);
                player.PlayerBody.GetRenderersNonAlloc(rendererList);
                if (weaponPrefab != null)
                {
                    rendererList.AddRange(weaponPrefab.Renderers);
                }
                rendererList.ForEach(renderer => renderer.forceRenderingOff = true);
            }

            firearmController = (Player.FirearmController)player.HandsController;

            btrBotShooterInitialized = true;
            botShooterId = profileId;

        }


        #endregion

        public void OnPlayerInteractDoor(PlayerInteractPacket interactPacket)
        {
            btrServerSide.LeftSlot0State = 0;
            btrServerSide.LeftSlot1State = 0;
            btrServerSide.RightSlot0State = 0;
            btrServerSide.RightSlot1State = 0;

            bool playerGoIn = interactPacket.InteractionType == EInteractionType.GoIn;

            if (interactPacket.SideId == 0 && playerGoIn)
            {
                if (interactPacket.SlotId == 0)
                {
                    btrServerSide.LeftSlot0State = 1;
                }
                else if (interactPacket.SlotId == 1)
                {
                    btrServerSide.LeftSlot1State = 1;
                }
            }
            else if (interactPacket.SideId == 1 && playerGoIn)
            {
                if (interactPacket.SlotId == 0)
                {
                    btrServerSide.RightSlot0State = 1;
                }
                else if (interactPacket.SlotId == 1)
                {
                    btrServerSide.RightSlot1State = 1;
                }
            }

            // If the player is going into the BTR, store their damage coefficient
            // and set it to 0, so they don't die while inside the BTR
            if (interactPacket.InteractionType == EInteractionType.GoIn)
            {
                originalDamageCoeff = gameWorld.MainPlayer.ActiveHealthController.DamageCoeff;
                gameWorld.MainPlayer.ActiveHealthController.SetDamageCoeff(0f);

            }
            // Otherwise restore the damage coefficient
            else if (interactPacket.InteractionType == EInteractionType.GoOut)
            {
                gameWorld.MainPlayer.ActiveHealthController.SetDamageCoeff(originalDamageCoeff);
            }
        }

        // Find `BTRControllerClass.method_9(PathDestination currentDestinationPoint, bool lastRoutePoint)`
        private bool IsUpdateTaxiPriceMethod(MethodInfo method)
        {
            return (method.GetParameters().Length == 2 && method.GetParameters()[0].ParameterType == typeof(PathDestination));
        }



        private void InitBtr()
        {
            // Initial setup
            botEventHandler = Singleton<BotEventHandler>.Instance;
            var botsController = Singleton<IBotGame>.Instance.BotsController;
            btrBotService = botsController.BotTradersServices.BTRServices;
            btrController.method_3(); // spawns server-side BTR game object
            botsController.BotSpawner.SpawnBotBTR(); // spawns the scav bot which controls the BTR's turret

            // Initial BTR configuration
            btrServerSide = btrController.BtrVehicle;
            btrClientSide = btrController.BtrView;
            //btrServerSide.transform.Find("KillBox").gameObject.AddComponent<BTRRoadKillTrigger>();

            // Get config from server and initialise respective settings
            ConfigureSettingsFromServer();

            var btrMapConfig = btrController.MapPathsConfiguration;
            btrServerSide.CurrentPathConfig = btrMapConfig.PathsConfiguration.pathsConfigurations.RandomElement();
            btrServerSide.Initialization(btrMapConfig);
            btrController.method_14(); // creates and assigns the BTR a fake stash

            DisableServerSideRenderers();

            gameWorld.MainPlayer.OnBtrStateChanged += HandleBtrDoorState;

            btrServerSide.MoveEnable();
            btrServerSide.IncomingToDestinationEvent += ToDestinationEvent;

            // Sync initial position and rotation
            UpdateDataPacket();
            btrClientSide.transform.position = btrDataPacket.position;
            btrClientSide.transform.rotation = btrDataPacket.rotation;

            // Initialise turret variables
            btrTurretServer = btrServerSide.BTRTurret;
            var btrTurretDefaultTargetTransform = (Transform)AccessTools.Field(btrTurretServer.GetType(), "defaultTargetTransform").GetValue(btrTurretServer);
            isTurretInDefaultRotation = btrTurretServer.targetTransform == btrTurretDefaultTargetTransform
                && btrTurretServer.targetPosition == btrTurretServer.defaultAimingPosition;
            btrMachineGunAmmo = (BulletClass)BTRUtil.CreateItem(BTRUtil.BTRMachineGunAmmoTplId);
            btrMachineGunWeapon = BTRUtil.CreateItem(BTRUtil.BTRMachineGunWeaponTplId);

            // Pull services data for the BTR from the server
            TraderServicesManager.Instance.GetTraderServicesDataFromServer(BTRUtil.BTRTraderId);
        }

        private void ConfigureSettingsFromServer()
        {
            var serverConfig = BTRUtil.GetConfigFromServer();

            btrServerSide.moveSpeed = serverConfig.MoveSpeed;
            btrServerSide.pauseDurationRange.x = serverConfig.PointWaitTime.Min;
            btrServerSide.pauseDurationRange.y = serverConfig.PointWaitTime.Max;
            btrServerSide.readyToDeparture = serverConfig.TaxiWaitTime;
            coverFireTime = serverConfig.CoverFireTime;
            machineGunAimDelay = serverConfig.MachineGunAimDelay;
            machineGunBurstCount = new Vector2(serverConfig.MachineGunBurstCount.Min, serverConfig.MachineGunBurstCount.Max);
            machineGunRecoveryTime = new Vector2(serverConfig.MachineGunRecoveryTime.Min, serverConfig.MachineGunRecoveryTime.Max);
        }

        private void InitBtrBotService()
        {
            btrBotShooter = btrController.BotShooterBtr;
            firearmController = btrBotShooter.GetComponent<Player.FirearmController>();
            var weaponPrefab = (WeaponPrefab)AccessTools.Field(firearmController.GetType(), "weaponPrefab_0").GetValue(firearmController);
            weaponSoundPlayer = weaponPrefab.GetComponent<WeaponSoundPlayer>();

            btrBotService.Reset(); // Player will be added to Neutrals list and removed from Enemies list
            TraderServicesManager.Instance.OnTraderServicePurchased += BtrTraderServicePurchased;
        }

        /**
         * BTR has arrived at a destination, re-calculate taxi prices and remove purchased taxi service
         */
        private void ToDestinationEvent(PathDestination destinationPoint, bool isFirst, bool isFinal, bool isLastRoutePoint)
        {
            // Remove purchased taxi service
            TraderServicesManager.Instance.RemovePurchasedService(ETraderServiceType.PlayerTaxi, BTRUtil.BTRTraderId);

            // Update the prices for the taxi service
            _updateTaxiPriceMethod.Invoke(btrController, new object[] { destinationPoint, isFinal });

            // Update the UI
            TraderServicesManager.Instance.GetTraderServicesDataFromServer(BTRUtil.BTRTraderId);
        }

        private bool IsBtrService(ETraderServiceType serviceType)
        {
            if (serviceType == ETraderServiceType.BtrItemsDelivery
                || serviceType == ETraderServiceType.PlayerTaxi
                || serviceType == ETraderServiceType.BtrBotCover)
            {
                return true;
            }

            return false;
        }

        private void BtrTraderServicePurchased(ETraderServiceType serviceType, string subserviceId)
        {
            if (!IsBtrService(serviceType))
            {
                return;
            }

            List<Player> passengers = gameWorld.AllAlivePlayersList.Where(x => x.BtrState == EPlayerBtrState.Inside).ToList();
            List<int> playersToNotify = passengers.Select(x => x.Id).ToList();
            btrController.method_6(playersToNotify, serviceType); // notify BTR passengers that a service has been purchased

            switch (serviceType)
            {
                case ETraderServiceType.BtrBotCover:
                    botEventHandler.ApplyTraderServiceBtrSupport(passengers);
                    StartCoverFireTimer(coverFireTime);
                    break;
                case ETraderServiceType.PlayerTaxi:
                    btrController.BtrVehicle.IsPaid = true;
                    btrController.BtrVehicle.MoveToDestination(subserviceId);
                    break;
            }
        }

        private void StartCoverFireTimer(float time)
        {
            _coverFireTimerCoroutine = StaticManager.BeginCoroutine(CoverFireTimer(time));
        }

        private IEnumerator CoverFireTimer(float time)
        {
            yield return new WaitForSecondsRealtime(time);
            botEventHandler.StopTraderServiceBtrSupport();
        }

        private void HandleBtrDoorState(EPlayerBtrState playerBtrState)
        {
            if (playerBtrState == EPlayerBtrState.GoIn || playerBtrState == EPlayerBtrState.GoOut)
            {
                // Open Door
                UpdateBTRSideDoorState(1);
            }
            else if (playerBtrState == EPlayerBtrState.Inside || playerBtrState == EPlayerBtrState.Outside)
            {
                // Close Door
                UpdateBTRSideDoorState(0);
            }
        }

        private void UpdateBTRSideDoorState(byte state)
        {
            try
            {
                var player = gameWorld.MainPlayer;

                BTRSide btrSide = player.BtrInteractionSide != null ? player.BtrInteractionSide : lastInteractedBtrSide;
                byte sideId = btrClientSide.GetSideId(btrSide);
                switch (sideId)
                {
                    case 0:
                        btrServerSide.LeftSideState = state;
                        break;
                    case 1:
                        btrServerSide.RightSideState = state;
                        break;
                }

                lastInteractedBtrSide = player.BtrInteractionSide;
            }
            catch
            {
                ConsoleScreen.LogError("[AKI-BTR] lastInteractedBtrSide is null when it shouldn't be. Check logs.");
                throw;
            }
        }

        private BTRDataPacket UpdateDataPacket()
        {
            btrDataPacket.position = btrServerSide.transform.position;
            btrDataPacket.rotation = btrServerSide.transform.rotation;
            if (btrTurretServer != null && btrTurretServer.gunsBlockRoot != null)
            {
                btrDataPacket.turretRotation = btrTurretServer.transform.rotation;
                btrDataPacket.gunsBlockRotation = btrTurretServer.gunsBlockRoot.rotation;
            }
            btrDataPacket.State = (byte)btrServerSide.BtrState;
            btrDataPacket.RouteState = (byte)btrServerSide.VehicleRouteState;
            btrDataPacket.LeftSideState = btrServerSide.LeftSideState;
            btrDataPacket.LeftSlot0State = btrServerSide.LeftSlot0State;
            btrDataPacket.LeftSlot1State = btrServerSide.LeftSlot1State;
            btrDataPacket.RightSideState = btrServerSide.RightSideState;
            btrDataPacket.RightSlot0State = btrServerSide.RightSlot0State;
            btrDataPacket.RightSlot1State = btrServerSide.RightSlot1State;
            btrDataPacket.currentSpeed = btrServerSide.currentSpeed;
            btrDataPacket.timeToEndPause = btrServerSide.timeToEndPause;
            btrDataPacket.moveDirection = (byte)btrServerSide.VehicleMoveDirection;
            btrDataPacket.MoveSpeed = btrServerSide.moveSpeed;
            if (btrController != null && btrController.BotShooterBtr != null)
            {
                btrDataPacket.BtrBotId = btrController.BotShooterBtr.Id;
            }

            return btrDataPacket;
        }

        private void DisableServerSideRenderers()
        {
            var meshRenderers = btrServerSide.transform.GetComponentsInChildren<MeshRenderer>();
            foreach (var renderer in meshRenderers)
            {
                renderer.enabled = false;
            }

            btrServerSide.turnCheckerObject.GetComponent<Renderer>().enabled = false; // Disables the red debug sphere
        }

        private void UpdateTarget()
        {
            currentTarget = btrBotShooter.Memory.GoalEnemy;
        }

        private bool HasTarget()
        {
            if (currentTarget != null)
            {
                return true;
            }

            return false;
        }

        private void SetAim()
        {
            if (currentTarget.IsVisible)
            {
                Vector3 targetPos = currentTarget.CurrPosition;
                Transform targetTransform = currentTarget.Person.Transform.Original;
                if (btrTurretServer.CheckPositionInAimingZone(targetPos) && btrTurretServer.targetTransform != targetTransform)
                {
                    btrTurretServer.EnableAimingObject(targetTransform);
                }
            }
            else
            {
                Vector3 targetLastPos = currentTarget.EnemyLastPositionReal;
                if (btrTurretServer.CheckPositionInAimingZone(targetLastPos)
                    && Time.time - currentTarget.PersonalLastSeenTime < 3f
                    && btrTurretServer.targetPosition != targetLastPos)
                {
                    btrTurretServer.EnableAimingPosition(targetLastPos);

                }
                else if (Time.time - currentTarget.PersonalLastSeenTime >= 3f && !isTurretInDefaultRotation)
                {
                    btrTurretServer.DisableAiming();
                }
            }
        }

        private bool CanShoot()
        {
            if (currentTarget.IsVisible && btrBotShooter.BotBtrData.CanShoot())
            {
                return true;
            }

            return false;
        }

        private void StartShooting()
        {
            _shootingTargetCoroutine = StaticManager.BeginCoroutine(ShootMachineGun());
        }

        /// <summary>
        /// Custom method to make the BTR coaxial machine gun shoot.
        /// </summary>
        private IEnumerator ShootMachineGun()
        {
            isShooting = true;

            yield return new WaitForSecondsRealtime(machineGunAimDelay);
            if (currentTarget?.Person == null || currentTarget?.IsVisible == false || !btrBotShooter.BotBtrData.CanShoot())
            {
                isShooting = false;
                yield break;
            }

            Transform machineGunMuzzle = btrTurretServer.machineGunLaunchPoint;
            var ballisticCalculator = gameWorld.SharedBallisticsCalculator;

            int burstMin = Mathf.FloorToInt(machineGunBurstCount.x);
            int burstMax = Mathf.FloorToInt(machineGunBurstCount.y);
            int burstCount = Random.Range(burstMin, burstMax + 1);
            Vector3 targetHeadPos = currentTarget.Person.PlayerBones.Head.position;
            while (burstCount > 0)
            {
                // Only update shooting position if the target isn't null
                if (currentTarget?.Person != null)
                {
                    targetHeadPos = currentTarget.Person.PlayerBones.Head.position;
                }
                Vector3 aimDirection = Vector3.Normalize(targetHeadPos - machineGunMuzzle.position);
                ballisticCalculator.Shoot(btrMachineGunAmmo, machineGunMuzzle.position, aimDirection, btrBotShooter.ProfileId, btrMachineGunWeapon, 1f, 0);
                firearmController.method_54(weaponSoundPlayer, btrMachineGunAmmo, machineGunMuzzle.position, aimDirection, false);
                burstCount--;
                yield return new WaitForSecondsRealtime(0.092308f); // 650 RPM
            }

            float waitTime = Random.Range(machineGunRecoveryTime.x, machineGunRecoveryTime.y);
            yield return new WaitForSecondsRealtime(waitTime);

            isShooting = false;
        }

        private void OnGUI()
        {
#if DEBUG
            if (btrServerSide == null || btrClientSide == null || Camera.current == null)
                return;

            Vector3 screenPos = Camera.current.WorldToScreenPoint(btrServerSide.botPosition);
            Rect rect = new();
            rect.x = screenPos.x;
            rect.y = Screen.height - (screenPos.y);
            GUI.Label(rect, $"BTR is Here!");

            screenPos = Camera.current.WorldToScreenPoint(btrClientSide.gameObject.transform.position);
            rect.x = screenPos.x;
            rect.y = Screen.height - (screenPos.y);
            GUI.Label(rect, $"BTR is Here!");
#endif
        }

        private void OnDestroy()
        {
            if (gameWorld == null)
            {
                return;
            }

            StaticManager.KillCoroutine(ref _shootingTargetCoroutine);
            StaticManager.KillCoroutine(ref _coverFireTimerCoroutine);

            if (TraderServicesManager.Instance != null)
            {
                TraderServicesManager.Instance.OnTraderServicePurchased -= BtrTraderServicePurchased;
            }

            if (gameWorld.MainPlayer != null)
            {
                gameWorld.MainPlayer.OnBtrStateChanged -= HandleBtrDoorState;
            }

            if (btrClientSide != null)
            {
                Debug.LogWarning("[AKI-BTR] BTRManager - Destroying btrClientSide");
                Destroy(btrClientSide.gameObject);
            }

            if (btrServerSide != null)
            {
                Debug.LogWarning("[AKI-BTR] BTRManager - Destroying btrServerSide");
                Destroy(btrServerSide.gameObject);
            }

            if (Comfort.Common.Singleton<BTRManager>.Instantiated)
                Singleton<BTRManager>.TryRelease(this);

        }
    }
}
