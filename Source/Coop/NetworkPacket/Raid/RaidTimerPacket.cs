using EFT.UI.BattleTimer;
using StayInTarkov.Coop.Components.CoopGameComponents;
using StayInTarkov.Coop.Matchmaker;
using StayInTarkov.Coop.SITGameModes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace StayInTarkov.Coop.NetworkPacket.Raid
{
    internal sealed class RaidTimerPacket : BasePacket
    {
        public long SessionTime { get; set; }

        public RaidTimerPacket() : base(nameof(RaidTimerPacket))
        {
        }

        public override byte[] Serialize()
        {
            var ms = new MemoryStream();
            using BinaryWriter writer = new BinaryWriter(ms);
            WriteHeader(writer);
            writer.Write(SessionTime);
            return ms.ToArray();
        }

        public override ISITPacket Deserialize(byte[] bytes)
        {
            using BinaryReader reader = new BinaryReader(new MemoryStream(bytes));
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

            if (coopGameComponent.LocalGameInstance is CoopSITGame coopGame)
            {
                var gameTimer = coopGame.GameTimer;
                if (gameTimer.StartDateTime.HasValue && gameTimer.SessionTime.HasValue)
                {
                    if (gameTimer.PastTime.TotalSeconds < 3)
                        return;

                    var timeRemain = gameTimer.PastTime + sessionTime;

                    if (Math.Abs(gameTimer.SessionTime.Value.TotalSeconds - timeRemain.TotalSeconds) < 5)
                        return;

                    StayInTarkovHelperConstants.Logger.LogInfo($"RaidTimer: New SessionTime {timeRemain.TraderFormat()}");
                    gameTimer.ChangeSessionTime(timeRemain);

                    MainTimerPanel mainTimerPanel = ReflectionHelpers.GetFieldOrPropertyFromInstance<MainTimerPanel>(coopGame.GameUi.TimerPanel, "_mainTimerPanel", false);
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
