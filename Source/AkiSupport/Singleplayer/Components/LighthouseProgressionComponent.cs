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
        private GameWorld _gameWorld;
        private Player _player;
        private float _timer;
        private List<MineDirectional> _bridgeMines;
        private RecodableItemClass _transmitter;
        private readonly List<IPlayer> _zryachiyAndFollowers = new List<IPlayer>();
        private bool _aggressor;
        private bool _isDoorDisabled;
        private readonly string _transmitterId = "62e910aaf957f2915e0a5e36";
        private readonly string _lightKeeperTid = "638f541a29ffd1183d187f57";

        public void Start()
        {
            _gameWorld = Singleton<GameWorld>.Instance;
            _player = _gameWorld?.MainPlayer;

            if (_gameWorld == null || _player == null)
            {
                Destroy(this);

                return;
            }


            // Get transmitter from players inventory
            _transmitter = GetTransmitterFromInventory();

            // Exit if transmitter does not exist and isnt green
            if (!PlayerHasActiveTransmitterInInventory())
            {
                Destroy(this);

                return;
            }

            var places = Singleton<IBotGame>.Instance.BotsController.CoversData.AIPlaceInfoHolder.Places;

            places.First(x => x.name == "Attack").gameObject.SetActive(false);

            // Zone was added in a newer version and the gameObject actually has a \
            places.First(y => y.name == "CloseZone\\").gameObject.SetActive(false);

            // Give access to Lightkeepers door
            _gameWorld.BufferZoneController.SetPlayerAccessStatus(_player.ProfileId, true);

            _bridgeMines = _gameWorld.MineManager.Mines;

            // Set mines to be non-active
            SetBridgeMinesStatus(false);
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
            if (_gameWorld == null || _isDoorDisabled || _transmitter == null)
            {
                return;
            }

            // Find Zryachiy and prep him
            if (_zryachiyAndFollowers.Count == 0)
            {
                SetupZryachiyAndFollowerHostility();
            }

            // If player becomes aggressor, block access to LK
            if (_aggressor)
            {
                DisableAccessToLightKeeper();
            }
        }

        /// <summary>
        /// Gets transmitter from players inventory
        /// </summary>
        private RecodableItemClass GetTransmitterFromInventory()
        {
            return (RecodableItemClass)_player.Profile.Inventory.AllRealPlayerItems.FirstOrDefault(x => x.TemplateId == _transmitterId);
        }

        /// <summary>
        /// Checks for transmitter status and exists in players inventory
        /// </summary>
        private bool PlayerHasActiveTransmitterInInventory()
        {
            return _transmitter != null &&
                   _transmitter?.RecodableComponent?.Status == RadioTransmitterStatus.Green;
        }

        /// <summary>
        /// Update _time to diff from last run of update()
        /// </summary>
        private void IncrementLastUpdateTimer()
        {
            _timer += Time.deltaTime;
        }

        /// <summary>
        /// Set all brdige mines to desire state
        /// </summary>
        /// <param name="active">What state mines should be</param>
        private void SetBridgeMinesStatus(bool active)
        {

            // Find mines with opposite state of what we want
            foreach (var mine in _bridgeMines.Where(mine => mine.gameObject.activeSelf == !active && mine.transform.parent.gameObject.name == "Directional_mines_LHZONE"))
            {
                mine.gameObject.SetActive(active);
            }
        }

        /// <summary>
        /// Put Zryachiy and followers into a list and sub to their death event
        /// Make player agressor if player kills them.
        /// </summary>
        private void SetupZryachiyAndFollowerHostility()
        {
            // only process non-players (ai)
            foreach (var aiBot in _gameWorld.AllAlivePlayersList.Where(x => !x.IsYourPlayer))
            {
                // Bots that die on mounted guns get stuck in AllAlivePlayersList, need to check health
                if (!aiBot.HealthController.IsAlive)
                {
                    continue;
                }

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
