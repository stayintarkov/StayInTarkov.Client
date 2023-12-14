using BepInEx.Logging;
using Comfort.Common;
using EFT;
using EFT.Interactive;
using EFT.InventoryLogic;
using LiteNetLib.Utils;
using StayInTarkov.Coop.Matchmaker;
using StayInTarkov.Coop.Player;
using StayInTarkov.Coop.Web;
using StayInTarkov.Core.Player;
using StayInTarkov.Networking;
using StayInTarkov.Networking.Packets;
using System;
using System.Collections;
using System.Collections.Generic;
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
        private float InterpolationRatio { get; set; } = 0;
        public PlayerStatePacket LastState { get; set; }
        public PlayerStatePacket NewState { get; set; }

        public static async Task<LocalPlayer>
            Create(int playerId
            , Vector3 position
            , Quaternion rotation
            , string layerName
            , string prefix
            , EPointOfView pointOfView
            , Profile profile
            , bool aiControl
            , EUpdateQueue updateQueue
            , EUpdateMode armsUpdateMode
            , EUpdateMode bodyUpdateMode
            , CharacterControllerSpawner.Mode characterControllerMode
            , Func<float> getSensitivity, Func<float> getAimingSensitivity
            , IFilterCustomization filter
            , QuestControllerClass questController = null
            , bool isYourPlayer = false
            , bool isClientDrone = false)
        {
            CoopPlayer player = null;

            if (isClientDrone)
            {
                player = EFT.Player.Create<CoopPlayerClient>(
                    ResourceBundleConstants.PLAYER_BUNDLE_NAME
                    , playerId
                    , position
                    , updateQueue
                    , EFT.Player.EUpdateMode.Manual
                    , EFT.Player.EUpdateMode.Manual
                    , characterControllerMode
                    , getSensitivity
                    , getAimingSensitivity
                    , prefix
                    , aiControl);
            }
            else
            {
                player = EFT.Player.Create<CoopPlayer>(
                    ResourceBundleConstants.PLAYER_BUNDLE_NAME
                    , playerId
                    , position
                    , updateQueue
                    , armsUpdateMode
                    , bodyUpdateMode
                    , characterControllerMode
                    , getSensitivity
                    , getAimingSensitivity
                    , prefix
                    , aiControl);
            }
            player.IsYourPlayer = isYourPlayer;

            InventoryController inventoryController = isYourPlayer && !isClientDrone
                ? new CoopInventoryController(player, profile, true)
                : new CoopInventoryControllerForClientDrone(player, profile, true);

            if (questController == null && isYourPlayer)
            {
                questController = new QuestController(profile, inventoryController, StayInTarkovHelperConstants.BackEndSession, fromServer: true);
                questController.Run();
            }

            await player
                .Init(rotation, layerName, pointOfView, profile, inventoryController
                , new CoopHealthController(profile.Health, player, inventoryController, profile.Skills, aiControl)
                , isYourPlayer ? new CoopPlayerStatisticsManager() : new NullStatisticsManager()
                , questController
                , filter
                , aiControl || isClientDrone ? EVoipState.NotAvailable : EVoipState.Available
                , aiControl
                , async: false);

            player._handsController = EmptyHandsController.smethod_5<EmptyHandsController>(player);
            player._handsController.Spawn(1f, delegate
            {
            });
            player.AIData = new AIData(null, player);
            player.AggressorFound = false;
            player._animators[0].enabled = true;
            player._animators[0].speed = isYourPlayer ? 0.9f : 0.6f;
            player.BepInLogger = BepInEx.Logging.Logger.CreateLogSource("CoopPlayer");

            // If this is a Client Drone add Player Replicated Component
            if (isClientDrone)
            {
                var prc = player.GetOrAddComponent<PlayerReplicatedComponent>();
                prc.IsClientDrone = true;
            }

            return player;
        }

        /// <summary>
        /// A way to block the same Damage Info being run multiple times on this Character
        /// TODO: Fix this at source. Something is replicating the same Damage multiple times!
        /// </summary>
        private HashSet<DamageInfo> PreviousDamageInfos { get; } = new();
        private HashSet<string> PreviousSentDamageInfoPackets { get; } = new();
        private HashSet<string> PreviousReceivedDamageInfoPackets { get; } = new();
        public bool IsFriendlyBot { get; internal set; }

        public override void ApplyDamageInfo(DamageInfo damageInfo, EBodyPart bodyPartType, float absorbed, EHeadSegment? headSegment = null)
        {
            // Quick check?
            if (PreviousDamageInfos.Any(x =>
                x.Damage == damageInfo.Damage
                && x.SourceId == damageInfo.SourceId
                && x.Weapon != null && damageInfo.Weapon != null && x.Weapon.Id == damageInfo.Weapon.Id
                && x.Player != null && damageInfo.Player != null && x.Player == damageInfo.Player
                ))
                return;

            PreviousDamageInfos.Add(damageInfo);

            //BepInLogger.LogInfo($"{nameof(ApplyDamageInfo)}:{this.ProfileId}:{DateTime.Now.ToString("T")}");
            //base.ApplyDamageInfo(damageInfo, bodyPartType, absorbed, headSegment);

            if (CoopGameComponent.TryGetCoopGameComponent(out var coopGameComponent))
            {
                // If we are not using the Client Side Damage, then only run this on the server
                if (MatchmakerAcceptPatches.IsServer && !coopGameComponent.SITConfig.useClientSideDamageModel)
                    SendDamageToAllClients(damageInfo, bodyPartType, absorbed, headSegment);
                else
                    SendDamageToAllClients(damageInfo, bodyPartType, absorbed, headSegment);
            }
        }

        private void SendDamageToAllClients(DamageInfo damageInfo, EBodyPart bodyPartType, float absorbed, EHeadSegment? headSegment = null)
        {
            Dictionary<string, object> packet = new();
            var bodyPartColliderType = ((BodyPartCollider)damageInfo.HittedBallisticCollider).BodyPartColliderType;
            damageInfo.HitCollider = null;
            damageInfo.HittedBallisticCollider = null;
            Dictionary<string, string> playerDict = new();
            if (damageInfo.Player != null)
            {
                playerDict.Add("d.p.aid", damageInfo.Player.iPlayer.Profile.AccountId);
                playerDict.Add("d.p.id", damageInfo.Player.iPlayer.ProfileId);
            }

            damageInfo.Player = null;
            Dictionary<string, string> weaponDict = new();

            if (damageInfo.Weapon != null)
            {
                packet.Add("d.w.tpl", damageInfo.Weapon.TemplateId);
                packet.Add("d.w.id", damageInfo.Weapon.Id);
            }
            damageInfo.Weapon = null;

            packet.Add("d", damageInfo.SITToJson());
            packet.Add("d.p", playerDict);
            packet.Add("d.w", weaponDict);
            packet.Add("bpt", bodyPartType.ToString());
            packet.Add("bpct", bodyPartColliderType.ToString());
            packet.Add("ab", absorbed.ToString());
            packet.Add("hs", headSegment.ToString());
            packet.Add("m", "ApplyDamageInfo");

            // -----------------------------------------------------------
            // An attempt to stop the same packet being sent multiple times
            if (PreviousSentDamageInfoPackets.Contains(packet.ToJson()))
                return;

            PreviousSentDamageInfoPackets.Add(packet.ToJson());
            // -----------------------------------------------------------

            AkiBackendCommunicationCoop.PostLocalPlayerData(this, packet);
        }

        public void ReceiveDamageFromServer(Dictionary<string, object> dict)
        {
            StartCoroutine(ReceiveDamageFromServerCR(dict));
        }

        public IEnumerator ReceiveDamageFromServerCR(Dictionary<string, object> dict)
        {
            if (PreviousReceivedDamageInfoPackets.Contains(dict.ToJson()))
                yield break;

            PreviousReceivedDamageInfoPackets.Add(dict.ToJson());

            //BepInLogger.LogDebug("ReceiveDamageFromServer");
            //BepInLogger.LogDebug(dict.ToJson());

            Enum.TryParse<EBodyPart>(dict["bpt"].ToString(), out var bodyPartType);
            Enum.TryParse<EHeadSegment>(dict["hs"].ToString(), out var headSegment);
            var absorbed = float.Parse(dict["ab"].ToString());

            var damageInfo = Player_ApplyShot_Patch.BuildDamageInfoFromPacket(dict);
            damageInfo.HitCollider = Player_ApplyShot_Patch.GetCollider(this, damageInfo.BodyPartColliderType);

            if (damageInfo.DamageType == EDamageType.Bullet && IsYourPlayer)
            {
                float handsShake = 0.05f;
                float cameraShake = 0.4f;
                float absorbedDamage = absorbed + damageInfo.Damage;

                switch (bodyPartType)
                {
                    case EBodyPart.Head:
                        handsShake = 0.1f;
                        cameraShake = 1.3f;
                        break;
                    case EBodyPart.LeftArm:
                    case EBodyPart.RightArm:
                        handsShake = 0.15f;
                        cameraShake = 0.5f;
                        break;
                    case EBodyPart.LeftLeg:
                    case EBodyPart.RightLeg:
                        cameraShake = 0.3f;
                        break;
                }

                ProceduralWeaponAnimation.ForceReact.AddForce(Mathf.Sqrt(absorbedDamage) / 10, handsShake, cameraShake);
                if (FPSCamera.Instance.EffectsController.TryGetComponent(out FastBlur fastBlur))
                {
                    fastBlur.enabled = true;
                    fastBlur.Hit(MovementContext.PhysicalConditionIs(EPhysicalCondition.OnPainkillers) ? absorbedDamage : (bodyPartType == EBodyPart.Head ? absorbedDamage * 6 : absorbedDamage * 3));
                }
            }

            base.ApplyDamageInfo(damageInfo, bodyPartType, absorbed, headSegment);
            //base.ShotReactions(damageInfo, bodyPartType);

            yield break;

        }

        public override void OnSkillLevelChanged(AbstractSkill skill)
        {
            //base.OnSkillLevelChanged(skill);
        }

        public override void OnWeaponMastered(MasterSkill masterSkill)
        {
            //base.OnWeaponMastered(masterSkill);
        }

        public override void Heal(EBodyPart bodyPart, float value)
        {
            base.Heal(bodyPart, value);
        }

        //public override PlayerHitInfo ApplyShot(DamageInfo damageInfo, EBodyPart bodyPartType, ShotId shotId)
        //{
        //    return base.ApplyShot(damageInfo, bodyPartType, shotId);
        //}

        //public void ReceiveApplyShotFromServer(Dictionary<string, object> dict)
        //{
        //    Logger.LogDebug("ReceiveApplyShotFromServer");
        //    Enum.TryParse<EBodyPart>(dict["bpt"].ToString(), out var bodyPartType);
        //    Enum.TryParse<EHeadSegment>(dict["hs"].ToString(), out var headSegment);
        //    var absorbed = float.Parse(dict["ab"].ToString());

        //    var damageInfo = Player_ApplyShot_Patch.BuildDamageInfoFromPacket(dict);
        //    damageInfo.HitCollider = Player_ApplyShot_Patch.GetCollider(this, damageInfo.BodyPartColliderType);

        //    var shotId = new ShotId();
        //    if (dict.ContainsKey("ammoid") && dict["ammoid"] != null)
        //    {
        //        shotId = new ShotId(dict["ammoid"].ToString(), 1);
        //    }

        //    base.ApplyShot(damageInfo, bodyPartType, shotId);
        //}

        public override Corpse CreateCorpse()
        {
            CancelInvoke("SendStatePacket");
            CancelInvoke("Interpolate");
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
            BepInLogger.LogDebug($"{nameof(ReceiveSay)}({trigger},{index})");

            var prc = GetComponent<PlayerReplicatedComponent>();
            if (prc == null || !prc.IsClientDrone)
                return;

            Speaker.PlayDirect(trigger, index);
        }

        public void Interpolate()
        {
            if (!IsYourPlayer)
            {
                InterpolationRatio += Time.deltaTime / Time.fixedDeltaTime;

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

                if (!IsInventoryOpened)
                {
                    Move(NewState.InputDirection);
                }
                Vector3 a = Vector3.Lerp(MovementContext.TransformPosition, NewState.Position, InterpolationRatio);
                CharacterController.Move(a - MovementContext.TransformPosition, InterpolationRatio);

                LastState = NewState;
                InterpolationRatio = 0;
            }
        }

        public void SendStatePacket()
        {
            if (Client != null && IsYourPlayer)
            {
                PlayerStatePacket playerStatePacket = new(ProfileId, Position, Rotation, HeadRotation,
                        MovementContext.MovementDirection, CurrentManagedState.Name, MovementContext.Tilt,
                        MovementContext.Step, CurrentAnimatorStateIndex, MovementContext.CharacterMovementSpeed,
                        IsInPronePose, PoseLevel, MovementContext.IsSprintEnabled, Physical.SerializationStruct, InputDirection);

                Writer.Reset();

                Client.SendData(Writer, ref playerStatePacket, LiteNetLib.DeliveryMethod.Unreliable);
                return;
            }
            else if (MatchmakerAcceptPatches.IsServer && Server != null)
            {
                PlayerStatePacket playerStatePacket = new(ProfileId, Position, Rotation, HeadRotation,
                        MovementContext.MovementDirection, CurrentManagedState.Name, MovementContext.Tilt,
                        MovementContext.Step, CurrentAnimatorStateIndex, MovementContext.CharacterMovementSpeed,
                        IsInPronePose, PoseLevel, MovementContext.IsSprintEnabled, Physical.SerializationStruct, InputDirection);

                Writer.Reset();

                Server.SendData(Writer, ref playerStatePacket, LiteNetLib.DeliveryMethod.Unreliable);
                return;
            }
            else if (MatchmakerAcceptPatches.IsServer)
            {
                PlayerStatePacket playerStatePacket = new(ProfileId, Position, Rotation, HeadRotation,
                        MovementContext.MovementDirection, CurrentManagedState.Name, MovementContext.Tilt,
                        MovementContext.Step, CurrentAnimatorStateIndex, MovementContext.CharacterMovementSpeed,
                        IsInPronePose, PoseLevel, MovementContext.IsSprintEnabled, Physical.SerializationStruct, InputDirection);

                var e = Singleton<GameWorld>.Instance.MainPlayer as CoopPlayer;
                Writer.Reset();

                e.Server.SendData(Writer, ref playerStatePacket, LiteNetLib.DeliveryMethod.Unreliable);
                return;
            }
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

            LastState = new(ProfileId, new Vector3(Position.x, Position.y, 1000), Rotation, HeadRotation,
                MovementContext.MovementDirection, CurrentManagedState.Name, MovementContext.Tilt,
                MovementContext.Step, CurrentAnimatorStateIndex, MovementContext.SmoothedCharacterMovementSpeed,
                IsInPronePose, PoseLevel, MovementContext.IsSprintEnabled, Physical.SerializationStruct, InputDirection);
            NewState = new(ProfileId, new Vector3(Position.x, Position.y, 1000), Rotation, HeadRotation,
                MovementContext.MovementDirection, CurrentManagedState.Name, MovementContext.Tilt,
                MovementContext.Step, CurrentAnimatorStateIndex, MovementContext.SmoothedCharacterMovementSpeed,
                IsInPronePose, PoseLevel, MovementContext.IsSprintEnabled, Physical.SerializationStruct, InputDirection);

            if (IsYourPlayer)
            {
                InvokeRepeating("SendStatePacket", 1f, 0.005f);
            }
            else if (MatchmakerAcceptPatches.IsServer)
            {
                InvokeRepeating("SendStatePacket", 1f, 0.005f);
            }
            if ((MatchmakerAcceptPatches.IsClient && !IsYourPlayer) || (MatchmakerAcceptPatches.IsServer && !IsAI && !IsYourPlayer))
            {
                InvokeRepeating("Interpolate", 1f, 0.005f);
            }
        }

        public override void OnDestroy()
        {
            BepInLogger.LogDebug("OnDestroy()");
            base.OnDestroy();
        }

    }
}
