using BepInEx.Logging;
using Comfort.Common;
using EFT;
using EFT.HealthSystem;
using EFT.Interactive;
using EFT.InventoryLogic;
using RootMotion.FinalIK;
using StayInTarkov.Coop.Matchmaker;
using StayInTarkov.Coop.Player;
using StayInTarkov.Coop.Web;
using StayInTarkov.Core.Player;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace StayInTarkov.Coop
{
    internal class CoopPlayer : LocalPlayer
    {
        public static NetworkPacket.Lacyway.PrevFrame prevFrame = new();
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
                ? new PlayerInventoryController(player, profile, true)
                : new InventoryController(profile, true);

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

        //public override void ApplyDamageInfo(DamageInfo damageInfo, EBodyPart bodyPartType, float absorbed, EHeadSegment? headSegment = null)
        //{
        //    // Quick check?
        //    if (PreviousDamageInfos.Any(x =>
        //        x.Damage == damageInfo.Damage
        //        && x.SourceId == damageInfo.SourceId
        //        && x.Weapon != null && damageInfo.Weapon != null && x.Weapon.Id == damageInfo.Weapon.Id
        //        && x.Player != null && damageInfo.Player != null && x.Player == damageInfo.Player
        //        ))
        //        return;

        //    PreviousDamageInfos.Add(damageInfo);

        //    //BepInLogger.LogInfo($"{nameof(ApplyDamageInfo)}:{this.ProfileId}:{DateTime.Now.ToString("T")}");
        //    //base.ApplyDamageInfo(damageInfo, bodyPartType, absorbed, headSegment);

        //    if (CoopGameComponent.TryGetCoopGameComponent(out var coopGameComponent))
        //    {
        //        // If we are not using the Client Side Damage, then only run this on the server
        //        if (MatchmakerAcceptPatches.IsServer && !coopGameComponent.SITConfig.useClientSideDamageModel)
        //            SendDamageToAllClients(damageInfo, bodyPartType, absorbed, headSegment);
        //        else
        //            SendDamageToAllClients(damageInfo, bodyPartType, absorbed, headSegment);
        //    }
        //}



        //private void SendDamageToAllClients(DamageInfo damageInfo, EBodyPart bodyPartType, float absorbed, EHeadSegment? headSegment = null)
        //{
        //    Dictionary<string, object> packet = new();
        //    var bodyPartColliderType = ((BodyPartCollider)damageInfo.HittedBallisticCollider).BodyPartColliderType;
        //    damageInfo.HitCollider = null;
        //    damageInfo.HittedBallisticCollider = null;
        //    Dictionary<string, string> playerDict = new();
        //    if (damageInfo.Player != null)
        //    {
        //        playerDict.Add("d.p.aid", damageInfo.Player.iPlayer.Profile.AccountId);
        //        playerDict.Add("d.p.id", damageInfo.Player.iPlayer.ProfileId);
        //    }

        //    damageInfo.Player = null;
        //    Dictionary<string, string> weaponDict = new();

        //    if (damageInfo.Weapon != null)
        //    {
        //        packet.Add("d.w.tpl", damageInfo.Weapon.TemplateId);
        //        packet.Add("d.w.id", damageInfo.Weapon.Id);
        //    }
        //    damageInfo.Weapon = null;

        //    packet.Add("d", damageInfo.SITToJson());
        //    packet.Add("d.p", playerDict);
        //    packet.Add("d.w", weaponDict);
        //    packet.Add("bpt", bodyPartType.ToString());
        //    packet.Add("bpct", bodyPartColliderType.ToString());
        //    packet.Add("ab", absorbed.ToString());
        //    packet.Add("hs", headSegment.ToString());
        //    packet.Add("m", "ApplyDamageInfo");

        //    // -----------------------------------------------------------
        //    // An attempt to stop the same packet being sent multiple times
        //    if (PreviousSentDamageInfoPackets.Contains(packet.ToJson()))
        //        return;

        //    PreviousSentDamageInfoPackets.Add(packet.ToJson());
        //    // -----------------------------------------------------------

        //    AkiBackendCommunicationCoop.PostLocalPlayerData(this, packet, true);
        //}

        //public void ReceiveDamageFromServer(Dictionary<string, object> dict)
        //{
        //    StartCoroutine(ReceiveDamageFromServerCR(dict));
        //}

        //public IEnumerator ReceiveDamageFromServerCR(Dictionary<string, object> dict)
        //{
        //    if (PreviousReceivedDamageInfoPackets.Contains(dict.ToJson()))
        //        yield break;

        //    PreviousReceivedDamageInfoPackets.Add(dict.ToJson());

        //    //BepInLogger.LogDebug("ReceiveDamageFromServer");
        //    //BepInLogger.LogDebug(dict.ToJson());

        //    Enum.TryParse<EBodyPart>(dict["bpt"].ToString(), out var bodyPartType);
        //    Enum.TryParse<EHeadSegment>(dict["hs"].ToString(), out var headSegment);
        //    var absorbed = float.Parse(dict["ab"].ToString());

        //    var damageInfo = Player_ApplyShot_Patch.BuildDamageInfoFromPacket(dict);
        //    damageInfo.HitCollider = Player_ApplyShot_Patch.GetCollider(this, damageInfo.BodyPartColliderType);

        //    if (damageInfo.DamageType == EDamageType.Bullet && IsYourPlayer)
        //    {
        //        float handsShake = 0.05f;
        //        float cameraShake = 0.4f;
        //        float absorbedDamage = absorbed + damageInfo.Damage;

        //        switch (bodyPartType)
        //        {
        //            case EBodyPart.Head:
        //                handsShake = 0.1f;
        //                cameraShake = 1.3f;
        //                break;
        //            case EBodyPart.LeftArm:
        //            case EBodyPart.RightArm:
        //                handsShake = 0.15f;
        //                cameraShake = 0.5f;
        //                break;
        //            case EBodyPart.LeftLeg:
        //            case EBodyPart.RightLeg:
        //                cameraShake = 0.3f;
        //                break;
        //        }

        //        ProceduralWeaponAnimation.ForceReact.AddForce(Mathf.Sqrt(absorbedDamage) / 10, handsShake, cameraShake);
        //        if (FPSCamera.Instance.EffectsController.TryGetComponent(out FastBlur fastBlur))
        //        {
        //            fastBlur.enabled = true;
        //            fastBlur.Hit(MovementContext.PhysicalConditionIs(EPhysicalCondition.OnPainkillers) ? absorbedDamage : (bodyPartType == EBodyPart.Head ? absorbedDamage * 6 : absorbedDamage * 3));
        //        }
        //    }

        //    base.ApplyDamageInfo(damageInfo, bodyPartType, absorbed, headSegment);
        //    //base.ShotReactions(damageInfo, bodyPartType);

        //    yield break;

        //}

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
            float force = EFTHardSettings.Instance.HIT_FORCE *= (0.3f + 0.7f * Mathf.InverseLerp(50f, 20f, LastDamageInfo.PenetrationPower));

            AddCommand(new DeathCommand()
            {
                CorpseImpulse = new()
                {
                    BodyPartColliderType = LastDamageInfo.BodyPartColliderType,
                    Direction = LastDamageInfo.Direction,
                    Force = force,
                    Point = LastDamageInfo.HitPoint,
                    OverallVelocity = LastDamageInfo.Direction
                },
                Inventory = Inventory,
                LastDamagedBodyPart = LastDamagedBodyPart,
                LastDamageType = LastDamageInfo.DamageType
            });

            return base.CreateCorpse();
        }

        public override void OnBeenKilledByAggressor(IAIDetails aggressor, DamageInfo damageInfo, EBodyPart bodyPart, EDamageType lethalDamageType)
        {
            base.OnBeenKilledByAggressor(aggressor, damageInfo, bodyPart, lethalDamageType);
        }

        public override void OnItemAddedOrRemoved(Item item, ItemAddress location, bool added)
        {
            base.OnItemAddedOrRemoved(item, location, added);
        }

        //public override void Move(Vector2 direction)
        //{
        //    base.Move(direction);

        //    //var prc = GetComponent<PlayerReplicatedComponent>();
        //    //if (prc.IsClientDrone)
        //    //    return;
        //}


        public override void SendHeadlightsPacket(bool isSilent)
        {
            LightsStates[] lightStates = _helmetLightControllers.Select(new Func<TacticalComboVisualController, LightsStates>(ClientPlayer.Class1383.class1383_0.method_0)).ToArray();
            if (lightStates.Length > 0)
            {
                foreach (var light in lightStates)
                {
                    AddCommand(new Command()
                    {
                        SetSilently = false,
                        ID = light.Id,
                        LightMode = light.LightMode,
                        State = light.IsActive
                    });
                }
            }
        }

        public override void OnPhraseTold(EPhraseTrigger @event, TaggedClip clip, TagBank bank, Speaker speaker)
        {
            base.OnPhraseTold(@event, clip, bank, speaker);
            AddCommand(new PhraseCommandMessage()
            {
                PhraseCommand = @event,
                PhraseId = clip.NetId
            });
        }

        public override void Proceed(bool withNetwork, Callback<IHandsController0> callback, bool scheduled = true)
        {
            base.Proceed(withNetwork, callback, scheduled);

            AddCommand(new HandsController2()
            {
                HandControllerType = EHandsControllerType.Empty,
                DrawAnimationSpeedMultiplier = 1
            });
        }

        public override void Proceed(Weapon weapon, Callback<IFirearmHandsController> callback, bool scheduled = true)
        {
            base.Proceed(weapon, callback, scheduled);

            bool fastHide = false;
            FirearmController firearmController;
            if ((firearmController = _handsController as FirearmController) != null)
            {
                fastHide = firearmController.CheckForFastWeaponSwitch(weapon);
            }

            var Components = Singleton<ItemFactory>.Instance.ItemToComponentialItem(weapon);

            AddCommand(new HandsController2()
            {
                FastHide = fastHide,
                Armed = weapon.Armed,
                HandControllerType = EHandsControllerType.Firearm,
                Item = Components,
                DrawAnimationSpeedMultiplier = 1
            });
        }

        public override void Proceed(ThrowWeap throwWeap, Callback<IThrowableCallback> callback, bool scheduled = true)
        {
            base.Proceed(throwWeap, callback, scheduled);

            var Components = Singleton<ItemFactory>.Instance.ItemToComponentialItem(throwWeap);

            AddCommand(new HandsController2()
            {
                HandControllerType = EHandsControllerType.Grenade,
                Item = Components,
                FastHide = true,
                DrawAnimationSpeedMultiplier = 1
            });
        }

        public override void Proceed(Meds0 meds, EBodyPart bodyPart, Callback<IHandsController5> callback, int animationVariant, bool scheduled = true)
        {
            base.Proceed(meds, bodyPart, callback, animationVariant, scheduled);

            var Components = Singleton<ItemFactory>.Instance.ItemToComponentialItem(meds);

            AddCommand(new HandsController2()
            {
                HandControllerType = EHandsControllerType.Meds,
                Item = Components,
                DrawAnimationSpeedMultiplier = 1
            });


        }

        public override void Proceed(KnifeComponent knife, Callback<IHandsController7> callback, bool scheduled = true)
        {
            base.Proceed(knife, callback, scheduled);

            var Components = Singleton<ItemFactory>.Instance.ItemToComponentialItem(knife.Item);

            AddCommand(new HandsController2()
            {
                HandControllerType = EHandsControllerType.Knife,
                Item = Components,
                FastHide = true,
                DrawAnimationSpeedMultiplier = 1
            });
        }

        public override void Proceed(KnifeComponent knife, Callback<IKnifeController> callback, bool scheduled = true)
        {
            base.Proceed(knife, callback, scheduled);

            var Components = Singleton<ItemFactory>.Instance.ItemToComponentialItem(knife.Item);

            AddCommand(new HandsController2()
            {
                HandControllerType = EHandsControllerType.Knife,
                Item = Components,
                DrawAnimationSpeedMultiplier = 1
            });
        }

        public override void Proceed<T>(Item item, Callback<IHandsController4> callback, bool scheduled = true)
        {
            base.Proceed<T>(item, callback, scheduled);

            var Components = Singleton<ItemFactory>.Instance.ItemToComponentialItem(item);

            AddCommand(new HandsController2()
            {
                HandControllerType = EHandsControllerType.UsableItem,
                Item = Components,
                DrawAnimationSpeedMultiplier = 1
            });
        }

        public override void Proceed(Item item, Callback<IQuickUseController> callback, bool scheduled = true)
        {
            base.Proceed(item, callback, scheduled);

            var Components = Singleton<ItemFactory>.Instance.ItemToComponentialItem(item);

            AddCommand(new HandsController2()
            {
                HandControllerType = EHandsControllerType.QuickUseItem,
                Item = Components,
                DrawAnimationSpeedMultiplier = 1
            });
        }

        public override void Proceed(FoodDrink foodDrink, float amount, Callback<IHandsController5> callback, int animationVariant, bool scheduled = true)
        {
            base.Proceed(foodDrink, amount, callback, animationVariant, scheduled);

            var Components = Singleton<ItemFactory>.Instance.ItemToComponentialItem(foodDrink);

            AddCommand(new HandsController2()
            {
                HandControllerType = EHandsControllerType.Meds,
                Item = Components,
                DrawAnimationSpeedMultiplier = 1
            });
        }

        public override void Proceed(ThrowWeap throwWeap, Callback<IGrenadeQuickUseController> callback, bool scheduled = true)
        {
            base.Proceed(throwWeap, callback, scheduled);

            var Components = Singleton<ItemFactory>.Instance.ItemToComponentialItem(throwWeap);

            AddCommand(new HandsController2()
            {
                HandControllerType = EHandsControllerType.QuickGrenade,
                Item = Components,
                DrawAnimationSpeedMultiplier = 1
            });
        }

        public override void vmethod_3(EGesture gesture)
        {
            base.vmethod_3(gesture);
            AddCommand(new GestureCommandMessage()
            {
                Gesture = gesture
            });
        }

        public override void Interact(IItemOwner loot, Callback callback)
        {
            base.Interact(loot, callback);


        }

        //public override void vmethod_0(WorldInteractiveObject interactiveObject, InteractionResult interactionResult, Action callback)
        //{
        //    base.vmethod_0(interactiveObject, interactionResult, callback);

        //    AddCommand(new DoorInteractionMessage()
        //    {
        //        InteractionDoor = interactiveObject.Id,
        //        InteractionDoorKey = (interactionResult is KeyInteractionResult) ? ((KeyInteractionResult)interactionResult).Key.Item.Id : string.Empty,
        //        InteractionDoorResult = false
        //    });
        //}

        //public override void vmethod_1(WorldInteractiveObject door, InteractionResult interactionResult)
        //{
        //    base.vmethod_1(door, interactionResult);

        //    AddCommand(new DoorInteractionMessage()
        //    {
        //        InteractionDoor = door.Id,
        //        InteractionDoorKey = (interactionResult is KeyInteractionResult) ? ((KeyInteractionResult)interactionResult).Key.Item.Id : string.Empty,
        //        InteractionDoorResult = true
        //    });
        //}

        public void AddCommand(ICommand command)
        {
            prevFrame.Commands.Add(command);
        }

        public override void LateUpdate()
        {
            base.LateUpdate();

            prevFrame.MovementInfoPacket = new()
            {
                Position = new Vector3(x: Position.x, y: Position.y, z: Position.z),
                AnimatorStateIndex = CurrentAnimatorStateIndex,
                PoseLevel = MovementContext.SmoothedPoseLevel,
                CharacterMovementSpeed = MovementContext.ClampSpeed(MovementContext.SmoothedCharacterMovementSpeed),
                Tilt = MovementContext.SmoothedTilt,
                Step = MovementContext.Step,
                BlindFire = MovementContext.BlindFire,
                HeadRotation = HeadRotation,
                Stamina = Physical.SerializationStruct,
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
                State = MovementContext.CurrentState.Name,
                Velocity = Velocity,
                WeaponOverlap = ProceduralWeaponAnimation.TurnAway.OverlapValue
            };

            // This needs to be sent during an event instead...
            AddCommand(new PhysicalParametersCommandMessage()
            {
                BreathIsAudible = Physical.BreathIsAudible,
                IsHeavyBreathing = Physical.Exhausted,
                MinStepSound = Physical.MinStepSound,
                Overweight = Physical.Overweight,
                SoundRadius = Physical.SoundRadius,
                TransitionSpeed = Physical.TransitionSpeed,
                WalkOverweight = Physical.WalkOverweight
            });

            if (this != null)
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
                        Velocity = prevFrame.MovementInfoPacket.Velocity,
                        InHandsObjectOverlap = prevFrame.MovementInfoPacket.WeaponOverlap
                    },
                    Commands = [.. prevFrame.Commands],
                    CommandsCount = prevFrame.Commands.Count,
                    RemoteTime = Time.deltaTime
                };

                var test = NetworkPacket.Lacyway.PrevFrame.Serialize(prevFrame.MovementInfoPacket, prevFrame.Commands);

                Dictionary<string, object> dictionary = new()
                {
                    { "model", test.ToJson() },
                    { "m", "test" }
                };

                AkiBackendCommunicationCoop.PostLocalPlayerData(this, dictionary);

                prevFrame.ClearFrame();
            }
            else
            {
                Logger.LogInfo("LOOKHERE: Null");
            }
        }


        public override void OnDestroy()
        {
            //BepInLogger.LogDebug("OnDestroy()");
            base.OnDestroy();
        }

    }
}
