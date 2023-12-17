using StayInTarkov.Coop.PacketQueues;
using StayInTarkov.Networking.Packets;
using System.Collections.Generic;
using UnityEngine;

namespace StayInTarkov.Coop
{
    public class NetworkFirearmController : FirearmController
    {
        HealthPacketQueue queue = new(100);

        private class NetworkFirearmActioneer : AbstractFirearmActioner
        {
            NetworkFirearmController NetworkFirearmController { get; set; }

            private NetworkFirearmActioneer(FirearmController controller) : base(controller)
            {
                NetworkFirearmController = controller as NetworkFirearmController;
            }
        }
        
    }
}
