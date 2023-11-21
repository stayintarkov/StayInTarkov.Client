using Comfort.Common;
using StayInTarkov.Coop.Player.GrenadeControllerPatches;
using StayInTarkov;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using StayInTarkov.Core.Player;
using StayInTarkov.Coop.Web;

namespace StayInTarkov.Coop.Player.Proceed
{
    internal class Player_Proceed_Throwable_Patch : ModuleReplicationPatch
    {
        public override Type InstanceType => typeof(EFT.Player);

        public override string MethodName => "ProceedThrowable";

        public static Dictionary<string, bool> CallLocally = new();

        public static MethodInfo method1 = null;

        protected override MethodBase GetTargetMethod()
        {
            var t = typeof(EFT.Player);
            if (t == null)
                Logger.LogInfo($"Player_Proceed_Throwable_Patch:Type is NULL");

            method1 = ReflectionHelpers.GetAllMethodsForType(t).FirstOrDefault(x => x.Name == "Proceed"
                && x.GetParameters().Length == 3
                && x.GetParameters()[0].Name == "throwWeap"
                && x.GetParameters()[1].Name == "callback"
                && x.GetParameters()[2].Name == "scheduled"

                && x.GetParameters()[1].ParameterType == typeof(Callback<IThrowableCallback>)

                );

            return method1;
        }


        [PatchPrefix]
        public static bool PrePatch(
           EFT.Player __instance
            )
        {
            if (CallLocally.TryGetValue(__instance.Profile.AccountId, out var expecting) && expecting)
            {
                return true;
            }

            return false;
        }

        [PatchPostfix]
        public static void PostPatch(EFT.Player __instance
            , ThrowWeap throwWeap
            , bool scheduled)
        {
            if (CallLocally.TryGetValue(__instance.Profile.AccountId, out var expecting) && expecting)
            {
                CallLocally.Remove(__instance.Profile.AccountId);
                return;
            }

            // Stop Client Drone sending a Proceed back to the player
            if (__instance.TryGetComponent<PlayerReplicatedComponent>(out var prc))
            {
                if (prc.IsClientDrone)
                    return;
            }

            //Logger.LogInfo($"PlayerOnTryProceedPatch:Patch");
            Dictionary<string, object> args = new();
            ItemAddressHelpers.ConvertItemAddressToDescriptor(throwWeap.CurrentAddress, ref args);

            args.Add("m", "ProceedThrowable");
            args.Add("t", DateTime.Now.Ticks);
            args.Add("item.id", throwWeap.Id);
            args.Add("item.tpl", throwWeap.TemplateId);
            args.Add("s", scheduled.ToString());
            AkiBackendCommunicationCoop.PostLocalPlayerData(__instance, args);
        }

        public override void Replicated(EFT.Player player, Dictionary<string, object> dict)
        {
            if (HasProcessed(GetType(), player, dict))
                return;

            var coopGC = CoopGameComponent.GetCoopGameComponent();
            if (coopGC == null)
                return;

            var allItemsOfTemplate = player.Profile.Inventory.GetAllItemByTemplate(dict["item.tpl"].ToString());

            if (!allItemsOfTemplate.Any())
                return;

            var item = allItemsOfTemplate.FirstOrDefault(x => x.Id == dict["item.id"].ToString());

            if (item != null && item is ThrowWeap throwable)
            {
                CallLocally.Add(player.Profile.AccountId, true);
                var callback = new Callback<IThrowableCallback>(async (IResult) =>
                {
                    if (IResult.Value != null)
                    {
                        await Task.Run(() =>
                        {
                            //Logger.LogInfo($"Player_Proceed_Throwable_Patch. Found {IResult.Value.GetType().FullName}");

                        if (ModuleReplicationPatch.Patches.Values.Any(x => x.InstanceType == typeof(GrenadeController_HighThrow_Patch)))
                            ModuleReplicationPatch.Patches.Remove(ModuleReplicationPatch.Patches.Values.First(x => x.InstanceType == typeof(GrenadeController_HighThrow_Patch)).MethodName);

                        if (ModuleReplicationPatch.Patches.Values.Any(x => x.InstanceType == typeof(GrenadeController_LowThrow_Patch)))
                            ModuleReplicationPatch.Patches.Remove(ModuleReplicationPatch.Patches.Values.First(x => x.InstanceType == typeof(GrenadeController_LowThrow_Patch)).MethodName);

                        if (ModuleReplicationPatch.Patches.Values.Any(x => x.InstanceType == typeof(GrenadeController_PullRingForHighThrow_Patch)))
                            ModuleReplicationPatch.Patches.Remove(ModuleReplicationPatch.Patches.Values.First(x => x.InstanceType == typeof(GrenadeController_PullRingForHighThrow_Patch)).MethodName);

                        if (ModuleReplicationPatch.Patches.Values.Any(x => x.InstanceType == typeof(GrenadeController_PullRingForLowThrow_Patch)))
                            ModuleReplicationPatch.Patches.Remove(ModuleReplicationPatch.Patches.Values.First(x => x.InstanceType == typeof(GrenadeController_PullRingForLowThrow_Patch)).MethodName);

                            // Enable patches
                            new GrenadeController_HighThrow_Patch(IResult.Value.GetType()).Enable();
                            new GrenadeController_LowThrow_Patch(IResult.Value.GetType()).Enable();
                            new GrenadeController_PullRingForHighThrow_Patch(IResult.Value.GetType()).Enable();
                            new GrenadeController_PullRingForLowThrow_Patch(IResult.Value.GetType()).Enable();
                        });
                    }
                });
                method1.Invoke(player, new object[] { throwable, callback, true });
            }
        }
    }
}
