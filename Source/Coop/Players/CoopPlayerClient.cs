using Comfort.Common;
using Diz.LanguageExtensions;
using EFT;
using EFT.Interactive;
using EFT.InventoryLogic;
using EFT.UI;
using StayInTarkov.Coop.Components.CoopGameComponents;
using StayInTarkov.Coop.Controllers;
using StayInTarkov.Coop.Matchmaker;
using StayInTarkov.Coop.NetworkPacket;
using StayInTarkov.Coop.NetworkPacket.Player.Proceed;
using StayInTarkov.Core.Player;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.Remoting.Lifetime;
using UnityEngine;
using static AHealthController<EFT.HealthSystem.ActiveHealthController.AbstractEffect>;

namespace StayInTarkov.Coop.Players
{
    public class CoopPlayerClient : CoopPlayer
    {
        public PlayerStatePacket LastState { get; set; } = new PlayerStatePacket();
        public PlayerStatePacket NewState { get; set; } = new PlayerStatePacket();

        //public override void InitVoip(EVoipState voipState)
        //{
        //    //base.InitVoip(voipState);
        //    SoundSettings settings = Singleton<SettingsManager>.Instance.Sound.Settings;
        //}

        //public override void Move(Vector2 direction)
        //{
        //    //base.Move(direction);
        //}

        public override void OnDead(EDamageType damageType)
        {
            //if (damageType == EDamageType.Fall)
            //    return;

            BepInLogger.LogDebug($"{nameof(CoopPlayerClient)}:{nameof(OnDead)}:{damageType}");
            base.OnDead(damageType);
            Singleton<BetterAudio>.Instance.UnsubscribeProtagonist();
        }

        public override PlayerHitInfo ApplyShot(DamageInfo damageInfo, EBodyPart bodyPartType, EBodyPartColliderType colliderType, EArmorPlateCollider armorPlateCollider, ShotId shotId)
        {
            // Paulov: This creates a server authorative Damage model
            // I am filtering out Bullet from this model (for now)
            if (SITMatchmaking.IsClient && damageInfo.DamageType != EDamageType.Bullet)
            {
                ReceiveDamage(damageInfo.Damage, bodyPartType, damageInfo.DamageType, 0, 0);
                return null;
            }

            BepInLogger.LogDebug($"{nameof(CoopPlayerClient)}:{nameof(ApplyShot)}:{damageInfo.DamageType}");
            return base.ApplyShot(damageInfo, bodyPartType, colliderType, armorPlateCollider, shotId);
        }

        public override void ApplyDamageInfo(DamageInfo damageInfo, EBodyPart bodyPartType, EBodyPartColliderType colliderType, float absorbed)
        {
            BepInLogger.LogDebug($"{nameof(CoopPlayerClient)}:{nameof(ApplyDamageInfo)}:{damageInfo.DamageType}");

            // Paulov: This creates a server authorative Damage model
            // I am filtering out Bullet from this model (for now)
            if (SITMatchmaking.IsClient && damageInfo.DamageType != EDamageType.Bullet)
                return;

            base.ApplyDamageInfo(damageInfo, bodyPartType, colliderType, absorbed);
        }

        public override void OnHealthEffectAdded(IEffect effect)
        {
            BepInLogger.LogDebug($"{nameof(CoopPlayerClient)}:{nameof(OnHealthEffectAdded)}");
        }

        public override void OnHealthEffectRemoved(IEffect effect)
        {
            BepInLogger.LogDebug($"{nameof(CoopPlayerClient)}:{nameof(OnHealthEffectRemoved)}");
        }

        public override void KillMe(EBodyPartColliderType colliderType, float damage)
        {
            BepInLogger.LogDebug($"{nameof(CoopPlayerClient)}:{nameof(KillMe)}");
        }

        DateTime? LastRPSP = null;

        public override void ReceivePlayerStatePacket(PlayerStatePacket playerStatePacket)
        {
            NewState = playerStatePacket;
            //BepInLogger.LogInfo($"{nameof(ReceivePlayerStatePacket)}:Packet took {DateTime.Now - new DateTime(long.Parse(NewState.TimeSerializedBetter))}.");
            if (CoopGameComponent.TryGetCoopGameComponent(out var coopGameComponent))
            {
                var ms = (DateTime.Now - new DateTime(long.Parse(NewState.TimeSerializedBetter))).Milliseconds;
                coopGameComponent.ServerPingSmooth.Enqueue(ms);
            }
            

            //BepInLogger.LogInfo(NewState.ToJson());


            if (LastRPSP == null)
                LastRPSP = DateTime.Now;

            //BepInLogger.LogInfo($"Time between {nameof(ReceivePlayerStatePacket)} {DateTime.Now - LastRPSP.Value}");

            LastRPSP = DateTime.Now;
        }

        public Queue<ISITPacket> ReceivedPackets = new Queue<ISITPacket>();

        void Update()
        {
            // Run through the Received Packets and Apply the action
            while (ReceivedPackets.Count > 0)
            {
                var packet = ReceivedPackets.Dequeue();
                if (packet is PlayerProceedFoodDrinkPacket foodDrinkPacket)
                {
                    if (ItemFinder.TryFindItem(foodDrinkPacket.ItemId, out Item item) && item is FoodClass foodDrink)
                    {
                        Proceed(foodDrink, foodDrinkPacket.Amount, null, foodDrinkPacket.AnimationVariant, foodDrinkPacket.Scheduled);
                    }
                }
                if (packet is PlayerProceedMedsPacket medsPacket)
                {
                    if (ItemFinder.TryFindItem(medsPacket.ItemId, out Item item) && item is MedsClass meds)
                    {
                        Proceed(meds, medsPacket.BodyPart, null, medsPacket.AnimationVariant, medsPacket.Scheduled);
                    }
                }
            }

            // Update the Health parts of this character using the packets from the Player State
            if (NewState != null)
            {
                var bodyPartDictionary = GetBodyPartDictionary(this);
                if (bodyPartDictionary != null)
                {
                    //BepInLogger.LogInfo(bodyPartDictionary.ToJson());
                    if (NewState.PlayerHealth != null)
                    {
                        foreach (var bodyPartPacket in NewState.PlayerHealth.BodyParts)
                        {
                            if (bodyPartPacket.BodyPart == EBodyPart.Common)
                                continue;

                            if (bodyPartDictionary.ContainsKey(bodyPartPacket.BodyPart))
                            {
                                //BepInLogger.LogInfo($"{nameof(Update)} set bodyPart current {bodyPartPacket.ToJson()}");
                                bodyPartDictionary[bodyPartPacket.BodyPart].Health.Current = bodyPartPacket.Current;
                            }
                            else
                            {
                                //BepInLogger.LogError($"{nameof(CoopPlayerClient)}:Unable to find {bodyPartPacket.BodyPart} in BodyPartDictionary {bodyPartDictionary.Keys.ToJson()}");
                            }
                        }
                    }
                    else
                    {
                        BepInLogger.LogDebug($"{nameof(CoopPlayerClient)}:{nameof(NewState.PlayerHealth)} is null");
                    }
                }
                else
                {
                    BepInLogger.LogError($"{nameof(CoopPlayerClient)}:Unable to obtain BodyPartDictionary");
                }
            }
        }

        private Dictionary<EBodyPart, BodyPartState> GetBodyPartDictionary(EFT.Player player)
        {
            try
            {
                var bodyPartDict
                = ReflectionHelpers.GetFieldOrPropertyFromInstance<Dictionary<EBodyPart, BodyPartState>>
                (player.PlayerHealthController, "Dictionary_0", false);
                if (bodyPartDict == null)
                {
                    Logger.LogError($"Could not retreive {player.ProfileId}'s Health State Dictionary");
                    return null;
                }
                //Logger.LogInfo(bodyPartDict.ToJson());
                return bodyPartDict;
            }
            catch (Exception)
            {

            }

            return null;
        }

        new void LateUpdate()
        {
            if (HealthController != null && HealthController.IsAlive)
            {
                VisualPass();
                _armsupdated = false;
                _bodyupdated = false;
                if (_nFixedFrames > 0)
                {
                    _nFixedFrames = 0;
                    _fixedTime = 0f;
                }
                ProceduralWeaponAnimation.StartFovCoroutine(this);
                PropUpdate();
            }
            ComplexLateUpdate(EUpdateQueue.Update, DeltaTime);

            if (LastState == null)
                return;

            if (LastState.LinearSpeed > 0.25)
            {
                Move(LastState.InputDirection);
            }

            ///
            // Paulov: NOTE
            // AnimatorStatesLateUpdate applies a "scheduled motion" and any "platform motion" to the character. Clients do not need this motion applied via this logic.
            //if (MovementContext != null)
            //{
            //    MovementContext?.AnimatorStatesLateUpdate();
            //}
            ApplyReplicatedMotion();
        }

        /// <summary>
        /// Created by: Lacyway - This code has been written by Lacyway (https://github.com/Lacyway) for the SIT Project (https://github.com/stayintarkov/StayInTarkov.Client).
        /// Updated by: Paulov
        /// </summary>
        protected override void Interpolate()
        {
            //BepInLogger.LogInfo(nameof(Interpolate));

            if (HealthController == null || !HealthController.IsAlive)
                return;

            if (MovementContext == null)
                return;

            var InterpolationRatio = Time.deltaTime * 5;

            Rotation = new Vector2(Mathf.LerpAngle(Yaw, NewState.Rotation.x, InterpolationRatio), Mathf.Lerp(Pitch, NewState.Rotation.y, InterpolationRatio));

            HeadRotation = Vector3.Lerp(HeadRotation, NewState.HeadRotation, InterpolationRatio);
            ProceduralWeaponAnimation.SetHeadRotation(Vector3.Lerp(LastState.HeadRotation, NewState.HeadRotation, InterpolationRatio));
            MovementContext.PlayerAnimatorSetMovementDirection(Vector2.Lerp(LastState.MovementDirection, NewState.MovementDirection, Time.deltaTime));
            MovementContext.PlayerAnimatorSetDiscreteDirection(BSGDirectionalHelpers.ConvertToMovementDirection(NewState.MovementDirection));

            EPlayerState currentPlayerState = MovementContext.CurrentState.Name;
            EPlayerState eplayerState = NewState.State;

            if (eplayerState == EPlayerState.ClimbUp || eplayerState == EPlayerState.ClimbOver || eplayerState == EPlayerState.VaultingLanding || eplayerState == EPlayerState.VaultingFallDown)
            {
                Vaulting();
            }

            if (eplayerState == EPlayerState.Jump)
            {
                Jump();
            }
            if (currentPlayerState == EPlayerState.Jump && eplayerState != EPlayerState.Jump)
            {
                MovementContext.PlayerAnimatorEnableJump(false);
                MovementContext.PlayerAnimatorEnableLanding(true);
            }
            if ((currentPlayerState == EPlayerState.ProneIdle || currentPlayerState == EPlayerState.ProneMove) && eplayerState != EPlayerState.ProneMove && eplayerState != EPlayerState.Transit2Prone && eplayerState != EPlayerState.ProneIdle)
            {
                MovementContext.IsInPronePose = false;
            }
            if ((eplayerState == EPlayerState.ProneIdle || eplayerState == EPlayerState.ProneMove) && currentPlayerState != EPlayerState.ProneMove && currentPlayerState != EPlayerState.Prone2Stand && currentPlayerState != EPlayerState.Transit2Prone && currentPlayerState != EPlayerState.ProneIdle)
            {
                MovementContext.IsInPronePose = true;
            }

            Physical.SerializationStruct = NewState.Stamina;
            MovementContext.SetTilt(Mathf.Round(NewState.Tilt)); // Round the float due to byte converting error...
            CurrentManagedState.SetStep(NewState.Step);
            MovementContext.PlayerAnimatorEnableSprint(NewState.IsSprinting);
            MovementContext.EnableSprint(NewState.IsSprinting);
            MovementContext.LeftStanceController.SetLeftStanceForce(NewState.LeftStance);
            MovementContext.IsInPronePose = NewState.IsProne;
            MovementContext.SetPoseLevel(Mathf.Lerp(LastState.PoseLevel, NewState.PoseLevel, InterpolationRatio));

            MovementContext.SetCurrentClientAnimatorStateIndex(NewState.AnimatorStateIndex);
            MovementContext.SetCharacterMovementSpeed(Mathf.Lerp(LastState.CharacterMovementSpeed, NewState.CharacterMovementSpeed, InterpolationRatio));
            MovementContext.PlayerAnimatorSetCharacterMovementSpeed(Mathf.Lerp(LastState.CharacterMovementSpeed, NewState.CharacterMovementSpeed, InterpolationRatio));

            MovementContext.SetBlindFire(NewState.Blindfire);

            ApplyReplicatedMotion();

            LastState = NewState;
            //BepInLogger.LogInfo($"{nameof(Interpolate)}:End");
        }

        private void ApplyReplicatedMotion()
        {
            if (HealthController == null || !HealthController.IsAlive)
                return;

            if (MovementContext == null) return;

            if (NewState == null) return;

            if (LastState == null) return;

            Vector3 lerpedMovement = Vector3.Lerp(MovementContext.TransformPosition, NewState.Position, Time.deltaTime * 1.33f);
            CharacterController.Move((lerpedMovement + MovementContext.PlatformMotion) - MovementContext.TransformPosition, Time.deltaTime);

            if (!IsInventoryOpened && LastState.LinearSpeed > 0.25)
            {
                Move(LastState.InputDirection);
            }
        }

        public override void UpdateTick()
        {
            base.UpdateTick();

            Interpolate();
        }

        public override void OnSkillExperienceChanged(AbstractSkill skill)
        {
        }

        protected override void OnSkillLevelChanged(AbstractSkill skill)
        {
        }

        protected override void OnWeaponMastered(MasterSkill masterSkill)
        {
        }


        public override void StartInflictSelfDamageCoroutine()
        {
        }

        public override void AddStateSpeedLimit(float speedDelta, ESpeedLimit cause)
        {
        }

        public override void UpdateSpeedLimit(float speedDelta, ESpeedLimit cause)
        {
        }

        public override void UpdateSpeedLimitByHealth()
        {
        }

        public override void UpdateArmsCondition()
        {
        }

        private Item LastUsedItem = null;

        public override void Proceed(FoodClass foodDrink, float amount, Callback<IMedsController> callback, int animationVariant, bool scheduled = true)
        {
            LastUsedItem = foodDrink;
            BepInLogger.LogDebug($"{nameof(CoopPlayerClient)}:{nameof(Proceed)}:{nameof(foodDrink)}:{amount}");
            Func<SITMedsControllerClient> controllerFactory = () => MedsController.smethod_5<SITMedsControllerClient>(this, foodDrink, EBodyPart.Head, amount, animationVariant);
            new Process<SITMedsControllerClient, IMedsController>(this, controllerFactory, foodDrink).method_0(null, (x) => { LastUsedItem = x.Value.Item; }, false);
        }

        public override void Proceed(MedsClass meds, EBodyPart bodyPart, Callback<IMedsController> callback, int animationVariant, bool scheduled = true)
        {
            LastUsedItem = meds;
            BepInLogger.LogDebug($"{nameof(CoopPlayerClient)}:{nameof(Proceed)}:{nameof(meds)}:{bodyPart}");
            Func<MedsController> controllerFactory = () => MedsController.smethod_5<MedsController>(this, meds, bodyPart, 1f, animationVariant);
            new Process<MedsController, IMedsController>(this, controllerFactory, meds).method_0(null, null, false);
        }

        public override void DropCurrentController(Action callback, bool fastDrop, Item nextControllerItem = null)
        {
            BepInLogger.LogDebug($"{nameof(CoopPlayerClient)}:{nameof(DropCurrentController)}");

            BepInLogger.LogDebug($"{nameof(DropCurrentController)}:{nameof(LastUsedItem)}:{LastUsedItem}");
            if (LastUsedItem != null)
            {
                if(LastUsedItem is FoodClass foodClass)
                {
                    BepInLogger.LogDebug("Last used item is food class");
                }
                if(LastUsedItem is MedsClass medClass)
                {
                    BepInLogger.LogDebug("Last used item is med class");
                }


                LastUsedItem = null;
            }

            base.DropCurrentController(callback, fastDrop, nextControllerItem);

        }


        public bool RemoveItem(Item item)
        {
            TraderControllerClass traderControllerClass = this._inventoryController;
            IOperationResult value;
            Error error;

            try
            {
                if (item.StackObjectsCount > 1)
                {
                    global::SOperationResult12<GIOperationResult1> sOperationResult = ItemMovementHandler.SplitToNowhere(item, 1, traderControllerClass, traderControllerClass, simulate: false);
                    value = sOperationResult.Value;
                    error = sOperationResult.Error;
                }
                else
                {
                    global::SOperationResult12<DiscardResult> sOperationResult2 = ItemMovementHandler.Discard(item, traderControllerClass, false, true);
                    value = sOperationResult2.Value;
                    error = sOperationResult2.Error;
                }
                if (error != null)
                {
                    BepInLogger.LogError($"Couldn't remove item: {error}");
                    return false;
                }
                value.RaiseEvents(traderControllerClass, CommandStatus.Begin);
                value.RaiseEvents(traderControllerClass, CommandStatus.Succeed);
            }
            catch (Exception)
            {

            }
            return true;
        }


        

    }
}
