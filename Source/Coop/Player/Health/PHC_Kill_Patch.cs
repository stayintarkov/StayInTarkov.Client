//using BepInEx.Logging;
//using EFT;
//using EFT.HealthSystem;
//using StayInTarkov.Coop.NetworkPacket;
//using StayInTarkov.Networking;
//using System;
//using System.Collections.Generic;
//using System.Reflection;

//namespace StayInTarkov.Coop.Player.Health
//{
//    internal class PHC_Kill_Patch : ModuleReplicationPatch
//    {
//        public override Type InstanceType => typeof(ActiveHealthController);

//        public override string MethodName => "Kill";

//        public static Dictionary<string, bool> CallLocally = new();

//        private MethodInfo WeaponSoundPlayerRelease;
//        private MethodInfo WeaponSoundPlayerStopSoundCoroutine;

//        public PHC_Kill_Patch()
//        {
//            WeaponSoundPlayerRelease = ReflectionHelpers.GetMethodForType(typeof(WeaponSoundPlayer), "Release");
//            WeaponSoundPlayerStopSoundCoroutine = ReflectionHelpers.GetMethodForType(typeof(WeaponSoundPlayer), "StopSoundCoroutine");
//        }

//        private ManualLogSource GetLogger()
//        {
//            return GetLogger(typeof(PHC_Kill_Patch));
//        }

//        protected override MethodBase GetTargetMethod()
//        {
//            return ReflectionHelpers.GetMethodForType(InstanceType, MethodName);


//        }

//        [PatchPrefix]
//        public static bool PrePatch(
//            ActiveHealthController __instance
//            )
//        {
//            var player = __instance.Player;

//            var result = false;
//            if (CallLocally.TryGetValue(player.ProfileId, out var expecting) && expecting)
//                result = true;
//            return result;
//        }

//        [PatchPostfix]
//        public static void PatchPostfix(
//            ActiveHealthController __instance
//            , EDamageType damageType
//            )
//        {
//            //Logger.LogDebug("RestoreBodyPartPatch:PatchPostfix");

//            var player = __instance.Player;

//            if (CallLocally.TryGetValue(player.ProfileId, out var expecting) && expecting)
//            {
//                CallLocally.Remove(player.ProfileId);
//                return;
//            }

//            KillPacket killPacket = new(player.ProfileId);
//            killPacket.DamageType = damageType;
//            var json = killPacket.Serialize();
//            AkiBackendCommunication.Instance.SendDataToPool(json);
//        }

//        public override void Replicated(EFT.Player player, Dictionary<string, object> dict)
//        {
//            if (!dict.ContainsKey("data"))
//                return;

//            KillPacket killPacket = new(player.ProfileId);
//            killPacket.DeserializePacketSIT(dict["data"].ToString());

//            if (HasProcessed(GetType(), player, killPacket))
//                return;

//            if (CallLocally.ContainsKey(player.ProfileId))
//                return;

//            GetLogger().LogDebug($"Replicated Kill {player.ProfileId}");

//            CallLocally.Add(player.ProfileId, true);
//            player.ActiveHealthController.Kill(killPacket.DamageType);
//            if (player.HandsController is EFT.Player.FirearmController firearmCont)
//            {
//                firearmCont.SetTriggerPressed(false);
//                WeaponSoundPlayerRelease.Invoke(firearmCont.WeaponSoundPlayer, new object[1] { 0f });
//                WeaponSoundPlayerStopSoundCoroutine.Invoke(firearmCont.WeaponSoundPlayer, new object[0]);
//            }
//        }

//        class KillPacket : BasePlayerPacket
//        {
//            public EDamageType DamageType { get; set; }

//            public KillPacket(string profileId) : base(profileId, "Kill")
//            {
//            }
//        }
//    }
//}
