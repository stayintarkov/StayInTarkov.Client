using EFT;
using EFT.Interactive;
using StayInTarkov.Coop.Components.CoopGameComponents;
using StayInTarkov.Coop.NetworkPacket.Raid;
using StayInTarkov.Networking;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace StayInTarkov.Coop.World
{
    internal class LootableContainer_Interact_Patch : ModulePatch
    {
        public static Type InstanceType => typeof(LootableContainer);

        public static string MethodName => "LootableContainer_Interact";

        protected override MethodBase GetTargetMethod()
        {
            return ReflectionHelpers.GetAllMethodsForType(InstanceType).FirstOrDefault(x => x.Name == "Interact" && x.GetParameters().Length == 1 && x.GetParameters()[0].Name == "interactionResult");
        }

        static ConcurrentBag<long> ProcessedCalls = new();

        protected static bool HasProcessed(Dictionary<string, object> dict)
        {
            var timestamp = long.Parse(dict["t"].ToString());

            if (!ProcessedCalls.Contains(timestamp))
            {
                ProcessedCalls.Add(timestamp);
                return false;
            }

            return true;
        }

        [PatchPrefix]
        public static bool Prefix(LootableContainer __instance)
        {
            return false;
        }

        [PatchPostfix]
        public static void Postfix(LootableContainer __instance, InteractionResult interactionResult)
        {
            //Dictionary<string, object> packet = new()
            //{
            //    { "t", DateTime.Now.Ticks.ToString("G") },
            //    { "serverId", SITGameComponent.GetServerId() },
            //    { "m", MethodName },
            //    { "lootableContainerId", __instance.Id },
            //    { "type", interactionResult.InteractionType.ToString() }
            //};

            LootableContainerInteractionPacket packet = new();
            packet.LootableContainerId = __instance.Id;
            packet.InteractionType = interactionResult.InteractionType; 

            GameClient.SendData(packet.Serialize());
        }

        public static void Replicated(Dictionary<string, object> packet)
        {
        }
    }
}