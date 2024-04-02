using Comfort.Common;
using EFT;
using EFT.Console.Core;
using EFT.UI;
using StayInTarkov.Coop.SITGameModes;
using System;
using System.Collections.Generic;
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
        [ConsoleCommand("mark", "", null, "Save current position as a teleport location", new string[] {})]
        public static void MarkCommand([ConsoleArgument("user", "name of teleport location")] string name)
        {
            Mark(name);
        }

        [ConsoleCommand("recall", "", null, "Teleport user to desired teleport location", new string[] { "tp" })]
        public static void RecallCommand([ConsoleArgument("user", "name of teleport location")] string name)
        {
            Recall(name);
        }

        public static void Mark(string name)
        {
            if (Singleton<ISITGame>.Instance is CoopSITGame game)
            {
                var filePath = tpPath(game, name);
                using var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 4096, useAsync: true);
                var writer = new BinaryWriter(fs);
                var player = Singleton<GameWorld>.Instance.MainPlayer;
                writer.Write(player.Position);
                ConsoleScreen.Log($"Saved {filePath}.");
            } else
            {
                ConsoleScreen.LogError($"Could not find a suitable {nameof(CoopSITGame)}.");
            }
        }

        public static void Recall(string name)
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

        private static string tpPath(CoopSITGame game, string name)
        {
            return $"..\\..\\dev\\teleport_{game.Location_0.Name}_{name}.bin";
        }
#endif
    }
}
