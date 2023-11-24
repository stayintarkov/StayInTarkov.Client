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
        //public override bool DisablePatch => true;

        protected override MethodBase GetTargetMethod()
        {
            var method = ReflectionHelpers.GetMethodForType(InstanceType, MethodName);
            return method;
        }

        public static Dictionary<string, bool> CallLocally
            = new();


        [PatchPrefix]
        public static bool PrePatch(EFT.Player.FirearmController __instance, EFT.Player ____player)
        {
            var player = ____player;
            if (player == null)
                return false;

            var result = false;
            if (CallLocally.TryGetValue(player.ProfileId, out var expecting) && expecting)
                result = true;

            //Logger.LogInfo("FirearmController_ChangeFireMode_Patch:PrePatch");

            return result;
        }

        [PatchPostfix]
        public static void PostPatch(
            EFT.Player.FirearmController __instance
            , Weapon.EFireMode fireMode
            , EFT.Player ____player)
        {
            var player = ____player;
            if (player == null)
                return;

            if (CallLocally.TryGetValue(player.ProfileId, out var expecting) && expecting)
            {
                CallLocally.Remove(player.ProfileId);
                return;
            }

            //Dictionary<string, object> dictionary = new();
            //dictionary.Add("f", fireMode.ToString());
            //dictionary.Add("m", "ChangeFireMode");
            //AkiBackendCommunicationCoopHelpers.PostLocalPlayerData(player, dictionary);
            //Logger.LogInfo("FirearmController_ChangeFireMode_Patch:PostPatch");

            FireModePacket fireModePacket = new(____player.ProfileId, fireMode);
            AkiBackendCommunication.Instance.SendDataToPool(fireModePacket.Serialize());

        }

        //private static List<long> ProcessedCalls = new List<long>();

        public override void Replicated(EFT.Player player, Dictionary<string, object> dict)
        {

            FireModePacket fmp = new(player.ProfileId, Weapon.EFireMode.single);

            if (dict.ContainsKey("data"))
                fmp = fmp.DeserializePacketSIT(dict["data"].ToString());

            if (HasProcessed(GetType(), player, fmp))
                return;

            if (player.HandsController is EFT.Player.FirearmController firearmCont)
            {
                try
                {
                    CallLocally.Add(player.ProfileId, true);
                    //if (Enum.TryParse<Weapon.EFireMode>(dict["f"].ToString(), out var firemode))
                    var firemode = (Weapon.EFireMode)fmp.FireMode;
                    {
                        //Logger.LogInfo("Replicated: Calling Change FireMode");
                        firearmCont.ChangeFireMode(firemode);
                    }
                }
                catch (Exception e)
                {
                    Logger.LogInfo(e);
                }
            }
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
