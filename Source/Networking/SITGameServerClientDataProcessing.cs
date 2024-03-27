using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using StayInTarkov.Coop.Components.CoopGameComponents;
using StayInTarkov.Coop.Matchmaker;
using StayInTarkov.Coop.NetworkPacket;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BepInEx.Logging;
using Comfort.Common;
using Mono.Cecil;
using StayInTarkov.Coop.SITGameModes;
using StayInTarkov.Coop.NetworkPacket.Player;

namespace StayInTarkov.Networking
{
    public static class SITGameServerClientDataProcessing
    {
        public static bool DEBUGPACKETS = false;

        public const string PACKET_TAG_METHOD = "m";
        public const string PACKET_TAG_SERVERID = "serverId";
        public const string PACKET_TAG_DATA = "data";


        public static ManualLogSource Logger { get; }

        static SITGameServerClientDataProcessing()
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource($"{nameof(SITGameServerClientDataProcessing)}");
        }

        public static void ProcessPacketBytes(byte[] data, string sData)
        {
            try
            {
                if (data == null)
                {
                    Logger.LogError($"{nameof(ProcessPacketBytes)}. Data is Null");
                    return;
                }

                if (data.Length == 0)
                {
                    Logger.LogError($"{nameof(ProcessPacketBytes)}. Data is Empty");
                    return;
                }

                Dictionary<string, object> packet = null;
                ISITPacket sitPacket = null;

                // Is a dictionary from Spt-Aki
                if (!string.IsNullOrEmpty(sData) && sData.StartsWith("{"))
                {
                    // Use StreamReader & JsonTextReader to improve memory / cpu usage
                    using (var streamReader = new StreamReader(new MemoryStream(data)))
                    {
                        using (var reader = new JsonTextReader(streamReader))
                        {
                            var serializer = new JsonSerializer();
                            packet = serializer.Deserialize<Dictionary<string, object>>(reader);
                        }
                    }
                }
                // Is a RAW SIT Serialized packet
                else
                {

                    //Logger.LogDebug(Encoding.UTF8.GetString(data));
                    //BasePlayerPacket basePlayerPacket = new BasePlayerPacket();
                    //packet = basePlayerPacket.ToDictionary(data);
                    ProcessSITPacket(data, ref packet, out sitPacket);

                }

                if (DEBUGPACKETS)
                {
                    Logger.LogInfo("GOT :" + sData);
                }

                var coopGameComponent = SITGameComponent.GetCoopGameComponent();

                if (coopGameComponent == null)
                {
                    Logger.LogError($"{nameof(ProcessPacketBytes)}. coopGameComponent is Null");
                    return;
                }

                if (packet == null)
                {
                    //Logger.LogError($"{nameof(ProcessPacketBytes)}. Packet is Null");
                    return;
                }

                if (DEBUGPACKETS)
                {
                    Logger.LogInfo("GOT :" + packet.SITToJson());
                }

                if (packet.ContainsKey("dataList"))
                {
                    if (ProcessDataListPacket(ref packet))
                        return;
                }

                //Logger.LogDebug($"Step.1. Packet exists. {packet.ToJson()}");

                // If this is a pong packet, resolve and create a smooth ping
                if (packet.ContainsKey("pong"))
                {
                    var pongRaw = long.Parse(packet["pong"].ToString());
                    var dtPong = new DateTime(pongRaw);
                    var serverPing = (int)(DateTime.UtcNow - dtPong).TotalMilliseconds;
                    coopGameComponent.UpdatePing(serverPing);
                    return;
                }

                if (packet.ContainsKey("HostPing"))
                {
                    var dtHP = new DateTime(long.Parse(packet["HostPing"].ToString()));
                    var timeSpanOfHostToMe = DateTime.UtcNow - dtHP;
                    //HostPing = (int)Math.Round(timeSpanOfHostToMe.TotalMilliseconds);
                    return;
                }

                // Receiving a Player Extracted packet. Process into ExtractedPlayers List
                if (packet.ContainsKey("Extracted"))
                {
                    if (Singleton<ISITGame>.Instantiated && !Singleton<ISITGame>.Instance.ExtractedPlayers.Contains(packet["profileId"].ToString()))
                    {
                        Singleton<ISITGame>.Instance.ExtractedPlayers.Add(packet["profileId"].ToString());
                    }
                    return;
                }

                // If this is an endSession packet, end the session for the clients
                if (packet.ContainsKey("endSession") && SITMatchmaking.IsClient)
                {
                    Logger.LogDebug("Received EndSession from Server. Ending Game.");
                    if (coopGameComponent.LocalGameInstance == null)
                        return;

                    coopGameComponent.ServerHasStopped = true;
                    return;
                }



                // -------------------------------------------------------
                // Add to the Coop Game Component Action Packets
                if (coopGameComponent == null || coopGameComponent.ActionPackets == null || coopGameComponent.ActionPacketHandler == null)
                    return;


                //if (packet.ContainsKey(PACKET_TAG_METHOD)
                //    && packet[PACKET_TAG_METHOD].ToString() == "Move")
                //    coopGameComponent.ActionPacketHandler.ActionPacketsMovement.TryAdd(packet);
                //else if (packet.ContainsKey(PACKET_TAG_METHOD)
                //    && packet[PACKET_TAG_METHOD].ToString() == "ApplyDamageInfo")
                //{
                //    coopGameComponent.ActionPacketHandler.ActionPacketsDamage.TryAdd(packet);
                //}
                //else
                //{
                if (sitPacket != null)
                    coopGameComponent.ActionPacketHandler.ActionSITPackets.Add(sitPacket);
                else
                {
#if DEBUG
                    Logger.LogDebug($">> DEV TODO <<");
                    Logger.LogDebug($">> Convert the following packet to binary <<");
                    Logger.LogDebug($"{packet.ToJson()}");
#endif 
                    coopGameComponent.ActionPacketHandler.ActionPackets.TryAdd(packet);
                }
                //}

            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
            }
        }

        public static void ProcessSITPacket(byte[] data, ref Dictionary<string, object> dictObject, out ISITPacket packet)
        {
            packet = null;

            var coopGameComponent = SITGameComponent.GetCoopGameComponent();
            if (coopGameComponent == null)
            {
                Logger.LogError($"{nameof(ProcessSITPacket)}. coopGameComponent is Null");
                return;
            }

            // If the data is empty. Return;
            if (data == null || data.Length == 0)
            {
                Logger.LogError($"{nameof(ProcessSITPacket)}. {nameof(data)} is null");
            }

            var stringData = Encoding.UTF8.GetString(data);
            // If the string Data isn't a SIT serialized string. Return;
            if (!stringData.StartsWith("SIT"))
            {
                //Logger.LogError($"{nameof(ProcessSITPacket)}. {stringData} does not start with SIT");
                return;
            }

            var serverId = stringData.Substring(3, 24);
            // If the serverId is not the same as the one we are connected to. Return;
            if (serverId != coopGameComponent.ServerId)
            {
                Logger.LogError($"{nameof(ProcessSITPacket)}. {serverId} does not equal {coopGameComponent.ServerId}");
                return;
            }

            var bp = new BasePacket("");
            using (var br = new BinaryReader(new MemoryStream(data)))
                bp.ReadHeader(br);

            dictObject = new Dictionary<string, object>();
            dictObject[PACKET_TAG_DATA] = data;
            dictObject[PACKET_TAG_METHOD] = bp.Method;

            if (!dictObject.ContainsKey("profileId"))
            {
                try
                {
                    var bpp = new BasePlayerPacket("", dictObject[PACKET_TAG_METHOD].ToString());
                    bpp.Deserialize(data);
                    dictObject.Add("profileId", new string(bpp.ProfileId.ToCharArray()));
                    bpp.Dispose();
                    bpp = null;
                }
                catch { }
            }

            if (DEBUGPACKETS)
            {
                Logger.LogInfo(" ==================SIT Packet============= ");
                Logger.LogInfo(dictObject.ToJson());
            }

            packet = DeserializeIntoPacket(data, packet, bp);
        }

        private static ISITPacket DeserializeIntoPacket(byte[] data, ISITPacket packet, BasePacket bp)
        {
            var sitPacketType =
                            StayInTarkovHelperConstants
                            .SITTypes
                            .Union(ReflectionHelpers.EftTypes)
                            .FirstOrDefault(x => x.Name == bp.Method);
            if (sitPacketType != null)
            {
                //Logger.LogInfo($"{sitPacketType} found");
                packet = (ISITPacket)Activator.CreateInstance(sitPacketType);
                packet = packet.Deserialize(data);
            }
            else
            {
#if DEBUG
                Logger.LogDebug($"{nameof(DeserializeIntoPacket)}:{bp.Method} could not find a matching ISITPacket type");
#endif
            }

            return packet;
        }

        public static bool ProcessDataListPacket(ref Dictionary<string, object> packet)
        {
            var coopGC = SITGameComponent.GetCoopGameComponent();
            if (coopGC == null)
                return false;

            if (!packet.ContainsKey("dataList"))
                return false;

            JArray dataList = JArray.FromObject(packet["dataList"]);

            //Logger.LogDebug(packet.ToJson());   

            foreach (var d in dataList)
            {
                // TODO: This needs to be a little more dynamic but for now. This switch will do.
                // Depending on the method defined, deserialize packet to defined type
                switch (packet[PACKET_TAG_METHOD].ToString())
                {
                    case "PlayerStates":
                        PlayerStatePacket playerStatePacket = new PlayerStatePacket();
                        playerStatePacket = (PlayerStatePacket)playerStatePacket.Deserialize((byte[])d);
                        if (playerStatePacket == null || string.IsNullOrEmpty(playerStatePacket.ProfileId))
                            continue;

                        if (coopGC.Players.ContainsKey(playerStatePacket.ProfileId))
                            coopGC.Players[playerStatePacket.ProfileId].ReceivePlayerStatePacket(playerStatePacket);


                        var serverPing = (int)(DateTime.UtcNow - new DateTime(long.Parse(packet["t"].ToString()))).TotalMilliseconds;
                        coopGC.ServerPingSmooth.Enqueue(serverPing);

                        break;
                    case "Multiple":
                        break;
                }

            }

            return true;
        }
    }
}
