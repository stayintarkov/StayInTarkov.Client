using EFT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StayInTarkov.Coop.NetworkPacket.Lacyway
{
    internal struct HandsChangePacket
    {
        public EOperationType OperationType;
        public uint CallbackId;
        public string ItemId;
        public EBodyPart MedsBodyPart;
        public float MedsAmount;
        public int AnimationVariant;
    }

    public enum EOperationType
    {
        None,
        Drop,
        FastDrop,
        CreateEmptyHands,
        CreateFirearm,
        CreateGrenade,
        CreateMeds,
        CreateKnife,
        CreateQuickGrenadeThrow,
        CreateQuickKnifeKick,
        CreateQuickUseItem,
        CreateUsableItem,
        DropAndCreateEmptyHands,
        DropAndCreateFirearm,
        DropAndCreateGrenade,
        DropAndCreateMeds,
        DropAndCreateKnife,
        DropAndCreateQuickGrenadeThrow,
        DropAndCreateQuickKnifeKick,
        DropAndCreateQuickUseItem,
        DropAndCreateUsableItem
    }
}
