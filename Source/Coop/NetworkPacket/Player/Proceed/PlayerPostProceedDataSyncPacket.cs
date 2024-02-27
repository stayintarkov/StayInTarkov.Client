using EFT.InventoryLogic;
using StayInTarkov.Coop.Components.CoopGameComponents;
using StayInTarkov.Coop.Players;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static GClass648;

namespace StayInTarkov.Coop.NetworkPacket.Player.Proceed
{
    public sealed class PlayerPostProceedDataSyncPacket : BasePlayerPacket
    {
        public PlayerPostProceedDataSyncPacket() : base("", nameof(PlayerPostProceedDataSyncPacket)) { }

        public PlayerPostProceedDataSyncPacket(string profileId) : base(new string(profileId.ToCharArray()), nameof(PlayerPostProceedDataSyncPacket))
        {

        }

        public PlayerPostProceedDataSyncPacket(string profileId, string itemId, float newValue) : this(new string(profileId.ToCharArray()))
        {
            ProfileId = profileId;
            ItemId = itemId;
            NewValue = newValue;
        }

        public string ItemId { get; set; }

        public float NewValue { get; set; }

        public override byte[] Serialize()
        {
            var ms = new MemoryStream();
            using BinaryWriter writer = new BinaryWriter(ms);
            WriteHeader(writer);
            writer.Write(ProfileId);
            writer.Write(ItemId);
            writer.Write(NewValue);
            writer.Write(TimeSerializedBetter);

            return ms.ToArray();
        }

        public override ISITPacket Deserialize(byte[] bytes)
        {
            using BinaryReader reader = new BinaryReader(new MemoryStream(bytes));
            ReadHeader(reader);
            ProfileId = reader.ReadString();
            ItemId = reader.ReadString();
            NewValue = reader.ReadSingle();
            TimeSerializedBetter = reader.ReadString();

            return this;
        }

        public override void Process()
        {
            if (Method != nameof(PlayerPostProceedDataSyncPacket))
                return;

            StayInTarkovHelperConstants.Logger.LogDebug($"{GetType()}:{nameof(Process)}");

            StayInTarkovPlugin.Instance.StartCoroutine(ProceedCoroutine());
        }

        private IEnumerator ProceedCoroutine()
        {
            bool done = false;
            while (!done)
            {
                if (!CoopGameComponent.TryGetCoopGameComponent(out var coopGameComponent))
                    break;

                if (coopGameComponent.Players.ContainsKey(ProfileId) && coopGameComponent.Players[ProfileId] is CoopPlayerClient client)
                {
                    if (ItemFinder.TryFindItem(this.ItemId, out Item item))
                    {
                        yield return new WaitForEndOfFrame();

                        if(item is MedsClass meds)
                        {
                            meds.MedKitComponent.HpResource = this.NewValue;
                            meds.RaiseRefreshEvent();
                        }

                        done = true;
                        break;
                    }
                }
                else
                    break;

                yield return new WaitForSeconds(10);
            }

        }

    }
}
