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
using StayInTarkov.Coop.Players;
using Newtonsoft.Json;
using StayInTarkov.ThirdParty;
using MonoMod.Utils;
using StayInTarkov.Coop.Matchmaker;
using ComponentAce.Compression.Libs.zlib;
using static EFT.Profile;

namespace StayInTarkov.Coop.NetworkPacket.Player
{
    /// <summary>
    /// Paulov: I have based this on GMessage2, which is used during ObservedPlayerView creation
    /// This is the Packet that holds the data that is used to spawn characters/players on clients.
    /// </summary>
    public sealed class PlayerInformationPacket : BasePlayerPacket
    {
        public PlayerInformationPacket() : base("", nameof(PlayerInformationPacket))
        {

        }

        public PlayerInformationPacket(string profileId) : base(new string(profileId.ToCharArray()), nameof(PlayerInformationPacket))
        {
            //ProfileID = profileId;
        }

        public float RemoteTime;

        public Vector3 BodyPosition;

        public Dictionary<EBodyModelPart, string> Customization;

        public EPlayerSide Side;

        public WildSpawnType WildSpawnType;

        public string GroupID;

        public string TeamID;

        public bool IsAI;

        //public string ProfileID;

        public string Voice;

        public EFT.Player.EVoipState VoIPState;

        public string NickName;

        public string AccountId;

        public EArmorPlateCollider ArmorPlateColliderMask;

        public EFT.InventoryLogic.Inventory Inventory;

        public HandsCommandMessage HandsController;

        public List<ArmorInfo> ArmorsInfo { get; set; } = new List<ArmorInfo>();

        /// <summary>
        /// TODO: Remove this. Current spawn system requires this.
        /// </summary>
        private EFT.Profile profile;

        public EFT.Profile Profile
        {
            get { return profile; }
            set 
            { 
                profile = value;
                //profile = value.Clone(); 
                //profile.EftStats = new ProfileStats();
                //profile.TradersInfo = new ();
                //profile.AchievementsData = new ();
                //profile.Bonuses = [];
                //profile.CheckedChambers = new ();
                //profile.CheckedMagazines = new ();
                //profile.Encyclopedia = new ();
                //profile.Info.GroupId = SITMatchmaking.GetGroupId();
                //profile.InsuredItems = [];

            }
        }


        public EFT.Profile.ProfileHealth ProfileHealth { get; set; }

        //public ArenaObservedPlayerSpawnMessage ArenaObservedPlayerSpawnMessage;

        public string InitialInventoryMongoId { get; set; } 


        public override byte[] Serialize()
        {
            using var ms = new MemoryStream();
            using BinaryWriter writer = new BinaryWriter(ms);
            WriteHeaderAndProfileId(writer);
            writer.Write(RemoteTime);   
            writer.Write(BodyPosition);   
            //SerializeCustomization(writer);
            writer.Write((byte)Side);
            writer.Write((byte)WildSpawnType);
            writer.Write(GroupID);
            writer.Write(TeamID);
            writer.Write(IsAI);
            //writer.Write(ProfileID);
            writer.Write(Voice);
            writer.Write((byte)VoIPState);
            writer.Write(NickName);
            writer.Write(AccountId);
            writer.Write((short)ArmorPlateColliderMask);

            writer.Write(InitialInventoryMongoId);

            //writer.Write(InventorySerializationHelpers.SerializeInventory(Inventory));
            //SerializeInventory(writer);
            //HandsController.Serialize(writer);
            //SerializeArmorsInfo(writer);
            // This has been #if cleared
            //ArenaObservedPlayerSpawnMessage.Serialize(writer);

            // TODO: Remove this. Current spawn system requires this.
            writer.Write(Profile != null);
            if (Profile != null)
                writer.WriteLengthPrefixedBytes(Encoding.Unicode.GetBytes(Profile.SITToJson()));
            // TODO: Remove this. Current spawn system requires this.
            writer.Write(ProfileHealth != null);
            if (ProfileHealth != null)
                writer.WriteLengthPrefixedBytes(Encoding.UTF8.GetBytes(ProfileHealth.SITToJson()));

            return ms.ToArray();
        }

        public override ISITPacket Deserialize(byte[] bytes)
        {
            using BinaryReader reader = new BinaryReader(new MemoryStream(bytes));
            ReadHeaderAndProfileId(reader);
            RemoteTime = reader.ReadFloat();
            BodyPosition = reader.ReadVector3();
            //DeserializeCustomization(reader);
            Side = (EPlayerSide)reader.ReadByte();
            WildSpawnType = (WildSpawnType)reader.ReadByte();
            GroupID = reader.ReadString();
            TeamID = reader.ReadString();
            IsAI = reader.ReadBool();
            //ProfileID = reader.ReadString();
            Voice = reader.ReadString();
            VoIPState = (EFT.Player.EVoipState)reader.ReadByte();
            NickName = reader.ReadString();
            AccountId = reader.ReadString();
            ArmorPlateColliderMask = (EArmorPlateCollider)reader.ReadShort();

            InitialInventoryMongoId = reader.ReadString();
            //Inventory = InventorySerializationHelpers.DeserializeInventory(Singleton<ItemFactory>.Instance, reader.ReadEFTInventoryDescriptor());
            //HandsController = GClass2310.GetCommand<HandsCommandMessage>(CommandMessageType.SetHands);
            //HandsController.Deserialize(reader);
            //DeserializeArmorsInfo(reader);
            //ArenaObservedPlayerSpawnMessage.Deserialize(reader);
            DeserializeProfile(reader);

            //var profileHealthString = reader.ReadString();
            //StayInTarkovHelperConstants.Logger.LogInfo($"{profileHealthString}");
            //if (profileHealthString.TrySITParseJson(out EFT.Profile.ProfileHealth ph))
            //    ProfileHealth = ph;
            //else
            //    StayInTarkovHelperConstants.Logger.LogError($"Unable to Process: {ProfileId}");

            return this;
        }

        private void DeserializeProfile(BinaryReader reader)
        {
            if (!reader.ReadBoolean())
                return;

            if (reader.BaseStream.Position >= reader.BaseStream.Length)
                return;

            var profileBytes = reader.ReadLengthPrefixedBytes();
            var profileString = Encoding.Unicode.GetString(profileBytes);
            //StayInTarkovHelperConstants.Logger.LogInfo($"{profileString}");
            if (profileString.TrySITParseJson(out EFT.Profile p))
                Profile = p;
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

        public override void Process()
        {
            StayInTarkovHelperConstants.Logger.LogInfo($"{nameof(PlayerInformationPacket)}.{nameof(Process)}");
            base.Process();
        }

        protected override void Process(CoopPlayerClient client)
        {
            StayInTarkovHelperConstants.Logger.LogInfo($"{nameof(PlayerInformationPacket)}.{nameof(Process)}(client)");
            base.Process(client);
        }
    }
}
