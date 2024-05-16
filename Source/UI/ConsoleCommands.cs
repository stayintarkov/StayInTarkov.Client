using Comfort.Common;
using EFT;
using EFT.Console.Core;
using EFT.Interactive;
using EFT.UI;
using EFT.Weather;
using StayInTarkov.Coop.Components.CoopGameComponents;
using StayInTarkov.Coop.FreeCamera;
using StayInTarkov.Coop.SITGameModes;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Systems.Effects;
using UnityEngine;
using static StayInTarkov.Networking.SITSerialization;

namespace StayInTarkov.UI
{
    public class ConsoleCommands : MonoBehaviour
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
                ConsoleScreen.LogException(ex);
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
                ConsoleScreen.LogException(ex);
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

                var hc = player.ActiveHealthController;
                EFT.UI.ConsoleScreen.Log($"Healing {player.Profile.Nickname}...");
                foreach (EBodyPart bodyPart in Enum.GetValues(typeof(EBodyPart)))
                {
                    if (bodyPart == EBodyPart.Common)
                    {
                        continue;
                    }

                    var cur = hc.GetBodyPartHealth(bodyPart).Current;
                    var max = hc.GetBodyPartHealth(bodyPart).Maximum;

                    EFT.UI.ConsoleScreen.Log($"Healing body part {bodyPart} from {cur} to {max}");
                    hc.FullRestoreBodyPart(bodyPart);
                }
                hc.ChangeEnergy(100);
                hc.ChangeHydration(100);
            }
            catch (Exception ex)
            {
                ConsoleScreen.LogException(ex);
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
                ConsoleScreen.LogException(ex);
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
                ConsoleScreen.LogException(ex);
            }
        }
        [ConsoleCommand("weather", "", null, "Dump weather properties", [])]
        public static void weather()
        {
            try
            {
                var weatherDebug = EFT.Weather.WeatherController.Instance.WeatherDebug;
                PropertyInfo[] properties = typeof(WeatherDebug).GetProperties();

                foreach (PropertyInfo property in properties)
                {
                    object value = property.GetValue(weatherDebug);
                    EFT.UI.ConsoleScreen.Log($"{property.Name}: {value}");
                }
            }
            catch (Exception ex)
            {
                ConsoleScreen.LogException(ex);
            }
        }

        [ConsoleCommand("freecam", "", null, "Activates / Deactivates freecam", [])]
        public static void Freecam()
        {
            try
            {
                //FreeCameraController CameraController = Singleton<GameWorld>.Instance.gameObject.GetOrAddComponent<FreeCameraController>();
                //CameraController.ToggleCamera();
            }
            catch (Exception ex)
            {
                ConsoleScreen.LogException(ex);
            }
        }

        [ConsoleCommand("freecam.toggleui", "", null, "Activates / Deactivates freecam's ability to hide the UI", [])]
        public static void Freecam_ToggleUI()
        {
            try
            {
                //FreeCameraController CameraController = Singleton<GameWorld>.Instance.gameObject.GetOrAddComponent<FreeCameraController>();
                //CameraController.ToggleUi();
            }
            catch (Exception ex)
            {
                ConsoleScreen.LogException(ex);
            }
        }

        [ConsoleCommand("unlockd", "", null, "Unlock doors that can be opened", [])]
        public static void UnlockDoors()
        {
            try
            {
                foreach (Door door in BSGUnityHelper.FindUnityObjectsOfType<Door>())
                {
                    if ((door.DoorState == EDoorState.Locked && !string.IsNullOrEmpty(door.KeyId)) || door.DoorState == EDoorState.Interacting)
                    {
                        door.DoorState = EDoorState.Shut;
                    }
                }
            }
            catch (Exception ex)
            {
                ConsoleScreen.LogException(ex);
            }
        }

        [ConsoleCommand("despawnbots", "", null, "Despawns all AI players", [])]
        public static void DespawnBots()
        {
            try
            {
                if (Singleton<ISITGame>.Instance is CoopSITGame game)
                {
                    foreach (var bot in game.PBotsController.Players.ToList())
                    {
                        if (bot.AIData.BotOwner == null)
                        {
                            continue;
                        }

                        //Taken from SWAG + DONUTS TO replicate this behavior
                        //Credit to dvize & p-kossa for their amazing work
                        //For reference: https://github.com/dvize/Donuts/blob/727e1de7c2a0714432ab03947deca2bfd5fad699/DonutComponent.cs#L388
                        ConsoleScreen.Log($"Despawning bot: {bot.Profile.Info.Nickname}");

                        BotOwner botOwner = bot.AIData.BotOwner;

                        Singleton<Effects>.Instance.EffectsCommutator.StopBleedingForPlayer(botOwner.GetPlayer);
                        botOwner.Deactivate();
                        botOwner.Dispose();
                        game.PBotsController.BotDied(botOwner);
                        game.PBotsController.DestroyInfo(botOwner.GetPlayer);
                        DestroyImmediate(botOwner.gameObject);
                        Destroy(botOwner);
                    }
                }
            }
            catch (Exception ex)
            {
                ConsoleScreen.LogException(ex);
            }
        }

        [ConsoleCommand("teleportbots", "", null, "Teleports all AI players to the person running this command", [])]
        public static void TeleportBots()
        {
            try
            {
                if (Singleton<ISITGame>.Instance is CoopSITGame game)
                {
                    var player = Singleton<GameWorld>.Instance?.MainPlayer;
                    UnityEngine.Vector3 playerPositon = player.Position;

                    foreach (var bot in game.PBotsController.Players.ToList())
                    {
                        if (bot.AIData.BotOwner == null)
                        {
                            continue;
                        }

                        BotOwner botOwner = bot.AIData.BotOwner;
                        botOwner.GetPlayer.Teleport(playerPositon);
                    }
                }
            }
            catch (Exception ex)
            {
                ConsoleScreen.LogException(ex);
            }
        }

        private static string tpPath(CoopSITGame game, string name)
        {
            return $"user\\teleport_{game.Location_0.Name}_{name}.bin";
        }
#endif
    }
}
