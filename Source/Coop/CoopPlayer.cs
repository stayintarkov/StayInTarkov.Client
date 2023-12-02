using BepInEx.Logging;
using EFT;
using EFT.HealthSystem;
using EFT.Interactive;
using EFT.InventoryLogic;
using StayInTarkov.Coop.Matchmaker;
using StayInTarkov.Coop.NetworkPacket.Lacyway;
using StayInTarkov.Coop.Player;
using StayInTarkov.Coop.Player.FirearmControllerPatches;
using StayInTarkov.Coop.Web;
using StayInTarkov.Core.Player;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

namespace StayInTarkov.Coop
{
    internal class CoopPlayer : LocalPlayer
    {
        private NetworkPacket.Lacyway.PrevFrame prevFrame;
        ManualLogSource BepInLogger { get; set; }

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
                , new PlayerHealthController(profile.Health, player, inventoryController, profile.Skills, aiControl)
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

            AkiBackendCommunicationCoop.PostLocalPlayerData(this, packet, true);
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
            return base.CreateCorpse();
        }

        public override void OnItemAddedOrRemoved(Item item, ItemAddress location, bool added)
        {
            base.OnItemAddedOrRemoved(item, location, added);
        }

        public override void Rotate(Vector2 deltaRotation, bool ignoreClamp = false)
        {
            if (!CoopGameComponent.TryGetCoopGameComponent(out var coopGC))
            {
                base.Rotate(deltaRotation, ignoreClamp);
                return;
            }

            // If using Client Side Damage Model and Pressing the Trigger, send rotation to Server
            if (coopGC.SITConfig.useClientSideDamageModel
                && FirearmController_SetTriggerPressed_Patch.LastPress.ContainsKey(this.ProfileId)
                && FirearmController_SetTriggerPressed_Patch.LastPress[this.ProfileId] == true)
            {
                // Send to Server

            }

            base.Rotate(deltaRotation, ignoreClamp);
        }

        //public override void LateUpdate()
        //{
        //    //base.LateUpdate();
        //}

        //public override void ComplexLateUpdate(EUpdateQueue queue, float deltaTime)
        //{
        //    //base.ComplexLateUpdate(queue, deltaTime);
        //}

        public override void Move(Vector2 direction)
        {
            base.Move(direction);            

            var prc = GetComponent<PlayerReplicatedComponent>();
            if (prc.IsClientDrone)
                return;            
        }

        public override void SendHeadlightsPacket(bool isSilent)
        {
            LightsStates[] lightStates = _helmetLightControllers.Select(new Func<TacticalComboVisualController, LightsStates>(ClientPlayer.Class1383.class1383_0.method_0)).ToArray();
            prevFrame.HelmetLightPacket = new()
            {
                IsSilent = isSilent,
                LightsStates = lightStates
            };
            Logger.LogInfo("CoopPlayer::SendHeadlightsPacket");
        }

        public override void SendWeaponLightPacket()
        {
            ClientFirearmController clientFirearmController;
            if ((clientFirearmController = (HandsController as ClientFirearmController)) != null)
            {
                LightsStates[] array = clientFirearmController.Item.AllSlots.Select(new Func<Slot, Item>(ClientPlayer.Class1383.class1383_0.method_1)).GetComponents<LightComponent>().Select(new Func<LightComponent, LightsStates>(ClientPlayer.Class1383.class1383_0.method_2)).ToArray<LightsStates>();
                if (array.Length == 0)
                {
                    return;
                }
                TacticalComboPacket toggleTacticalCombo = new()
                {
                    ToggleTacticalCombo = true,
                    TacticalComboStatuses = new ScopePacket[array.Length]
                };
                for (int i = 0; i < array.Length; i++)
                {
                    LightsStates lightsStates = array[i];
                    toggleTacticalCombo.TacticalComboStatuses[i] = new ScopePacket
                    {
                        Id = lightsStates.Id,
                        IsActive = lightsStates.IsActive,
                        SelectedMode = lightsStates.LightMode
                    };
                }
                prevFrame.TacticalComboPacket = toggleTacticalCombo;
            }
        }

        public override void LateUpdate()
        {
            base.LateUpdate();

            prevFrame = new()
            {
                MovementInfoPacket = new()
                {
                    EPlayerState = CurrentManagedState.Name,
                    Position = new Vector3(x: Position.x + 2, y: Position.y, z: Position.z),
                    AnimatorStateIndex = CurrentAnimatorStateIndex,
                    PoseLevel = MovementContext.SmoothedPoseLevel,
                    CharacterMovementSpeed = MovementContext.ClampSpeed(MovementContext.SmoothedCharacterMovementSpeed),
                    Tilt = MovementContext.SmoothedTilt,
                    Step = MovementContext.Step,
                    BlindFire = MovementContext.BlindFire,
                    HeadRotation = HeadRotation,
                    Stamina = Physical.SerializationStruct,
                    DiscreteDirection = (int)MovementContext.DiscreteDirection,
                    Direction = MovementContext.MovementDirection,
                    IsGrounded = MovementContext.IsGrounded,
                    AimRotation = Rotation.y,
                    FallHeight = MovementContext.FallHeight,
                    FallTime = MovementContext.FreefallTime,
                    FootRotation = Rotation.x,
                    JumpHeight = MovementContext.JumpHeight,
                    MaxSpeed = MovementContext.MaxSpeed,
                    MovementDirection = MovementContext.MovementDirection,
                    PhysicalCondition = MovementContext.PhysicalCondition,
                    Pose = Pose,
                    SprintSpeed = MovementContext.SprintSpeed,
                    State = CurrentManagedState.Name,
                    Velocity = Velocity
                }
            };

            LightsStates[] lightStates = _helmetLightControllers.Select(new Func<TacticalComboVisualController, LightsStates>(ClientPlayer.Class1383.class1383_0.method_0)).ToArray();
            prevFrame.HelmetLightPacket = new HelmetLightPacket?(new HelmetLightPacket
            {
                IsSilent = true,
                LightsStates = lightStates
            });

            prevFrame.ReadPreviousFrame();

            if (CoopGame.TestController != null)
            {
                GStruct256 nextModel = new()
                {
                    Movement = new()
                    {
                        AimRotation = prevFrame.MovementInfoPacket.AimRotation,
                        BlindFire = prevFrame.MovementInfoPacket.BlindFire,
                        BodyPosition = prevFrame.MovementInfoPacket.Position,
                        FallHeight = prevFrame.MovementInfoPacket.FallHeight,
                        FallTime = prevFrame.MovementInfoPacket.FallTime,
                        FootRotation = Quaternion.AngleAxis(prevFrame.MovementInfoPacket.FootRotation, Vector3.up),
                        HeadRotation = prevFrame.MovementInfoPacket.HeadRotation,
                        IsGrounded = prevFrame.MovementInfoPacket.IsGrounded,
                        JumpHeight = prevFrame.MovementInfoPacket.JumpHeight,
                        MaxSpeed = prevFrame.MovementInfoPacket.MaxSpeed,
                        MovementDirection = prevFrame.MovementInfoPacket.MovementDirection,
                        MovementSpeed = prevFrame.MovementInfoPacket.CharacterMovementSpeed,
                        PhysicalCondition = prevFrame.MovementInfoPacket.PhysicalCondition,
                        Pose = prevFrame.MovementInfoPacket.Pose,
                        PoseLevel = prevFrame.MovementInfoPacket.PoseLevel,
                        SprintSpeed = prevFrame.MovementInfoPacket.SprintSpeed,
                        State = prevFrame.MovementInfoPacket.State,
                        StateAnimatorIndex = prevFrame.MovementInfoPacket.AnimatorStateIndex,
                        Step = prevFrame.MovementInfoPacket.Step,
                        Tilt = prevFrame.MovementInfoPacket.Tilt,
                        Velocity = prevFrame.MovementInfoPacket.Velocity
                    },
                    Commands = [.. prevFrame.Commands],
                    CommandsCount = prevFrame.Commands.Count
                    
                };

                CoopGame.TestController.Apply(nextModel);
                CoopGame.TestController.ManualUpdate();
                prevFrame = default;
            }
            else
            {
                Logger.LogInfo("LOOKHERE: Null");
            }
        }

    //    Movement = new ()
    //                {
    //                    BodyPosition = new Vector3(x: Position.x + 2, y: Position.y, z: Position.z),
    //                    HeadRotation = HeadRotation,
    //                    MovementDirection = new Vector2(MovementContext.MovementDirection.x* -1, MovementContext.MovementDirection.y* -1),
    //                    Velocity = Velocity,
    //                    Tilt = MovementContext.Tilt,
    //                    Step = MovementContext.Step,
    //                    BlindFire = MovementContext.BlindFire,
    //                    StateAnimatorIndex = CurrentAnimatorStateIndex,
    //                    State = CurrentManagedState.Name,
    //                    PhysicalCondition = MovementContext.PhysicalCondition,
    //                    MovementSpeed = MovementContext.CharacterMovementSpeed,
    //                    SprintSpeed = MovementContext.SprintSpeed,
    //                    MaxSpeed = MovementContext.MaxSpeed,
    //                    Pose = Pose,
    //                    PoseLevel = PoseLevel,
    //                    InHandsObjectOverlap = 1f,
    //                    IsGrounded = MovementContext.IsGrounded,
    //                    JumpHeight = MovementContext.JumpHeight,
    //                    FallHeight = MovementContext.FallHeight,
    //                    FallTime = MovementContext.FreefallTime,
    //                    AimRotation = Rotation.y,
    //                    FootRotation = Quaternion.AngleAxis(Rotation.x, Vector3.up)
    //}


    public override void OnDestroy()
        {
            BepInLogger.LogDebug("OnDestroy()");
            base.OnDestroy();
        }

    }
}
