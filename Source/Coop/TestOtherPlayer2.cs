//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Runtime.CompilerServices;
//using System.Text;
//using System.Threading.Tasks;
//using Comfort.Common;
//using EFT.AssetsManager;
//using EFT;
//using EFT.NextObservedPlayer;
//using UnityEngine;
//using static RootMotion.FinalIK.InteractionObject;
//using RootMotion.FinalIK;
//using EFT.HealthSystem;
//using EFT.InventoryLogic;
//using EFT.PrefabSettings;

//namespace StayInTarkov.Coop
//{
//    internal class TestOtherPlayer : MonoBehaviour, IAIDetails
//    {
//        public int Id {  get; set; }

//        public EPlayerSide Side {  get; set; }

//        public string GroupId {  get; set; }

//        public string TeamId {  get; set; }

//        public Vector3 LookDirection {  get; set; }

//        public Vector3 Position {  get; set; }

//        public BifacialTransform Transform {  get; set; }

//        public BifacialTransform WeaponRoot {  get; set; }

//        public IHealthController HealthController {  get; set; }

//        public Profile Profile {  get; set; }

//        public AIData AIData {  get; set; }

//        public PlayerLoyaltyData Loyalty {  get; set; }

//        public bool IsAI {  get; set; }

//        public Dictionary<BodyPartType, EnemyPart> MainParts {  get; set; }

//        public string ProfileId {  get; set; }

//        public string AccountId {  get; set; }

//        public PlayerBones PlayerBones {  get; set; }

//        public bool IsYourPlayer {  get; set; }

//        public PlayerBody PlayerBody {  get; set; }

//        public ICharacterController CharacterController {  get; set; }

//        public byte ChannelIndex {  get; set; }

//        public bool IsInBufferZone {  get; set; }

//        public bool StateIsSuitableForHandInput {  get; set; }

//        public Vector2 Rotation {  get; set; }

//        public Vector3 Velocity {  get; set; }

//        public EFT.Player.EUpdateMode ArmsUpdateMode {  get; set; }

//        public EUpdateQueue ArmsUpdateQueue {  get; set; }

//        public event Action<IAIDetails> OnIPlayerDeadOrUnspawn;
//        public FullBodyBipedIK FullBodyBipedIK { get; set; }
//        public string Nickname { get; set; }
//        public string Voice { get; set; }
//        public Collider[] Colliders { get; set; }
//        public IAnimator[] Animators { get; set; }
//        public IAnimator BodyAnimator { get; set; }
//        public PlayerAnimator PlayerAnimator { get; set; }
//        public BundleAnimationBones BundleAnimationBones { get; set; }

//        public static void Create(int playerId, EPlayerSide side, string groupId, string teamId, bool isAi, string nickname, string profileId, string accountId, string voice, Vector3 position, WildSpawnType wildSpawnType)
//        {
//            GameObject gameObject = Singleton<PoolManager>.Instance.CreatePlayerObject(ResourceBundleConstants.PLAYER_BUNDLE_NAME);
//            gameObject.name = "Observed_" + gameObject.name;
//            gameObject.transform.parent = null;
//            gameObject.SetActive(true);
//            gameObject.layer = LayerMask.NameToLayer("Player");
//            TestOtherPlayer tobservedPlayerView = gameObject.AddComponent<TestOtherPlayer>();
//            PlayerPoolObject component = gameObject.GetComponent<PlayerPoolObject>();
//            Animator componentInChildren = gameObject.GetComponentInChildren<Animator>(true);
//            PlayerOverlapManager playerOverlapManager = component.PlayerOverlapManager;
//            if (playerOverlapManager != null)
//            {
//                playerOverlapManager.Off();
//            }
//            tobservedPlayerView.Id = playerId;
//            component.RegisteredComponentsToClean.Add(tobservedPlayerView);
//            tobservedPlayerView.FullBodyBipedIK = component.FullBodyBipedIk;
//            tobservedPlayerView.FullBodyBipedIK.enabled = false;
//            tobservedPlayerView.FullBodyBipedIK.solver.Quick = true;
//            tobservedPlayerView.PlayerBones = component.PlayerBones;
//            tobservedPlayerView.PlayerBones.Player = null;
//            tobservedPlayerView.PlayerBody = tobservedPlayerView.PlayerBones.AnimatedTransform.Original.gameObject.GetComponent<PlayerBody>();
//            tobservedPlayerView.Side = side;
//            tobservedPlayerView.GroupId = groupId;
//            tobservedPlayerView.TeamId = teamId;
//            tobservedPlayerView.IsAI = isAi;
//            tobservedPlayerView.Nickname = nickname;
//            tobservedPlayerView.ProfileId = profileId;
//            tobservedPlayerView.AccountId = accountId;
//            tobservedPlayerView.Voice = voice;
//            List<Collider> list = new List<Collider>();
//            foreach (Collider collider in component.Colliders)
//            {
//                if (!playerOverlapManager.IsHeadCollider(collider) && !(playerOverlapManager.CameraCollider == collider))
//                {
//                    list.Add(collider);
//                }
//            }
//            tobservedPlayerView.Colliders = list.ToArray();
//            Collider[] colliders = tobservedPlayerView.Colliders;
//            for (int i = 0; i < colliders.Length; i++)
//            {
//                colliders[i].enabled = false;
//            }
//            tobservedPlayerView.Transform.Original.position = position;
//            ICharacterController characterController = component.CharacterControllerSpawner.Spawn(BackendConfigManager.Config.CharacterController.ObservedPlayerMode, tobservedPlayerView, tobservedPlayerView.gameObject, false, true);
//            tobservedPlayerView.CharacterController = characterController;
//            tobservedPlayerView.SetUpAnimator(componentInChildren);
//            tobservedPlayerView.HandleWildSpawn(wildSpawnType);
//            tobservedPlayerView.SetUpPlayerColliders();
//            Singleton<GameWorld>.Instance.RegisterPlayer(tobservedPlayerView);
//            tobservedPlayerView.BundleAnimationBones = new BundleAnimationBones(tobservedPlayerView.PlayerBones, tobservedPlayerView.GetBodyAnimatorCommon(), tobservedPlayerView);
//            tobservedPlayerView.hitReaction_0 = component.HitReaction;
//            tobservedPlayerView.HitReaction.enabled = true;
//            tobservedPlayerView.transform_0 = tobservedPlayerView.PlayerBones.LootRaycastOrigin;
//            tobservedPlayerView.method_6();
//            return tobservedPlayerView;
//        }

//        private void SetUpPlayerColliders()
//        {
//            foreach (BodyPartCollider bodyPartCollider in base.GetComponentsInChildren<BodyPartCollider>())
//            {
//                bodyPartCollider.SetUpPlayer(this);
//                bodyPartCollider.PlayerProfileID = this.ProfileId;
//                bodyPartCollider.gameObject.layer = LayerMaskClass.HitColliderLayer;
//            }
//        }

//        private void SetUpAnimator(Animator animator)
//        {
//            Animators = new IAnimator[2];
//            Animators[0] = GClass1194.CreateAnimator(animator);
//            Animators[0].cullingMode = AnimatorCullingMode.AlwaysAnimate;
//            Animators[0].updateMode = AnimatorUpdateMode.Normal;
//            BodyAnimator = Animators[0];
//            PlayerAnimator = new PlayerAnimator(new Func<IAnimator>(GetBodyAnimatorCommon));
//        }

//        public IAnimator GetBodyAnimatorCommon()
//        {
//            return Animators[0];
//        }

//        private void HandleWildSpawn(WildSpawnType role)
//        {
//            PlayerAnimator.SetLayerWeight(16, 0f);
//            if (Side != EPlayerSide.Savage)
//            {
//                return;
//            }
//            if (role == WildSpawnType.bossBoar)
//            {
//                Animators[0].runtimeAnimatorController = Singleton<IAssets>.Instance.GetAsset<RuntimeAnimatorController>(ResourceBundleConstants.BOSS_KABAN_ANIMATOR_CONTROLLER);
//                PlayerAnimator.SetLayerWeight(16, 1f);
//                return;
//            }
//        }

//        public void OnDeserializeFromServer(byte channelId, IBitReaderStream reader)
//        {
//            throw new NotImplementedException();
//        }

//        public RadioTransmitterRecodableComponent FindRadioTransmitter()
//        {
//            throw new NotImplementedException();
//        }

//        public void SetInteractInHands(bool isInteracting, int animationId = 1)
//        {
//            throw new NotImplementedException();
//        }

//        public void PlantItemLocalOnly(Item item, string zone)
//        {
//            throw new NotImplementedException();
//        }

//        public void UpdateInteractionCast()
//        {
//            throw new NotImplementedException();
//        }

//        public void HandleFlareSuccessEvent(Vector3 position, FlareEventType eventType)
//        {
//            throw new NotImplementedException();
//        }

//        public Vector3 PlayerColliderPointOnCenterAxis(float relativeHeight)
//        {
//            throw new NotImplementedException();
//        }

//        public IAnimator GetArmsAnimatorCommon()
//        {
//            throw new NotImplementedException();
//        }

//        public void SetArmsAnimatorCommon(IAnimator animator)
//        {
//            throw new NotImplementedException();
//        }
//    }
//}
