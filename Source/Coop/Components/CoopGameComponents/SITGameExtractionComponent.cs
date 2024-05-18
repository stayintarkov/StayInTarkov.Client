using BepInEx.Logging;
using Comfort.Common;
using EFT.Counters;
using EFT;
using EFT.Interactive;
using HarmonyLib.Tools;
using StayInTarkov.Coop.Players;
using StayInTarkov.Coop.SITGameModes;
using StayInTarkov.Coop.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using StayInTarkov.Networking;
using StayInTarkov.Coop.NetworkPacket.Raid;

namespace StayInTarkov.Coop.Components.CoopGameComponents
{
    public sealed class SITGameExtractionComponent : MonoBehaviour
    {
        ManualLogSource Logger { get; set; }
        HashSet<string> ExtractedProfilesSent {get;set;} = new HashSet<string>();


        void Awake()
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(nameof(SITGameExtractionComponent));
        }

        void Update()
        {
            if (!Singleton<ISITGame>.Instantiated)
                return;

            if (!Singleton<GameWorld>.Instantiated)
                return;

            ProcessExtractingPlayers();
            ProcessExtractionRequirements();
            HideExtractedPlayers();
           
        }

        private void HideExtractedPlayers()
        {
            var world = Singleton<GameWorld>.Instance;
            var gameInstance = Singleton<ISITGame>.Instance;

            // Hide extracted Players
            foreach (var profileId in gameInstance.ExtractedPlayers)
            {
                var player = world.RegisteredPlayers.Find(x => x.ProfileId == profileId) as EFT.Player;
                if (player == null)
                    continue;

                if (!ExtractedProfilesSent.Contains(profileId))
                {
                    ExtractedProfilesSent.Add(profileId);
                    if (player.Profile.Side == EPlayerSide.Savage)
                    {
                        player.Profile.EftStats.SessionCounters.AddDouble(0.01,
                        [
                            CounterTag.FenceStanding,
                                    EFenceStandingSource.ExitStanding
                        ]);
                    }
                    // Send the Extracted Packet to other Clients
                    GameClient.SendData(new ExtractedPlayerPacket(profileId).Serialize());
                }

                if (player.ActiveHealthController != null)
                {
                    if (!player.ActiveHealthController.MetabolismDisabled)
                    {
                        player.ActiveHealthController.AddDamageMultiplier(0);
                        player.ActiveHealthController.SetDamageCoeff(0);
                        player.ActiveHealthController.DisableMetabolism();
                        player.ActiveHealthController.PauseAllEffects();
                    }
                }

                //force close all screens to disallow looting open crates after extract
                if (profileId == world.MainPlayer.ProfileId)
                {
                    ScreenManager instance = ScreenManager.Instance;
                    instance.CloseAllScreensForced();
                }

                PlayerUtils.MakeVisible(player, false);
            }
        }

        private void ProcessExtractionRequirements()
        {
            var gameInstance = Singleton<ISITGame>.Instance;
            // Trigger all countdown exfils (e.g. car), clients are responsible for their own extract
            // since exfilpoint.Entered is local because of collision logic being local
            // we start from the end because we remove as we go in `CoopSITGame.ExfiltrationPoint_OnStatusChanged`
            for (int i = gameInstance.EnabledCountdownExfils.Count - 1; i >= 0; i--)
            {
                var ep = gameInstance.EnabledCountdownExfils[i];
                if (gameInstance.PastTime - ep.ExfiltrationStartTime >= ep.Settings.ExfiltrationTime)
                {
                    var game = Singleton<ISITGame>.Instance;
                    foreach (var player in ep.Entered)
                    {
                        var hasUnmetRequirements = ep.UnmetRequirements(player).Any();
                        if (player != null && player.HealthController.IsAlive && !hasUnmetRequirements)
                        {
                            game.ExtractingPlayers.Remove(player.ProfileId);
                            game.ExtractedPlayers.Add(player.ProfileId);
                        }
                    }
                    ep.SetStatusLogged(ep.Reusable ? EExfiltrationStatus.UncompleteRequirements : EExfiltrationStatus.NotPresent, nameof(ProcessExtractionRequirements));
                }
            }
        }

        private void ProcessExtractingPlayers()
        {
            var gameInstance = Singleton<ISITGame>.Instance;
            var playersToExtract = new HashSet<string>();
            foreach (var exfilPlayer in gameInstance.ExtractingPlayers)
            {
                var exfilTime = new TimeSpan(0, 0, (int)exfilPlayer.Value.Item1);
                var timeInExfil = new TimeSpan(DateTime.Now.Ticks - exfilPlayer.Value.Item2);
                if (timeInExfil >= exfilTime)
                {
                    if (!playersToExtract.Contains(exfilPlayer.Key))
                    {
#if DEBUG
                        Logger.LogDebug(exfilPlayer.Key + " should extract");
#endif
                        playersToExtract.Add(exfilPlayer.Key);
                    }
                }
#if DEBUG
                else
                {
                    Logger.LogDebug(exfilPlayer.Key + " extracting " + timeInExfil);
                }
#endif
            }

            foreach (var player in playersToExtract)
            {
                gameInstance.ExtractingPlayers.Remove(player);
                gameInstance.ExtractedPlayers.Add(player);
            }
        }
    }
}
