//#pragma warning disable CS0618 // Type or member is obsolete
//using BepInEx.Logging;
//using EFT;
//using EFT.InventoryLogic;
//using StayInTarkov.Coop;
//using StayInTarkov.Coop.Components;
//using StayInTarkov.Coop.Components.CoopGameComponents;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Reflection;
//using System.Security.Cryptography;
//using System.Text;
//using UnityEngine;

//namespace StayInTarkov.Core.Player
//{
//    /// <summary>
//    /// Player Replicated Component is the Player/AI direct communication to the Server
//    /// </summary>
//    internal class PlayerReplicatedComponent : MonoBehaviour
//    {
//        internal EFT.LocalPlayer player { get; set; }
//        public bool IsClientDrone { get; internal set; }
//        private HashSet<IPlayerPacketHandler> PacketHandlerComponents { get; } = new();

//        void Awake()
//        {
//            //PatchConstants.Logger.LogDebug("PlayerReplicatedComponent:Awake");
//            // ----------------------------------------------------
//            // Create a BepInEx Logger for CoopGameComponent
//            Logger = BepInEx.Logging.Logger.CreateLogSource(nameof(PlayerReplicatedComponent));
//            Logger.LogDebug($"{nameof(PlayerReplicatedComponent)}:Awake");
//        }

//        void Start()
//        {
//            //PatchConstants.Logger.LogDebug($"PlayerReplicatedComponent:Start");

//            if (player == null)
//            {
//                player = this.GetComponentInParent<EFT.LocalPlayer>();
//                StayInTarkovHelperConstants.Logger.LogDebug($"PlayerReplicatedComponent:Start:Set Player to {player}");
//            }

//            if (player.Side != EPlayerSide.Savage && ReflectionHelpers.GetDogtagItem(player) == null)
//            {
//                if (!SITGameComponent.TryGetCoopGameComponent(out SITGameComponent coopGameComponent))
//                    return;

//                Slot dogtagSlot = player.Inventory.Equipment.GetSlot(EquipmentSlot.Dogtag);
//                if (dogtagSlot == null)
//                    return;

//                string itemId = "";
//                using (SHA256 sha256 = SHA256.Create())
//                {
//                    StringBuilder sb = new();

//                    byte[] hashes = sha256.ComputeHash(Encoding.UTF8.GetBytes(coopGameComponent.ServerId + player.ProfileId + coopGameComponent.Timestamp));
//                    for (int i = 0; i < hashes.Length; i++)
//                        sb.Append(hashes[i].ToString("x2"));

//                    itemId = sb.ToString().Substring(0, 24);
//                }

//                Item dogtag = Spawners.ItemFactory.CreateItem(itemId, player.Side == EPlayerSide.Bear ? DogtagComponent.BearDogtagsTemplate : DogtagComponent.UsecDogtagsTemplate);
//                if (dogtag != null)
//                    dogtagSlot.AddWithoutRestrictions(dogtag);
//            }

//            //GCHelpers.EnableGC();

//            // TODO: Add PacketHandlerComponents here. Possibly via Reflection?
//            //PacketHandlerComponents.Add(new MoveOperationPlayerPacketHandler());
//            var packetHandlers = Assembly.GetAssembly(typeof(IPlayerPacketHandler))
//               .GetTypes()
//               .Where(x => x.GetInterface(nameof(IPlayerPacketHandler)) != null);
//            foreach (var handler in packetHandlers)
//            {
//                if (handler.IsAbstract
//                    || handler == typeof(IPlayerPacketHandler)
//                    || handler.Name == nameof(IPlayerPacketHandler)
//                    )
//                    continue;

//                if (PacketHandlerComponents.Any(x => x.GetType().Name == handler.Name))
//                    continue;

//                PacketHandlerComponents.Add((IPlayerPacketHandler)Activator.CreateInstance(handler));
//                Logger.LogDebug($"Added {handler.Name} to {nameof(PacketHandlerComponents)}");
//            }
//        }

//        public void ProcessPacket(Dictionary<string, object> packet)
//        {
//            if (!packet.ContainsKey("m"))
//                return;

//            var method = packet["m"].ToString();

//            //ProcessPlayerState(packet);

//            // Iterate through the PacketHandlerComponents
//            foreach (var packetHandlerComponent in PacketHandlerComponents)
//            {
//                packetHandlerComponent.ProcessPacket(packet);
//            }

//            if (!ModuleReplicationPatch.Patches.ContainsKey(method))
//                return;

//            var patch = ModuleReplicationPatch.Patches[method];
//            if (patch != null)
//            {
//                patch.Replicated(player, packet);
//                return;
//            }


//        }

//        public ManualLogSource Logger { get; private set; }

//    }
//}
