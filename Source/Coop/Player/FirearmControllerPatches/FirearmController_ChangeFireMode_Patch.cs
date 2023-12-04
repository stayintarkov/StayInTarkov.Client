using EFT.InventoryLogic;
using Newtonsoft.Json;
using StayInTarkov.Coop.NetworkPacket;
using StayInTarkov.Networking;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace StayInTarkov.Coop.Player.FirearmControllerPatches
{
    public class FirearmController_ChangeFireMode_Patch : ModuleReplicationPatch
    {
        public override Type InstanceType => typeof(EFT.Player.FirearmController);
        public override string MethodName => "ChangeFireMode";

        protected override MethodBase GetTargetMethod()
        {
            return ReflectionHelpers.GetMethodForType(InstanceType, MethodName);
        }

        [PatchPostfix]
        public static void PostPatch(EFT.Player.FirearmController __instance, Weapon.EFireMode fireMode, EFT.Player ____player)
        {
            var coopPlayer = ____player as CoopPlayer;
            if (coopPlayer != null)
            {
                coopPlayer.AddCommand(new GClass2137()
                {
                    FireMode = fireMode
                });
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

        public class FireModePacket : BasePlayerPacket
        {
            [JsonProperty("f")]
            public byte FireMode { get; set; }

            public FireModePacket(string profileId, Weapon.EFireMode fireMode)
                : base(profileId, "ChangeFireMode")
            {
                FireMode = (byte)fireMode;
            }
        }
    }
}
