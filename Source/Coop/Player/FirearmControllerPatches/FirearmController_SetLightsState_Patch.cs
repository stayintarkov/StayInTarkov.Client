using StayInTarkov.Coop.NetworkPacket;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace StayInTarkov.Coop.Player.FirearmControllerPatches
{
    internal class FirearmController_SetLightsState_Patch : ModuleReplicationPatch
    {
        public override Type InstanceType => typeof(EFT.Player.FirearmController);

        public override string MethodName => "SetLightsState";

        [PatchPostfix]
        public static void Postfix(EFT.Player.FirearmController __instance, EFT.Player ____player, LightsStates[] lightsStates, bool force)
        {
            var coopPlayer = ____player as CoopPlayer;
            if (coopPlayer != null)
            {
                if (force || __instance.CurrentOperation.CanChangeLightState(lightsStates))
                {
                    ToggleTacticalCombo toggleTacticalCombo = new ToggleTacticalCombo
                    {
                        ToggleTacticalCombo = true,
                        TacticalComboStatuses = new GStruct173[lightsStates.Length]
                    };
                    for (int i = 0; i < lightsStates.Length; i++)
                    {
                        LightsStates lightsStates2 = lightsStates[i];
                        toggleTacticalCombo.TacticalComboStatuses[i] = new GStruct173
                        {
                            Id = lightsStates2.Id,
                            IsActive = lightsStates2.IsActive,
                            SelectedMode = lightsStates2.LightMode
                        };
                    }

                    foreach (GStruct173 gStruct173 in toggleTacticalCombo.TacticalComboStatuses)
                    {
                        coopPlayer.AddCommand(new Command()
                        {
                            ID = gStruct173.Id,
                            State = gStruct173.IsActive,
                            LightMode = gStruct173.SelectedMode,
                            SetSilently = false
                        });
                    }
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

        public class LightStatePacket : BasePlayerPacket
        {
            public string Id { get; set; }
            public bool IsActive { get; set; }
            public int LightMode { get; set; }

            public LightStatePacket(string id, bool isActive, int lightMode, string profileId)
                : base(profileId, "SetLightsState")
            {
                Id = id;
                IsActive = isActive;
                LightMode = lightMode;
            }
        }
    }
}
