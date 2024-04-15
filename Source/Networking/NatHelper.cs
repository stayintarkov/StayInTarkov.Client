using BepInEx.Logging;
using EFT.UI;
using LiteNetLib.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Open.Nat;
using StayInTarkov.Configuration;
using StayInTarkov.Coop.Matchmaker;
using STUN;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.Networking;
using WebSocketSharp;

namespace StayInTarkov.Networking
{

    public enum NatTraversalMethod
    {
        Upnp,
        NatPunch,
        PortForward
    }

    public class NatHelper
    {
        public LiteNetLib.NetManager NetManager;
        public WebSocket WebSocket { get; set; }
        public Dictionary<string, TaskCompletionSource<object>> RequestCompletionSourceList = new Dictionary<string, TaskCompletionSource<object>>();
        public Dictionary<string, IPEndPoint> PublicEndPoints = new Dictionary<string, IPEndPoint>();

        private static readonly ManualLogSource Logger = BepInEx.Logging.Logger.CreateLogSource("NAT Helper");
        private readonly string _sessionId;

        public NatHelper(LiteNetLib.NetManager netManager, string sid)
        {
            NetManager = netManager;
            _sessionId = sid;
        }

        public void Connect()
        {
            var wsUrl = $"{StayInTarkovHelperConstants.GetREALWSURL()}:{PluginConfigSettings.Instance.CoopSettings.SITNatHelperPort}/{_sessionId}?";
            var msg = $"Connecting to NAT Helper at {wsUrl}...";

            WebSocket = new(wsUrl)
            {
                WaitTime = TimeSpan.FromMinutes(1),
                EmitOnPing = true
            };

            WebSocket.OnOpen += WebSocket_OnOpen;
            WebSocket.OnError += WebSocket_OnError;
            WebSocket.OnMessage += WebSocket_OnMessage;
            WebSocket.Connect();
        }

        public void Close()
        {
            WebSocket.Close();
        }

        private void WebSocket_OnMessage(object sender, WebSocketSharp.MessageEventArgs e)
        {
            if (e == null)
                return;

            if (string.IsNullOrEmpty(e.Data))
                return;

            ProcessMessage(e.Data);
        }

        private void WebSocket_OnOpen(object sender, EventArgs e)
        {
            Logger.LogInfo($"Connected to NAT Helper");
        }

        private void WebSocket_OnError(object sender, WebSocketSharp.ErrorEventArgs e)
        {
            Logger.LogError($"WebSocket error {e}");
            WebSocket.Close();
        }

        private void ProcessMessage(string message)
        {
            Logger.LogDebug($"Received {message}");
            JObject msgObj = JObject.Parse(message);

            if (msgObj.ContainsKey("requestId") && msgObj.ContainsKey("requestType") && msgObj.ContainsKey("profileId"))
            {
                var requestId = msgObj["requestId"].ToString();
                var requestType = msgObj["requestType"].ToString();
                var profileId = msgObj["profileId"].ToString();

                if (requestType == "getEndPointsRequest")
                {
                    var getEndPointsResponse = new Dictionary<string, object>
                    {
                        { "serverId", _sessionId },
                        { "requestId", requestId },
                        { "requestType", "getEndPointsResponse" },
                        { "profileId", profileId },
                        { "publicEndPoints", PublicEndPoints.ToDictionary(x => x.Key, x => x.Value.ToString()) }
                    };

                    WebSocket.SendAsync(JsonConvert.SerializeObject(getEndPointsResponse), (success) =>
                    {
                        if (!success)
                        {
                            Logger.LogError("Could not send getEndpoints response");
                        }
                    });
                }

                if (requestType == "natPunchRequest")
                {
                    var publicEndPoints = msgObj["publicEndPoints"]
                        .ToObject<Dictionary<string, string>>()
                        .ToDictionary(x => x.Key, x => x.Value.ToIPEndPoint());

                    if (publicEndPoints.ContainsKey("stun"))
                    {
                        PunchNat(publicEndPoints["stun"]);
                    } else
                    {
                        Logger.LogWarning("Could not find stun endpoint in NAT punch request");
                    }

                    var natPunchResponse = new Dictionary<string, object>
                    {
                        { "requestId", requestId },
                        { "requestType", "natPunchResponse" },
                        { "profileId", profileId },
                    };

                    WebSocket.SendAsync(JsonConvert.SerializeObject(natPunchResponse), (success) =>
                    {
                        if (!success)
                        {
                            Logger.LogError("Could not send NAT punch response");
                        }
                    });
                }

                if (requestType == "getEndPointsResponse")
                {
                    var publicEndPoints = msgObj["publicEndPoints"]
                        .ToObject<Dictionary<string, string>>()
                        .ToDictionary(x => x.Key, x => x.Value.ToIPEndPoint());

                    if (RequestCompletionSourceList.ContainsKey(requestId))
                    {
                        RequestCompletionSourceList[requestId].SetResult(publicEndPoints);
                    } else
                    {
                        Logger.LogWarning("Could not find request corresponding to NAT punch response");
                    }
                }

                if (requestType == "natPunchResponse")
                {
                    if (RequestCompletionSourceList.ContainsKey(requestId))
                    {
                        RequestCompletionSourceList[requestId].SetResult(true);
                    } else
                    {
                        Logger.LogWarning("Could not find request corresponding to NAT punch response");
                    }
                }
            }
        }

        public bool IsConnected()
        {
            return WebSocket.ReadyState == WebSocketState.Open;
        }

        public async Task<Dictionary<string, IPEndPoint>> GetEndpointsRequestAsync(string serverId, string profileId)
        {
            var requestId = Guid.NewGuid().ToString();

            RequestCompletionSourceList.Add(requestId, new TaskCompletionSource<object>());

            var getServerEndPointsRequest = new Dictionary<string, object>
            {
                { "requestId", requestId },
                { "requestType", "getEndPointsRequest" },
                { "serverId", serverId },
                { "profileId", profileId },
            };

            WebSocket.SendAsync(JsonConvert.SerializeObject(getServerEndPointsRequest), (success) =>
            {
                if (!success)
                {
                    Logger.LogError("Could not send getEndpoints request");
                }
            });

            var publicEndPoints = (Dictionary<string, IPEndPoint>)await RequestCompletionSourceList[requestId].Task;

            return publicEndPoints;
        }

        public async Task<bool> NatPunchRequestAsync(string serverId, string profileId, IPEndPoint remoteEndPoint)
        {
            var requestId = Guid.NewGuid().ToString();

            RequestCompletionSourceList.Add(requestId, new TaskCompletionSource<object>());

            var natPunchRequest = new Dictionary<string, object>
            {
                { "requestId", requestId },
                { "requestType", "natPunchRequest" },
                { "serverId", serverId },
                { "profileId", profileId },
                { "publicEndPoints", PublicEndPoints.ToDictionary(x => x.Key, x => x.Value.ToString()) }
            };

            WebSocket.SendAsync(JsonConvert.SerializeObject(natPunchRequest), (success) =>
            {
                if (!success)
                {
                    Logger.LogError("Could not send NAT punch request");
                }
            });

            await RequestCompletionSourceList[requestId].Task;

            //PunchNat(remoteEndPoint);

            return true;
        }

        public void PunchNat(IPEndPoint endPoint)
        {
            // bogus punch data
            var resp = new NetDataWriter();
            resp.Put(9999);

            Logger.LogMessage($"Punching {endPoint} from local port {NetManager.LocalPort}");

            // send a couple of packets to punch a hole
            for (int i = 0; i < 10; i++)
            {
                NetManager.SendUnconnectedMessage(resp, endPoint);
            }
        }

        public async Task<bool> AddUpnpEndPoint(int localPort, int publicPort, int lifetime, string desc)
        {
            if (publicPort == 0)
            {
                return false;
            }

            try
            {
                var discoverer = new NatDiscoverer();
                var cts = new CancellationTokenSource(5000);
                var device = await discoverer.DiscoverDeviceAsync(PortMapper.Upnp, cts);
                var extIp = await device.GetExternalIPAsync();
                var externalIp = extIp.MapToIPv4();
                await device.CreatePortMapAsync(new Mapping(Protocol.Udp, localPort, publicPort, lifetime, desc));

                PublicEndPoints.Add("upnp", new IPEndPoint(externalIp, publicPort));

                return true;
            }
            catch (Exception ex)
            {
                var warnmsg = $"Warning: UPNP mapping failed: {ex.Message}";
                Logger.LogWarning(warnmsg);
            }

            return false;
        }

        public bool AddStunEndPoint(ref int localPort)
        {
            bool success = false;
            var stunUdpClient = new UdpClient();
            try
            {
                var queryResult = new STUNQueryResult();

                if (localPort > 0)
                    stunUdpClient.Client.Bind(new IPEndPoint(IPAddress.Any, localPort));

                IPAddress stunIp = Array.Find(Dns.GetHostEntry("stun.l.google.com").AddressList, a => a.AddressFamily == AddressFamily.InterNetwork);
                int stunPort = 19302;

                var stunEndPoint = new IPEndPoint(stunIp, stunPort);

                queryResult = STUNClient.Query(stunUdpClient.Client, stunEndPoint, STUNQueryType.ExactNAT, NATTypeDetectionRFC.Rfc3489);

                success = queryResult.PublicEndPoint != null;
                if (success)
                {
                    Logger.LogInfo($"Found NAT type {queryResult.NATType}, local endpoint {queryResult.LocalEndPoint} and public endpoint {queryResult.PublicEndPoint}");
                    PublicEndPoints.Add("stun", queryResult.PublicEndPoint);
                    localPort = ((IPEndPoint)stunUdpClient.Client.LocalEndPoint).Port;
                }
                else
                {
                    Logger.LogWarning($"Warning: STUN query failed.");
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"STUN Error: {ex.Message}");
            }
            finally
            {
                stunUdpClient.Client.Close();
            }

            return success;
        }

        private async Task<IPAddress?> GetExternalIPAddressByWebCall(string address)
        {
            try
            {
                using (HttpClient client = new())
                {
                    client.Timeout = new TimeSpan(0, 0, 0, 1);
                    string result = await client.GetStringAsync(address);
                    return IPAddress.Parse(result);
                }
            }
            catch { }

            return null;
        }

        public void AddEndPoint(string name, string ip, int port)
        {
            if (IPAddress.TryParse(ip, out var ipAddress))
            {
                PublicEndPoints.Add(name, new IPEndPoint(ipAddress, port));
            } else
            {
                Logger.LogWarning($"Could not parse IP from {ip}");
            }
        }
    }

    public static class ExtensionMethods
    {
        public static IPEndPoint ToIPEndPoint(this string ipEndPoint)
        {
            var ipEndPointArr = ipEndPoint.Split(':');
            var ip = ipEndPointArr[0];
            var port = int.Parse(ipEndPointArr[1]);

            return new IPEndPoint(IPAddress.Parse(ip), port);
        }
    }
}
