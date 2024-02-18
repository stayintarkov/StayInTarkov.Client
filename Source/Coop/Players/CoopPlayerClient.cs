using Comfort.Common;
using EFT;
using EFT.Interactive;
using StayInTarkov.Coop.Components.CoopGameComponents;
using StayInTarkov.Coop.NetworkPacket;
using StayInTarkov.Core.Player;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Lifetime;
using UnityEngine;

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

        void Update()
        {
            //BepInLogger.LogDebug("Update");

            //var prc = GetComponent<PlayerReplicatedComponent>();
            //if (prc == null || !prc.IsClientDrone)
            //    return;

            //prc.UpdateTick();

            //Interpolate();

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

        protected override void Interpolate()
        {
            //BepInLogger.LogInfo(nameof(Interpolate));

            if (MovementContext == null)
            {
                BepInLogger.LogInfo($"{nameof(Interpolate)}:{nameof(MovementContext)} is null");
                return;
            }

            var InterpolationRatio = Time.deltaTime * 5;
            /* 
            * This code has been written by Lacyway (https://github.com/Lacyway) for the SIT Project (https://github.com/stayintarkov/StayInTarkov.Client).
            * You are free to re-use this in your own project, but out of respect please leave credit where it's due according to the MIT License
            */

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


           
            //else
            //{
            ApplyReplicatedMotion();
            //}

            LastState = NewState;
            //BepInLogger.LogInfo($"{nameof(Interpolate)}:End");
        }

        private void ApplyReplicatedMotion()
        {
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

      
    }
}
