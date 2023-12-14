#pragma warning disable CS0618 // Type or member is obsolete
using BepInEx.Logging;
using EFT;
using EFT.HealthSystem;
using EFT.InventoryLogic;
using StayInTarkov.Coop;
using StayInTarkov.Coop.Components;
using StayInTarkov.Coop.NetworkPacket;
using StayInTarkov.Coop.Player;
using StayInTarkov.Coop.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using static AHealthController<EFT.HealthSystem.ActiveHealthController.AbstractEffect>;

namespace StayInTarkov.Core.Player
{
    /// <summary>
    /// Player Replicated Component is the Player/AI direct communication to the Server
    /// </summary>
    internal class PlayerReplicatedComponent : MonoBehaviour
    {
        internal const int PacketTimeoutInSeconds = 1;
        //internal ConcurrentQueue<Dictionary<string, object>> QueuedPackets { get; } = new();
        internal Dictionary<string, object> LastMovementPacket { get; set; }
        internal EFT.LocalPlayer player { get; set; }
        public bool IsMyPlayer { get { return player != null && player.IsYourPlayer; } }
        public bool IsClientDrone { get; internal set; }

        public float ReplicatedMovementSpeed { get; set; }
        private float PoseLevelSmoothed { get; set; } = 1;

        private HashSet<IPlayerPacketHandlerComponent> PacketHandlerComponents { get; } = new();

        void Awake()
        {
            //PatchConstants.Logger.LogDebug("PlayerReplicatedComponent:Awake");
            // ----------------------------------------------------
            // Create a BepInEx Logger for CoopGameComponent
            Logger = BepInEx.Logging.Logger.CreateLogSource(nameof(PlayerReplicatedComponent));
            Logger.LogDebug($"{nameof(PlayerReplicatedComponent)}:Awake");
        }

        void Start()
        {
            //PatchConstants.Logger.LogDebug($"PlayerReplicatedComponent:Start");

            if (player == null)
            {
                player = this.GetComponentInParent<EFT.LocalPlayer>();
                StayInTarkovHelperConstants.Logger.LogDebug($"PlayerReplicatedComponent:Start:Set Player to {player}");
            }

            if (player.ProfileId.StartsWith("pmc"))
            {
                if (ReflectionHelpers.GetDogtagItem(player) == null)
                {
                    if (!CoopGameComponent.TryGetCoopGameComponent(out CoopGameComponent coopGameComponent))
                        return;

                    Slot dogtagSlot = player.Inventory.Equipment.GetSlot(EquipmentSlot.Dogtag);
                    if (dogtagSlot == null)
                        return;

                    string itemId = "";
                    using (SHA256 sha256 = SHA256.Create())
                    {
                        StringBuilder sb = new();

                        byte[] hashes = sha256.ComputeHash(Encoding.UTF8.GetBytes(coopGameComponent.ServerId + player.ProfileId + coopGameComponent.Timestamp));
                        for (int i = 0; i < hashes.Length; i++)
                            sb.Append(hashes[i].ToString("x2"));

                        itemId = sb.ToString().Substring(0, 24);
                    }

                    Item dogtag = Spawners.ItemFactory.CreateItem(itemId, player.Side == EPlayerSide.Bear ? DogtagComponent.BearDogtagsTemplate : DogtagComponent.UsecDogtagsTemplate);

                    if (dogtag != null)
                    {
                        if (!dogtag.TryGetItemComponent(out DogtagComponent dogtagComponent))
                            return;

                        dogtagComponent.GroupId = player.Profile.Info.GroupId;
                        dogtagSlot.AddWithoutRestrictions(dogtag);
                    }
                }
            }

            //GCHelpers.EnableGC();

            // TODO: Add PacketHandlerComponents here. Possibly via Reflection?
            //PacketHandlerComponents.Add(new MoveOperationPlayerPacketHandler());
            var packetHandlers = Assembly.GetAssembly(typeof(IPlayerPacketHandlerComponent))
               .GetTypes()
               .Where(x => x.GetInterface(nameof(IPlayerPacketHandlerComponent)) != null);
            foreach (var handler in packetHandlers)
            {
                if (handler.IsAbstract
                    || handler == typeof(IPlayerPacketHandlerComponent)
                    || handler.Name == nameof(IPlayerPacketHandlerComponent)
                    )
                    continue;

                if (PacketHandlerComponents.Any(x => x.GetType().Name == handler.Name))
                    continue;

                PacketHandlerComponents.Add((IPlayerPacketHandlerComponent)Activator.CreateInstance(handler));
                Logger.LogDebug($"Added {handler.Name} to {nameof(PacketHandlerComponents)}");
            }
        }

        public void ProcessPacket(Dictionary<string, object> packet)
        {
            if (!packet.ContainsKey("m"))
                return;

            var method = packet["m"].ToString();

            //ProcessPlayerState(packet);

            // Iterate through the PacketHandlerComponents
            foreach (var packetHandlerComponent in PacketHandlerComponents)
            {
                packetHandlerComponent.ProcessPacket(packet);
            }

            if (!ModuleReplicationPatch.Patches.ContainsKey(method))
                return;

            var patch = ModuleReplicationPatch.Patches[method];
            if (patch != null)
            {
                patch.Replicated(player, packet);
                return;
            }


        }

       


        

        private void ShouldTeleport(Vector3 desiredPosition)
        {
            var direction = (player.Position - desiredPosition).normalized;
            Ray ray = new(player.Position, direction);
            LayerMask layerMask = LayerMaskClass.HighPolyWithTerrainNoGrassMask;
        }

        void Update()
        {
            Update_ClientDrone();

            

            if (IsClientDrone)
                return;

            if (player.ActiveHealthController.IsAlive)
            {
                var bodyPartHealth = player.ActiveHealthController.GetBodyPartHealth(EBodyPart.Common);
                if (bodyPartHealth.AtMinimum)
                {
                    var packet = new Dictionary<string, object>();
                    packet.Add("dmt", EDamageType.Undefined.ToString());
                    packet.Add("m", "Kill");
                    AkiBackendCommunicationCoop.PostLocalPlayerData(player, packet);
                }
            }
        }

        void LateUpdate()
        {
            if (!IsClientDrone)
                return;

            // This must exist in Update AND LateUpdate to function correctly.
            //player.MovementContext.EnableSprint(ShouldSprint);
            player.MovementContext.PlayerAnimator.EnableSprint(ShouldSprint);
            if (ShouldSprint)
            {
                player.Rotation = ReplicatedRotation.Value;
                player.MovementContext.Rotation = ReplicatedRotation.Value;
                player.MovementContext.PlayerAnimator.SetMovementDirection(ReplicatedDirection.HasValue ? ReplicatedDirection.Value : player.InputDirection);
            }

        }

        private void Update_ClientDrone()
        {
            if (!IsClientDrone)
                return;

            // Replicate Rotation.
            // Smooth Lerp to the Desired Rotation
            if (ReplicatedRotation.HasValue && !IsSprinting && !ShouldSprint)
            {
                player.Rotation = Vector3.Lerp(player.Rotation, ReplicatedRotation.Value, Time.deltaTime * 2);
            }

            //if (ReplicatedDirection.HasValue)
            //{
            //    if (_playerMovePatch == null)
            //        _playerMovePatch = (Player_Move_Patch)ModuleReplicationPatch.Patches["Move"];

            //    _playerMovePatch?.ReplicatedMove(player,
            //        new ReceivedPlayerMoveStruct(0, 0, 0, ReplicatedDirection.Value.x, ReplicatedDirection.Value.y, ReplicatedMovementSpeed));
            //}

            player.MovementContext.PlayerAnimator.EnableSprint(ShouldSprint);
            if (!ShouldSprint)
            {
                PoseLevelSmoothed = Mathf.Lerp(PoseLevelSmoothed, PoseLevelDesired, Time.deltaTime);
                player.MovementContext.SetPoseLevel(PoseLevelSmoothed, true);
            }
            else
            {
                // This must exist in Update AND LateUpdate to function correctly.
                player.Rotation = ReplicatedRotation.Value;
                if (ReplicatedDirection.HasValue)
                    player.MovementContext.PlayerAnimatorSetMovementDirection(ReplicatedDirection.Value);
            }

            if (ReplicatedHeadRotation.HasValue)
            {
                player.HeadRotation = Vector3.Lerp(player.HeadRotation, ReplicatedHeadRotation.Value, Time.deltaTime * 20);
            }

            if (ReplicatedTilt.HasValue)
            {
                player.MovementContext.SetTilt(Mathf.Lerp(player.MovementContext.Tilt, ReplicatedTilt.Value, Time.deltaTime * 10), true);
            }

            // Process Prone
            if (ReplicatedPlayerStatePacket != null)
            {
                bool prone = ReplicatedPlayerStatePacket.IsProne;
                if (!player.IsInPronePose)
                {
                    if (prone)
                    {
                        player.CurrentManagedState.Prone();
                    }
                }
                else
                {
                    if (!prone)
                    {
                        player.ToggleProne();
                        player.MovementContext.UpdatePoseAfterProne();
                    }
                }
            }
        }


        //private void ProcessPlayerStateProne(Dictionary<string, object> packet)
        //{
        //    bool prone = bool.Parse(packet["prn"].ToString());
        //    if (!player.IsInPronePose)
        //    {
        //        if (prone)
        //        {
        //            player.CurrentManagedState.Prone();
        //        }
        //    }
        //    else
        //    {
        //        if (!prone)
        //        {
        //            player.ToggleProne();
        //            player.MovementContext.UpdatePoseAfterProne();
        //        }
        //    }
        //}

        Player_Move_Patch _playerMovePatch = (Player_Move_Patch)ModuleReplicationPatch.Patches["Move"];

        public Vector2? ReplicatedDirection => ReplicatedPlayerStatePacket != null ? new Vector2(ReplicatedPlayerStatePacket.MovementDirectionX, ReplicatedPlayerStatePacket.MovementDirectionY) : null;
        public Vector2? ReplicatedRotation => ReplicatedPlayerStatePacket != null ? new Vector2(ReplicatedPlayerStatePacket.RotationX, ReplicatedPlayerStatePacket.RotationY) : null;
        public Vector3? ReplicatedPosition => ReplicatedPlayerStatePacket != null ? new Vector3(ReplicatedPlayerStatePacket.PositionX, ReplicatedPlayerStatePacket.PositionY, ReplicatedPlayerStatePacket.PositionZ) : null;
        public Vector3? ReplicatedHeadRotation => ReplicatedPlayerStatePacket != null ? new Vector3(ReplicatedPlayerStatePacket.HeadRotationX, ReplicatedPlayerStatePacket.HeadRotationY, ReplicatedPlayerStatePacket.HeadRotationZ) : null;
        public float? ReplicatedTilt => ReplicatedPlayerStatePacket != null ? ReplicatedPlayerStatePacket.Tilt : null;
        public bool ShouldSprint => ReplicatedPlayerStatePacket != null ? ReplicatedPlayerStatePacket.IsSprinting : false;
        private float PoseLevelDesired => ReplicatedPlayerStatePacket != null ? ReplicatedPlayerStatePacket.PoseLevel : 1;


        public bool IsSprinting
        {
            get { return player.IsSprintEnabled; }
        }
        public PlayerStatePacket ReplicatedPlayerStatePacket { get; internal set; }

        public ManualLogSource Logger { get; private set; }

        public Dictionary<string, object> PreMadeMoveDataPacket = new()
        {
            { "dX", "0" },
            { "dY", "0" },
            { "rX", "0" },
            { "rY", "0" },
            { "m", "Move" }
        };
        public Dictionary<string, object> PreMadeTiltDataPacket = new()
        {
            { "tilt", "0" },
            { "m", "Tilt" }
        };

        public bool IsAI()
        {
            return player.IsAI && !player.Profile.Id.StartsWith("pmc");
        }

        public bool IsOwnedPlayer()
        {
            return player.Profile.Id.StartsWith("pmc") && !IsClientDrone;
        }
    }
}
