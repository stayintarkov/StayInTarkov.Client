using BepInEx.Logging;
using Comfort.Common;
using EFT;
using EFT.UI.BattleTimer;
using HarmonyLib.Tools;
using StayInTarkov.Coop.Components.CoopGameComponents;
using StayInTarkov.Coop.Matchmaker;
using StayInTarkov.Coop.SITGameModes;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace StayInTarkov.Coop.NetworkPacket.Raid
{
    internal sealed class RaidTimerPacket : BasePacket
    {
        static RaidTimerPacket()
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(nameof(RaidTimerPacket));
        }

        public long SessionTime { get; set; }
        public static ManualLogSource Logger { get; }

        public RaidTimerPacket() : base(nameof(RaidTimerPacket))
        {
        }

        public override byte[] Serialize()
        {
            var ms = new MemoryStream();
            using BinaryWriter writer = new(ms);
            WriteHeader(writer);
            writer.Write(SessionTime);
            return ms.ToArray();
        }

        public override ISITPacket Deserialize(byte[] bytes)
        {
            using BinaryReader reader = new(new MemoryStream(bytes));
            ReadHeader(reader);
            SessionTime = reader.ReadInt64();
            return this;
        }

        public override void Process()
        {
            SITGameComponent coopGameComponent = SITGameComponent.GetCoopGameComponent();
            if (coopGameComponent == null)
                return;

            if (!SITMatchmaking.IsClient)
                return;

            var sessionTime = new TimeSpan(SessionTime);

            if (!Singleton<ISITGame>.Instantiated)
            {
                Logger.LogError($"{nameof(Process)} failed {nameof(ISITGame)} was not instantiated!");
                return;
            }

            if (!Singleton<AbstractGame>.Instantiated)
            {
                Logger.LogError($"{nameof(Process)} failed {nameof(AbstractGame)} was not instantiated!");
                return;
            }

            var sitGame = Singleton<ISITGame>.Instance;
            var abstractGame = Singleton<AbstractGame>.Instance;

            //if (coopGameComponent.LocalGameInstance is CoopSITGame coopGame)
            {
                var gameTimer = sitGame.GameTimer;
                if (gameTimer.StartDateTime.HasValue && gameTimer.SessionTime.HasValue)
                {
                    if (gameTimer.PastTime.TotalSeconds < 3)
                        return;

                    var timeRemain = gameTimer.PastTime + sessionTime;

                    if (Math.Abs(gameTimer.SessionTime.Value.TotalSeconds - timeRemain.TotalSeconds) < 5)
                        return;

                    StayInTarkovHelperConstants.Logger.LogInfo($"RaidTimer: New SessionTime {timeRemain.TraderFormat()}");
                    gameTimer.ChangeSessionTime(timeRemain);

                    MainTimerPanel mainTimerPanel = ReflectionHelpers.GetFieldOrPropertyFromInstance<MainTimerPanel>(abstractGame.GameUi.TimerPanel, "_mainTimerPanel", false);
                    if (mainTimerPanel != null)
                    {
                        FieldInfo extractionDateTimeField = ReflectionHelpers.GetFieldFromType(typeof(TimerPanel), "dateTime_0");
                        extractionDateTimeField.SetValue(mainTimerPanel, gameTimer.StartDateTime.Value.AddSeconds(timeRemain.TotalSeconds));

                        MethodInfo UpdateTimerMI = ReflectionHelpers.GetMethodForType(typeof(MainTimerPanel), "UpdateTimer");
                        UpdateTimerMI.Invoke(mainTimerPanel, new object[] { });
                    }
                }
            }
        }
    }
}
