using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace StayInTarkov.Coop.Components
{
    internal class RemoteBrain : MonoBehaviour
    {
        public List<ObservedPlayerController> observedPlayers;
        public GStruct256[] Models = new GStruct256[256];

        public void ManualLateUpdate()
        {
            for (int i = 0; i < observedPlayers.Count; i++) 
            {
                observedPlayers[i].Apply(Models[i]);
            }
        }
    }
}
