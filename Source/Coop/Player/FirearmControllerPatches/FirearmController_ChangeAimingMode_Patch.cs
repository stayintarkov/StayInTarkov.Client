using StayInTarkov;
using StayInTarkov.Coop;
using StayInTarkov.Coop.Web;
using StayInTarkov.Core.Player;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace SIT.Core.Coop.Player.FirearmControllerPatches
{
    public class FirearmController_ChangeAimingMode_Patch : ModuleReplicationPatch
    {
        public override Type InstanceType => typeof(EFT.Player.FirearmController);

        public override string MethodName => "ChangeAimingMode";

        public static HashSet<string> CallLocally = new();


        [PatchPrefix]
        public static bool PrePatch(
            EFT.Player.FirearmController __instance, EFT.Player ____player)
        {
            //Logger.LogInfo("FirearmController_SetLightsState_Patch.PrePatch");
            var player = ____player;
            if (player == null)
                return false;

            var result = false;
            if (CallLocally.Contains(player.ProfileId))
                result = true;

            return result;
        }

        [PatchPostfix]
        public static void Postfix(
           EFT.Player.FirearmController __instance, EFT.Player ____player)
        {
            //Logger.LogInfo("FirearmController_SetLightsState_Patch.Postfix");
            var player = ____player;
            if (player == null)
                return;

            if (CallLocally.Contains(player.ProfileId))
            {
                CallLocally.Remove(player.ProfileId);
                return;
            }

            Dictionary<string, object> dictionary = new()
            {
                { "i", __instance.Item.AimIndex.Value.ToString() },
                { "m", "ChangeAimingMode" }
            };

            AkiBackendCommunicationCoop.PostLocalPlayerData(player, dictionary);

        }

        public override void Replicated(EFT.Player player, Dictionary<string, object> dict)
        {
            if (HasProcessed(GetType(), player, dict))
                return;

            if (!player.TryGetComponent<PlayerReplicatedComponent>(out var prc))
                return;

            if (CallLocally.Contains(player.ProfileId))
                return;

            if (int.TryParse(dict["i"].ToString(), out var i))
            {
                if (player.HandsController is EFT.Player.FirearmController firearmCont)
                {
                    try
                    {
                        CallLocally.Add(player.ProfileId);
                        firearmCont.Item.AimIndex.Value = i;
                        firearmCont.ChangeAimingMode();
                        Logger.LogInfo(i);
                    }
                    catch (Exception e)
                    {
                        Logger.LogInfo(e);
                    }
                }
            }
        }

        protected override MethodBase GetTargetMethod()
        {
            return ReflectionHelpers.GetMethodForType(InstanceType, MethodName);
        }
    }
}
