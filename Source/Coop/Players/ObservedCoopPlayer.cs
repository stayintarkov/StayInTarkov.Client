using Comfort.Common;
using EFT;
using EFT.HealthSystem;
using EFT.Interactive;
using EFT.InventoryLogic;
using StayInTarkov.Coop.Matchmaker;
using StayInTarkov.Core.Player;
//using StayInTarkov.Networking.Packets;
using System;
using System.Collections;
using System.Runtime.Remoting.Lifetime;
using System.Threading.Tasks;
using UnityEngine;
using UnityStandardAssets.Water;
using StayInTarkov.Coop.Controllers;

/* 
* This code has been written by Lacyway (https://github.com/Lacyway) for the SIT Project (https://github.com/stayintarkov/StayInTarkov.Client).
* You are free to re-use this in your own project, but out of respect please leave credit where it's due according to the MIT License
*/

namespace StayInTarkov.Coop.Players
{
    /// <summary>
    /// Observed players are any other players in the world for a client, including bots. They are all being handled by the server that moves them through packets.
    /// </summary>
    public class ObservedCoopPlayer : CoopPlayer
    {
        public CoopPlayer MainPlayer => Singleton<GameWorld>.Instance.MainPlayer as CoopPlayer;
        private float InterpolationRatio { get; set; } = 0.5f;
        public bool IsObservedAI;

        public static async Task<LocalPlayer> CreateObservedPlayer(
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
            AbstractQuestController1 questController = null,
            AbstractAchievementsController1 achievementController = null,
            bool isYourPlayer = false,
            bool isClientDrone = false)
        {
            ObservedCoopPlayer player = null;

            player = Create<ObservedCoopPlayer>(
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

            player.IsYourPlayer = false;

            InventoryController inventoryController = new ObservedInventoryController(player, profile, false);
            questController = PlayerFactory.GetQuestController(profile, inventoryController);
            var statisticsManager = PlayerFactory.GetStatisticsManager(player);
            achievementController = PlayerFactory.GetAchievementController(profile, inventoryController);

            await player.Init(rotation, layerName, pointOfView, profile, inventoryController,
                new CoopHealthController(profile.Health, player, inventoryController, profile.Skills, aiControl),
                isYourPlayer ? statisticsManager : new NullStatisticsManager(), questController, achievementController, filter,
                aiControl || isClientDrone ? EVoipState.NotAvailable : EVoipState.Available, aiControl, async: false);

            player._handsController = EmptyHandsController.smethod_5<EmptyHandsController>(player);
            player._handsController.Spawn(1f, delegate { });
            player.AIData = new AIData(null, player);
            player.AggressorFound = false;
            player._animators[0].enabled = true;
            player._armsUpdateQueue = EUpdateQueue.Update;
            // If this is a Client Drone add Player Replicated Component
            if (isClientDrone)
            {
                var prc = player.GetOrAddComponent<PlayerReplicatedComponent>();
                prc.IsClientDrone = true;
            }

            // networkgame.method5

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

        public override void ApplyDamageInfo(DamageInfo damageInfo, EBodyPart bodyPartType, EBodyPartColliderType colliderType, float absorbed)
        {
            base.ApplyDamageInfo(damageInfo, bodyPartType, colliderType, absorbed);
        }

        //public override void ApplyDamageInfo(DamageInfo damageInfo, EBodyPart bodyPartType, float absorbed, EHeadSegment? headSegment = null)
        //{
        //    // TODO: Try to run all of this locally so we do not rely on the server / fight lag
        //    // TODO: Send information on who shot us to prevent the end screen to be empty / kill feed being wrong
        //    // TODO: Do this on ApplyShot instead, and check if instigator is local
        //    // Also do check if it's a server and shooter is AI

        //    if (damageInfo.Player == null || !damageInfo.Player.iPlayer.IsYourPlayer)
        //        return;

        //    if (!IsObservedAI)
        //        return;

        //    if (damageInfo.DamageType.IsWeaponInduced())
        //    {
        //        HealthPacket.HasDamageInfo = true;
        //        HealthPacket.ApplyDamageInfo = new()
        //        {
        //            Damage = damageInfo.Damage,
        //            DamageType = damageInfo.DamageType,
        //            BodyPartType = bodyPartType,
        //            Absorbed = absorbed,
        //            //ProfileId = damageInfo.Player == null ? "null" : damageInfo.Player.iPlayer.ProfileId
        //        };
        //        HealthPacket.ToggleSend();

        //        base.ApplyDamageInfo(damageInfo, bodyPartType, absorbed, headSegment);
        //    }
        //}

        //public override PlayerHitInfo ApplyShot(DamageInfo damageInfo, EBodyPart bodyPartType, ShotId shotId)
        //{
        //    if (damageInfo.Player != null && damageInfo.Player.iPlayer.IsYourPlayer)
        //    {
        //        return base.ApplyShot(damageInfo, bodyPartType, shotId);
        //    }
        //    else
        //    {
        //        return null;
        //    }
        //}

        //protected override void Interpolate()
        //{

        //    /* 
        //    * This code has been written by Lacyway (https://github.com/Lacyway) for the SIT Project (https://github.com/stayintarkov/StayInTarkov.Client).
        //    * You are free to re-use this in your own project, but out of respect please leave credit where it's due according to the MIT License
        //    */

        //    if (!IsYourPlayer)
        //    {

        //        Rotation = new Vector2(Mathf.LerpAngle(Yaw, NewState.Rotation.x, InterpolationRatio), Mathf.Lerp(Pitch, NewState.Rotation.y, InterpolationRatio));

        //        HeadRotation = Vector3.Lerp(LastState.HeadRotation, NewState.HeadRotation, InterpolationRatio);
        //        ProceduralWeaponAnimation.SetHeadRotation(Vector3.Lerp(LastState.HeadRotation, NewState.HeadRotation, InterpolationRatio));
        //        MovementContext.PlayerAnimatorSetMovementDirection(Vector2.Lerp(LastState.MovementDirection, NewState.MovementDirection, InterpolationRatio));
        //        MovementContext.PlayerAnimatorSetDiscreteDirection(GClass1595.ConvertToMovementDirection(NewState.MovementDirection));

        //        EPlayerState name = MovementContext.CurrentState.Name;
        //        EPlayerState eplayerState = NewState.State;
        //        if (eplayerState == EPlayerState.Jump)
        //        {
        //            Jump();
        //        }
        //        if (name == EPlayerState.Jump && eplayerState != EPlayerState.Jump)
        //        {
        //            MovementContext.PlayerAnimatorEnableJump(false);
        //            MovementContext.PlayerAnimatorEnableLanding(true);
        //        }
        //        if ((name == EPlayerState.ProneIdle || name == EPlayerState.ProneMove) && eplayerState != EPlayerState.ProneMove && eplayerState != EPlayerState.Transit2Prone && eplayerState != EPlayerState.ProneIdle)
        //        {
        //            MovementContext.IsInPronePose = false;
        //        }
        //        if ((eplayerState == EPlayerState.ProneIdle || eplayerState == EPlayerState.ProneMove) && name != EPlayerState.ProneMove && name != EPlayerState.Prone2Stand && name != EPlayerState.Transit2Prone && name != EPlayerState.ProneIdle)
        //        {
        //            MovementContext.IsInPronePose = true;
        //        }

        //        Physical.SerializationStruct = NewState.Stamina;
        //        MovementContext.SetTilt(Mathf.Round(NewState.Tilt)); // Round the float due to byte converting error...
        //        CurrentManagedState.SetStep(NewState.Step);
        //        MovementContext.PlayerAnimatorEnableSprint(NewState.IsSprinting);
        //        MovementContext.EnableSprint(NewState.IsSprinting);

        //        MovementContext.IsInPronePose = NewState.IsProne;
        //        MovementContext.SetPoseLevel(Mathf.Lerp(LastState.PoseLevel, NewState.PoseLevel, InterpolationRatio));

        //        MovementContext.SetCurrentClientAnimatorStateIndex(NewState.AnimatorStateIndex);
        //        MovementContext.SetCharacterMovementSpeed(Mathf.Lerp(LastState.CharacterMovementSpeed, NewState.CharacterMovementSpeed, InterpolationRatio));
        //        MovementContext.PlayerAnimatorSetCharacterMovementSpeed(Mathf.Lerp(LastState.CharacterMovementSpeed, NewState.CharacterMovementSpeed, InterpolationRatio));

        //        MovementContext.SetBlindFire(NewState.Blindfire);

        //        if (!IsInventoryOpened && NewState.LinearSpeed > 0.25)
        //        {
        //            Move(NewState.InputDirection);
        //        }
        //        Vector3 a = Vector3.Lerp(MovementContext.TransformPosition, NewState.Position, InterpolationRatio);
        //        CharacterController.Move(a - MovementContext.TransformPosition, InterpolationRatio);

        //        LastState = NewState;
        //    }
        //}

        //protected override IEnumerator SendStatePacket()
        //{
        //    // TODO: Improve this by not resetting the writer and send many packets instead, rewrite the function in the client/server.
        //    var waitSeconds = new WaitForSeconds(0.125f);

        //    while (true)
        //    {
        //        yield return waitSeconds;

        //        if (MatchmakerAcceptPatches.IsClient)
        //        {
        //            Writer.Reset();

        //            if (HealthPacket.ShouldSend && !string.IsNullOrEmpty(HealthPacket.ProfileId))
        //            {
        //                Writer.Reset();
        //                MainPlayer.Client.SendData(Writer, ref HealthPacket, LiteNetLib.DeliveryMethod.ReliableOrdered);
        //                HealthPacket = new(ProfileId);
        //            }
        //        }
        //    }
        //}

        //protected IEnumerator SpawnObservedPlayer()
        //{
        //    yield return new WaitForSeconds(1);
        //    Teleport(new Vector3(NewState.Position.x + 0.25f, NewState.Position.y + 1, NewState.Position.z + 0.25f));
        //    yield return new WaitForSeconds(2);

        //    if (Vector3.Distance(Position, NewState.Position) > 0.25)
        //    {
        //        LastState = NewState;
        //        EFT.UI.ConsoleScreen.LogError($"Spawn distance was too far on {Profile.Nickname}!");
        //        Teleport(new Vector3(NewState.Position.x + 0.25f, NewState.Position.y + 5, NewState.Position.z + 0.25f));
        //    }

        //    //yield return new WaitForSeconds(10);

        //    //if (Vector3.Distance(Position, NewState.Position) > 0.25)
        //    //{
        //    //    LastState = NewState;
        //    //    EFT.UI.ConsoleScreen.LogError($"Spawn distance was too far on {Profile.Nickname} again!");
        //    //    Teleport(new Vector3(MainPlayer.Transform.position.x + 0.25f, MainPlayer.Transform.position.y + 5, MainPlayer.Transform.position.z + 0.25f));
        //    //}

        //    yield return new WaitForSeconds(2);
        //    ActiveHealthController.SetDamageCoeff(1);
        //    yield break;
        //}

        //private IEnumerator SpawnObservedBot()
        //{
        //    yield return new WaitForSeconds(1);
        //    ActiveHealthController.SetDamageCoeff(1);
        //    yield break;
        //}

        //protected override void Start()
        //{
        //    Writer = new();

        //    if (ProfileId.StartsWith("pmc"))
        //        IsObservedAI = false;
        //    else
        //        IsObservedAI = true;

        //    WeaponPacket = new(ProfileId);
        //    HealthPacket = new(ProfileId);
        //    InventoryPacket = new(ProfileId);
        //    CommonPlayerPacket = new(ProfileId);

        //    LastState = new(ProfileId, Position, Rotation, HeadRotation,
        //        MovementContext.MovementDirection, CurrentManagedState.Name, MovementContext.Tilt,
        //        MovementContext.Step, CurrentAnimatorStateIndex, MovementContext.SmoothedCharacterMovementSpeed,
        //        IsInPronePose, PoseLevel, MovementContext.IsSprintEnabled, Physical.SerializationStruct, InputDirection,
        //        MovementContext.BlindFire, MovementContext.ActualLinearSpeed);

        //    NewState = new(ProfileId, Position, Rotation, HeadRotation,
        //        MovementContext.MovementDirection, CurrentManagedState.Name, MovementContext.Tilt,
        //        MovementContext.Step, CurrentAnimatorStateIndex, MovementContext.SmoothedCharacterMovementSpeed,
        //        IsInPronePose, PoseLevel, MovementContext.IsSprintEnabled, Physical.SerializationStruct, InputDirection,
        //        MovementContext.BlindFire, MovementContext.ActualLinearSpeed);

        //    if (MatchmakerAcceptPatches.IsClient)
        //        StartCoroutine(SendStatePacket());

        //    if (IsObservedAI) // Prevents AI from dying to fall damage
        //    {
        //        ActiveHealthController.SetDamageCoeff(0);
        //        StartCoroutine(SpawnObservedBot());
        //    }
        //    else
        //    {
        //        ActiveHealthController.SetDamageCoeff(0);
        //        StartCoroutine(SpawnObservedPlayer());
        //    }
        //}

        //public override void UpdateTick()
        //{
        //    base.UpdateTick();

        //    Interpolate();

        //    if (FirearmPackets.Count > 0)
        //    {
        //        HandleWeaponPacket();
        //    }
        //    if (HealthPackets.Count > 0)
        //    {
        //        HandleHealthPacket();
        //    }
        //    if (InventoryPackets.Count > 0)
        //    {
        //        HandleInventoryPacket();
        //    }
        //    if (CommonPlayerPackets.Count > 0)
        //    {
        //        HandleCommonPacket();
        //    }
        //}

        public override void OnDestroy()
        {
            base.OnDestroy();
        }
    }
}
