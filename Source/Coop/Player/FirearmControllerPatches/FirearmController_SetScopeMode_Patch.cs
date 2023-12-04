using StayInTarkov.Coop.NetworkPacket;
using StayInTarkov.Core.Player;
using StayInTarkov.Networking;
using StayInTarkov;
using System;
using System.Collections.Generic;
using System.Reflection;
using StayInTarkov.Coop;

namespace SIT.Core.Coop.Player.FirearmControllerPatches
{
    internal class FirearmController_SetScopeMode_Patch : ModuleReplicationPatch
    {
        public override Type InstanceType => typeof(EFT.Player.FirearmController);

        public override string MethodName => "SetScopeMode";

        [PatchPostfix]
        public static void Postfix(EFT.Player.FirearmController __instance, EFT.Player ____player, ScopeStates[] scopeStates)
        {
            var coopPlayer = ____player as CoopPlayer;

            if (coopPlayer != null)
            {
                if (!__instance.CurrentOperation.CanChangeScopeStates(scopeStates))
                {
                    return;
                }
                ChangeSightsMode changeSightsMode = new()
                {
                    ChangeSightMode = true,
                    SightModeStatuses = new GStruct176[scopeStates.Length]
                };
                for (int i = 0; i < scopeStates.Length; i++)
                {
                    ScopeStates scopeStates2 = scopeStates[i];
                    changeSightsMode.SightModeStatuses[i] = new GStruct176
                    {
                        Id = scopeStates2.Id,
                        SelectedMode = scopeStates2.ScopeMode,
                        ScopeIndexInsideSight = scopeStates2.ScopeIndexInsideSight,
                        ScopeCalibrationIndex = scopeStates2.ScopeCalibrationIndex
                    };
                }

                foreach (GStruct176 gStruct176 in changeSightsMode.SightModeStatuses)
                {
                    coopPlayer.AddCommand(new GClass2116()
                    {
                        ID = gStruct176.Id,
                        ScopeMode = gStruct176.SelectedMode,
                        ScopeIndexInsideSight = gStruct176.ScopeIndexInsideSight,
                        ScopeCalibrationIndex = gStruct176.ScopeCalibrationIndex,
                        SetSilently = false
                    });
                }
            }
            else
            {
                Logger.LogError("No CoopPlayer found!");
            }
        }

        public override void Replicated(EFT.Player player, Dictionary<string, object> dict)
        {
            return;
        }

        protected override MethodBase GetTargetMethod()
        {
            return ReflectionHelpers.GetMethodForType(InstanceType, MethodName);
        }

        public class ScopeModePacket : BasePlayerPacket
        {
            public string Id { get; set; }
            public int ScopeMode { get; set; }
            public int ScopeIndexInsideSight { get; set; }
            public int ScopeCalibrationIndex { get; set; }

            public ScopeModePacket(string id, int scopeMode, int scopeIndexInsideSight, int scopeCalibrationIndex, string profileId) : base(profileId, "SetScopeMode")
            {
                Id = id;
                ScopeMode = scopeMode;
                ScopeIndexInsideSight = scopeIndexInsideSight;
                ScopeCalibrationIndex = scopeCalibrationIndex;
            }
        }
    }
}
