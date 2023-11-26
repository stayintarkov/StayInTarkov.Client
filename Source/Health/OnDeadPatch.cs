using EFT;
using Newtonsoft.Json;
using StayInTarkov.Networking;
using StayInTarkov.UI;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace StayInTarkov.Health
{
    /// <summary>
    /// Created by Paulov
    /// Description: When a person dies in Raid (can be scav, player or boss) this patch records the death
    /// </summary>
    public class OnDeadPatch : ModulePatch
    {
        public static event Action<Player, EDamageType> OnPersonKilled;
        public static bool DisplayDeathMessage = true;

        public OnDeadPatch(BepInEx.Configuration.ConfigFile config)
        {
            var enableDeathMessage = config.Bind("Coop", "ShowFeed", true);
            if (enableDeathMessage != null)
            {
                DisplayDeathMessage = enableDeathMessage.Value;

            }
        }

        protected override MethodBase GetTargetMethod() => ReflectionHelpers.GetMethodForType(typeof(Player), "OnDead");

        [PatchPostfix]
        public static void PatchPostfix(Player __instance, EDamageType damageType)
        {
            Player victim = __instance;
            if (victim == null)
                return;

            OnPersonKilled?.Invoke(victim, damageType);

            var attacker = ReflectionHelpers.GetFieldOrPropertyFromInstance<Player>(victim, "LastAggressor", false);

            if (DisplayDeathMessage)
                DisplayMessageNotifications.DisplayMessageNotification(attacker != null ? $"\"{GeneratePlayerNameWithSide(attacker)}\" killed \"{GeneratePlayerNameWithSide(victim)}\"" : $"\"{GeneratePlayerNameWithSide(victim)}\" has died because of \"{("DamageType_" + damageType.ToString()).Localized()}\"");

            Dictionary<string, object> packet = new()
            {
                { "diedAID", victim.Profile.AccountId },
                { "diedProfileId", victim.ProfileId },
                { "diedFaction", victim.Side }
            };

            if (victim.Profile.Info != null && victim.Profile.Info.Settings != null)
                packet.Add("diedWST", victim.Profile.Info.Settings.Role);

            if (attacker != null)
            {
                packet.Add("killedByAID", attacker.Profile.AccountId);
                packet.Add("killedByProfileId", attacker.ProfileId);
                packet.Add("killerFaction", attacker.Side);
            }

            AkiBackendCommunication.Instance.PostJsonAndForgetAsync("/client/raid/person/killed", JsonConvert.SerializeObject(packet));
        }

        public static string GeneratePlayerNameWithSide(Player player)
        {
            if (player == null)
                return "";

            var side = "Scav";

            if (player.AIData.IAmBoss)
                side = "Boss";
            else if (player.Side != EPlayerSide.Savage)
                side = player.Side.ToString();

            return $"[{side}] {player.Profile.GetCorrectedNickname()}";
        }
    }
}
