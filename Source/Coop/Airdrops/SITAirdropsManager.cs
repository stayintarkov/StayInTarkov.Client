using Aki.Custom.Airdrops.Models;
using Aki.Custom.Airdrops.Utils;
using BepInEx.Logging;
using Comfort.Common;
using EFT;
using StayInTarkov;
using StayInTarkov.AkiSupport.Airdrops;
using StayInTarkov.AkiSupport.Airdrops.Models;
using StayInTarkov.AkiSupport.Airdrops.Utils;
using StayInTarkov.Coop.Components.CoopGameComponents;
using StayInTarkov.Coop.Matchmaker;
using StayInTarkov.Coop.NetworkPacket.Airdrop;
using StayInTarkov.Networking;
using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

namespace Aki.Custom.Airdrops
{
    /// <summary>
    /// Created by: SPT-Aki team
    /// Link: https://dev.sp-tarkov.com/SPT-AKI/Modules/src/branch/master/project/Aki.Custom/Airdrops/AirdropsManager.cs
    /// Modified by: Paulov. Added syncronization packet handling from Server to Clients.
    /// </summary>
    public class SITAirdropsManager : MonoBehaviour
    {
        private AirdropPlane airdropPlane;
        private ItemFactoryUtil factory;

        private float distanceTravelled = 0;

        private DateTime LastSyncTime { get; set; }
        private ManualLogSource Logger { get; } = BepInEx.Logging.Logger.CreateLogSource(nameof(SITAirdropsManager));

        public AirdropBox AirdropBox { get; private set; }
        public AirdropParametersModel AirdropParameters { get; set; }
        public bool ClientPlaneSpawned { get; private set; }
        public AirdropLootResultModel ClientAirdropLootResultModel { get; private set; }
        public AirdropConfigModel ClientAirdropConfigModel { get; private set; }
        public bool ClientLootBuilt { get; private set; }
        public bool IsFlareDrop { get; set; }

        void Awake()
        {
            Logger.LogInfo("Awake");
            Singleton<SITAirdropsManager>.Create(this);
        }

        public async void Start()
        {
            var gameWorld = Singleton<GameWorld>.Instance;

            Logger.LogDebug("Start");

            if (gameWorld == null)
            {
                Logger.LogDebug("gameWorld is NULL");
                Destroy(this);
            }

            string location = gameWorld.MainPlayer.Location;
            if (location.StartsWith("factory") || location == "laboratory" || location == "Sandbox")
            {
                Destroy(this);
                return;
            }

            // If this is not the server, then this manager will have to wait for the packet to initialize stuff.
            if (SITMatchmaking.IsClient)
                return;

            // The server will generate stuff ready for the packet

            AirdropParameters = await AirdropUtil.InitAirdropParams(gameWorld, IsFlareDrop);

            if (!AirdropParameters.AirdropAvailable)
            {
                Logger.LogDebug("Airdrop is not available");
                Destroy(this);
                return;
            }

            try
            {
                airdropPlane = await AirdropPlane.Init(
                    AirdropParameters.RandomAirdropPoint,
                    AirdropParameters.DropHeight,
                    AirdropParameters.Config.PlaneVolume,
                    AirdropParameters.Config.PlaneSpeed,
                    AirdropParameters.PlaneLookAt);
                AirdropBox = await AirdropBox.Init(AirdropParameters.Config.CrateFallSpeed);
                factory = new ItemFactoryUtil();
                AirdropParameters.PlaneSpawnPoint = airdropPlane.planeServerPosition;
                AirdropParameters.PlaneLookAt = airdropPlane.planeServerLookAt;
            }
            catch
            {
                Logger.LogError("[AKI-AIRDROPS]: Unable to create plane or crate, airdrop won't occur");
                Destroy(this);
                throw;
            }

            SetDistanceToDrop();

            await BuildLootContainer(AirdropParameters.Config);

            StartCoroutine(SendParamsToClients());
        }

        public IEnumerator SendParamsToClients()
        {
            if (!SITMatchmaking.IsServer)
                yield break;

            yield return new WaitForSeconds(AirdropParameters.TimeToStart);

            Logger.LogDebug("Sending Airdrop Params");
            AirdropPacket airdropPacket = new()
            {
                AirdropParametersModelJson = AirdropParameters.SITToJson()
            };
            GameClient.SendData(airdropPacket.Serialize());

            yield break;
        }

        public async void FixedUpdate()
        {
            if (AirdropParameters == null || AirdropParameters.Config == null)
                return;

            // If we are a client. Wait until the server has sent all the data.
            if (SITMatchmaking.IsClient && (ClientAirdropLootResultModel == null || ClientAirdropConfigModel == null))
                return;

            // If we have all the parameters sent from the Server. Lets build the plane, box, container and loot
            if (SITMatchmaking.IsClient && !ClientLootBuilt)
            {
                ClientLootBuilt = true;
                Logger.LogInfo("Client::Building Plane, Box, Factory and Loot.");

                airdropPlane = await AirdropPlane.Init(
                    AirdropParameters.PlaneSpawnPoint,
                    AirdropParameters.DropHeight,
                    AirdropParameters.Config.PlaneVolume,
                    AirdropParameters.Config.PlaneSpeed,
                    AirdropParameters.PlaneLookAt);
                AirdropBox = await AirdropBox.Init(AirdropParameters.Config.CrateFallSpeed);
                factory = new ItemFactoryUtil();

                factory.BuildContainer(AirdropBox.Container, ClientAirdropConfigModel, ClientAirdropLootResultModel.DropType);
                factory.AddLoot(AirdropBox.Container, ClientAirdropLootResultModel);

                if (AirdropBox.Container != null && SITGameComponent.TryGetCoopGameComponent(out SITGameComponent coopGameComponent))
                {
                    Logger.LogDebug($"Adding Airdrop box with id {AirdropBox.Container.Id}");
                    coopGameComponent.WorldnteractiveObjects.TryAdd(AirdropBox.Container.Id, AirdropBox.Container);
                }
            }

            if (!ClientLootBuilt)
                return;

            if (airdropPlane == null || AirdropBox == null || factory == null)
                return;

            if (SITMatchmaking.IsServer || SITMatchmaking.IsSinglePlayer)
            {
                AirdropParameters.Timer += 0.02f;

                if (AirdropParameters.Timer >= AirdropParameters.TimeToStart && !AirdropParameters.PlaneSpawned)
                {
                    SendParamsToClients();
                    StartPlane();
                }

                if (!AirdropParameters.PlaneSpawned)
                {
                    return;
                }
            }
            else
            {
                AirdropParameters.Timer += 0.02f;

                if (!ClientPlaneSpawned)
                {
                    ClientPlaneSpawned = true;
                    StartPlane();
                }
            }

            if (distanceTravelled >= AirdropParameters.DistanceToDrop && !AirdropParameters.BoxSpawned)
            {
                StartBox();
            }

            if (AirdropParameters.BoxSpawned && !SITMatchmaking.IsClient)
            {
                if (this.LastSyncTime < DateTime.Now.AddSeconds(-1))
                {
                    this.LastSyncTime = DateTime.Now;

                    AirdropBoxPositionSyncPacket packet = new()
                    {
                        Position = AirdropBox.transform.position
                    };
                    GameClient.SendData(packet.Serialize());
                }
            }

            if (distanceTravelled < AirdropParameters.DistanceToTravel)
            {
                distanceTravelled += Time.deltaTime * AirdropParameters.Config.PlaneSpeed;
                var distanceToDrop = AirdropParameters.DistanceToDrop - distanceTravelled;
                airdropPlane.ManualUpdate(distanceToDrop);
            }
            else
            {
                Destroy(airdropPlane.gameObject);
                Destroy(this);
            }
        }

        private void StartPlane()
        {
            airdropPlane.gameObject.SetActive(true);
            AirdropParameters.PlaneSpawned = true;
        }

        private void StartBox()
        {
            AirdropParameters.BoxSpawned = true;
            var pointPos = AirdropParameters.RandomAirdropPoint;
            var dropPos = new Vector3(pointPos.x, AirdropParameters.DropHeight, pointPos.z);
            AirdropBox.gameObject.SetActive(true);
            AirdropBox.StartCoroutine(AirdropBox.DropCrate(dropPos));
        }

        private async Task BuildLootContainer(AirdropConfigModel config)
        {
            if (!SITMatchmaking.IsServer)
                return;

            // Get the lootData for this Raid
            AirdropLootResultModel lootData = await factory.GetLoot() ?? throw new Exception("Airdrops. Tried to BuildLootContainer without any Loot.");

            // Send the lootData to Clients.
            AirdropLootPacket airdropLootPacket = new()
            {
                AirdropLootResultModelJson = lootData.SITToJson(),
                AirdropConfigModelJson = config.SITToJson()
            };
            GameClient.SendData(airdropLootPacket.Serialize());

            factory.BuildContainer(AirdropBox.Container, config, lootData.DropType);
            factory.AddLoot(AirdropBox.Container, lootData);
            ClientLootBuilt = true;

            if (AirdropBox.Container != null && SITGameComponent.TryGetCoopGameComponent(out SITGameComponent coopGameComponent))
            {
                Logger.LogDebug($"Adding Airdrop box with id {AirdropBox.Container.Id}");
                coopGameComponent.WorldnteractiveObjects.TryAdd(AirdropBox.Container.Id, AirdropBox.Container);
            }
        }

        public void ReceiveBuildLootContainer(AirdropLootResultModel lootData, AirdropConfigModel config)
        {
            Logger.LogDebug(nameof(ReceiveBuildLootContainer));
            ClientAirdropConfigModel = config;
            ClientAirdropLootResultModel = lootData;
        }

        private void SetDistanceToDrop()
        {
            AirdropParameters.DistanceToDrop = Vector3.Distance(
                new Vector3(AirdropParameters.RandomAirdropPoint.x, AirdropParameters.DropHeight, AirdropParameters.RandomAirdropPoint.z),
                airdropPlane.transform.position);
        }

        protected void OnDestroy()
        {
            if (Singleton<SITAirdropsManager>.Instantiated)
                Singleton<SITAirdropsManager>.Release(Singleton<SITAirdropsManager>.Instance);
        }
    }
}