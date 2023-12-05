using Comfort.Common;
using EFT;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace StayInTarkov.Coop.Components
{
    internal class RemoteBrain : MonoBehaviour
    {
        public List<ObservedPlayerController> observedPlayers;
        public GStruct256[] Models = new GStruct256[256];

        public void Update()
        {
            if (observedPlayers != null)
            {
                foreach (var player in observedPlayers)
                {
                    player.ManualUpdate();
                }
            }
        }

        public void Awake()
        {
            var gameWorld = Singleton<GameWorld>.Instance;

            EFT.UI.ConsoleScreen.Log("Brain::Awake Start");

            if (gameWorld.allObservedPlayersByID != null && gameWorld.allObservedPlayersByID.Count > 0)
            {
                EFT.UI.ConsoleScreen.Log("Brain::Awake Found ObservedPlayers");
                foreach (var player in gameWorld.allObservedPlayersByID)
                {
                    observedPlayers.Add(player.Value.ObservedPlayerController);
                }
            }
        }

        public void ManualUpdate()
        {
            for (int i = 0; i < observedPlayers.Count; i++)
            {
                Models[i] = observedPlayers[i].Model;
                observedPlayers[i].ManualUpdate();
            }
        }

        public void ManualLateUpdate()
        {
            for (int i = 0; i < observedPlayers.Count; i++)
            {
                observedPlayers[i].Apply(Models[i]);
            }
        }

        public void Test(GStruct256 package)
        {
            Singleton<GameWorld>.Instance.allObservedPlayersByID.First().Value.ObservedPlayerController.Apply(package);
        }
    }
}
