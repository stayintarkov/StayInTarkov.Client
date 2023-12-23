using BepInEx.Logging;
using Comfort.Common;
using EFT;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StayInTarkov.Configuration;
using StayInTarkov.Coop.Matchmaker;
using StayInTarkov.Coop.NetworkPacket;
using StayInTarkov.Networking;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace StayInTarkov.Coop.Networking
{
    public class SITWebSocket
    {
        public const string PACKET_TAG_METHOD = "m";
        public const string PACKET_TAG_SERVERID = "serverId";
        public const string PACKET_TAG_DATA = "data";

        private const string SITDEBUGFILEPATH = "SITWebSocketDEBUG.bin";


        public ClientWebSocket ClientWebSocket { get; set; }
        public Profile MyProfile { get; private set; }
        public ManualLogSource Logger { get; private set; }
        private AkiBackendCommunication BackendCommunication { get; set; }

        public SITWebSocket(in AkiBackendCommunication backendCommunication) 
        {
            BackendCommunication = backendCommunication;
            Logger = BepInEx.Logging.Logger.CreateLogSource("SITWebSocket");

            if (File.Exists(SITDEBUGFILEPATH))
                File.Delete(SITDEBUGFILEPATH);
        }

        public void WebSocketCreate(Profile profile)
        {
            MyProfile = profile;

            Logger.LogDebug("WebSocketCreate");
            Logger.LogDebug("Request Instance is connecting to WebSocket");

            var webSocketPort = PluginConfigSettings.Instance.CoopSettings.SITWebSocketPort;
            var wsUrl = $"{StayInTarkovHelperConstants.GetREALWSURL()}:{webSocketPort}/{profile.ProfileId}?";
            Logger.LogDebug(webSocketPort);
            Logger.LogDebug(StayInTarkovHelperConstants.GetREALWSURL());
            Logger.LogDebug(wsUrl);

            ClientWebSocket = new ClientWebSocket();
            ClientWebSocket.Options.KeepAliveInterval = TimeSpan.FromSeconds(1);
            if (Uri.TryCreate(wsUrl, UriKind.Absolute, out var uri))
            {
                ClientWebSocket.ConnectAsync(uri, CancellationToken.None);
                Task.Run(async() => { 
                
                    while(true)
                    {
                        Send("");
                        await Task.Delay(1000);
                    }
                
                });
                Send("CONNECTED FROM SIT COOP");
                Receive();
            }
        }

       

        public void WebSocketClose()
        {
            ClientWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
            //if (WebSocket != null)
            //{
            //    Logger.LogDebug("WebSocketClose");
            //    WebSocket.OnError -= WebSocket_OnError;
            //    WebSocket.OnMessage -= WebSocket_OnMessage;
            //    WebSocket.Close(WebSocketSharp.CloseStatusCode.Normal);
            //    WebSocket = null;
            //}
        }

        private List<byte[]> _ConcatReceivedData = new List<byte[]>();

        public void Receive()
        {
            Logger.LogInfo("Receive:Start");

            Task.Run(async() => {

                var buffer = new byte[20480];
                while (true)
                {
                    //Logger.LogDebug($"Awaiting Receive [{ClientWebSocket.State}]");
                    if (ClientWebSocket.State == WebSocketState.Connecting)
                        continue;

                    if (ClientWebSocket.State == WebSocketState.Closed)
                        return;

                        //try
                        //{
                        //    Logger.LogInfo("Awaiting Receive...");

                    var asb = new ArraySegment<byte>(buffer);
                    var result = await ClientWebSocket.ReceiveAsync(asb, CancellationToken.None);
                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        Logger.LogError("Received Close");
                        await ClientWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, null, CancellationToken.None);
                    }
                    else
                    {
                        if (result.EndOfMessage)
                        {
                            //Logger.LogDebug(Encoding.UTF8.GetString(buffer, 0, result.Count));
                            if (_ConcatReceivedData.Count > 0)
                            {
                                byte[] finalBuffer = new byte[_ConcatReceivedData.Sum(x => x.Length)];
                                MemoryStream ms = new MemoryStream();
                                using (var bw = new BinaryWriter(ms, Encoding.UTF8, true))
                                {
                                    foreach (var item in _ConcatReceivedData)
                                        bw.Write(item);
                                }
                                _ConcatReceivedData.Clear();
                                var combinedBytes = ms.ToArray();
                                ms.Close();
                                ms.Dispose();
                                ms = null;
                                finalBuffer = null;
                                ProcessPacketBytes(combinedBytes);
                                _ConcatReceivedData.Clear();
                            }
                            else
                            {
                                ProcessPacketBytes(buffer);
                            }
                        }
                        else
                        {
                            Logger.LogError("Unhandled LARGE message received. TODO: Create message handler");
                            //_ConcatReceivedData.Add(buffer);
                        }
                    }
                  
                    //await Task.Delay(1);
                }


            });
        }

        private bool _Sending = false;

        private object _SendingLock = new object();

        public async void Send(string message)
        {
            if (ClientWebSocket == null)
            {
                Logger.LogError($"{nameof(ClientWebSocket)} is null");
                return;
            }

            // If i don't put this loginfo here. The WebSocket aborts / closes? Wtf
            //Logger.LogDebug($"Sending {message}");
            lock (_SendingLock)
            {
                File.WriteAllText(SITDEBUGFILEPATH, $"Sending {message}");
            }

            await Task.Delay(5);
            while (_Sending)
            {
                lock (_SendingLock)
                {
                    //Logger.LogDebug($"Send Waiting with {message}");
                    File.WriteAllText(SITDEBUGFILEPATH, $"Sending {message}");
                }
                await Task.Delay(10);
            }

            try
            {
                _Sending = true;
                await ClientWebSocket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(message)), WebSocketMessageType.Text, true, CancellationToken.None);
                _Sending = false;
            }
            catch
            {

            }
        }

        public void Send(byte[] message)
        {
            if (ClientWebSocket == null)
            {
                Logger.LogError($"{nameof(ClientWebSocket)} is null");
                return;
            }

            Send(Encoding.UTF8.GetString(message));
        }

        private void ProcessPacketBytes(byte[] data)
        {
            try
            {
                if (data == null)
                    return;

                if (data.Length == 0)
                    return;

                Dictionary<string, object> packet = null;

                var s = Encoding.UTF8.GetString(data);
                if (s.StartsWith("{") || s.StartsWith("["))
                {
                    var streamReader = new StreamReader(new MemoryStream(data));
                    var reader = new JsonTextReader(streamReader);
                    var serializer = new JsonSerializer();
                    packet = serializer.Deserialize<Dictionary<string, object>>(reader);
                    serializer = null;
                    reader.Close();
                    reader = null;
                    streamReader.Close();
                    streamReader.Dispose();
                    streamReader = null;
                }
                s.Clear();
                s = null;

                var coopGameComponent = CoopGameComponent.GetCoopGameComponent();

                if (coopGameComponent == null)
                    return;

                if (packet == null)
                    return;

                //if (DEBUGPACKETS)
                //{
                //    Logger.LogInfo(packet.SITToJson());
                //}

                if (packet.ContainsKey("dataList"))
                {
                    if (ProcessDataListPacket(ref packet))
                        return;
                }

                ////Logger.LogDebug($"Step.1. Packet exists. {packet.ToJson()}");

                // If this is a pong packet, resolve and create a smooth ping
                if (ProcessPong(ref packet, ref coopGameComponent))
                    return;

                if (packet.ContainsKey("HostPing"))
                {
                    var dtHP = new DateTime(long.Parse(packet["HostPing"].ToString()));
                    var timeSpanOfHostToMe = DateTime.UtcNow - dtHP;
                    //HostPing = (int)Math.Round(timeSpanOfHostToMe.TotalMilliseconds);
                    BackendCommunication.HostPing = (int)Math.Round(timeSpanOfHostToMe.TotalMilliseconds);
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
                if (packet.ContainsKey("endSession") && MatchmakerAcceptPatches.IsClient)
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

                ProcessSITPacket(ref packet);

                if (packet.ContainsKey(PACKET_TAG_METHOD)
                    && packet[PACKET_TAG_METHOD].ToString() == "Move")
                    coopGameComponent.ActionPacketHandler.ActionPacketsMovement.TryAdd(packet);
                else if (packet.ContainsKey(PACKET_TAG_METHOD)
                    && packet[PACKET_TAG_METHOD].ToString() == "ApplyDamageInfo")
                {
                    coopGameComponent.ActionPacketHandler.ActionPacketsDamage.TryAdd(packet);
                }
                else
                    coopGameComponent.ActionPacketHandler.ActionPackets.TryAdd(packet);

            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
            }
        }

        private void ProcessSITPacket(ref Dictionary<string, object> packet)
        {
            // If this is a SIT serialization packet
            if (packet.ContainsKey(PACKET_TAG_DATA) && packet.ContainsKey(PACKET_TAG_METHOD))
            {
                var data = packet[PACKET_TAG_DATA];
                if (data == null)
                    return;


                if (!packet.ContainsKey("profileId"))
                {
                    //Logger.LogInfo(nameof(ProcessSITPacket));
                    //Logger.LogInfo("No profileId found");
                    var bpp = new BasePlayerPacket();
                    bpp = bpp.DeserializePacketSIT(data.ToString());
                    if (!string.IsNullOrEmpty(bpp.ProfileId))
                        packet.Add("profileId", bpp.ProfileId);

                    bpp = null;
                    //Logger.LogInfo(packet.ToJson());
                }
            }
        }

        private bool ProcessPong(ref Dictionary<string, object> packet, ref CoopGameComponent coopGameComponent)
        {
            if (packet.ContainsKey("pong"))
            {
                var pongRaw = long.Parse(packet["pong"].ToString());
                var dtPong = new DateTime(pongRaw);
                var serverPing = (int)(DateTime.UtcNow - dtPong).TotalMilliseconds;
                if (coopGameComponent.ServerPingSmooth.Count > 60)
                    coopGameComponent.ServerPingSmooth.TryDequeue(out _);
                coopGameComponent.ServerPingSmooth.Enqueue(serverPing);
                coopGameComponent.ServerPing = coopGameComponent.ServerPingSmooth.Count > 0 ? (int)Math.Round(coopGameComponent.ServerPingSmooth.Average()) : 1;
                return true;
            }

            return false;
        }

        private bool ProcessDataListPacket(ref Dictionary<string, object> packet)
        {
            var coopGC = CoopGameComponent.GetCoopGameComponent();
            if (coopGC == null)
                return false;

            if (!packet.ContainsKey("dataList"))
                return false;

            JArray dataList = JArray.FromObject(packet["dataList"]);

            foreach (var d in dataList)
            {
                // This needs to be a little more dynamic but for now. This switch will do.
                // Depending on the method defined, deserialize packet to defined type
                switch (packet[PACKET_TAG_METHOD].ToString())
                {
                    case "PlayerStates":
                        PlayerStatePacket playerStatePacket = new PlayerStatePacket();
                        playerStatePacket = (PlayerStatePacket)playerStatePacket.Deserialize((byte[])d);
                        if (coopGC.Players.ContainsKey(playerStatePacket.ProfileId))
                            coopGC.Players[playerStatePacket.ProfileId].ReceivePlayerStatePacket(playerStatePacket);

                        break;
                }

            }

            return true;
        }

    }
}
