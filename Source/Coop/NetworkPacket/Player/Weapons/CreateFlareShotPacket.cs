/**
 * This file is written and licensed by Paulov (https://github.com/paulov-t) for Stay in Tarkov (https://github.com/stayintarkov)
 * You are not allowed to reproduce this file in any other project
 */

using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using StayInTarkov.Coop.Controllers.HandControllers;
using StayInTarkov.Coop.Players;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static StayInTarkov.Networking.SITSerialization;

namespace StayInTarkov.Coop.NetworkPacket.Player.Weapons
{
    public sealed class CreateFlareShotPacket : BasePlayerPacket
    {

        public Vector3 ShotPosition { get; set; }

        public Vector3 ShotForward { get; set; }

        public string FlareTemplateId { get; set; }


        public CreateFlareShotPacket() : base("", nameof(CreateFlareShotPacket))
        {
        }

        public CreateFlareShotPacket(string profileId, Vector3 shotPosition, Vector3 shotForward, string flareTpl) : base(profileId, nameof(CreateFlareShotPacket))
        {
            this.ShotPosition = shotPosition;
            this.ShotForward = shotForward;
            this.FlareTemplateId = flareTpl;
        }

        public override byte[] Serialize()
        {
            var ms = new MemoryStream();
            using BinaryWriter writer = new BinaryWriter(ms);
            WriteHeaderAndProfileId(writer);
            Vector3Utils.Serialize(writer, ShotPosition);
            Vector3Utils.Serialize(writer, ShotForward);
            writer.Write(FlareTemplateId);
            return ms.ToArray();
        }

        public override ISITPacket Deserialize(byte[] bytes)
        {
            using BinaryReader reader = new BinaryReader(new MemoryStream(bytes));  
            ReadHeaderAndProfileId(reader);
            ShotPosition = Vector3Utils.Deserialize(reader);
            ShotForward = Vector3Utils.Deserialize(reader);
            FlareTemplateId = reader.ReadString();
            return this;
        }

        protected override void Process(CoopPlayerClient client)
        {
            StayInTarkovHelperConstants.Logger.LogInfo($"{nameof(CreateFlareShotPacket)}.{nameof(Process)}(client)");

            if (client.HandsController is SITFirearmControllerClient firearmControllerClient)
            {
                StayInTarkovHelperConstants.Logger.LogInfo(firearmControllerClient);
                StayInTarkovHelperConstants.Logger.LogInfo(firearmControllerClient.Weapon);
                MagazineClass currentMagazine = firearmControllerClient.Weapon.GetCurrentMagazine();
                if (currentMagazine != null)
                {
                    StayInTarkovHelperConstants.Logger.LogInfo(currentMagazine);

                }
                //var pic = ItemFinder.GetPlayerInventoryController(client);

                BulletClass flareItem = (BulletClass)Singleton<ItemFactory>.Instance.CreateItem(MongoID.Generate(), this.FlareTemplateId, null);
                firearmControllerClient.InitiateFlare(flareItem, this.ShotPosition, this.ShotForward);
            }
        }

    }
}
