using StayInTarkov.Coop.NetworkPacket;
using StayInTarkov.Core.Player;
using StayInTarkov.Networking;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace StayInTarkov.Coop.Player.FirearmControllerPatches
{
    internal class FirearmController_SetLightsState_Patch : ModuleReplicationPatch
    {
        public override Type InstanceType => typeof(EFT.Player.FirearmController);

        public override string MethodName => "SetLightsState";

        public static Dictionary<string, bool> CallLocally = new();


        [PatchPrefix]
        public static bool PrePatch(
            EFT.Player.FirearmController __instance
           , EFT.Player ____player
           , LightsStates[] lightsStates, bool force
            )
        {
            //Logger.LogInfo("FirearmController_SetLightsState_Patch.PrePatch");
            var player = ____player;
            if (player == null)
                return false;

            var result = false;
            if (CallLocally.TryGetValue(player.ProfileId, out var expecting) && expecting)
                result = true;

            return result;
        }

        [PatchPostfix]
        public static void Postfix(
           EFT.Player.FirearmController __instance
           , EFT.Player ____player
           , LightsStates[] lightsStates, bool force
           )
        {
            //Logger.LogInfo("FirearmController_SetLightsState_Patch.Postfix");
            var player = ____player;
            if (player == null)
                return;

            if (CallLocally.TryGetValue(player.ProfileId, out var expecting) && expecting)
            {
                CallLocally.Remove(player.ProfileId);
                return;
            }


            foreach (var light in lightsStates)
            {
                LightStatePacket lightStatePacket = new(light.Id, light.IsActive, light.LightMode, player.ProfileId);
                AkiBackendCommunication.Instance.SendDataToPool(lightStatePacket.Serialize());
            }

        }

        public override void Replicated(EFT.Player player, Dictionary<string, object> dict)
        {
            //Logger.LogInfo("FirearmController_SetLightsState_Patch.Replicated");
            LightStatePacket lsp = new(null, false, 0, null);

            if (dict.ContainsKey("data"))
            {
                lsp = lsp.DeserializePacketSIT(dict["data"].ToString());
            }

            if (HasProcessed(GetType(), player, lsp))
                return;

            if (!player.TryGetComponent<PlayerReplicatedComponent>(out var prc))
                return;

            if (CallLocally.ContainsKey(player.ProfileId))
                return;

            CallLocally.Add(player.ProfileId, true);

            if (player.HandsController is EFT.Player.FirearmController firearmCont)
            {
                try
                {
                    firearmCont.SetLightsState(new LightsStates[1] { new LightsStates() { Id = lsp.Id, IsActive = lsp.IsActive, LightMode = lsp.LightMode } });
                }
                catch (Exception e)
                {
                    Logger.LogInfo(e);
                }
            }
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
