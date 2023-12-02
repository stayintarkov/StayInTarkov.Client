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

        public static HashSet<string> CallLocally = new();


        [PatchPrefix]
        public static bool PrePatch(
            EFT.Player.FirearmController __instance, EFT.Player ____player, ScopeStates[] scopeStates)
        {
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
           EFT.Player.FirearmController __instance, EFT.Player ____player, ScopeStates[] scopeStates)
        {
            var player = ____player;
            if (player == null)
                return;

            if (CallLocally.Contains(player.ProfileId))
            {
                CallLocally.Remove(player.ProfileId);
                return;
            }

            foreach (var scope in scopeStates)
            {
                ScopeModePacket scopeModePacket = new(scope.Id, scope.ScopeMode, scope.ScopeIndexInsideSight, scope.ScopeCalibrationIndex, player.ProfileId);
                AkiBackendCommunication.Instance.SendDataToPool(scopeModePacket.Serialize());
            }

        }

        public override void Replicated(EFT.Player player, Dictionary<string, object> dict)
        {
            ScopeModePacket smp = new(null, 0, 0, 0, null);

            if (dict.ContainsKey("data"))
            {
                smp = smp.DeserializePacketSIT(dict["data"].ToString());
            }

            if (HasProcessed(GetType(), player, smp))
                return;

            if (!player.TryGetComponent<PlayerReplicatedComponent>(out var prc))
                return;

            if (CallLocally.Contains(player.ProfileId))
                return;            

            if (player.HandsController is EFT.Player.FirearmController firearmCont)
            {
                try
                {
                    CallLocally.Add(player.ProfileId);
                    firearmCont.SetScopeMode([new ScopeStates() { Id = smp.Id, ScopeMode = smp.ScopeMode, ScopeIndexInsideSight = smp.ScopeIndexInsideSight, ScopeCalibrationIndex = smp.ScopeCalibrationIndex }]);
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
