using BepInEx.Logging;
using Comfort.Common;
using Diz.LanguageExtensions;
using EFT;
using EFT.Interactive;
using EFT.InventoryLogic;
using GPUInstancer;
using StayInTarkov.Coop.Components.CoopGameComponents;
using StayInTarkov.Coop.Controllers;
using StayInTarkov.Coop.Controllers.HandControllers;
using StayInTarkov.Coop.Matchmaker;
using StayInTarkov.Coop.NetworkPacket;
using StayInTarkov.Coop.NetworkPacket.Player;
using StayInTarkov.Coop.NetworkPacket.Player.Proceed;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Security.AccessControl;
using UnityEngine;
using UnityEngine.Networking;

namespace StayInTarkov.Coop.Players
{
    public class CoopPlayerClient : CoopPlayer
    {
        public override ManualLogSource BepInLogger { get; } = BepInEx.Logging.Logger.CreateLogSource(nameof(CoopPlayerClient));

        public PlayerStatePacket LastState { get; set; }
        public PlayerStatePacket NewState { get; set; }

        public ConcurrentQueue<PlayerPostProceedDataSyncPacket> ReplicatedPostProceedData { get; } = new();

        protected AbstractHealth NetworkHealthController => base.HealthController as AbstractHealth;

        public override void OnDead(EDamageType damageType)
        {
#if DEBUG
            BepInLogger.LogDebug($"{nameof(CoopPlayerClient)}:{nameof(OnDead)}:{damageType}");
#endif
            base.OnDead(damageType);
        }

        public override ApplyShot ApplyShot(DamageInfo damageInfo, EBodyPart bodyPartType, EBodyPartColliderType colliderType, EArmorPlateCollider armorPlateCollider, ShotId shotId)
        {
#if DEBUGDAMAGE
            BepInLogger.LogDebug($"{nameof(CoopPlayerClient)}:{nameof(ApplyShot)}:{damageInfo.DamageType}");
#endif

            return base.ApplyShot(damageInfo, bodyPartType, colliderType, armorPlateCollider, shotId);
        }

        public override void ApplyDamageInfo(DamageInfo damageInfo, EBodyPart bodyPartType, EBodyPartColliderType colliderType, float absorbed)
        {
#if DEBUGDAMAGE
            BepInLogger.LogDebug($"{nameof(CoopPlayerClient)}:{nameof(ApplyDamageInfo)}:{damageInfo.DamageType}");
#endif

            if (!SITGameComponent.TryGetCoopGameComponent(out var coopGameComponent))
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

            if (LastRPSP == null)
                LastRPSP = DateTime.Now;

            LastRPSP = DateTime.Now;
        }

        public Queue<ISITPacket> ReceivedPackets = new Queue<ISITPacket>();

        void Update()
        {
            // Run through the Received Packets and Apply the action
            while (ReceivedPackets.Count > 0)
            {
                var packet = ReceivedPackets.Dequeue();
                BepInLogger.LogDebug($"{nameof(Update)}:{nameof(ReceivedPackets)}:Dequeue:{packet.GetType().Name}");

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
            UpdatePlayerHealthByPlayerState();
        }

        private void UpdatePlayerHealthByPlayerState()
        {
            if (NewState == null)
                return;

            if (NewState.PlayerHealth == null)
                return;

            var bodyPartDictionary = GetBodyPartDictionary(this);
            if (bodyPartDictionary == null)
            {
                BepInLogger.LogError($"{nameof(CoopPlayerClient)}:Unable to obtain BodyPartDictionary");
                return;
            }

            foreach (var bodyPartPacket in NewState.PlayerHealth.BodyParts)
            {
                if (bodyPartPacket.BodyPart == EBodyPart.Common)
                    continue;

                if (bodyPartDictionary.ContainsKey(bodyPartPacket.BodyPart))
                {
                    bodyPartDictionary[bodyPartPacket.BodyPart].Health.Current = bodyPartPacket.Current;
                }
                else
                {
                }
            }

        }

        private Dictionary<EBodyPart, AHealthController.BodyPartState> GetBodyPartDictionary(EFT.Player player)
        {
            try
            {
                var bodyPartDict
                = ReflectionHelpers.GetFieldOrPropertyFromInstance<Dictionary<EBodyPart, AHealthController.BodyPartState>>
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

        new void FixedUpdate()
        {
            base.FixedUpdate();

            if (FPSCamera.Instance == null)
                return;

            var mainCamera = FPSCamera.Instance.Camera;
            if (mainCamera == null)
            {
                return;
            }

            var startPosition = mainCamera.transform.position + (mainCamera.transform.TransformDirection(Vector3.forward) * 0.1f);

            var headPosition = this.MainParts[BodyPartType.head].Position;
            var dir = (headPosition - mainCamera.transform.position);
            var distanceFromCamera = Vector3.Distance(startPosition, headPosition);

            RaycastHit hit;
            // Does the ray intersect any objects excluding the player layer
            if (Physics.Raycast(startPosition, dir, out hit, Mathf.Infinity, LayerMaskClass.LowPolyColliderLayerMask))
            {
                _isTeleporting = false;

                foreach (var c in this._hitColliders)
                {
                    if (hit.collider == c)
                        return;
                }

                foreach (var c in this._armorPlateColliders)
                {
                    if (hit.collider == c)
                        return;
                }

                var objName = hit.transform.parent?.gameObject?.name;

                if (objName == this.gameObject.name)
                    return;

                if (Vector3.Distance(hit.point, this.Position) < 1f)
                    return;

                if (Vector3.Distance(hit.point, startPosition) > distanceFromCamera)
                    return;

                //if (_raycastHitCube == null)
                //{
                //    _raycastHitCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                //    _raycastHitCube.GetComponent<Collider>().enabled = false;
                //}
                //_raycastHitCube.transform.position = hit.point;

                // If the guy is further than 40m away. Use the Teleportation system.
                if (NewState != null && distanceFromCamera > 40)
                {
                    Teleport(NewState.Position);
                    this.Position = NewState.Position;
                    this.Rotation = NewState.Rotation;
                    //BepInLogger.LogDebug($"Teleporting {ProfileId}");
                    _isTeleporting = true;
                }
            }
            else
            {
                _isTeleporting = false;
            }

            if (!_isTeleporting)
            {
                //GameObject.Destroy(_raycastHitCube);
            }


        }

        bool _isTeleporting = false;
        //GameObject _raycastHitCube;

        public void InterpolateOrTeleport()
        {
            if (!_isTeleporting)
                Interpolate();
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

            if (NewState == null)
                return;

            if (LastState == null)
                LastState = NewState;

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

            InterpolateOrTeleport();
        }

        public override void OnSkillExperienceChanged(AbstractSkill skill)
        {
        }

        public override void OnSkillLevelChanged(AbstractSkill skill)
        {
        }

        public override void OnWeaponMastered(MasterSkill masterSkill)
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

        public override void DropCurrentController(Action callback, bool fastDrop, Item nextControllerItem = null)
        {
            // just use normal
            if (LastUsedItem == null || nextControllerItem == LastUsedItem)
            {
                base.DropCurrentController(callback, fastDrop, nextControllerItem);
                return;
            }

            BepInLogger.LogDebug($"{nameof(CoopPlayerClient)}:{nameof(DropCurrentController)}");

            base.DropCurrentController(callback, fastDrop, nextControllerItem);


            // Sync up Equipment items
            while (ReplicatedPostProceedData.TryDequeue(out var postPostProceedPacket))
            {
                if (ItemFinder.TryFindItem(postPostProceedPacket.ItemId, out Item item))
                {
                    if (item is MedsClass meds)
                    {
                        if (meds.MedKitComponent != null)
                        {
                            meds.MedKitComponent.HpResource = postPostProceedPacket.NewValue;
                            BepInLogger.LogDebug($"{nameof(CoopPlayerClient)}:{nameof(DropCurrentController)}:Updating Item:{item}");
                        }
                    }
                    if (item is FoodClass food)
                    {
                        if (food.FoodDrinkComponent != null)
                        {
                            food.FoodDrinkComponent.HpPercent = postPostProceedPacket.NewValue;
                            BepInLogger.LogDebug($"{nameof(CoopPlayerClient)}:{nameof(DropCurrentController)}:Updating Item:{item}");
                        }
                    }
                    item.StackObjectsCount = postPostProceedPacket.StackObjectsCount;
                    item.RaiseRefreshEvent(true, true);
                }
            }
        }

        public override void ReceiveSay(EPhraseTrigger trigger, int index, ETagStatus mask, bool aggressive)
        {
            BepInLogger.LogDebug($"{nameof(ReceiveSay)}({trigger},{mask})");

            Speaker.PlayDirect(trigger, index);

            ETagStatus eTagStatus = ((!aggressive && !(Awareness > Time.time)) ? ETagStatus.Unaware : ETagStatus.Combat);
            Speaker.Play(trigger, HealthStatus | mask | eTagStatus, true, 100);
        }

        public override void Proceed(Weapon weapon, Callback<IFirearmHandsController> callback, bool scheduled = true)
        {
            if (_handsController != null && _handsController.Item == weapon)
                return;

            Func<SITFirearmControllerClient> controllerFactory = (Func<SITFirearmControllerClient>)(() => FirearmController.smethod_5<SITFirearmControllerClient>(this, weapon));
            bool fastHide = true;
            if (_handsController is SITFirearmControllerClient firearmController)
                firearmController.ClearPreWarmOperationsDict();

            new Process<SITFirearmControllerClient, IFirearmHandsController>(this, controllerFactory, weapon, fastHide).method_0(null, callback, scheduled);
        }

        public override void Proceed(KnifeComponent knife, Callback<IKnifeController> callback, bool scheduled = true)
        {
            Func<SITKnifeControllerClient> controllerFactory = () => KnifeController.smethod_8<SITKnifeControllerClient>(this, knife);
            new Process<SITKnifeControllerClient, IKnifeController>(this, controllerFactory, knife.Item, fastHide: true).method_0(null, callback, scheduled);
        }

        public override void Proceed(KnifeComponent knife, Callback<IQuickKnifeKickController> callback, bool scheduled = true)
        {
            Func<QuickKnifeKickController> controllerFactory = () => QuickKnifeKickController.smethod_8<QuickKnifeKickController>(this, knife);
            Process<QuickKnifeKickController, IQuickKnifeKickController> process = new Process<QuickKnifeKickController, IQuickKnifeKickController>(this, controllerFactory, knife.Item, fastHide: true, AbstractProcess.Completion.Sync, AbstractProcess.Confirmation.Succeed, skippable: false);
            Action confirmCallback = delegate
            {

            };
            process.method_0(delegate (IResult result)
            {
                if (result.Succeed)
                {

                }
            }, callback, scheduled);
        }

        public override void Proceed(GrenadeClass throwWeap, Callback<IThrowableCallback> callback, bool scheduled = true)
        {
            Func<GrenadeController> controllerFactory = () => GrenadeController.smethod_8<GrenadeController>(this, throwWeap);
            new Process<GrenadeController, IThrowableCallback>(this, controllerFactory, throwWeap).method_0(null, callback, scheduled);
        }


        public override void Proceed(bool withNetwork, Callback<IGIController1> callback, bool scheduled = true)
        {
            Func<EmptyHandsController> controllerFactory = () => EmptyHandsController.smethod_5<EmptyHandsController>(this);
            new Process<EmptyHandsController, IGIController1>(this, controllerFactory, null).method_0(null, callback, scheduled);
        }


        public override void Proceed(FoodClass foodDrink, float amount, Callback<IMedsController> callback, int animationVariant, bool scheduled = true)
        {
            Func<MedsController> controllerFactory = () => MedsController.smethod_5<MedsController>(this, foodDrink, EBodyPart.Head, amount, animationVariant);
            new Process<MedsController, IMedsController>(this, controllerFactory, foodDrink).method_0(null, callback, scheduled);
        }


        public override void vmethod_0(WorldInteractiveObject interactiveObject, InteractionResult interactionResult, Action callback)
        {
            EInteractionType interactionType = interactionResult.InteractionType;
            BepInLogger.LogDebug($"interact with door, interaction type {interactionType}");
            CurrentManagedState.StartDoorInteraction(interactiveObject, interactionResult, callback);
            UpdateInteractionCast();
        }

        public override void vmethod_1(WorldInteractiveObject door, InteractionResult interactionResult)
        {
            if (!(door == null))
            {
                CurrentManagedState.ExecuteDoorInteraction(door, interactionResult, null, this);
            }
        }
    }
}
