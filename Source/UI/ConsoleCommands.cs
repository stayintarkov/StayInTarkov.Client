using Comfort.Common;
using EFT;
using EFT.Console.Core;
using EFT.UI;
using StayInTarkov.Coop.Components.CoopGameComponents;
using StayInTarkov.Coop.SITGameModes;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static StayInTarkov.Networking.SITSerialization;

namespace StayInTarkov.UI
{
    public class ConsoleCommands
    {
#if DEBUG
        [ConsoleCommand("mark", "", null, "Save current position as a teleport location", [])]
        public static void MarkCommand([ConsoleArgument("user", "name of teleport location")] string name)
        {
            try
            {
                if (Singleton<ISITGame>.Instance is CoopSITGame game)
                {
                    var filePath = tpPath(game, name);
                    using var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 4096, useAsync: true);
                    var writer = new BinaryWriter(fs);
                    var player = Singleton<GameWorld>.Instance.MainPlayer;
                    writer.Write(player.Position);
                    ConsoleScreen.Log($"Saved {filePath}.");
                }
                else
                {
                    ConsoleScreen.LogError($"Could not find a suitable {nameof(CoopSITGame)}.");
                }
            }
            catch (Exception ex)
            {
                ConsoleScreen.LogError(ex.ToString());
            }
        }

        [ConsoleCommand("recall", "", null, "Teleport user to desired teleport location", ["tp"])]
        public static void RecallCommand([ConsoleArgument("user", "name of teleport location")] string name)
        {
            try
            {
                if (Singleton<ISITGame>.Instance is CoopSITGame game)
                {
                    var filePath = tpPath(game, name);
                    using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.None, bufferSize: 4096, useAsync: true);
                    var reader = new BinaryReader(fs);
                    var player = Singleton<GameWorld>.Instance.MainPlayer;
                    player.Teleport(Vector3Utils.Deserialize(reader));
                }
                else
                {
                    ConsoleScreen.LogError($"Could not find a suitable {nameof(CoopSITGame)}.");
                }
            }
            catch (Exception ex)
            {
                ConsoleScreen.LogError(ex.ToString());
            }
        }

        [ConsoleCommand("heal", "", null, "Heal current player to full", [])]
        public static void Heal()
        {
            try
            {
                var player = Singleton<GameWorld>.Instance?.MainPlayer;
                if (player == null)
                {
                    EFT.UI.ConsoleScreen.LogError("could not find player");
                    return;
                }

                EFT.UI.ConsoleScreen.Log($"Healing {player.Profile.Nickname}...");
                foreach (EBodyPart bodyPart in Enum.GetValues(typeof(EBodyPart)))
                {
                    var hc = player.ActiveHealthController;
                    var cur = hc.GetBodyPartHealth(bodyPart).Current;
                    var max = hc.GetBodyPartHealth(bodyPart).Maximum;

                    EFT.UI.ConsoleScreen.Log($"Healing body part {bodyPart} from {cur} to {max}");
                    hc.FullRestoreBodyPart(bodyPart);
                }
            }
            catch (Exception ex)
            {
                ConsoleScreen.LogError(ex.ToString());
            }
        }

        [ConsoleCommand("damagehistory", "", null, "Dump damage history to console", [])]
        public static void DamageHistory()
        {
            try
            {
                var stats = Singleton<GameWorld>.Instance?.MainPlayer.StatisticsManager;
                if (stats == null)
                {
                    EFT.UI.ConsoleScreen.LogError($"could not find stats");
                }

                var dmgHistory = (DamageHistory)ReflectionHelpers.GetFieldFromType(stats.GetType(), "damageHistory_0").GetValue(stats);

                EFT.UI.ConsoleScreen.Log($"Dumping history {dmgHistory}");
                foreach (var kv in dmgHistory.BodyParts)
                {
                    EFT.UI.ConsoleScreen.Log($"Body part {kv.Key}");

                    foreach (var stat in kv.Value)
                    {
                        EFT.UI.ConsoleScreen.Log($"  {stat} {stat.Amount}dmg");
                    }
                }
            }
            catch (Exception ex)
            {
                ConsoleScreen.LogError(ex.ToString());
            }
        }

        [ConsoleCommand("players", "", null, "Dump player list to console", [])]
        public static void Players()
        {
            try
            {
                var gamecomp = SITGameComponent.GetCoopGameComponent();
                if (gamecomp != null)
                {
                    EFT.UI.ConsoleScreen.Log($"PLAYERS -------------------------");
                    foreach (var kv in gamecomp.Players)
                    {
                        var p = kv.Value;
                        EFT.UI.ConsoleScreen.Log($"{p.ProfileId} Name={p.Profile.Nickname} IsAI={p.IsAI} IsYourPlayer={p.IsYourPlayer}");
                    }
                    EFT.UI.ConsoleScreen.Log($"PLAYER USERS -------------------------");
                    foreach (var p in gamecomp.PlayerUsers)
                    {
                        EFT.UI.ConsoleScreen.Log($"{p.ProfileId} Name={p.Profile.Nickname} IsAI={p.IsAI} IsYourPlayer={p.IsYourPlayer}");
                    }
                    EFT.UI.ConsoleScreen.Log($"PLAYER CLIENTS -------------------------");
                    foreach (var p in gamecomp.PlayerClients)
                    {
                        EFT.UI.ConsoleScreen.Log($"{p.ProfileId} Name={p.Profile.Nickname} IsAI={p.IsAI} IsYourPlayer={p.IsYourPlayer}");
                    }
                    EFT.UI.ConsoleScreen.Log($"PLAYER BOTS -------------------------");
                    foreach (var p in gamecomp.PlayerBots)
                    {
                        EFT.UI.ConsoleScreen.Log($"{p.ProfileId} Name={p.Profile.Nickname} IsAI={p.IsAI} IsYourPlayer={p.IsYourPlayer}");
                    }
                }
                else
                {
                    EFT.UI.ConsoleScreen.LogError($"could not find game component");
                    return;
                }
            }
            catch (Exception ex)
            {
                ConsoleScreen.LogError(ex.ToString());
            }
        }

        private static string tpPath(CoopSITGame game, string name)
        {
            return $"user\\teleport_{game.Location_0.Name}_{name}.bin";
        }
#endif
    }
}
