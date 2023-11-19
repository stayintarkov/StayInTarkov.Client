using EFT;
using SIT.Tarkov.Core;
using StayInTarkov;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace SIT.Core.Coop.Player
{
    public class Player_ApplyDamageInfo_Patch : ModuleReplicationPatch
    {
        //private static ConcurrentDictionary<string, long> ProcessedCalls = new();
        public static List<string> CallLocally = new();
        public override Type InstanceType => typeof(EFT.Player);
        public override string MethodName => "ApplyDamageInfo";

        //public override bool DisablePatch => true;

        protected override MethodBase GetTargetMethod()
        {
            return ReflectionHelpers.GetMethodForType(InstanceType, MethodName);
        }

        public static Dictionary<string, EDamageType> LastDamageTypes = new();

        [PatchPrefix]
        public static bool PrePatch(EFT.Player __instance
            , ref DamageInfo damageInfo
            , EBodyPart bodyPartType)
        {

            //var result = false;
            //if (CallLocally.Contains(__instance.ProfileId))
            //    result = true;

            //if (!LastDamageTypes.ContainsKey(__instance.ProfileId))
            //    LastDamageTypes.Add(__instance.ProfileId, EDamageType.Undefined);

            //if (result)
            //{
            //    if (PluginConfigSettings.Instance != null)
            //    {
            //        if (PluginConfigSettings.Instance.CoopSettings.SETTING_HeadshotsAlwaysKill)
            //        {
            //            if (bodyPartType == EBodyPart.Head && damageInfo.DamageType == EFT.EDamageType.Bullet)
            //            {
            //                if (damageInfo.DidArmorDamage == 0)
            //                {
            //                    damageInfo.Damage = 999;
            //                    damageInfo.DidBodyDamage = 999;
            //                }
            //            }
            //        }
            //    }


            //    LastDamageTypes[__instance.ProfileId] = damageInfo.DamageType;
            //}
            //return result;

            return true;
        }

        //[PatchPostfix]
        //public static void PostPatch(
        //   EFT.Player __instance,
        //    ref DamageInfo damageInfo
        //    , EBodyPart bodyPartType, float absorbed, EHeadSegment? headSegment = null
        //    )
        //{
        //    var player = __instance;

        //    if (CallLocally.Contains(player.ProfileId))
        //    {
        //        CallLocally.Remove(player.ProfileId);
        //        return;
        //    }

        //    if (PluginConfigSettings.Instance != null)
        //    {
        //        if (PluginConfigSettings.Instance.CoopSettings.SETTING_HeadshotsAlwaysKill)
        //        {
        //            if (bodyPartType == EBodyPart.Head && damageInfo.DamageType == EFT.EDamageType.Bullet)
        //            {
        //                if (damageInfo.DidArmorDamage == 0)
        //                {
        //                    damageInfo.Damage = 999;
        //                    damageInfo.DidBodyDamage = 999;
        //                }
        //            }
        //        }
        //    }

        //    // Test.
        //    // Lets see if we can only run this from a server only perspective
        //    // 
        //    //if (MatchmakerAcceptPatches.IsClient)
        //    //    return;

        //    //Dictionary<string, object> packet = new();
        //    //var bodyPartColliderType = ((BodyPartCollider)damageInfo.HittedBallisticCollider).BodyPartColliderType;
        //    //damageInfo.HitCollider = null;
        //    //damageInfo.HittedBallisticCollider = null;
        //    //Dictionary<string, string> playerDict = new();
        //    //try
        //    //{
        //    //    if (damageInfo.Player != null)
        //    //    {
        //    //        //playerDict.Add("d.p.aid", damageInfo.Player.Profile.AccountId);
        //    //        playerDict.Add("d.p.aid", damageInfo.Player.iPlayer.Profile.AccountId);
        //    //        playerDict.Add("d.p.id", damageInfo.Player.iPlayer.ProfileId);
        //    //    }
        //    //}
        //    //catch (Exception e)
        //    //{
        //    //    Logger.LogError(e);
        //    //}
        //    //damageInfo.Player = null;
        //    //Dictionary<string, string> weaponDict = new();

        //    ////Dictionary<string, object> packet = new();
        //    ////damageInfo.HitCollider = null;
        //    ////damageInfo.HittedBallisticCollider = null;
        //    ////Dictionary<string, string> playerDict = new();
        //    ////try
        //    ////{
        //    ////    if (damageInfo.Player != null)
        //    ////    {
        //    ////        playerDict.Add("d.p.aid", damageInfo.Player.iPlayer.Profile.AccountId);
        //    ////        playerDict.Add("d.p.id", damageInfo.Player.iPlayer.ProfileId);
        //    ////    }
        //    ////}
        //    ////catch (Exception e)
        //    ////{
        //    ////    Logger.LogError(e);
        //    ////}
        //    ////damageInfo.Player = null;
        //    ////Dictionary<string, string> weaponDict = new();

        //    //packet.Add("d", SerializeObject(damageInfo));
        //    //packet.Add("d.p", playerDict);
        //    //packet.Add("d.w", weaponDict);
        //    //packet.Add("bpt", bodyPartType.ToString());
        //    //packet.Add("bpct", bodyPartColliderType.ToString());
        //    //packet.Add("ab", absorbed.ToString());
        //    //packet.Add("hs", headSegment.ToString());
        //    //packet.Add("m", "ApplyDamageInfo");
        //    //AkiBackendCommunicationCoopHelpers.PostLocalPlayerData(player, packet, true);


        //    // ---------------------------- KILL ------------------------------
        //    //if (MatchmakerAcceptPatches.IsServer)  // should we do this on the server and send out to others only?
        //    //    {
        //    //        var bodyPartHealth = player.ActiveHealthController.GetBodyPartHealth(bodyPartType);
        //    //        if (
        //    //            ((bodyPartType == EBodyPart.Head || bodyPartType == EBodyPart.Common || bodyPartType == EBodyPart.Chest) && bodyPartHealth.AtMinimum)
        //    //            || !player.ActiveHealthController.IsAlive
        //    //            || !player.PlayerHealthController.IsAlive
        //    //            )
        //    //        {
        //    //            packet = new();
        //    //            packet.Add("accountId", player.Profile.AccountId);
        //    //            packet.Add("serverId", CoopGameComponent.GetServerId());
        //    //            packet.Add("t", DateTime.Now.Ticks.ToString("G"));
        //    //            packet.Add("dmt", damageInfo.DamageType.ToString());
        //    //            packet.Add("m", "Kill");
        //    //            AkiBackendCommunication.Instance.SendDataToPool(packet.ToJson());
        //    //        }
        //    //    }
        //}

        public override void Replicated(EFT.Player player, Dictionary<string, object> dict)
        {

            //if (player is CoopPlayer coopPlayer)

            //{
            Logger.LogDebug($"Player_ApplyDamageInfo_Patch:Replicated:{player.ProfileId}");
            ((CoopPlayer)player).ReceiveDamageFromServer(dict);
            //}
        }
    }
}

