using Aki.Custom.Airdrops.Models;
using Aki.Custom.Airdrops.Utils;
using BepInEx.Logging;
using Comfort.Common;
using EFT;
using StayInTarkov;
using StayInTarkov.AkiSupport.Airdrops;
using StayInTarkov.AkiSupport.Airdrops.Models;
using StayInTarkov.AkiSupport.Airdrops.Utils;
using StayInTarkov.Coop;
using StayInTarkov.Coop.Matchmaker;
using StayInTarkov.Coop.NetworkPacket;
using StayInTarkov.Coop.Web;
using StayInTarkov.Networking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GClass1657;

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
        private AirdropBox airdropBox;
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

            // If this is not the server, then this manager will have to wait for the packet to initialize stuff.
            if (!MatchmakerAcceptPatches.IsServer)
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
                    AirdropParameters.Config.PlaneSpeed);
                airdropBox = await AirdropBox.Init(AirdropParameters.Config.CrateFallSpeed);
                factory = new ItemFactoryUtil();
            }
            catch
            {
                Logger.LogError("[AKI-AIRDROPS]: Unable to create plane or crate, airdrop won't occur");
                Destroy(this);
                throw;
            }

            SetDistanceToDrop();

            BuildLootContainer(AirdropParameters.Config);


            StartCoroutine(PeriodicallySendParams());

        }

        public IEnumerator PeriodicallySendParams()
        {
            if (!MatchmakerAcceptPatches.IsServer)
                yield break;

            yield return new WaitForSeconds(AirdropParameters.TimeToStart);

            Logger.LogDebug("Sending Airdrop Params");
            var packet = new Dictionary<string, object>();
            packet.Add("serverId", CoopGameComponent.GetServerId());
            packet.Add("m", "AirdropPacket");
            packet.Add("model", AirdropParameters);
            AkiBackendCommunication.Instance.SendDataToPool(packet.SITToJson());

            //packet = new Dictionary<string, object>();
            //packet.Add("serverId", CoopGameComponent.GetServerId());
            //packet.Add("m", "AirdropLootPacket");
            //packet.Add("config", config);
            //packet.Add("result", lootData);
            //AkiBackendCommunication.Instance.SendDataToPool(packet.SITToJson());


            yield break;
        }

        public async void FixedUpdate()
        {
            if (AirdropParameters == null || AirdropParameters.Config == null)
                return;

            // If we are a client. Wait until the server has sent all the data.
            if (MatchmakerAcceptPatches.IsClient && (ClientAirdropLootResultModel == null || ClientAirdropConfigModel == null))
                return;

            // If we have all the parameters sent from the Server. Lets build the plane, box, container and loot
            if (MatchmakerAcceptPatches.IsClient && !ClientLootBuilt)
            {
                ClientLootBuilt = true;
                Logger.LogInfo("Client::Building Plane, Box, Factory and Loot.");

                airdropPlane = await AirdropPlane.Init(
                    AirdropParameters.RandomAirdropPoint,
                    AirdropParameters.DropHeight,
                    AirdropParameters.Config.PlaneVolume,
                    AirdropParameters.Config.PlaneSpeed);
                airdropBox = await AirdropBox.Init(AirdropParameters.Config.CrateFallSpeed);
                factory = new ItemFactoryUtil();

                factory.BuildContainer(airdropBox.container, ClientAirdropConfigModel, ClientAirdropLootResultModel.DropType);
                factory.AddLoot(airdropBox.container, ClientAirdropLootResultModel);
            }

            if (!ClientLootBuilt)
                return;

            if (airdropPlane == null || airdropBox == null || factory == null)
                return;

            if (MatchmakerAcceptPatches.IsServer || MatchmakerAcceptPatches.IsSinglePlayer)
            {
                AirdropParameters.Timer += 0.02f;

                if (AirdropParameters.Timer >= AirdropParameters.TimeToStart && !AirdropParameters.PlaneSpawned)
                {
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
            airdropBox.gameObject.SetActive(true);
            airdropBox.StartCoroutine(airdropBox.DropCrate(dropPos));
        }

        public bool ClientPlaneSpawned { get; private set; }
        public AirdropLootResultModel ClientAirdropLootResultModel { get; private set; }
        public AirdropConfigModel ClientAirdropConfigModel { get; private set; }
        public bool ClientLootBuilt { get; private set; }

        private void BuildLootContainer(AirdropConfigModel config)
        {
            if (MatchmakerAcceptPatches.IsClient)
                return;

            var lootData = factory.GetLoot();

            // Get the lootData. Sent to Clients.
            if (MatchmakerAcceptPatches.IsServer)
            {
                var packet = new Dictionary<string, object>();
                packet.Add("serverId", CoopGameComponent.GetServerId());
                packet.Add("m", "AirdropLootPacket");
                packet.Add("config", config);
                packet.Add("result", lootData);
                AkiBackendCommunication.Instance.SendDataToPool(packet.SITToJson());
            }

            if (lootData == null)
                throw new System.Exception("Airdrops. Tried to BuildLootContainer without any Loot.");
            
            factory.BuildContainer(airdropBox.container, config, lootData.DropType);
            factory.AddLoot(airdropBox.container, lootData);
            ClientLootBuilt = true;
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