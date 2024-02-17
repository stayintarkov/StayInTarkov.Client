using Comfort.Common;
using EFT;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace StayInTarkov.AkiSupport.Singleplayer.Components
{
    /// <summary>
    /// Original class writen by SPT-Aki Devs. 
    /// </summary>
    public class LighthouseProgressionComponent : MonoBehaviour
    {
        private bool _isScav;
        private GameWorld _gameWorld;
        private Player _player;
        private float _timer;
        private bool _playerFlaggedAsEnemyToBosses;
        private List<MineDirectionalColliders> _bridgeMines;
        private RecodableItemClass _transmitter;
        private readonly List<IPlayer> _zryachiyAndFollowers = new();
        private bool _aggressor;
        private bool _isDoorDisabled;
        private readonly string _transmitterId = "62e910aaf957f2915e0a5e36";
        private readonly string _lightKeeperTid = "638f541a29ffd1183d187f57";

        public void Start()
        {
            _gameWorld = Singleton<GameWorld>.Instance;
            _player = _gameWorld?.MainPlayer;

            // Exit if not on lighthouse
            if (_gameWorld == null || !string.Equals(_player.Location, "lighthouse", System.StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            // Expensive, run after gameworld / lighthouse checks above
            _bridgeMines = FindObjectsOfType<MineDirectionalColliders>().ToList();

            // Player is a scav, exit
            if (_player.Side == EPlayerSide.Savage)
            {
                _isScav = true;

                return;
            }

            _transmitter = GetTransmitterFromInventory();
            if (PlayerHasTransmitterInInventory())
            {
                GameObject.Find("Attack").SetActive(false);

                // Zone was added in a newer version and the gameObject actually has a \
                GameObject.Find("CloseZone\\").SetActive(false);

                // Give access to Lightkeepers door
                _gameWorld.BufferZoneController.SetPlayerAccessStatus(_player.ProfileId, true);
            }
        }

        public void Update()
        {
            IncrementLastUpdateTimer();

            // Exit early if last update() run time was < 10 secs ago
            if (_timer < 10f)
            {
                return;
            }

            // Skip if:
            // GameWorld missing
            // Player not an enemy to Zryachiy
            // Lk door not accessible
            // Player has no transmitter on thier person
            if (_gameWorld == null || _playerFlaggedAsEnemyToBosses || _isDoorDisabled || _transmitter == null)
            {
                return;
            }

            // Find Zryachiy and prep him
            if (_zryachiyAndFollowers.Count == 0)
            {
                SetupZryachiyAndFollowerHostility();
            }

            if (_isScav)
            {
                MakeZryachiyAndFollowersHostileToPlayer();

                return;
            }

            // (active/green)
            if (PlayerHasActiveTransmitterInHands())
            {
                SetBridgeMinesStatus(false);
            }
            else
            {
                SetBridgeMinesStatus(true);
            }

            if (_aggressor)
            {
                DisableAccessToLightKeeper();
            }
        }

        private RecodableItemClass GetTransmitterFromInventory()
        {
            return (RecodableItemClass)_player.Profile.Inventory.AllRealPlayerItems.FirstOrDefault(x => x.TemplateId == _transmitterId);
        }

        private bool PlayerHasTransmitterInInventory()
        {
            return _transmitter != null;
        }

        /// <summary>
        /// Update _time to diff from last run of update()
        /// </summary>
        private void IncrementLastUpdateTimer()
        {
            _timer += Time.deltaTime;
        }

        private bool PlayerHasActiveTransmitterInHands()
        {
            return _gameWorld?.MainPlayer?.HandsController?.Item?.TemplateId == _transmitterId
                && _transmitter?.RecodableComponent?.Status == RadioTransmitterStatus.Green;
        }

        /// <summary>
        /// Set all brdige mines to desire state
        /// </summary>
        /// <param name="active">What state mines should be</param>
        private void SetBridgeMinesStatus(bool active)
        {
            // Find mines with opposite state of what we want
            foreach (var mine in _bridgeMines.Where(mine => mine.gameObject.activeSelf == !active))
            {
                mine.gameObject.SetActive(active);
            }
        }

        private void SetupZryachiyAndFollowerHostility()
        {
            // only process non-players (ai)
            foreach (var aiBot in _gameWorld.AllAlivePlayersList.Where(x => !x.IsYourPlayer))
            {
                // Edge case of bossZryachiy not being hostile to player
                if (aiBot.AIData.BotOwner.IsRole(WildSpawnType.bossZryachiy) || aiBot.AIData.BotOwner.IsRole(WildSpawnType.followerZryachiy))
                {
                    // Subscribe to bots OnDeath event
                    aiBot.OnPlayerDeadOrUnspawn += player1 =>
                    {
                        // If player kills zryachiy or follower, force aggressor state
                        // Also set players Lk standing to negative (allows access to quest chain (Making Amends))
                        if (player1?.KillerId == _player?.ProfileId)
                        {
                            _aggressor = true;
                            _player?.Profile.TradersInfo[_lightKeeperTid].SetStanding(-0.01);
                        }
                    };

                    // Save bot to list for later access
                    if (!_zryachiyAndFollowers.Contains(aiBot))
                    {
                        _zryachiyAndFollowers.Add(aiBot);
                    }
                }
            }
        }

        /// <summary>
        /// Iterate over bots gathered from SetupZryachiyHostility()
        /// </summary>
        private void MakeZryachiyAndFollowersHostileToPlayer()
        {
            // If player is a scav, they must be added to the bosses enemy list otherwise they wont kill them
            foreach (var bot in _zryachiyAndFollowers)
            {
                bot.AIData.BotOwner.BotsGroup.CheckAndAddEnemy(_player);
            }

            // Flag player was added to enemy list
            _playerFlaggedAsEnemyToBosses = true;
        }

        /// <summary>
        /// Disable door + set transmitter to 'red'
        /// </summary>
        private void DisableAccessToLightKeeper()
        {
            // Disable access to Lightkeepers door for the player
            _gameWorld.BufferZoneController.SetPlayerAccessStatus(_gameWorld.MainPlayer.ProfileId, false);
            _transmitter?.RecodableComponent?.SetStatus(RadioTransmitterStatus.Yellow);
            _transmitter?.RecodableComponent?.SetEncoded(false);
            _isDoorDisabled = true;
        }
    }
}
