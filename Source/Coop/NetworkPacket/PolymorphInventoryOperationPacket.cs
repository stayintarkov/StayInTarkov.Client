using Comfort.Common;
using EFT;
using SIT.Core.Coop.PacketHandlers;
using StayInTarkov.Coop.Components.CoopGameComponents;
using StayInTarkov.Coop.NetworkPacket.Player;
using StayInTarkov.Coop.Players;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StayInTarkov.Coop.NetworkPacket
{
    /// <summary>
    /// A PolymorphInventoryOperationPacket is a packet that is sent when a character does anything with its inventory or loot
    /// See CoopInventoryController for more details
    /// </summary>
    public class PolymorphInventoryOperationPacket : ItemPlayerPacket
    {
        /// <summary>
        /// This is called via Reflection
        /// </summary>
        public PolymorphInventoryOperationPacket() : base("","","", nameof(PolymorphInventoryOperationPacket))
        {

        }

        /// <summary>
        /// This is called when creating the packet from the InventoryController
        /// </summary>
        /// <param name="profileId"></param>
        /// <param name="itemId"></param>
        /// <param name="templateId"></param>
        public PolymorphInventoryOperationPacket(string profileId, string itemId, string templateId) : base(profileId, itemId, templateId, nameof(PolymorphInventoryOperationPacket))
        {

        }

        /// <summary>
        /// This is called via Reflection
        /// Process this Packet by sending the packet back to the InventoryController to do its action
        /// If the Character doesn't exist (for whatever reason), then hold the packet until they do
        /// </summary>
        //public override void Process()
        //{
        //    if (Method != "PolymorphInventoryOperationPacket")
        //        return;

        //    StayInTarkovHelperConstants.Logger.LogDebug($"{GetType()}:{nameof(Process)}");

        //    if (SITGameComponent.TryGetCoopGameComponent(out var coopGameComponent))
        //    {
        //        // If the player exists, process
        //        if (coopGameComponent.Players.ContainsKey(ProfileId))
        //            PlayerInventoryPacketHandler.ProcessPolymorphOperation(coopGameComponent.Players[ProfileId], this);
        //        else
        //        {
        //            // If the player doesn't exist, hold the packet until they do exist
        //            Task.Run(async () =>
        //            {

        //                while (true)
        //                {
        //                    await Task.Delay(10 * 1000);

        //                    if (coopGameComponent.Players.ContainsKey(ProfileId))
        //                    {
        //                        PlayerInventoryPacketHandler.ProcessPolymorphOperation(coopGameComponent.Players[ProfileId], this);
        //                        break;
        //                    }
        //                }

        //            });
        //        }
        //        return;
        //    }
        //}

        public override void Process()
        {
            //base.Process();
            foreach(var p in Singleton<GameWorld>.Instance.AllAlivePlayersList)
            {
                if(p.ProfileId == ProfileId)
                {
                    PlayerInventoryPacketHandler.ProcessPolymorphOperation(p, this);
                    break;
                }
            }
        }

        //protected override void Process(CoopPlayerClient client)
        //{
        //    //base.Process(client);

        //    PlayerInventoryPacketHandler.ProcessPolymorphOperation(client, this);
        //}
    }
}
