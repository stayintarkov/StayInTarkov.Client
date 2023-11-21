#pragma warning disable CS0618 // Type or member is obsolete
using BepInEx.Logging;
using EFT;
using EFT.HealthSystem;
using EFT.InventoryLogic;
using StayInTarkov.Coop;
using StayInTarkov.Coop.Components;
using StayInTarkov.Coop.NetworkPacket;
using StayInTarkov.Coop.Player;
using StayInTarkov;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using static AHealthController<EFT.HealthSystem.ActiveHealthController.AbstractHealthEffect>;
using StayInTarkov.Coop.Web;

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

        private float PoseLevelDesired { get; set; } = 1;
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

                    Item dogtagContainter = null;
                    foreach (Item item in player.Inventory.GetAllItemByTemplate("55d7217a4bdc2d86028b456d"))
                        if (item.IsContainer)
                            dogtagContainter = item; // should be only 1 result.

                    if (dogtagContainter == null)
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

            ProcessPlayerState(packet);

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

        void ProcessPlayerState(Dictionary<string, object> packet)
        {
            if (!packet.ContainsKey("m"))
                return;

            var method = packet["m"].ToString();
            if (method != "PlayerState")
                return;


            if (!IsClientDrone)
                return;

            {
                // Pose
                float poseLevel = float.Parse(packet["pose"].ToString());
                PoseLevelDesired = poseLevel;

                // Speed
                if (packet.ContainsKey("spd"))
                {
                    ReplicatedMovementSpeed = float.Parse(packet["spd"].ToString());
                    player.CurrentManagedState.ChangeSpeed(ReplicatedMovementSpeed);
                }
                // ------------------------------------------------------
                // Prone -- With fixes. Thanks @TehFl0w
                ProcessPlayerStateProne(packet);

                // Rotation
                if (packet.ContainsKey("rX") && packet.ContainsKey("rY"))
                {
                    Vector2 packetRotation = new(
                float.Parse(packet["rX"].ToString())
                , float.Parse(packet["rY"].ToString())
                );
                    //player.Rotation = packetRotation;
                    ReplicatedRotation = packetRotation;
                }

                if (packet.ContainsKey("spr"))
                {
                    // Sprint
                    ShouldSprint = bool.Parse(packet["spr"].ToString());
                    //ProcessPlayerStateSprint(packet);
                }

                // Position
                Vector3 packetPosition = new(
                    float.Parse(packet["pX"].ToString())
                    , float.Parse(packet["pY"].ToString())
                    , float.Parse(packet["pZ"].ToString())
                    );

                ReplicatedPosition = packetPosition;

                // Move / Direction
                if (packet.ContainsKey("dX") && packet.ContainsKey("dY"))
                {
                    Vector2 packetDirection = new(
                    float.Parse(packet["dX"].ToString())
                    , float.Parse(packet["dY"].ToString())
                    );
                    ReplicatedDirection = packetDirection;
                }
                else
                {
                    ReplicatedDirection = null;
                }

                if (packet.ContainsKey("tilt"))
                {
                    var tilt = float.Parse(packet["tilt"].ToString());
                    player.MovementContext.SetTilt(tilt);
                }


                if (packet.ContainsKey("dX") && packet.ContainsKey("dY") && packet.ContainsKey("spr") && packet.ContainsKey("spd"))
                {
                    // Force Rotation
                    player.Rotation = ReplicatedRotation.Value;
                    var playerMovePatch = (Player_Move_Patch)ModuleReplicationPatch.Patches["Move"];
                    playerMovePatch?.Replicated(player, packet);
                }

                if (packet.ContainsKey("alive"))
                {
                    bool isCharAlive = bool.Parse(packet.ContainsKey("alive").ToString());
                    if (!isCharAlive && player.ActiveHealthController.IsAlive)
                    {
                        player.ActiveHealthController.Kill(Player_ApplyDamageInfo_Patch.LastDamageTypes.ContainsKey(packet["profileId"].ToString()) ? Player_ApplyDamageInfo_Patch.LastDamageTypes[packet["profileId"].ToString()] : EDamageType.Undefined);
                    }
                }

                if (packet.ContainsKey("hp.Chest") && packet.ContainsKey("en") && packet.ContainsKey("hy"))
                {
                    var dictionary = ReflectionHelpers.GetFieldOrPropertyFromInstance<Dictionary<EBodyPart, BodyPartState>>(player.ActiveHealthController, "Dictionary_0", false);

                    if (dictionary != null)
                    {
                        foreach (EBodyPart bodyPart in Enum.GetValues(typeof(EBodyPart)))
                        {
                            if (packet.ContainsKey($"hp.{bodyPart}"))
                            {
                                BodyPartState bodyPartState = dictionary[bodyPart];
                                if (bodyPartState != null)
                                {
                                    bodyPartState.Health = new(float.Parse(packet[$"hp.{bodyPart}"].ToString()), float.Parse(packet[$"hp.{bodyPart}.m"].ToString()));
                                }
                            }
                        }
                    }

                    HealthValue energy = ReflectionHelpers.GetFieldOrPropertyFromInstance<HealthValue>(player.ActiveHealthController, "healthValue_0", false);
                    if (energy != null)
                        energy.Current = float.Parse(packet["en"].ToString());

                    HealthValue hydration = ReflectionHelpers.GetFieldOrPropertyFromInstance<HealthValue>(player.ActiveHealthController, "healthValue_1", false);
                    if (hydration != null)
                        hydration.Current = float.Parse(packet["hy"].ToString());
                }

                return;
            }

        }

        public bool ShouldSprint { get; set; }
        private bool isSprinting;

        public bool IsSprinting
        {
            get { return isSprinting || player.IsSprintEnabled; }
            set { isSprinting = value; }
        }


        private void ProcessPlayerStateSprint(Dictionary<string, object> packet)
        {
            ShouldSprint = bool.Parse(packet["spr"].ToString());

            //    // If we are requesting to sprint but we are alreadying sprinting, don't do anything
            //    //if (ShouldSprint && IsSprinting)
            //    //    return;

            //    if (ShouldSprint)
            //    {
            //        // normalize the movement direction. sprint requires 0 on the Y.
            //        player.MovementContext.MovementDirection = new Vector2(1, 0);
            //        player.MovementContext.PlayerAnimatorEnableSprint(true);
            //        //player.Physical.Sprint(true);
            //        //player.Physical.StaminaCapacity = 100;
            //        //player.Physical.StaminaRestoreRate = 100;
            //        IsSprinting = true;
            //    }
            //    else
            //    {
            //        //player.Physical.Sprint(false);
            //        IsSprinting = false;
            //        player.MovementContext.PlayerAnimatorEnableSprint(false);

            //}
        }

        private void ProcessPlayerStateProne(Dictionary<string, object> packet)
        {
            bool prone = bool.Parse(packet["prn"].ToString());
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

        private void ShouldTeleport(Vector3 desiredPosition)
        {
            var direction = (player.Position - desiredPosition).normalized;
            Ray ray = new(player.Position, direction);
            LayerMask layerMask = LayerMaskClass.HighPolyWithTerrainNoGrassMask;
        }

        void Update()
        {
            Update_ClientDrone();

            if (IsClientDrone && ShouldSprint)
            {
                player.Physical.Sprint(ShouldSprint);
            }

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
                    AkiBackendCommunicationCoop.PostLocalPlayerData(player, packet, true);
                }
            }
        }

        private void Update_ClientDrone()
        {
            if (!IsClientDrone)
                return;

            //if (!CoopGameComponent.TryGetCoopGameComponent(out _))
            //    return;

            // Replicate Position.
            // If a short distance -> Smooth Lerp to the Desired Position
            // If the other side of a wall -> Teleport to the correct side (TODO)
            // If far away -> Teleport
            //if (ReplicatedPosition.HasValue)
            //{
            //    var replicationDistance = Vector3.Distance(ReplicatedPosition.Value, player.Position);
            //    var replicatedPositionDirection = ReplicatedPosition.Value - player.Position;
            //    if (replicationDistance >= 3)
            //    {
            //        player.Teleport(ReplicatedPosition.Value, true);
            //    }
            //    else
            //    {
            //        player.Position = Vector3.Lerp(player.Position, ReplicatedPosition.Value, Time.deltaTime * 7);
            //    }
            //}

            // Replicate Rotation.
            // Smooth Lerp to the Desired Rotation
            if (ReplicatedRotation.HasValue)
            {
                player.Rotation = ShouldSprint ? ReplicatedRotation.Value : Vector3.Lerp(player.Rotation, ReplicatedRotation.Value, Time.deltaTime * 4);
            }

            // This will continue movements set be Player_Move_Patch
            //if (ReplicatedDirection.HasValue)
            //{
            //    player.CurrentManagedState.Move(ReplicatedDirection.Value);
            //    player.InputDirection = ReplicatedDirection.Value;
            //}
            //else
            //{
            //    player.InputDirection = Vector2.zero;
            //}

            if (!ShouldSprint)
            {
                PoseLevelSmoothed = Mathf.Lerp(PoseLevelSmoothed, PoseLevelDesired, Time.deltaTime);
                player.MovementContext.SetPoseLevel(PoseLevelSmoothed, true);
            }

            if (ReplicatedDirection.HasValue)
            {
                if (_playerMovePatch == null)
                    _playerMovePatch = (Player_Move_Patch)ModuleReplicationPatch.Patches["Move"];

                _playerMovePatch?.ReplicatedMove(player,
                    new PlayerMovePacket(player.ProfileId, 0, 0, 0, ReplicatedDirection.Value.x, ReplicatedDirection.Value.y, ReplicatedMovementSpeed));
            }
        }

        Player_Move_Patch _playerMovePatch = (Player_Move_Patch)ModuleReplicationPatch.Patches["Move"];

        private Vector2 LastDirection { get; set; } = Vector2.zero;
        private DateTime LastDirectionSent { get; set; } = DateTime.Now;
        private Vector2 LastRotation { get; set; } = Vector2.zero;
        private DateTime LastRotationSent { get; set; } = DateTime.Now;
        private Vector3 LastPosition { get; set; } = Vector3.zero;
        private DateTime LastPositionSent { get; set; } = DateTime.Now;
        public Vector2? ReplicatedDirection { get; internal set; }
        public Vector2? ReplicatedRotation { get; internal set; }
        public bool? ReplicatedRotationClamp { get; internal set; }
        public Vector3? ReplicatedPosition { get; internal set; }
        public DateTime LastPoseSent { get; private set; }
        public float LastPose { get; private set; }
        public DateTime LastSpeedSent { get; private set; }
        public float LastSpeed { get; private set; }
        public DateTime LastPlayerStateSent { get; private set; } = DateTime.Now;
        public bool TriggerPressed { get; internal set; }
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
