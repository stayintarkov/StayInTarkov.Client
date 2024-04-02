using Aki.Custom.Airdrops.Models;
using Aki.Custom.Airdrops.Utils;
using BepInEx.Logging;
using Comfort.Common;
using EFT;
using EFT.Game.Spawning;
using EFT.Interactive;
using StayInTarkov;
using StayInTarkov.AkiSupport.Airdrops;
using StayInTarkov.AkiSupport.Airdrops.Models;
using StayInTarkov.AkiSupport.Airdrops.Utils;
using StayInTarkov.Coop.Components.CoopGameComponents;
using StayInTarkov.Coop.Matchmaker;
using StayInTarkov.Coop.NetworkPacket;
using StayInTarkov.Coop.Web;
using StayInTarkov.Networking;
using System.Collections;
using System.Collections.Generic;
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
        public AirdropBox AirdropBox { get; private set; }
        private ItemFactoryUtil factory;

        public bool isFlareDrop;
        public AirdropParametersModel AirdropParameters { get; set; }
        private ManualLogSource Logger { get; set; }

        void Awake()
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource("SITAirdropsManager");
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

            AirdropParameters = AirdropUtil.InitAirdropParams(gameWorld, isFlareDrop);

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

            BuildLootContainer(AirdropParameters.Config);

            StartCoroutine(SendParamsToClients());

        }

        public IEnumerator SendParamsToClients()
        {
            if (!SITMatchmaking.IsServer)
                yield break;

            yield return new WaitForSeconds(AirdropParameters.TimeToStart);

            Logger.LogDebug("Sending Airdrop Params");
            var packet = new Dictionary<string, object>();
            packet.Add("serverId", SITGameComponent.GetServerId());
            packet.Add("m", "AirdropPacket");
            packet.Add("model", AirdropParameters);
            GameClient.SendData(packet.SITToJson());

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
                if (AirdropBox.Container != null)
                {
                    if (SITGameComponent.TryGetCoopGameComponent(out var coopGameComponent))
                    {
                        List<WorldInteractiveObject> oldInteractiveObjectList = new List<WorldInteractiveObject>(coopGameComponent.ListOfInteractiveObjects)
                        {
                            AirdropBox.Container
                        };
                        coopGameComponent.ListOfInteractiveObjects = [.. oldInteractiveObjectList];
                    }
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

        float distanceTravelled = 0;

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

        public bool ClientPlaneSpawned { get; private set; }
        public AirdropLootResultModel ClientAirdropLootResultModel { get; private set; }
        public AirdropConfigModel ClientAirdropConfigModel { get; private set; }
        public bool ClientLootBuilt { get; private set; }

        private void BuildLootContainer(AirdropConfigModel config)
        {
            if (SITMatchmaking.IsClient)
                return;

            var lootData = factory.GetLoot();

            // Get the lootData. Sent to Clients.
            if (SITMatchmaking.IsServer)
            {
                var packet = new Dictionary<string, object>();
                packet.Add("serverId", SITGameComponent.GetServerId());
                packet.Add("m", "AirdropLootPacket");
                packet.Add("config", config);
                packet.Add("result", lootData);
                GameClient.SendData(packet.SITToJson());
            }

            if (lootData == null)
                throw new System.Exception("Airdrops. Tried to BuildLootContainer without any Loot.");
            
            factory.BuildContainer(AirdropBox.Container, config, lootData.DropType);
            factory.AddLoot(AirdropBox.Container, lootData);
            ClientLootBuilt = true;
            if (AirdropBox.Container != null)
            {
                if (SITGameComponent.TryGetCoopGameComponent(out var coopGameComponent))
                {
                    List<WorldInteractiveObject> oldInteractiveObjectList = new List<WorldInteractiveObject>(coopGameComponent.ListOfInteractiveObjects)
                    {
                        AirdropBox.Container
                    };
                    coopGameComponent.ListOfInteractiveObjects = [.. oldInteractiveObjectList];
                }
            }
        }

        public void ReceiveBuildLootContainer(AirdropLootResultModel lootData, AirdropConfigModel config)
        {
            ClientAirdropConfigModel = config;
            ClientAirdropLootResultModel = lootData;
        }

        private void SetDistanceToDrop()
        {
            AirdropParameters.DistanceToDrop = Vector3.Distance(
                new Vector3(AirdropParameters.RandomAirdropPoint.x, AirdropParameters.DropHeight, AirdropParameters.RandomAirdropPoint.z),
                airdropPlane.transform.position);
        }
    }
}