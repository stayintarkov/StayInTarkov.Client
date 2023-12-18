using BepInEx.Logging;
using Comfort.Common;
using EFT;
using EFT.HealthSystem;
using EFT.Interactive;
using EFT.InventoryLogic;
using LiteNetLib.Utils;
using StayInTarkov.Coop.ItemControllerPatches;
using StayInTarkov.Coop.Matchmaker;
using StayInTarkov.Coop.PacketQueues;
using StayInTarkov.Coop.Web;
using StayInTarkov.Core.Player;
using StayInTarkov.Networking;
using StayInTarkov.Networking.Packets;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace StayInTarkov.Coop
{
    public class CoopPlayer : LocalPlayer
    {
        ManualLogSource BepInLogger { get; set; }
        public SITServer Server { get; set; }
        public SITClient Client { get; set; }
        public NetDataWriter Writer { get; set; }
        private float InterpolationRatio { get; set; } = 0.5f;
        public PlayerStatePacket LastState { get; set; }
        public PlayerStatePacket NewState { get; set; }
        public WeaponPacket WeaponPacket = new("null");
        public WeaponPacketQueue FirearmPackets { get; set; } = new(100);
        public HealthPacket HealthPacket = new("null");
        public HealthPacketQueue HealthPackets { get; set; } = new(100);
        public InventoryPacket InventoryPacket = new("null");
        public InventoryPacketQueue InventoryPackets = new(100);

        public static async Task<LocalPlayer> Create(
            int playerId,
            Vector3 position,
            Quaternion rotation,
            string layerName,
            string prefix,
            EPointOfView pointOfView,
            Profile profile,
            bool aiControl,
            EUpdateQueue updateQueue,
            EUpdateMode armsUpdateMode,
            EUpdateMode bodyUpdateMode,
            CharacterControllerSpawner.Mode characterControllerMode,
            Func<float> getSensitivity, Func<float> getAimingSensitivity,
            IFilterCustomization filter,
            QuestControllerClass questController = null,
            bool isYourPlayer = false,
            bool isClientDrone = false)
        {
            CoopPlayer player = null;

            if (isClientDrone)
            {
                player = EFT.Player.Create<CoopPlayerClient>(
                    ResourceBundleConstants.PLAYER_BUNDLE_NAME,
                    playerId,
                    position,
                    updateQueue,
                    EUpdateMode.Manual,
                    EUpdateMode.Auto,
                    characterControllerMode,
                    getSensitivity,
                    getAimingSensitivity,
                    prefix,
                    aiControl);
            }
            else
            {
                player = EFT.Player.Create<CoopPlayer>(
                    ResourceBundleConstants.PLAYER_BUNDLE_NAME,
                    playerId,
                    position,
                    updateQueue,
                    armsUpdateMode,
                    bodyUpdateMode,
                    characterControllerMode,
                    getSensitivity,
                    getAimingSensitivity,
                    prefix,
                    aiControl);
            }
            player.IsYourPlayer = isYourPlayer;

            InventoryController inventoryController = new PlayerInventoryController(player, profile, true);

            if (questController == null && isYourPlayer)
            {
                questController = new QuestController(profile, inventoryController, StayInTarkovHelperConstants.BackEndSession, fromServer: true);
                questController.Run();
            }

            await player.Init(rotation, layerName, pointOfView, profile, inventoryController,
                new CoopHealthController(profile.Health, player, inventoryController, profile.Skills, aiControl),
                isYourPlayer ? new CoopPlayerStatisticsManager() : new NullStatisticsManager(), questController, filter,
                aiControl || isClientDrone ? EVoipState.NotAvailable : EVoipState.Available, aiControl, async: false);

            player._handsController = EmptyHandsController.smethod_5<EmptyHandsController>(player);
            player._handsController.Spawn(1f, delegate { });
            player.AIData = new AIData(null, player);
            player.AggressorFound = false;
            player._animators[0].enabled = true;
            player.BepInLogger = BepInEx.Logging.Logger.CreateLogSource("CoopPlayer");
            if (!player.IsYourPlayer)
            {
                player._armsUpdateQueue = EUpdateQueue.Update;
            }
            // If this is a Client Drone add Player Replicated Component
            if (isClientDrone)
            {
                var prc = player.GetOrAddComponent<PlayerReplicatedComponent>();
                prc.IsClientDrone = true;
            }

            return player;
        }

        public override void OnSkillLevelChanged(AbstractSkill skill)
        {
            //base.OnSkillLevelChanged(skill);
        }

        public override void OnWeaponMastered(MasterSkill masterSkill)
        {
            //base.OnWeaponMastered(masterSkill);
        }

        public override void ApplyDamageInfo(DamageInfo damageInfo, EBodyPart bodyPartType, float absorbed, EHeadSegment? headSegment = null)
        {
            // TODO: Try to run all of this locally so we do not rely on the server / fight lag
            // TODO: Send information on who shot us to prevent the end screen to be empty / kill feed being wrong
            // TODO: Do this on ApplyShot instead, and check if instigator is local
            // Also do check if it's a server and shooter is AI

            if (!MatchmakerAcceptPatches.IsServer)
                return;

            HealthPacket.HasDamageInfo = true;
            HealthPacket.ApplyDamageInfo = new()
            {
                Damage = damageInfo.Damage,
                DamageType = damageInfo.DamageType,
                BodyPartType = bodyPartType,
                Absorbed = absorbed
            };
            HealthPacket.ToggleSend();

            base.ApplyDamageInfo(damageInfo, bodyPartType, absorbed, headSegment);
        }

        public void ClientApplyDamageInfo(DamageInfo damageInfo, EBodyPart bodyPartType, float absorbed, EHeadSegment? headSegment = null)
        {
            base.ApplyDamageInfo(damageInfo, bodyPartType, absorbed, null);
        }

        public override PlayerHitInfo ApplyShot(DamageInfo damageInfo, EBodyPart bodyPartType, ShotId shotId)
        {
            return base.ApplyShot(damageInfo, bodyPartType, shotId);
        }

        public override Corpse CreateCorpse()
        {
            StopCoroutine(SendStatePacket());
            return base.CreateCorpse();
        }

        public override void OnItemAddedOrRemoved(Item item, ItemAddress location, bool added)
        {
            base.OnItemAddedOrRemoved(item, location, added);
        }

        public override void OnPhraseTold(EPhraseTrigger @event, TaggedClip clip, TagBank bank, Speaker speaker)
        {
            base.OnPhraseTold(@event, clip, bank, speaker);

            if (IsYourPlayer)
            {
                Dictionary<string, object> packet = new()
                {
                    { "event", @event.ToString() },
                    { "index", clip.NetId },
                    { "m", "Say" }
                };
                AkiBackendCommunicationCoop.PostLocalPlayerData(this, packet);
            }
        }

        public void ReceiveSay(EPhraseTrigger trigger, int index)
        {
            // Look at breathing problem with packets?
            BepInLogger.LogDebug($"{nameof(ReceiveSay)}({trigger},{index})");

            var prc = GetComponent<PlayerReplicatedComponent>();
            if (prc == null || !prc.IsClientDrone)
                return;

            Speaker.PlayDirect(trigger, index);
        }

        public void Interpolate()
        {

            /* 
            * This code has been written by Lacyway (https://github.com/Lacyway) for the SIT Project (https://github.com/stayintarkov/StayInTarkov.Client).
            * You are free to re-use this in your own project, but out of respect please leave credit where it's due according to the MIT License
            */

            if (!IsYourPlayer)
            {

                Rotation = new Vector2(Mathf.LerpAngle(Yaw, NewState.Rotation.x, InterpolationRatio), Mathf.Lerp(Pitch, NewState.Rotation.y, InterpolationRatio));

                HeadRotation = Vector3.Lerp(LastState.HeadRotation, NewState.HeadRotation, InterpolationRatio);
                ProceduralWeaponAnimation.SetHeadRotation(Vector3.Lerp(LastState.HeadRotation, NewState.HeadRotation, InterpolationRatio));
                MovementContext.PlayerAnimatorSetMovementDirection(Vector2.Lerp(LastState.MovementDirection, NewState.MovementDirection, InterpolationRatio));
                MovementContext.PlayerAnimatorSetDiscreteDirection(GClass1595.ConvertToMovementDirection(NewState.MovementDirection));

                EPlayerState name = MovementContext.CurrentState.Name;
                EPlayerState eplayerState = NewState.State;
                if (eplayerState == EPlayerState.Jump)
                {
                    Jump();
                }
                if (name == EPlayerState.Jump && eplayerState != EPlayerState.Jump)
                {
                    MovementContext.PlayerAnimatorEnableJump(false);
                    MovementContext.PlayerAnimatorEnableLanding(true);
                }
                if ((name == EPlayerState.ProneIdle || name == EPlayerState.ProneMove) && eplayerState != EPlayerState.ProneMove && eplayerState != EPlayerState.Transit2Prone && eplayerState != EPlayerState.ProneIdle)
                {
                    MovementContext.IsInPronePose = false;
                }
                if ((eplayerState == EPlayerState.ProneIdle || eplayerState == EPlayerState.ProneMove) && name != EPlayerState.ProneMove && name != EPlayerState.Prone2Stand && name != EPlayerState.Transit2Prone && name != EPlayerState.ProneIdle)
                {
                    MovementContext.IsInPronePose = true;
                }

                Physical.SerializationStruct = NewState.Stamina;
                MovementContext.SetTilt(Mathf.Round(NewState.Tilt)); // Round the float due to byte converting error...
                CurrentManagedState.SetStep(NewState.Step);
                MovementContext.PlayerAnimatorEnableSprint(NewState.IsSprinting);
                MovementContext.EnableSprint(NewState.IsSprinting);

                MovementContext.IsInPronePose = NewState.IsProne;
                MovementContext.SetPoseLevel(Mathf.Lerp(LastState.PoseLevel, NewState.PoseLevel, InterpolationRatio));

                MovementContext.SetCurrentClientAnimatorStateIndex(NewState.AnimatorStateIndex);
                MovementContext.SetCharacterMovementSpeed(Mathf.Lerp(LastState.CharacterMovementSpeed, NewState.CharacterMovementSpeed, InterpolationRatio));
                MovementContext.PlayerAnimatorSetCharacterMovementSpeed(Mathf.Lerp(LastState.CharacterMovementSpeed, NewState.CharacterMovementSpeed, InterpolationRatio));

                MovementContext.SetBlindFire(NewState.Blindfire);

                if (!IsInventoryOpened && NewState.LinearSpeed > 0.25)
                {
                    Move(NewState.InputDirection);
                }
                Vector3 a = Vector3.Lerp(MovementContext.TransformPosition, NewState.Position, InterpolationRatio);
                CharacterController.Move(a - MovementContext.TransformPosition, InterpolationRatio);

                LastState = NewState;
            }
        }

        public IEnumerator SendStatePacket()
        {
            // TODO: Improve this by not resetting the writer and send many packets instead, rewrite the function in the client/server.
            var waitSeconds = new WaitForSeconds(0.025f);

            while (true)
            {
                yield return waitSeconds;

                if (Client != null && IsYourPlayer)
                {
                    PlayerStatePacket playerStatePacket = new(ProfileId, Position, Rotation, HeadRotation,
                            MovementContext.MovementDirection, CurrentManagedState.Name, MovementContext.Tilt,
                            MovementContext.Step, CurrentAnimatorStateIndex, MovementContext.CharacterMovementSpeed,
                            IsInPronePose, PoseLevel, MovementContext.IsSprintEnabled, Physical.SerializationStruct, InputDirection,
                            MovementContext.BlindFire, MovementContext.ActualLinearSpeed);

                    Writer.Reset();

                    Client.SendData(Writer, ref playerStatePacket, LiteNetLib.DeliveryMethod.Unreliable);

                    if (WeaponPacket.ShouldSend && !string.IsNullOrEmpty(WeaponPacket.ProfileId))
                    {
                        Writer.Reset();
                        Client.SendData(Writer, ref WeaponPacket, LiteNetLib.DeliveryMethod.ReliableOrdered);
                        WeaponPacket = new(ProfileId);
                    }

                    if (HealthPacket.ShouldSend && !string.IsNullOrEmpty(HealthPacket.ProfileId))
                    {
                        Writer.Reset();
                        Client.SendData(Writer, ref HealthPacket, LiteNetLib.DeliveryMethod.ReliableOrdered);
                        HealthPacket = new(ProfileId);
                    }

                    if (InventoryPacket.ShouldSend && !string.IsNullOrEmpty(InventoryPacket.ProfileId))
                    {
                        Writer.Reset();
                        Client.SendData(Writer, ref InventoryPacket, LiteNetLib.DeliveryMethod.ReliableOrdered);
                        InventoryPacket = new(ProfileId);
                    }
                }
                else if (MatchmakerAcceptPatches.IsServer && Server != null)
                {
                    PlayerStatePacket playerStatePacket = new(ProfileId, Position, Rotation, HeadRotation,
                            MovementContext.MovementDirection, CurrentManagedState.Name, MovementContext.Tilt,
                            MovementContext.Step, CurrentAnimatorStateIndex, MovementContext.CharacterMovementSpeed,
                            IsInPronePose, PoseLevel, MovementContext.IsSprintEnabled, Physical.SerializationStruct, InputDirection,
                            MovementContext.BlindFire, MovementContext.ActualLinearSpeed);

                    Writer.Reset();

                    Server.SendDataToAll(Writer, ref playerStatePacket, LiteNetLib.DeliveryMethod.Unreliable);

                    if (WeaponPacket.ShouldSend && !string.IsNullOrEmpty(WeaponPacket.ProfileId))
                    {
                        Writer.Reset();
                        Server.SendDataToAll(Writer, ref WeaponPacket, LiteNetLib.DeliveryMethod.ReliableOrdered);
                        WeaponPacket = new(ProfileId);
                    }

                    if (HealthPacket.ShouldSend && !string.IsNullOrEmpty(HealthPacket.ProfileId))
                    {
                        Writer.Reset();
                        Server.SendDataToAll(Writer, ref HealthPacket, LiteNetLib.DeliveryMethod.ReliableOrdered);
                        HealthPacket = new(ProfileId);
                    }

                    if (InventoryPacket.ShouldSend && !string.IsNullOrEmpty(InventoryPacket.ProfileId))
                    {
                        Writer.Reset();
                        Server.SendDataToAll(Writer, ref InventoryPacket, LiteNetLib.DeliveryMethod.ReliableOrdered);
                        InventoryPacket = new(ProfileId);
                    }
                }
                else if (MatchmakerAcceptPatches.IsServer)
                {
                    PlayerStatePacket playerStatePacket = new(ProfileId, Position, Rotation, HeadRotation,
                            MovementContext.MovementDirection, CurrentManagedState.Name, MovementContext.Tilt,
                            MovementContext.Step, CurrentAnimatorStateIndex, MovementContext.CharacterMovementSpeed,
                            IsInPronePose, PoseLevel, MovementContext.IsSprintEnabled, Physical.SerializationStruct, InputDirection,
                            MovementContext.BlindFire, MovementContext.ActualLinearSpeed);

                    // TODO: Improve this? Not sure if singleton getter is expensive.
                    var e = Singleton<GameWorld>.Instance.MainPlayer as CoopPlayer;
                    Writer.Reset();

                    e.Server.SendDataToAll(Writer, ref playerStatePacket, LiteNetLib.DeliveryMethod.Unreliable);

                    if (WeaponPacket.ShouldSend && !string.IsNullOrEmpty(WeaponPacket.ProfileId))
                    {
                        Writer.Reset();
                        e.Server.SendDataToAll(Writer, ref WeaponPacket, LiteNetLib.DeliveryMethod.ReliableOrdered);
                        WeaponPacket = new(ProfileId);
                    }

                    if (HealthPacket.ShouldSend && !string.IsNullOrEmpty(HealthPacket.ProfileId))
                    {
                        Writer.Reset();
                        e.Server.SendDataToAll(Writer, ref HealthPacket, LiteNetLib.DeliveryMethod.ReliableOrdered);
                        HealthPacket = new(ProfileId);
                    }

                    if (InventoryPacket.ShouldSend && !string.IsNullOrEmpty(InventoryPacket.ProfileId))
                    {
                        Writer.Reset();
                        e.Server.SendDataToAll(Writer, ref InventoryPacket, LiteNetLib.DeliveryMethod.ReliableOrdered);
                        InventoryPacket = new(ProfileId);
                    }
                }
            }
        }

        public IEnumerator SyncWorld()
        {
            // TODO: Consolidate into one packet.

            while (true)
            {
                yield return new WaitForSeconds(5f);

                EFT.UI.ConsoleScreen.Log("Sending synchronization packets.");
                Writer.Reset();
                GameTimerPacket gameTimerPacket = new(true);
                Client.SendData(Writer, ref gameTimerPacket, LiteNetLib.DeliveryMethod.ReliableOrdered);
                Writer.Reset();
                WeatherPacket weatherPacket = new() { IsRequest = true };
                Client.SendData(Writer, ref weatherPacket, LiteNetLib.DeliveryMethod.ReliableOrdered);

                yield return new WaitForSeconds(25f);
            }
        }

        IEnumerator SpawnPlayer()
        {
            // Temp fix to spawn players spawning underground. Still not completely fixed,
            // might have to run a function every X second to compare Vector3 with current state and if it doesn't match teleport them up.
            // Don't want to run that every state though as comparing Vector3s is expensive.

            yield return new WaitForSeconds(5);

            var pos1 = new Vector3(NewState.Position.x, NewState.Position.y + 0.75f, NewState.Position.z);
            Teleport(pos1);
            CharacterController.Move(pos1, DeltaTime);

            yield return new WaitForSeconds(15);

            if (Vector3.Distance(Position, NewState.Position) > 0.25)
            {
                EFT.UI.ConsoleScreen.LogError("SpawnDistance was too far!");
                var pos2 = new Vector3(NewState.Position.x, NewState.Position.y + 0.75f, NewState.Position.z);
                Teleport(pos2);
                CharacterController.Move(pos2, DeltaTime);
            }


            yield return new WaitForSeconds(3);

            ActiveHealthController.SetDamageCoeff(1);
            yield break;
        }

        private void Start()
        {
            if (MatchmakerAcceptPatches.IsServer && IsYourPlayer)
            {
                Server = this.GetOrAddComponent<SITServer>();
            }
            else if (IsYourPlayer)
            {
                Client = this.GetOrAddComponent<SITClient>();
                Client.Player = this;
            }

            Writer = new();

            WeaponPacket = new(ProfileId);
            HealthPacket = new(ProfileId);
            InventoryPacket = new(ProfileId);

            LastState = new(ProfileId, new Vector3(Position.x, Position.y + 0.5f, Position.z), Rotation, HeadRotation,
                MovementContext.MovementDirection, CurrentManagedState.Name, MovementContext.Tilt,
                MovementContext.Step, CurrentAnimatorStateIndex, MovementContext.SmoothedCharacterMovementSpeed,
                IsInPronePose, PoseLevel, MovementContext.IsSprintEnabled, Physical.SerializationStruct, InputDirection,
                MovementContext.BlindFire, MovementContext.ActualLinearSpeed);

            NewState = new(ProfileId, new Vector3(Position.x, Position.y + 0.5f, Position.z), Rotation, HeadRotation,
                MovementContext.MovementDirection, CurrentManagedState.Name, MovementContext.Tilt,
                MovementContext.Step, CurrentAnimatorStateIndex, MovementContext.SmoothedCharacterMovementSpeed,
                IsInPronePose, PoseLevel, MovementContext.IsSprintEnabled, Physical.SerializationStruct, InputDirection,
                MovementContext.BlindFire, MovementContext.ActualLinearSpeed);

            if (IsYourPlayer) // Run if it's us
            {
                StartCoroutine(SendStatePacket());
            }
            else if (MatchmakerAcceptPatches.IsServer) // Only run on AI when we are the server
            {
                StartCoroutine(SendStatePacket());
            }
            if ((MatchmakerAcceptPatches.IsClient && !IsYourPlayer) || (MatchmakerAcceptPatches.IsServer && !IsAI && !IsYourPlayer)) // Interpolate only if it's other clients
            {
                //StartCoroutine(Interpolate());
            }
            if (MatchmakerAcceptPatches.IsClient && IsYourPlayer)
            {
                StartCoroutine(SyncWorld());
            }

            if (!IsYourPlayer && !IsAI)
            {
                ActiveHealthController.SetDamageCoeff(0);
                StartCoroutine(SpawnPlayer());
            }
        }

        public override void UpdateTick()
        {
            base.UpdateTick();
            if ((MatchmakerAcceptPatches.IsClient && !IsYourPlayer) || (MatchmakerAcceptPatches.IsServer && !IsAI && !IsYourPlayer) && HealthController.IsAlive) // Interpolate only if it's other clients and alive
            {
                Interpolate();
                if (FirearmPackets.Count > 0)
                {
                    HandleWeaponPacket();
                }
            }
            if (HealthPackets.Count > 0)
            {
                HandleHealthPacket();
            }
            if (InventoryPackets.Count > 0)
            {
                HandleInventoryPacket();
            }
        }

        private void HandleInventoryPacket()
        {
            EFT.UI.ConsoleScreen.Log("I received a InventoryPacket");
            var packet = InventoryPackets.Dequeue();

            // TODO: Sometimes host drops items that AI dropped?
            // Seems like we can loot other players now without problems, maybe it was a problem when testing locally.

            if (packet.HasItemControllerExecutePacket)
            {
                var inventory = Singleton<GameWorld>.Instance.FindControllerById(packet.ItemControllerExecutePacket.InventoryId);
                if (inventory != null)
                {
                    // Look at method_117 on NetworkPlayer
                    // UnloadMag does not work: AmmoManipulationOperation.vmethod_0 NullReferenceException: Object reference not set to an instance of an object
                    using MemoryStream memoryStream = new(packet.ItemControllerExecutePacket.OperationBytes);
                    using BinaryReader binaryReader = new(memoryStream);
                    try
                    {
                        var convOp = binaryReader.ReadPolymorph<AbstractDescriptor1>();
                        var result = ToInventoryOperation(convOp);

                        if (result.Succeeded)
                        {
                            ItemController_Execute_Patch.RunLocally = false;
                            EFT.UI.ConsoleScreen.Log("ItemControllerExecutePacket: Executing operation " + result.Value.Id);
                            inventory.Execute(result.Value, null);
                        }
                    }
                    catch (Exception exception)
                    {
                        Debug.LogException(exception);
                    }
                }
                else
                {
                    EFT.UI.ConsoleScreen.Log("ItemControllerExecutePacket: inventory was null!");
                }
            }

            //if (packet.HasItemMovementHandlerMovePacket)
            //{
            //    if (ItemFinder.TryFindItem(packet.ItemMovementHandlerMovePacket.ItemId, out Item item))
            //    {
            //        if (ItemFinder.TryFindItemController(packet.ItemMovementHandlerMovePacket.Descriptor.Container.ParentId, out ItemController itemController))
            //        {
            //            ItemAddress address = itemController.ToItemAddress(packet.ItemMovementHandlerMovePacket.Descriptor);
            //            if (address != null)
            //            {
            //                //ItemControllerHandler_Move_Patch.RunLocally = false;
            //                ItemMovementHandler.Move(item, address, itemController, false);
            //            }
            //            else
            //            {
            //                EFT.UI.ConsoleScreen.LogError("ItemMovementHandlerMovePacket: Could not find ContainerID: " + packet.ItemMovementHandlerMovePacket.Descriptor.Container.ParentId);
            //            }
            //        }
            //        else
            //        {
            //            EFT.UI.ConsoleScreen.LogError("ItemMovementHandlerMovePacket: Could not find ContainerID: " + packet.ItemMovementHandlerMovePacket.Descriptor.Container.ParentId);
            //        }
            //    }
            //    else
            //    {
            //        EFT.UI.ConsoleScreen.LogError("ItemMovementHandlerMovePacket: Item " + packet.ItemMovementHandlerMovePacket.ItemId + " not found!");
            //    }
            //}
        }

        private void HandleWeaponPacket()
        {

            /* 
            * This code has been written by Lacyway (https://github.com/Lacyway) for the SIT Project (https://github.com/stayintarkov/StayInTarkov.Client).
            * You are free to re-use this in your own project, but out of respect please leave credit where it's due according to the MIT License
            */

            var firearmController = HandsController as FirearmController;
            var packet = FirearmPackets.Dequeue();
            if (firearmController != null)
            {
                firearmController.SetTriggerPressed(false);
                if (packet.IsTriggerPressed)
                    firearmController.SetTriggerPressed(true);

                if (packet.ChangeFireMode)
                    firearmController.ChangeFireMode(packet.FireMode);

                if (packet.ExamineWeapon)
                    firearmController.ExamineWeapon();

                if (packet.ToggleAim)
                    firearmController.SetAim(packet.AimingIndex);

                if (packet.CheckAmmo)
                    firearmController.CheckAmmo();

                if (packet.CheckChamber)
                    firearmController.CheckChamber();

                if (packet.CheckFireMode)
                    firearmController.CheckFireMode();

                if (packet.ToggleTacticalCombo)
                {
                    firearmController.SetLightsState(packet.LightStatesPacket.LightStates, true);
                }

                if (packet.ChangeSightMode)
                {
                    firearmController.SetScopeMode(packet.ScopeStatesPacket.ScopeStates);
                }

                if (packet.ToggleLauncher)
                    firearmController.ToggleLauncher();

                if (packet.EnableInventory)
                    firearmController.SetInventoryOpened(packet.InventoryStatus);

                if (packet.ReloadMag.Reload)
                {
                    MagazineClass magazine;
                    try
                    {
                        Item item = _inventoryController.FindItem(itemId: packet.ReloadMag.MagId);
                        magazine = item as MagazineClass;
                        if (magazine == null)
                        {
                            EFT.UI.ConsoleScreen.LogError($"HandleFirearmPacket::ReloadMag could not cast {packet.ReloadMag.MagId} as a magazine, got {item.ShortName}");
                        }
                    }
                    catch (Exception ex)
                    {
                        EFT.UI.ConsoleScreen.LogException(ex);
                        EFT.UI.ConsoleScreen.LogError($"There is no item {packet.ReloadMag.MagId} in profile {ProfileId}");
                        throw;
                    }
                    GridItemAddress gridItemAddress = null;
                    if (packet.ReloadMag.LocationDescription != null && packet.ReloadMag.LocationDescription.Length != 0)
                    {
                        using MemoryStream memoryStream = new(packet.ReloadMag.LocationDescription);
                        using BinaryReader binaryReader = new(memoryStream);
                        try
                        {
                            if (packet.ReloadMag.LocationDescription.Length != 0)
                            {
                                GridItemAddressDescriptor descriptor = binaryReader.ReadEFTGridItemAddressDescriptor();
                                gridItemAddress = _inventoryController.ToGridItemAddress(descriptor);
                            }
                        }
                        catch (GException4 exception2)
                        {
                            Debug.LogException(exception2);
                        }
                    }
                    if (magazine != null && gridItemAddress != null)
                        firearmController.ReloadMag(magazine, gridItemAddress, null);
                    else
                    {
                        EFT.UI.ConsoleScreen.LogError("HandleFirearmPacket::ReloadMag final variables were null!");
                    }
                }

                if (packet.QuickReloadMag.Reload)
                {
                    MagazineClass magazine;
                    try
                    {
                        Item item = _inventoryController.FindItem(packet.QuickReloadMag.MagId);
                        magazine = item as MagazineClass;
                        if (magazine == null)
                        {
                            EFT.UI.ConsoleScreen.LogError($"HandleFirearmPacket::QuickReloadMag could not cast {packet.ReloadMag.MagId} as a magazine, got {item.ShortName}");
                        }
                    }
                    catch (Exception ex)
                    {
                        EFT.UI.ConsoleScreen.LogException(ex);
                        EFT.UI.ConsoleScreen.LogError($"There is no item {packet.ReloadMag.MagId} in profile {ProfileId}");
                        throw;
                    }
                    firearmController.QuickReloadMag(magazine, null);
                }

                // Do we need a switch depending on the status or is that handled with SetTriggerPressed?
                if (packet.ReloadWithAmmo.Reload && !packet.CylinderMag.Changed)
                {
                    if (packet.ReloadWithAmmo.Status == SITSerialization.ReloadWithAmmoPacket.EReloadWithAmmoStatus.StartReload)
                    {
                        List<BulletClass> bullets = firearmController.FindAmmoByIds(packet.ReloadWithAmmo.AmmoIds);
                        AmmoPack ammoPack = new(bullets);
                        firearmController.ReloadWithAmmo(ammoPack, null);
                    }
                }

                if (packet.ReloadWithAmmo.Reload && packet.CylinderMag.Changed)
                {
                    if (packet.ReloadWithAmmo.Status == SITSerialization.ReloadWithAmmoPacket.EReloadWithAmmoStatus.StartReload)
                    {
                        List<BulletClass> bullets = firearmController.FindAmmoByIds(packet.ReloadWithAmmo.AmmoIds);
                        AmmoPack ammoPack = new(bullets);
                        firearmController.ReloadCylinderMagazine(ammoPack, null);
                    }
                }

                if (packet.ReloadLauncher.Reload)
                {
                    List<BulletClass> ammo = firearmController.FindAmmoByIds(packet.ReloadLauncher.AmmoIds);
                    AmmoPack ammoPack = new(ammo);
                    firearmController.ReloadGrenadeLauncher(ammoPack, null);
                }

                if (packet.ReloadBarrels.Reload)
                {
                    List<BulletClass> ammo = firearmController.FindAmmoByIds(packet.ReloadBarrels.AmmoIds);
                    AmmoPack ammoPack = new(ammo);

                    GridItemAddress gridItemAddress = null;
                    if (packet.ReloadBarrels.LocationDescription != null && packet.ReloadBarrels.LocationDescription.Length != 0)
                    {
                        using MemoryStream memoryStream = new(packet.ReloadBarrels.LocationDescription);
                        using BinaryReader binaryReader = new(memoryStream);
                        try
                        {
                            if (packet.ReloadBarrels.LocationDescription.Length != 0)
                            {
                                GridItemAddressDescriptor descriptor = binaryReader.ReadEFTGridItemAddressDescriptor();
                                gridItemAddress = _inventoryController.ToGridItemAddress(descriptor);
                            }
                        }
                        catch (GException4 exception2)
                        {
                            Debug.LogException(exception2);
                        }
                    }
                    if (ammoPack != null && gridItemAddress != null)
                        firearmController.ReloadBarrels(ammoPack, gridItemAddress, null);
                    else
                    {
                        EFT.UI.ConsoleScreen.LogError("HandleFirearmPacket::ReloadMag final variables were null!");
                    }
                }

            }
            else
            {
                EFT.UI.ConsoleScreen.LogError("HandsController was not of type FirearmController when processing FirearmPacket!");
            }

            if (packet.Gesture != EGesture.None)
                vmethod_3(packet.Gesture);

            if (packet.Loot)
                HandsController.Loot(packet.Loot);

            if (packet.Pickup)
                HandsController.Pickup(packet.Pickup);
        }

        private void HandleHealthPacket()
        {
            EFT.UI.ConsoleScreen.Log("I received a HealthPacket");
            var packet = HealthPackets.Dequeue();

            if (packet.HasDamageInfo) // Currently damage is being handled by the server, so we run this one on ourselves too
            {
                EFT.UI.ConsoleScreen.Log("I received a DamageInfoPacket");
                DamageInfo damageInfo = new()
                {
                    Damage = packet.ApplyDamageInfo.Damage,
                    DamageType = packet.ApplyDamageInfo.DamageType
                };
                ClientApplyDamageInfo(damageInfo, packet.ApplyDamageInfo.BodyPartType, packet.ApplyDamageInfo.Absorbed);
            }

            if (packet.HasBodyPartRestoreInfo && !IsYourPlayer)
            {
                EFT.UI.ConsoleScreen.Log("I received a RestoreBodyPartPacket");
                ActiveHealthController.RestoreBodyPart(packet.RestoreBodyPartPacket.BodyPartType, packet.RestoreBodyPartPacket.HealthPenalty);
            }

            if (packet.HasChangeHealthPacket && !IsYourPlayer)
            {
                EFT.UI.ConsoleScreen.Log("I received a ChangeHealthPacket");
                DamageInfo dInfo = new()
                {
                    DamageType = EDamageType.Medicine
                };
                ActiveHealthController.ChangeHealth(packet.ChangeHealthPacket.BodyPartType, packet.ChangeHealthPacket.Value, dInfo);
            }

            if (packet.HasEnergyChange && !IsYourPlayer)
            {
                EFT.UI.ConsoleScreen.Log("I received a EnergyChangePacket");
                ActiveHealthController.ChangeEnergy(packet.EnergyChangeValue);
            }

            if (packet.HasHydrationChange && !IsYourPlayer)
            {
                EFT.UI.ConsoleScreen.Log("I received a HydrationChangePacket");
                ActiveHealthController.ChangeHydration(packet.HydrationChangeValue);
            }

            if (packet.HasAddEffect && !IsYourPlayer)
            {
                EFT.UI.ConsoleScreen.Log("I received an AddEffectPacket");
                var coopHealthController = ActiveHealthController as CoopHealthController;
                coopHealthController.AddNetworkEffect(packet.AddEffectPacket.Type, packet.AddEffectPacket.BodyPartType, packet.AddEffectPacket.DelayTime,
                    packet.AddEffectPacket.WorkTime, packet.AddEffectPacket.ResidueTime, packet.AddEffectPacket.Strength);
            }

            if (packet.HasRemoveEffect && !IsYourPlayer)
            {
                // TODO: Fix sprint bug where sometimes the effects don't sync so clients still think the other player can't sprint

                if (packet.RemoveEffectPacket.Type == "MedEffect")
                    return;

                EFT.UI.ConsoleScreen.Log($"I received a RemoveEffectPacket: {packet.RemoveEffectPacket.Id} + {packet.RemoveEffectPacket.Type} + {packet.RemoveEffectPacket.BodyPartType}");

                var effects = ActiveHealthController.GetAllEffects(packet.RemoveEffectPacket.BodyPartType);
                var toRemove = effects.Where(x => x.GetType().Name == packet.RemoveEffectPacket.Type).FirstOrDefault();
                if (toRemove != default)
                {
                    EFT.UI.ConsoleScreen.Log($"RemoveEffectPacket: toRemove was {toRemove}");
                    if (toRemove is ActiveHealthController.AbstractEffect effect)
                    {
                        EFT.UI.ConsoleScreen.Log($"RemoveEffectPacket: Removing {effect}");
                        effect.ForceRemove();
                    }
                    else
                    {
                        EFT.UI.ConsoleScreen.Log("RemoveEffectPacket: effect was null!");
                    }
                }
                else
                {
                    EFT.UI.ConsoleScreen.Log("RemoveEffectPacket: toRemove was null!");
                }
            }
        }

        public override void OnDestroy()
        {
            BepInLogger.LogDebug("OnDestroy()");
            base.OnDestroy();
        }

    }
}
