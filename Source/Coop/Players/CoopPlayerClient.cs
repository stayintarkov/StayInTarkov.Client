using Comfort.Common;
using EFT;
using EFT.Interactive;
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
        private float InterpolationRatio { get; set; } = 0.03f;
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
            //base.LateUpdate();
            //BepInLogger.LogDebug("LateUpdate");

            //MovementContext?.AnimatorStatesLateUpdate();
            //DistanceDirty = true;
            //OcclusionDirty = true;
            if (HealthController != null && HealthController.IsAlive)
            {
                //Physical.LateUpdate();
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

            //var prc = GetComponent<PlayerReplicatedComponent>();
            //if (prc == null || !prc.IsClientDrone)
            //    return;

            //prc.UpdateTick();

            if (LastState == null)
                return;

            if (LastState.LinearSpeed > 0.25)
            {
                Move(LastState.InputDirection);
            }
        }

        protected override void Interpolate()
        {
            //BepInLogger.LogInfo(nameof(Interpolate));

            if (MovementContext == null)
            {
                BepInLogger.LogInfo($"{nameof(Interpolate)}:{nameof(MovementContext)} is null");
                return;
            }

            /* 
            * This code has been written by Lacyway (https://github.com/Lacyway) for the SIT Project (https://github.com/stayintarkov/StayInTarkov.Client).
            * You are free to re-use this in your own project, but out of respect please leave credit where it's due according to the MIT License
            */

            Rotation = new Vector2(Mathf.LerpAngle(Yaw, NewState.Rotation.x, InterpolationRatio), Mathf.Lerp(Pitch, NewState.Rotation.y, InterpolationRatio));

            HeadRotation = Vector3.Lerp(HeadRotation, NewState.HeadRotation, InterpolationRatio);
            ProceduralWeaponAnimation.SetHeadRotation(Vector3.Lerp(LastState.HeadRotation, NewState.HeadRotation, InterpolationRatio));
            MovementContext.PlayerAnimatorSetMovementDirection(Vector2.Lerp(LastState.MovementDirection, NewState.MovementDirection, InterpolationRatio));
            MovementContext.PlayerAnimatorSetDiscreteDirection(BSGDirectionalHelpers.ConvertToMovementDirection(NewState.MovementDirection));

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
            //else
            //{
            Vector3 a = Vector3.Lerp(MovementContext.TransformPosition, NewState.Position, Time.deltaTime * 2);
            CharacterController.Move(a - MovementContext.TransformPosition, Time.deltaTime);
            //}

            //BepInLogger.LogInfo($"{nameof(Interpolate)}:Packet took {DateTime.Now - new DateTime(long.Parse(NewState.TimeSerializedBetter))} to fully process.");
            LastState = NewState;
            //BepInLogger.LogInfo($"{nameof(Interpolate)}:End");
        }

        public override void UpdateTick()
        {
            base.UpdateTick();

            Interpolate();

            if (LastState == null)
                return;

            if (LastState.LinearSpeed > 0.25)
            {
                Move(LastState.InputDirection);
            }

        }
    }
}
