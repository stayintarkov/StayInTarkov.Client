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

                    string itemId = new MongoID(true);
                    Logger.LogInfo($"New Dogtag Id: {itemId}");
                    //using (SHA256 sha256 = SHA256.Create())
                    //{
                    //    StringBuilder sb = new();

                    //    byte[] hashes = sha256.ComputeHash(Encoding.UTF8.GetBytes(coopGameComponent.ServerId + player.ProfileId + coopGameComponent.Timestamp));
                    //    for (int i = 0; i < hashes.Length; i++)
                    //        sb.Append(hashes[i].ToString("x2"));

                    //    itemId = sb.ToString().Substring(0, 24);
                    //}

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
            //Update_ClientDrone();

            

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

            //Update_ClientDrone();

            // This must exist in Update AND LateUpdate to function correctly.
            //player.MovementContext.EnableSprint(ShouldSprint);
            //player.MovementContext.PlayerAnimator.EnableSprint(ShouldSprint);
            //if (ShouldSprint)
            //{
            //    player.Rotation = ReplicatedRotation.Value;
            //    player.MovementContext.Rotation = ReplicatedRotation.Value;
            //    player.MovementContext.PlayerAnimator.SetMovementDirection(ReplicatedDirection.HasValue ? ReplicatedDirection.Value : player.InputDirection);
            //}

        }

        private void Update_ClientDrone()
        {

            if (!IsClientDrone)
            {
                return;
            }

            //Logger.LogDebug($"{nameof(Update_ClientDrone)}:IsClientDrone:{IsClientDrone}");

            if (ReplicatedPlayerStatePacket == null)
            {
                //Logger.LogError($"{nameof(Update_ClientDrone)}:ReplicatedPlayerStatePacket is Null");
                return;
            }


            // Head Rotation
            var newHeadRotation = new Vector3(ReplicatedPlayerStatePacket.HeadRotationX, ReplicatedPlayerStatePacket.HeadRotationY, ReplicatedPlayerStatePacket.HeadRotationZ);
            player.HeadRotation = Vector3.Lerp(player.HeadRotation, newHeadRotation, Time.deltaTime * 4);
            player.ProceduralWeaponAnimation.SetHeadRotation(player.HeadRotation);

            var lastMoveDir = new Vector2(LastReplicatedPlayerStatePacket.MovementDirectionX, LastReplicatedPlayerStatePacket.MovementDirectionY);
            var newMoveDir = new Vector2(ReplicatedPlayerStatePacket.MovementDirectionX, ReplicatedPlayerStatePacket.MovementDirectionY);
            player.MovementContext.PlayerAnimatorSetMovementDirection(Vector2.Lerp(lastMoveDir, newMoveDir, Time.deltaTime * 2));
            //player.MovementContext.PlayerAnimatorSetDiscreteDirection(GClass1595.ConvertToMovementDirection(NewState.MovementDirection));

            // Replicate Rotation.
            // Smooth Lerp to the Desired Rotation
            if (ReplicatedRotation.HasValue && Vector2.Dot(player.Rotation, ReplicatedDirection.Value) < 0.9)
            {
                var r = Vector2.Lerp(player.Rotation, ReplicatedRotation.Value, Time.deltaTime * 4);
                player.Rotate((r - player.Rotation).normalized, true);
            }

            //if (!ShouldSprint && ReplicatedPosition.HasValue && Vector3.Distance(ReplicatedPosition.Value, player.Position) > 1)
            //{
            //    if(Vector3.Distance(ReplicatedPosition.Value, player.Position) > 3)
            //        player.Position = ReplicatedPosition.Value;
            //    //else
            //    //    player.Position = Vector3.Lerp(player.Position, ReplicatedPosition.Value, Time.deltaTime * 7);
            //}
            //else if (ReplicatedPlayerStatePacket != null)
            //{
            //    //player.CurrentManagedState.Move(new Vector2(ReplicatedPlayerStatePacket.InputDirectionX, ReplicatedPlayerStatePacket.InputDirectionY));
            //}

            //if (!player.IsInventoryOpened)
            //{
            //    var inputDir = new Vector2(ReplicatedPlayerStatePacket.InputDirectionX, ReplicatedPlayerStatePacket.InputDirectionY);
            //    player.CurrentManagedState.Move(inputDir);

            //    if(Vector3.Distance(ReplicatedPosition.Value, player.Position) > 1)
            //    {
            //        player.CurrentManagedState.Move((ReplicatedPosition.Value - player.Position).normalized);
            //    }
            //}

            //player.MovementContext.PlayerAnimator.EnableSprint(ShouldSprint);
            if (!ShouldSprint)
            {
                if (PoseLevelDesired.HasValue)
                {
                    PoseLevelSmoothed = Mathf.Lerp(PoseLevelSmoothed, PoseLevelDesired.Value, Time.deltaTime);
                    player.MovementContext.SetPoseLevel(PoseLevelSmoothed, true);
                }
            }
            else
            {
                player.MovementContext.PlayerAnimator.EnableSprint(ShouldSprint);
                player.EnableSprint(ShouldSprint);
            }

            if (ReplicatedHeadRotation.HasValue)
            {
                player.HeadRotation = Vector3.Lerp(player.HeadRotation, ReplicatedHeadRotation.Value, Time.deltaTime * 7);
            }

            if (ReplicatedTilt.HasValue)
            {
                player.MovementContext.SetTilt(Mathf.Lerp(player.MovementContext.Tilt, ReplicatedTilt.Value, Time.deltaTime * 7), true);
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

                if (ReplicatedPlayerHealth != null)
                {
                    //Logger.LogDebug($"{nameof(ReplicatedPlayerHealth)} found");

                    if (_healthDictionary == null)
                        _healthDictionary = ReflectionHelpers.GetFieldOrPropertyFromInstance<Dictionary<EBodyPart, BodyPartState>>(player.HealthController, "dictionary_0", false);

                    if (_healthDictionary != null && ReplicatedPlayerHealth.BodyParts != null)
                    {
                        //Logger.LogDebug($"{nameof(_healthDictionary)} found");

                        foreach (PlayerBodyPartHealthPacket bodyPartHP in ReplicatedPlayerHealth.BodyParts)
                        {
                            if (_healthDictionary.ContainsKey(bodyPartHP.BodyPart))
                            {
                                BodyPartState bodyPartState = _healthDictionary[bodyPartHP.BodyPart];
                                if (bodyPartState != null)
                                {
                                    bodyPartState.Health = new HealthValue(bodyPartHP.Current, bodyPartHP.Maximum);
                                    //Logger.LogDebug($"Set {player.Profile.Nickname} {bodyPartHP.BodyPart} health to {bodyPartHP.Current}/{bodyPartHP.Maximum}");
                                }
                            }
                        }

                        //ReflectionHelpers.SetFieldOrPropertyFromInstance(player.ActiveHealthController, "Dictionary_0", _healthDictionary);
                    }

                    HealthValue energy = ReflectionHelpers.GetFieldOrPropertyFromInstance<HealthValue>(player.HealthController, "healthValue_0", false);
                    if (energy != null)
                        energy.Current = ReplicatedPlayerStatePacket.PlayerHealth.Energy;

                    HealthValue hydration = ReflectionHelpers.GetFieldOrPropertyFromInstance<HealthValue>(player.HealthController, "healthValue_1", false);
                    if (hydration != null)
                        hydration.Current = ReplicatedPlayerStatePacket.PlayerHealth.Hydration;
                }
            }

            LastReplicatedPlayerStatePacket = ReplicatedPlayerStatePacket;
        }

        private Dictionary<EBodyPart, BodyPartState> _healthDictionary;
        private static Array BodyPartEnumValues => Enum.GetValues(typeof(EBodyPart));

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
        private float? PoseLevelDesired => ReplicatedPlayerStatePacket != null ? ReplicatedPlayerStatePacket.PoseLevel : null;
        public PlayerHealthPacket ReplicatedPlayerHealth => ReplicatedPlayerStatePacket != null ? ReplicatedPlayerStatePacket.PlayerHealth : null;

        public bool IsSprinting
        {
            get { return player.IsSprintEnabled; }
        }
        public PlayerStatePacket ReplicatedPlayerStatePacket { get; internal set; } = new();
        public PlayerStatePacket LastReplicatedPlayerStatePacket { get; internal set; } = new();

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

        internal void UpdateTick()
        {
            Update_ClientDrone();
        }
    }
}
