using EFT.InventoryLogic;
using EFT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using EFT.NextObservedPlayer;
using EFT.Ballistics;
using Comfort.Common;
using System.IO;
using UnityEngine.Networking;

namespace StayInTarkov.Coop.NetworkPacket.Player
{
    /// <summary>
    /// Paulov: I have based this on GMessage2, which is used during ObservedPlayerView creation
    /// </summary>
    public sealed class PlayerInformationPacket : BasePlayerPacket
    {
        public PlayerInformationPacket() { }

        public float RemoteTime;

        public Vector3 BodyPosition;

        public Dictionary<EBodyModelPart, string> Customization;

        public EPlayerSide Side;

        public WildSpawnType WildSpawnType;

        public string GroupID;

        public string TeamID;

        public bool IsAI;

        public string ProfileID;

        public string Voice;

        public EFT.Player.EVoipState VoIPState;

        public string NickName;

        public string AccountId;

        public EArmorPlateCollider ArmorPlateColliderMask;

        public Inventory Inventory;

        public HandsCommandMessage HandsController;

        public List<ArmorInfo> ArmorsInfo;

        //public ArenaObservedPlayerSpawnMessage ArenaObservedPlayerSpawnMessage;

        

        public override byte[] Serialize()
        {
            using var ms = new MemoryStream();
            using BinaryWriter writer = new BinaryWriter(ms);
            WriteHeaderAndProfileId(writer);
            writer.Write(RemoteTime);   
            writer.Write(BodyPosition);   
            SerializeCustomization(writer);
            writer.Write((byte)Side);
            writer.Write((byte)WildSpawnType);
            writer.Write(GroupID);
            writer.Write(TeamID);
            writer.Write(IsAI);
            writer.Write(ProfileID);
            writer.Write(Voice);
            writer.Write((byte)VoIPState);
            writer.Write(NickName);
            writer.Write(AccountId);
            writer.Write((short)ArmorPlateColliderMask);
            writer.Write(InventorySerializationHelpers.SerializeInventory(Inventory));
            SerializeInventory(writer);
            //HandsController.Serialize(writer);
            SerializeArmorsInfo(writer);
            // This has been #if cleared
            //ArenaObservedPlayerSpawnMessage.Serialize(writer);

            return ms.ToArray();
        }

        public override ISITPacket Deserialize(byte[] bytes)
        {
            using BinaryReader reader = new BinaryReader(new MemoryStream(bytes));
            RemoteTime = reader.ReadFloat();
            BodyPosition = reader.ReadVector3();
            DeserializeCustomization(reader);
            Side = (EPlayerSide)reader.ReadByte();
            WildSpawnType = (WildSpawnType)reader.ReadByte();
            GroupID = reader.ReadString();
            TeamID = reader.ReadString();
            IsAI = reader.ReadBool();
            ProfileID = reader.ReadString();
            Voice = reader.ReadString();
            VoIPState = (EFT.Player.EVoipState)reader.ReadByte();
            NickName = reader.ReadString();
            AccountId = reader.ReadString();
            ArmorPlateColliderMask = (EArmorPlateCollider)reader.ReadShort();
            Inventory = InventorySerializationHelpers.DeserializeInventory(Singleton<ItemFactory>.Instance, reader.ReadEFTInventoryDescriptor());
            //HandsController = GClass2310.GetCommand<HandsCommandMessage>(CommandMessageType.SetHands);
            //HandsController.Deserialize(reader);
            DeserializeArmorsInfo(reader);
            //ArenaObservedPlayerSpawnMessage.Deserialize(reader);
            return base.Deserialize(bytes);
        }

        public void Deserialize(BSGNetworkReader reader)
        {
            //RemoteTime = GClass1079.ReadFloat(reader);
            //BodyPosition = GClass1079.ReadVector3(reader);
            //DeserializeCustomization(reader);
            //Side = (EPlayerSide)reader.ReadByte();
            //WildSpawnType = (WildSpawnType)reader.ReadByte();
            //GroupID = GClass1079.ReadString(reader);
            //TeamID = GClass1079.ReadString(reader);
            //IsAI = GClass1079.ReadBool(reader);
            //ProfileID = GClass1079.ReadString(reader);
            //Voice = GClass1079.ReadString(reader);
            //VoIPState = (EFT.Player.EVoipState)reader.ReadByte();
            //NickName = GClass1079.ReadString(reader);
            //AccountId = GClass1079.ReadString(reader);
            //ArmorPlateColliderMask = (EArmorPlateCollider)GClass1079.ReadShort(reader);
            //Inventory = GClass1524.DeserializeInventory(Singleton<ItemFactory>.Instance, GClass2952.ReadEFTInventoryDescriptor(reader));
            //HandsController = GClass2310.GetCommand<HandsCommandMessage>(CommandMessageType.SetHands);
            //HandsController.Deserialize(reader);
            //DeserializeArmorsInfo(reader);
            //ArenaObservedPlayerSpawnMessage.Deserialize(reader);
        }

        public void SerializeInventory(BinaryWriter writer)
        {
            InventoryDescriptor target = new InventoryDescriptor
            {
                //Equipment = InventorySerializationHelpers.SerializeOnlyVisibleEquipment(Inventory.Equipment)
                Equipment = InventorySerializationHelpers.SerializeOnlyVisibleEquipment(Inventory.Equipment)
            };
            writer.WriteEFTInventoryDescriptor(target);
        }

        public void SerializeCustomization(BinaryWriter writer)
        {
            writer.Write((byte)Customization.Count);
            foreach (var (eBodyModelPart2, id) in Customization)
            {
                writer.Write((byte)eBodyModelPart2);
                var mngoId = new MongoID(id);
                mngoId.Write(writer);
            }
        }

        public void DeserializeCustomization(BinaryReader reader)
        {
            byte b = reader.ReadByte();
            Customization = new Dictionary<EBodyModelPart, string>(b);
            for (int i = 0; i < b; i++)
            {
                Customization.Add((EBodyModelPart)reader.ReadByte(), MongoID.Read(reader));
            }
        }

        public void SerializeArmorsInfo(BinaryWriter writer)
        {
            writer.Write((byte)ArmorsInfo.Count);
            foreach (ArmorInfo item in ArmorsInfo)
            {
                writer.Write(item.itemID);
                writer.Write((byte)item.armorType);
                writer.Write(item.maxDurability);
                writer.Write(item.durability);
                writer.Write(item.templateDurability);
                writer.Write(item.ricochetValues);
                writer.Write(item.armorClass);
                writer.Write((byte)item.material);
                writer.Write(item.armorColliders.Count);
                foreach (EBodyPartColliderType armorCollider in item.armorColliders)
                {
                    writer.Write((byte)armorCollider);
                }
                writer.Write((short)item.armorPlateColliderMask);
                writer.Write(item.isComposite);
                if (item.isComposite)
                {
                    writer.Write(item.isToggeldAndOff);
                }
            }
        }

        public void DeserializeArmorsInfo(BinaryReader reader)
        {
            byte b = reader.ReadByte();
            ArmorsInfo = new List<ArmorInfo>(b);
            for (int i = 0; i < b; i++)
            {
                ArmorInfo armorInfo = new ArmorInfo
                {
                    itemID = reader.ReadString(),
                    armorType = (EArmorType)reader.ReadByte(),
                    maxDurability = reader.ReadFloat(),
                    durability = reader.ReadFloat(),
                    templateDurability = reader.ReadInt(),
                    ricochetValues = reader.ReadVector3(),
                    armorClass = reader.ReadInt(),
                    material = (MaterialType)reader.ReadByte()
                };
                int num = reader.ReadInt();
                if (num > 0)
                {
                    armorInfo.armorColliders = new List<EBodyPartColliderType>(num);
                    for (int j = 0; j < num; j++)
                    {
                        armorInfo.armorColliders.Add((EBodyPartColliderType)reader.ReadByte());
                    }
                }
                armorInfo.armorPlateColliderMask = (EArmorPlateCollider)reader.ReadShort();
                armorInfo.isComposite = reader.ReadBool();
                if (!armorInfo.isComposite)
                {
                    ArmorsInfo.Add(armorInfo);
                    continue;
                }
                armorInfo.isToggeldAndOff = reader.ReadBool();
                ArmorsInfo.Add(armorInfo);
            }
        }
    }
}
