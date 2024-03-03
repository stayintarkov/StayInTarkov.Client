﻿using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using StayInTarkov.Coop.Components.CoopGameComponents;
using StayInTarkov.Coop.NetworkPacket.Player.Health;
using StayInTarkov.Coop.Players;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace StayInTarkov.Coop.NetworkPacket.Player
{
    public sealed class ApplyDamagePacket : BasePlayerPacket
    {
        public ApplyDamagePacket() : base("", nameof(ApplyDamagePacket))
        {
        }

        public EDamageType DamageType { get; set; }

        public float Damage { get; set; }

        public EBodyPart BodyPart { get; set; }

        public EBodyPartColliderType ColliderType { get; set; }

        public float Absorbed { get; set; }

        public string AggressorProfileId { get; set; }
        public string AggressorWeaponId { get; set; }

        public override byte[] Serialize()
        {
            var ms = new MemoryStream();
            using BinaryWriter writer = new BinaryWriter(ms);
            WriteHeaderAndProfileId(writer);
            writer.Write((uint)DamageType);
            writer.Write(Damage);
            writer.Write((byte)BodyPart);
            writer.Write((byte)ColliderType);
            writer.Write(Absorbed);

            writer.Write(!string.IsNullOrEmpty(AggressorProfileId));
            if(!string.IsNullOrEmpty(AggressorProfileId))
            {
                writer.Write(AggressorProfileId);
                writer.Write(AggressorWeaponId);
            }
            return ms.ToArray();
        }

        public override ISITPacket Deserialize(byte[] bytes)
        {
            using BinaryReader reader = new BinaryReader(new MemoryStream(bytes));
            ReadHeaderAndProfileId(reader);
            DamageType = (EDamageType)reader.ReadUInt32();
            Damage = reader.ReadSingle();
            BodyPart = (EBodyPart)reader.ReadByte();
            ColliderType = (EBodyPartColliderType)reader.ReadByte();
            Absorbed = reader.ReadSingle();

            var hasAggressor = reader.ReadBoolean();
            if (hasAggressor)
            {
                AggressorProfileId = reader.ReadString();
                AggressorWeaponId = reader.ReadString();
            }

            return this;
        }

        public override void Process()
        {
            if (Method != nameof(ApplyDamagePacket))
                return;

            StayInTarkovHelperConstants.Logger.LogDebug($"{GetType()}:{nameof(Process)}");

            DamageInfo damageInfo = default(DamageInfo);
            damageInfo.Damage = this.Damage;
            damageInfo.DamageType = this.DamageType;
            damageInfo.BodyPartColliderType = this.ColliderType;
            if (!string.IsNullOrEmpty(AggressorProfileId))
            {
                var bridge = Singleton<GameWorld>.Instance.GetAlivePlayerBridgeByProfileID(AggressorProfileId);
                if (bridge != null)
                {
                    damageInfo.Player = bridge;
                    var aggressorPlayer = Singleton<GameWorld>.Instance.GetAlivePlayerByProfileID(AggressorProfileId);
                    if (aggressorPlayer == null)
                        aggressorPlayer = CoopGameComponent.GetCoopGameComponent().Players[AggressorProfileId];

                    if (aggressorPlayer.HandsController.Item is Weapon weapon)
                        damageInfo.Weapon = weapon;
                }
            }

            var bridgeOwner = Singleton<GameWorld>.Instance.GetAlivePlayerBridgeByProfileID(ProfileId);
            if (bridgeOwner == null)
            {
                StayInTarkovHelperConstants.Logger.LogError($"{GetType()}:{nameof(Process)}:Unable to find BridgeOwner for {ProfileId}");
                return;
            }

            var playerOwner = Singleton<GameWorld>.Instance.GetAlivePlayerByProfileID(ProfileId);
            if (playerOwner == null)
            {
                StayInTarkovHelperConstants.Logger.LogError($"{GetType()}:{nameof(Process)}:Unable to find PlayerOwner for {ProfileId}");
                return;
            }

            var coopPlayer = playerOwner as CoopPlayer;
            if (coopPlayer == null)
            {
                StayInTarkovHelperConstants.Logger.LogError($"{GetType()}:{nameof(Process)}:Unable to cast {ProfileId}:{playerOwner.GetType()} to CoopPlayer");
                return;
            }
            
            
            coopPlayer.ReceiveDamageFromServer(damageInfo, BodyPart, ColliderType, Absorbed);



        }

    }
}