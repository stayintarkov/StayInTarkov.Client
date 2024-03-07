using Comfort.Common;
using ComponentAce.Compression.Libs.zlib;
using EFT;
using EFT.InventoryLogic;
using LiteNetLib.Utils;
using Newtonsoft.Json;
using StayInTarkov.Coop.NetworkPacket;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Security.Policy;
using System.Xml;
using UnityEngine;
using UnityEngine.Profiling;
using static Physical;
using static StayInTarkov.Networking.SITSerialization;

// GClass2800

namespace StayInTarkov.Networking
{
    public class SITSerialization
    {
        public class Vector3Utils
        {
            public static void Serialize(NetDataWriter writer, Vector3 vector)
            {
                writer.Put(vector.x);
                writer.Put(vector.y);
                writer.Put(vector.z);
            }

            public static Vector3 Deserialize(NetDataReader reader)
            {
                return new Vector3(reader.GetFloat(), reader.GetFloat(), reader.GetFloat());
            }

            public static void Serialize(BinaryWriter writer, Vector3 vector)
            {
                writer.Write(vector.x);
                writer.Write(vector.y);
                writer.Write(vector.z);
            }

            public static Vector3 Deserialize(BinaryReader reader)
            {
                return new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
            }
        }

        public class Vector2Utils
        {
            public static void Serialize(NetDataWriter writer, Vector2 vector)
            {
                writer.Put(vector.x);
                writer.Put(vector.y);
            }

            public static Vector2 Deserialize(NetDataReader reader)
            {
                return new Vector2(reader.GetFloat(), reader.GetFloat());
            }

            public static void Serialize(BinaryWriter writer, Vector2 vector)
            {
                writer.Write(vector.x);
                writer.Write(vector.y);
            }

            public static Vector2 Deserialize(BinaryReader reader)
            {
                return new Vector2(reader.ReadSingle(), reader.ReadSingle());
            }
        }

        public class PhysicalUtils
        {
            public static void Serialize(NetDataWriter writer, Physical.PhysicalStamina physicalStamina)
            {
                writer.Put(physicalStamina.StaminaExhausted);
                writer.Put(physicalStamina.OxygenExhausted);
                writer.Put(physicalStamina.HandsExhausted);
            }

            public static void Serialize(BinaryWriter writer, Physical.PhysicalStamina physicalStamina)
            {
                writer.Write(physicalStamina.StaminaExhausted);
                writer.Write(physicalStamina.OxygenExhausted);
                writer.Write(physicalStamina.HandsExhausted);
            }

            public static Physical.PhysicalStamina Deserialize(NetDataReader reader)
            {
                return new Physical.PhysicalStamina() { StaminaExhausted = reader.GetBool(), OxygenExhausted = reader.GetBool(), HandsExhausted = reader.GetBool() };
            }

            public static Physical.PhysicalStamina Deserialize(BinaryReader reader)
            {
                return new Physical.PhysicalStamina() { StaminaExhausted = reader.ReadBoolean(), OxygenExhausted = reader.ReadBoolean(), HandsExhausted = reader.ReadBoolean() };
            }
        }

        public static class AddressUtils
        {
            public static void SerializeGridItemAddressDescriptor(NetDataWriter writer, GridItemAddressDescriptor gridItemAddressDescriptor)
            {
                SerializeLocationInGrid(writer, gridItemAddressDescriptor.LocationInGrid);
                SerializeContainerDescriptor(writer, gridItemAddressDescriptor.Container);
            }

            public static void SerializeGridItemAddressDescriptor(BinaryWriter writer, GridItemAddressDescriptor gridItemAddressDescriptor)
            {
                SerializeLocationInGrid(writer, gridItemAddressDescriptor.LocationInGrid);
                SerializeContainerDescriptor(writer, gridItemAddressDescriptor.Container);
            }

            public static GridItemAddressDescriptor DeserializeGridItemAddressDescriptor(NetDataReader reader)
            {
                return new GridItemAddressDescriptor()
                {
                    LocationInGrid = DeserializeLocationInGrid(reader),
                    Container = DeserializeContainerDescriptor(reader)
                };
            }

            public static GridItemAddressDescriptor DeserializeGridItemAddressDescriptor(BinaryReader reader)
            {
                return new GridItemAddressDescriptor()
                {
                    LocationInGrid = DeserializeLocationInGrid(reader),
                    Container = DeserializeContainerDescriptor(reader)
                };
            }

            public static void SerializeContainerDescriptor(NetDataWriter writer, ContainerDescriptor containerDescriptor)
            {
                writer.Put(containerDescriptor.ParentId);
                writer.Put(containerDescriptor.ContainerId);
            }

            public static void SerializeContainerDescriptor(BinaryWriter writer, ContainerDescriptor containerDescriptor)
            {
                writer.Write(containerDescriptor.ParentId);
                writer.Write(containerDescriptor.ContainerId);
            }

            public static ContainerDescriptor DeserializeContainerDescriptor(NetDataReader reader)
            {
                return new ContainerDescriptor()
                {
                    ParentId = reader.GetString(),
                    ContainerId = reader.GetString(),
                };
            }

            public static ContainerDescriptor DeserializeContainerDescriptor(BinaryReader reader)
            {
                return new ContainerDescriptor()
                {
                    ParentId = reader.GetString(),
                    ContainerId = reader.GetString(),
                };
            }

            //public static void SerializeItemInGridDescriptor(NetDataWriter writer, ItemInGridDescriptor itemInGridDescriptor)
            //{
            //    GClass1040 polyWriter = new();
            //    SerializeLocationInGrid(writer, itemInGridDescriptor.Location);
            //    SerializeItemDescriptor(polyWriter, itemInGridDescriptor.Item);
            //    writer.Put(polyWriter.ToArray());
            //}

            //public static ItemInGridDescriptor DeserializeItemInGridDescriptor(NetDataReader reader)
            //{
            //    GClass1035 polyReader = new(reader.RawData);
            //    return new ItemInGridDescriptor()
            //    {
            //        Location = DeserializeLocationInGrid(reader),
            //        Item = DeserializeItemDescriptor(polyReader)
            //    };
            //}

            public static void SerializeLocationInGrid(NetDataWriter writer, LocationInGrid locationInGrid)
            {
                writer.Put(locationInGrid.x);
                writer.Put(locationInGrid.y);
                writer.Put((int)locationInGrid.r);
                writer.Put(locationInGrid.isSearched);
            }

            public static void SerializeLocationInGrid(BinaryWriter writer, LocationInGrid locationInGrid)
            {
                writer.Put(locationInGrid.x);
                writer.Put(locationInGrid.y);
                writer.Put((int)locationInGrid.r);
                writer.Put(locationInGrid.isSearched);
            }

            public static LocationInGrid DeserializeLocationInGrid(NetDataReader reader)
            {
                return new LocationInGrid()
                {
                    x = reader.GetInt(),
                    y = reader.GetInt(),
                    r = (ItemRotation)reader.GetInt(),
                    isSearched = reader.GetBool()
                };
            }

            public static LocationInGrid DeserializeLocationInGrid(BinaryReader reader)
            {
                return new LocationInGrid()
                {
                    x = reader.GetInt(),
                    y = reader.GetInt(),
                    r = (ItemRotation)reader.GetInt(),
                    isSearched = reader.GetBool()
                };
            }

            //public static void SerializeItemDescriptor(GClass1040 writer, ItemDescriptor itemDescriptor)
            //{
            //    writer.WriteString(itemDescriptor.Id);
            //    writer.WriteString(itemDescriptor.TemplateId);
            //    writer.WriteInt(itemDescriptor.StackCount);
            //    writer.WriteBool(itemDescriptor.SpawnedInSession);
            //    writer.WriteByte(itemDescriptor.ActiveCamora);
            //    writer.WriteBool(itemDescriptor.IsUnderBarrelDeviceActive);
            //    for (int i = 0; i < itemDescriptor.Components.Count; i++)
            //    {
            //        writer.WritePolymorph(itemDescriptor.Components[i]);
            //    }
            //    writer.WriteInt(itemDescriptor.Slots.Count);
            //    for (int j = 0; j < itemDescriptor.Slots.Count; j++)
            //    {
            //        writer.WriteEFTSlotDescriptor(itemDescriptor.Slots[j]);
            //    }
            //    writer.WriteInt(itemDescriptor.ShellsInWeapon.Count);
            //    for (int k = 0; k < itemDescriptor.ShellsInWeapon.Count; k++)
            //    {
            //        writer.WriteEFTShellTemplateDescriptor(itemDescriptor.ShellsInWeapon[k]);
            //    }
            //    writer.WriteInt(itemDescriptor.ShellsInUnderbarrelWeapon.Count);
            //    for (int l = 0; l < itemDescriptor.ShellsInUnderbarrelWeapon.Count; l++)
            //    {
            //        writer.WriteEFTShellTemplateDescriptor(itemDescriptor.ShellsInUnderbarrelWeapon[l]);
            //    }
            //    writer.WriteInt(itemDescriptor.Grids.Count);
            //    for (int m = 0; m < itemDescriptor.Grids.Count; m++)
            //    {
            //        writer.WriteEFTGridDescriptor(itemDescriptor.Grids[m]);
            //    }
            //    writer.WriteInt(itemDescriptor.StackSlots.Count);
            //    for (int n = 0; n < itemDescriptor.StackSlots.Count; n++)
            //    {
            //        writer.WriteEFTStackSlotDescriptor(itemDescriptor.StackSlots[n]);
            //    }
            //    writer.WriteInt(itemDescriptor.Malfunction.Count);
            //    for (int num = 0; num < itemDescriptor.Malfunction.Count; num++)
            //    {
            //        writer.WriteEFTMalfunctionDescriptor(itemDescriptor.Malfunction[num]);
            //    }
            //}

            //public static ItemDescriptor DeserializeItemDescriptor(GClass1035 reader)
            //{
            //    ItemDescriptor itemDescriptor = new();
            //    itemDescriptor.Id = reader.ReadString();
            //    itemDescriptor.TemplateId = reader.ReadString();
            //    itemDescriptor.StackCount = reader.ReadInt();
            //    itemDescriptor.SpawnedInSession = reader.ReadBool();
            //    itemDescriptor.ActiveCamora = reader.ReadByte();
            //    itemDescriptor.IsUnderBarrelDeviceActive = reader.ReadBool();
            //    int num = reader.ReadInt();
            //    itemDescriptor.Components = new List<AbstractDescriptor2>(num);
            //    for (int i = 0; i < num; i++)
            //    {
            //        itemDescriptor.Components.Add(reader.ReadPolymorph<AbstractDescriptor2>());
            //    }
            //    int num2 = reader.ReadInt();
            //    itemDescriptor.Slots = new List<SlotDescriptor>(num2);
            //    for (int j = 0; j < num2; j++)
            //    {
            //        itemDescriptor.Slots.Add(reader.ReadEFTSlotDescriptor());
            //    }
            //    int num3 = reader.ReadInt();
            //    itemDescriptor.ShellsInWeapon = new List<ShellTemplateDescriptor>(num3);
            //    for (int k = 0; k < num3; k++)
            //    {
            //        itemDescriptor.ShellsInWeapon.Add(reader.ReadEFTShellTemplateDescriptor());
            //    }
            //    int num4 = reader.ReadInt();
            //    itemDescriptor.ShellsInUnderbarrelWeapon = new List<ShellTemplateDescriptor>(num4);
            //    for (int l = 0; l < num4; l++)
            //    {
            //        itemDescriptor.ShellsInUnderbarrelWeapon.Add(reader.ReadEFTShellTemplateDescriptor());
            //    }
            //    int num5 = reader.ReadInt();
            //    itemDescriptor.Grids = new List<GridDescriptor>(num5);
            //    for (int m = 0; m < num5; m++)
            //    {
            //        itemDescriptor.Grids.Add(reader.ReadEFTGridDescriptor());
            //    }
            //    int num6 = reader.ReadInt();
            //    itemDescriptor.StackSlots = new List<StackSlotDescriptor>(num6);
            //    for (int n = 0; n < num6; n++)
            //    {
            //        itemDescriptor.StackSlots.Add(reader.ReadEFTStackSlotDescriptor());
            //    }
            //    int num7 = reader.ReadInt();
            //    itemDescriptor.Malfunction = new List<MalfunctionDescriptor>(num7);
            //    for (int num8 = 0; num8 < num7; num8++)
            //    {
            //        itemDescriptor.Malfunction.Add(reader.ReadEFTMalfunctionDescriptor());
            //    }
            //    return itemDescriptor;
            //}

            public static void SerializeSlotItemAddressDescriptor(NetDataWriter writer, SlotItemAddressDescriptor slotItemAddressDescriptor)
            {
                SerializeContainerDescriptor(writer, slotItemAddressDescriptor.Container);
            }

            public static SlotItemAddressDescriptor DeserializeSlotItemAddressDescriptor(NetDataReader reader)
            {
                return new SlotItemAddressDescriptor()
                {
                    Container = DeserializeContainerDescriptor(reader)
                };
            }

            public static void SerializeStackSlotItemAddressDescriptor(NetDataWriter writer, StackSlotItemAddressDescriptor stackSlotItemAddressDescriptor)
            {
                SerializeContainerDescriptor(writer, stackSlotItemAddressDescriptor.Container);
            }

            public static StackSlotItemAddressDescriptor DeserializeStackSlotItemAddressDescriptor(NetDataReader reader)
            {
                return new StackSlotItemAddressDescriptor()
                {
                    Container = DeserializeContainerDescriptor(reader)
                };
            }
        }

        //// Do not use
        //public class PlayerUtils
        //{
        //    public static void SerializeProfile(NetDataWriter writer, Profile profile)
        //    {
        //        byte[] profileBytes = SimpleZlib.CompressToBytes(profile.ToJson(), 9, null);
        //        writer.Put(profileBytes);
        //        EFT.UI.ConsoleScreen.Log(profileBytes.Length.ToString());
        //        Profile profile2 = SimpleZlib.Decompress(profileBytes, null).ParseJsonTo<Profile>();
        //        EFT.UI.ConsoleScreen.Log(profile2.ProfileId);
        //    }

        //    public static Profile DeserializeProfile(byte[] profileBytes)
        //    {
        //        Profile profile = SimpleZlib.Decompress(profileBytes, null).ParseJsonTo<Profile>();
        //        EFT.UI.ConsoleScreen.Log(profile.ToString());
        //        return profile;
        //    }

        //    public static void SerializeInventory(NetDataWriter writer, Inventory inventory)
        //    {
        //        InventoryDescriptor inventoryDescriptor = new InventoryDescriptor()
        //        {
        //            Equipment = GClass1463.SerializeItem(inventory.Equipment),
        //            Stash = GClass1463.SerializeItem(inventory.Stash),
        //            QuestRaidItems = GClass1463.SerializeItem(inventory.QuestRaidItems),
        //            QuestStashItems = GClass1463.SerializeItem(inventory.QuestStashItems),
        //            SortingTable = GClass1463.SerializeItem(inventory.SortingTable),
        //            FastAccess = GClass1463.SerializeFastAccess(inventory.FastAccess),
        //            DiscardLimits = GClass1463.SerializeDiscardLimits(inventory.DiscardLimits)
        //        };
        //        GClass1040 polyWriter = new();
        //        polyWriter.WriteEFTInventoryDescriptor(inventoryDescriptor);
        //        writer.Put(polyWriter.ToArray());
        //    }

        //    public static Inventory DeserializeInventory(byte[] inventoryBytes)
        //    {
        //        GClass1035 polyReader = new(inventoryBytes);
        //        Inventory inventory = GClass1463.DeserializeInventory(Singleton<ItemFactory>.Instance, polyReader.ReadEFTInventoryDescriptor());
        //        return inventory;
        //    }
        //}

        public struct PlayerInfoPacket()
        {
            public int ProfileLength { get; set; }
            public Profile Profile { get; set; }

            public static void Serialize(NetDataWriter writer, PlayerInfoPacket packet)
            {
                byte[] profileBytes = SimpleZlib.CompressToBytes(packet.Profile.ToJson(), 9, null);
                writer.Put(profileBytes.Length);
                writer.Put(profileBytes);
            }

            public static PlayerInfoPacket Deserialize(NetDataReader reader)
            {
                PlayerInfoPacket packet = new();
                packet.ProfileLength = reader.GetInt();
                byte[] profileBytes = new byte[packet.ProfileLength];
                reader.GetBytes(profileBytes, packet.ProfileLength);
                packet.Profile = SimpleZlib.Decompress(profileBytes, null).ParseJsonTo<Profile>();
                return packet;
            }
        }

        public struct LightStatesPacket
        {
            public int Amount { get; set; }
            public LightsStates[] LightStates { get; set; }
            public static LightStatesPacket Deserialize(NetDataReader reader)
            {
                LightStatesPacket packet = new();
                packet.Amount = reader.GetInt();
                if (packet.Amount > 0)
                {
                    packet.LightStates = new LightsStates[packet.Amount];
                    for (int i = 0; i < packet.Amount; i++)
                    {
                        packet.LightStates[i] = new()
                        {
                            Id = reader.GetString(),
                            IsActive = reader.GetBool(),
                            LightMode = reader.GetInt()
                        };
                    }
                }
                return packet;
            }

            public static void Serialize(NetDataWriter writer, LightStatesPacket packet)
            {
                writer.Put(packet.Amount);
                if (packet.Amount > 0)
                {
                    for (int i = 0; i < packet.Amount; i++)
                    {
                        writer.Put(packet.LightStates[i].Id);
                        writer.Put(packet.LightStates[i].IsActive);
                        writer.Put(packet.LightStates[i].LightMode);
                    }
                }
            }
        }

        public struct HeadLightsPacket
        {
            public int Amount { get; set; }
            public LightsStates[] LightStates { get; set; }
            public static HeadLightsPacket Deserialize(NetDataReader reader)
            {
                HeadLightsPacket packet = new();
                packet.Amount = reader.GetInt();
                if (packet.Amount > 0)
                {
                    packet.LightStates = new LightsStates[packet.Amount];
                    for (int i = 0; i < packet.Amount; i++)
                    {
                        packet.LightStates[i] = new()
                        {
                            Id = reader.GetString(),
                            IsActive = reader.GetBool(),
                            LightMode = reader.GetInt()
                        };
                    }
                }
                return packet;
            }

            public static void Serialize(NetDataWriter writer, HeadLightsPacket packet)
            {
                writer.Put(packet.Amount);
                if (packet.Amount > 0)
                {
                    for (int i = 0; i < packet.Amount; i++)
                    {
                        writer.Put(packet.LightStates[i].Id);
                        writer.Put(packet.LightStates[i].IsActive);
                        writer.Put(packet.LightStates[i].LightMode);
                    }
                }
            }
        }

        public struct ScopeStatesPacket
        {
            public int Amount { get; set; }
            public ScopeStates[] ScopeStates { get; set; }
            public static ScopeStatesPacket Deserialize(NetDataReader reader)
            {
                ScopeStatesPacket packet = new();
                packet.Amount = reader.GetInt();
                if (packet.Amount > 0)
                {
                    packet.ScopeStates = new ScopeStates[packet.Amount];
                    for (int i = 0; i < packet.Amount; i++)
                    {
                        packet.ScopeStates[i] = new()
                        {
                            Id = reader.GetString(),
                            ScopeMode = reader.GetInt(),
                            ScopeIndexInsideSight = reader.GetInt(),
                            ScopeCalibrationIndex = reader.GetInt()
                        };
                    }
                }
                return packet;
            }

            public static void Serialize(NetDataWriter writer, ScopeStatesPacket packet)
            {
                writer.Put(packet.Amount);
                if (packet.Amount > 0)
                {
                    for (int i = 0; i < packet.Amount; i++)
                    {
                        writer.Put(packet.ScopeStates[i].Id);
                        writer.Put(packet.ScopeStates[i].ScopeMode);
                        writer.Put(packet.ScopeStates[i].ScopeIndexInsideSight);
                        writer.Put(packet.ScopeStates[i].ScopeCalibrationIndex);
                    }
                }
            }
        }

        public struct ReloadMagPacket
        {
            public bool Reload { get; set; }
            public string MagId { get; set; }
            public int LocationLength { get; set; }
            public byte[] LocationDescription { get; set; }

            public static ReloadMagPacket Deserialize(NetDataReader reader)
            {
                ReloadMagPacket packet = new();
                packet.Reload = reader.GetBool();
                if (packet.Reload)
                {
                    packet.MagId = reader.GetString();
                    packet.LocationLength = reader.GetInt();
                    packet.LocationDescription = new byte[packet.LocationLength];
                    reader.GetBytes(packet.LocationDescription, packet.LocationLength);
                }
                return packet;
            }
            public static void Serialize(NetDataWriter writer, ReloadMagPacket packet)
            {
                writer.Put(packet.Reload);
                if (packet.Reload)
                {
                    writer.Put(packet.MagId);
                    writer.Put(packet.LocationLength);
                    writer.Put(packet.LocationDescription);
                }
            }
        }

        public struct QuickReloadMagPacket
        {
            public bool Reload { get; set; }
            public string MagId { get; set; }

            public static QuickReloadMagPacket Deserialize(NetDataReader reader)
            {
                QuickReloadMagPacket packet = new();
                packet.Reload = reader.GetBool();
                if (packet.Reload)
                    packet.MagId = reader.GetString();
                return packet;
            }

            public static void Serialize(NetDataWriter writer, QuickReloadMagPacket packet)
            {
                writer.Put(packet.Reload);
                if (packet.Reload)
                    writer.Put(packet.MagId);
            }
        }

        public struct ReloadWithAmmoPacket
        {
            public bool Reload { get; set; }
            public EReloadWithAmmoStatus Status { get; set; }
            public int AmmoLoadedToMag { get; set; }
            public int AmmoIdsCount { get; set; }
            public string[] AmmoIds { get; set; }

            public enum EReloadWithAmmoStatus
            {
                None = 0,
                StartReload,
                EndReload,
                AbortReload
            }

            public static ReloadWithAmmoPacket Deserialize(NetDataReader reader)
            {
                ReloadWithAmmoPacket packet = new();
                packet.Reload = reader.GetBool();
                if (packet.Reload)
                {
                    packet.Status = (EReloadWithAmmoStatus)reader.GetInt();
                    packet.AmmoIdsCount = reader.GetInt();
                    packet.AmmoIds = new string[packet.AmmoIdsCount];
                    for (int i = 0; i < packet.AmmoIdsCount; i++)
                    {
                        packet.AmmoIds[i] = reader.GetString();
                    }
                    if (packet.Status == EReloadWithAmmoStatus.EndReload)
                        packet.AmmoLoadedToMag = reader.GetInt();
                }
                return packet;
            }

            public static void Serialize(NetDataWriter writer, ReloadWithAmmoPacket packet)
            {
                writer.Put(packet.Reload);
                if (packet.Reload)
                {
                    writer.Put((int)packet.Status);
                    writer.Put(packet.AmmoIdsCount);
                    for (int i = 0; i < packet.AmmoIdsCount; ++i)
                    {
                        writer.Put(packet.AmmoIds[i]);
                    }
                    if (packet.AmmoLoadedToMag > 0)
                    {
                        writer.Put(packet.AmmoLoadedToMag);
                    }
                }
            }
        }

        public struct CylinderMagPacket
        {
            public bool Changed { get; set; }
            public int CamoraIndex { get; set; }
            public bool HammerClosed { get; set; }

            public static CylinderMagPacket Deserialize(NetDataReader reader)
            {
                CylinderMagPacket packet = new CylinderMagPacket();
                packet.Changed = reader.GetBool();
                if (packet.Changed)
                {
                    packet.CamoraIndex = reader.GetInt();
                    packet.HammerClosed = reader.GetBool();
                }
                return packet;
            }

            public static void Serialize(NetDataWriter writer, CylinderMagPacket packet)
            {
                writer.Put(packet.Changed);
                if (packet.Changed)
                {
                    writer.Put(packet.CamoraIndex);
                    writer.Put(packet.HammerClosed);
                }
            }
        }

        public struct ReloadLauncherPacket
        {
            public bool Reload { get; set; }
            public int AmmoIdsCount { get; set; }
            public string[] AmmoIds { get; set; }

            public static ReloadLauncherPacket Deserialize(NetDataReader reader)
            {
                ReloadLauncherPacket packet = new();
                packet.Reload = reader.GetBool();
                if (packet.Reload)
                {
                    packet.AmmoIdsCount = reader.GetInt();
                    packet.AmmoIds = new string[packet.AmmoIdsCount];
                    for (int i = 0; i < packet.AmmoIdsCount; i++)
                    {
                        packet.AmmoIds[i] = reader.GetString();
                    }
                }
                return packet;
            }

            public static void Serialize(NetDataWriter writer, ReloadLauncherPacket packet)
            {
                writer.Put(packet.Reload);
                if (packet.Reload)
                {
                    writer.Put(packet.AmmoIdsCount);
                    for (int i = 0; i < packet.AmmoIdsCount; ++i)
                    {
                        writer.Put(packet.AmmoIds[i]);
                    }
                }
            }
        }

        public struct ReloadBarrelsPacket
        {
            public bool Reload { get; set; }
            public int AmmoIdsCount { get; set; }
            public string[] AmmoIds { get; set; }
            public int LocationLength { get; set; }
            public byte[] LocationDescription { get; set; }

            public static ReloadBarrelsPacket Deserialize(NetDataReader reader)
            {
                ReloadBarrelsPacket packet = new();
                packet.Reload = reader.GetBool();
                if (packet.Reload)
                {
                    packet.AmmoIdsCount = reader.GetInt();
                    packet.AmmoIds = new string[packet.AmmoIdsCount];
                    for (int i = 0; i < packet.AmmoIdsCount; i++)
                    {
                        packet.AmmoIds[i] = reader.GetString();
                    }
                    packet.LocationLength = reader.GetInt();
                    packet.LocationDescription = new byte[packet.LocationLength];
                    reader.GetBytes(packet.LocationDescription, packet.LocationLength);
                }
                return packet;
            }
            public static void Serialize(NetDataWriter writer, ReloadBarrelsPacket packet)
            {
                writer.Put(packet.Reload);
                if (packet.Reload)
                {
                    writer.Put(packet.AmmoIdsCount);
                    for (int i = 0; i < packet.AmmoIdsCount; ++i)
                    {
                        writer.Put(packet.AmmoIds[i]);
                    }
                    writer.Put(packet.LocationLength);
                    writer.Put(packet.LocationDescription);
                }
            }
        }

        public struct GrenadePacket
        {
            public GrenadePacketType PacketType { get; set; }
            public enum GrenadePacketType
            {
                ExamineWeapon,
                HighThrow,
                LowThrow,
                PullRingForHighThrow,
                PullRingForLowThrow
            }

            public static GrenadePacket Deserialize(NetDataReader reader)
            {
                return new GrenadePacket
                {
                    PacketType = (GrenadePacketType)reader.GetInt()
                };
            }
            public static void Serialize(NetDataWriter writer, GrenadePacket packet)
            {
                writer.Put((int)packet.PacketType);
            }
        }

        public struct ApplyDamageInfoPacket()
        {
            public EDamageType DamageType { get; set; }
            public float Damage { get; set; }
            public EBodyPart BodyPartType { get; set; }
            public float Absorbed { get; set; }
            public string ProfileId { get; set; } = "null";

            public static ApplyDamageInfoPacket Deserialize(NetDataReader reader)
            {
                ApplyDamageInfoPacket packet = new();
                packet.DamageType = (EDamageType)reader.GetInt();
                packet.Damage = reader.GetFloat();
                packet.BodyPartType = (EBodyPart)reader.GetInt();
                packet.Absorbed = reader.GetFloat();
                packet.ProfileId = reader.GetString();
                return packet;
            }
            public static void Serialize(NetDataWriter writer, ApplyDamageInfoPacket packet)
            {
                writer.Put((int)packet.DamageType);
                writer.Put(packet.Damage);
                writer.Put((int)packet.BodyPartType);
                writer.Put(packet.Absorbed);
                writer.Put(packet.ProfileId);
            }
        }

        public struct RestoreBodyPartPacket
        {
            public EBodyPart BodyPartType { get; set; }
            public float HealthPenalty { get; set; }

            public static RestoreBodyPartPacket Deserialize(NetDataReader reader)
            {
                RestoreBodyPartPacket packet = new()
                {
                    BodyPartType = (EBodyPart)reader.GetInt(),
                    HealthPenalty = reader.GetFloat()
                };
                return packet;
            }
            public static void Serialize(NetDataWriter writer, RestoreBodyPartPacket packet)
            {
                writer.Put((int)packet.BodyPartType);
                writer.Put(packet.HealthPenalty);
            }
        }

        public struct DestroyBodyPartPacket
        {
            public EBodyPart BodyPartType { get; set; }
            public EDamageType DamageType { get; set; }

            public static DestroyBodyPartPacket Deserialize(NetDataReader reader)
            {
                DestroyBodyPartPacket packet = new()
                {
                    BodyPartType = (EBodyPart)reader.GetInt(),
                    DamageType = (EDamageType)reader.GetInt()
                };
                return packet;
            }
            public static void Serialize(NetDataWriter writer, DestroyBodyPartPacket packet)
            {
                writer.Put((int)packet.BodyPartType);
                writer.Put((int)packet.DamageType);
            }
        }

        public struct ChangeHealthPacket
        {
            public EBodyPart BodyPartType { get; set; }
            public float Value { get; set; }

            public static ChangeHealthPacket Deserialize(NetDataReader reader)
            {
                ChangeHealthPacket packet = new();
                packet.BodyPartType = (EBodyPart)reader.GetInt();
                packet.Value = reader.GetFloat();
                return packet;
            }
            public static void Serialize(NetDataWriter writer, ChangeHealthPacket packet)
            {
                writer.Put((int)packet.BodyPartType);
                writer.Put(packet.Value);
            }
        }

        public struct ObservedDeathPacket()
        {
            public EDamageType DamageType { get; set; }
            public string ProfileId { get; set; } = "null";

            public static ObservedDeathPacket Deserialize(NetDataReader reader)
            {
                return new ObservedDeathPacket()
                {
                    DamageType = (EDamageType)reader.GetInt(),
                    ProfileId = reader.GetString()
                };
            }
            public static void Serialize(NetDataWriter writer, ObservedDeathPacket packet)
            {
                writer.Put((int)packet.DamageType);
                writer.Put(packet.ProfileId);
            }
        }

        public struct AddEffectPacket()
        {
            public int Id { get; set; }
            public string Type { get; set; }
            public EBodyPart BodyPartType { get; set; }
            public float DelayTime { get; set; } = 0;
            public float BuildUpTime { get; set; } = 0;
            public float WorkTime { get; set; } = 0;
            public float ResidueTime { get; set; } = 0;
            public float Strength { get; set; } = 0;
            public ExtraDataType ExtraDataTypeValue { get; set; } = 0;
            public ExtraData ExtraDataValue;
            public enum ExtraDataType
            {
                None = 0,
                MedEffect,
                Stimulator
            }
            public struct ExtraData()
            {
                public MedEffect MedEffectValue { get; set; }
                public Stimulator StimulatorValue { get; set; }
                public struct MedEffect
                {
                    public string ItemId { get; set; }
                    public float Amount { get; set; }
                    public static MedEffect Deserialize(NetDataReader reader)
                    {
                        MedEffect packet = new();
                        packet.ItemId = reader.GetString();
                        packet.Amount = reader.GetFloat();
                        return packet;
                    }
                    public static void Serialize(NetDataWriter writer, MedEffect packet)
                    {
                        writer.Put(packet.ItemId);
                        writer.Put(packet.Amount);
                    }
                }
                public struct Stimulator
                {
                    public string BuffsName { get; set; }
                    public string ItemTemplateId { get; set; }
                    public EBodyPart BodyPartType { get; set; }
                    public static Stimulator Deserialize(NetDataReader reader)
                    {
                        Stimulator packet = new();
                        packet.BuffsName = reader.GetString();
                        packet.ItemTemplateId = reader.GetString();
                        packet.BodyPartType = (EBodyPart)reader.GetInt();
                        return packet;
                    }
                    public static void Serialize(NetDataWriter writer, Stimulator packet)
                    {
                        writer.Put(packet.BuffsName);
                        writer.Put(packet.ItemTemplateId);
                        writer.Put((int)packet.BodyPartType);
                    }
                }
            }
            public static AddEffectPacket Deserialize(NetDataReader reader)
            {
                AddEffectPacket packet = new();
                packet.Id = reader.GetInt();
                packet.Type = reader.GetString();
                packet.BodyPartType = (EBodyPart)reader.GetInt();
                packet.DelayTime = reader.GetFloat();
                packet.BuildUpTime = reader.GetFloat();
                packet.WorkTime = reader.GetFloat();
                packet.ResidueTime = reader.GetFloat();
                packet.Strength = reader.GetFloat();
                packet.ExtraDataTypeValue = (ExtraDataType)reader.GetInt();
                if (packet.ExtraDataTypeValue == ExtraDataType.MedEffect)
                {
                    packet.ExtraDataValue = new();
                    packet.ExtraDataValue.MedEffectValue = ExtraData.MedEffect.Deserialize(reader);
                }
                else if (packet.ExtraDataTypeValue == ExtraDataType.Stimulator)
                {
                    packet.ExtraDataValue = new();
                    packet.ExtraDataValue.StimulatorValue = ExtraData.Stimulator.Deserialize(reader);
                }
                return packet;
            }
            public static void Serialize(NetDataWriter writer, AddEffectPacket packet)
            {
                writer.Put(packet.Id);
                writer.Put(packet.Type);
                writer.Put((int)packet.BodyPartType);
                writer.Put(packet.DelayTime);
                writer.Put(packet.BuildUpTime);
                writer.Put(packet.WorkTime);
                writer.Put(packet.ResidueTime);
                writer.Put(packet.Strength);
                writer.Put((int)packet.ExtraDataTypeValue);
                if (packet.ExtraDataTypeValue == ExtraDataType.MedEffect)
                    ExtraData.MedEffect.Serialize(writer, packet.ExtraDataValue.MedEffectValue);
                else if (packet.ExtraDataTypeValue == ExtraDataType.Stimulator)
                    ExtraData.Stimulator.Serialize(writer, packet.ExtraDataValue.StimulatorValue);
            }
        }

        public struct RemoveEffectPacket()
        {
            public int Id { get; set; }
            public string Type { get; set; }
            public EBodyPart BodyPartType { get; set; }
            public static RemoveEffectPacket Deserialize(NetDataReader reader)
            {
                RemoveEffectPacket packet = new();
                packet.Id = reader.GetInt();
                packet.Type = reader.GetString();
                packet.BodyPartType = (EBodyPart)reader.GetInt();
                return packet;
            }
            public static void Serialize(NetDataWriter writer, RemoveEffectPacket packet)
            {
                writer.Put(packet.Id);
                writer.Put(packet.Type);
                writer.Put((int)packet.BodyPartType);
            }
        }

        //public struct ItemMovementHandlerMovePacket
        //{
        //    public string ItemId { get; set; }
        //    public AbstractDescriptor Descriptor { get; set; }
        //    public static ItemMovementHandlerMovePacket Deserialize(NetDataReader reader)
        //    {
        //        GClass1035 polyReader = new(reader.RawData);
        //        return new ItemMovementHandlerMovePacket()
        //        {
        //            ItemId = polyReader.ReadString(),
        //            Descriptor = polyReader.ReadPolymorph<AbstractDescriptor>()
        //        };
        //    }
        //    public static void Serialize(NetDataWriter writer, ItemMovementHandlerMovePacket packet)
        //    {
        //        GClass1040 polyWriter = new();
        //        polyWriter.WriteString(packet.ItemId);
        //        polyWriter.WritePolymorph(packet.Descriptor);
        //        writer.Put(polyWriter.ToArray());
        //    }

        //}

        public struct ItemControllerExecutePacket
        {
            public uint CallbackId { get; set; }
            public int OperationBytesLength { get; set; }
            public byte[] OperationBytes { get; set; }
            public string InventoryId { get; set; }
            public static ItemControllerExecutePacket Deserialize(NetDataReader reader)
            {
                ItemControllerExecutePacket packet = new();
                packet.CallbackId = reader.GetUInt();
                packet.OperationBytesLength = reader.GetInt();
                packet.OperationBytes = new byte[packet.OperationBytesLength];
                reader.GetBytes(packet.OperationBytes, packet.OperationBytesLength);
                packet.InventoryId = reader.GetString();
                return packet;
            }
            public static void Serialize(NetDataWriter writer, ItemControllerExecutePacket packet)
            {
                writer.Put(packet.CallbackId);
                writer.Put(packet.OperationBytes.Length);
                writer.Put(packet.OperationBytes);
                writer.Put(packet.InventoryId);
            }
        }

       

        public struct ContainerInteractionPacket
        {
            public string InteractiveId { get; set; }
            public EInteractionType InteractionType { get; set; }

            public static ContainerInteractionPacket Deserialize(NetDataReader reader)
            {
                ContainerInteractionPacket packet = new();
                packet.InteractiveId = reader.GetString();
                packet.InteractionType = (EInteractionType)reader.GetInt();
                return packet;
            }
            public static void Serialize(NetDataWriter writer, ContainerInteractionPacket packet)
            {
                writer.Put(packet.InteractiveId);
                writer.Put((int)packet.InteractionType);
            }
        }

        public struct ProceedPacket()
        {
            public EProceedType ProceedType { get; set; }
            public string ItemId { get; set; } = "";
            public string ItemTemplateId { get; set; } = "";
            public float Amount { get; set; } = 0f;
            public int AnimationVariant { get; set; } = 0;
            public bool Scheduled { get; set; } = false;
            public EBodyPart BodyPart { get; set; } = EBodyPart.Common;

            public static ProceedPacket Deserialize(NetDataReader reader)
            {
                return new ProceedPacket
                {
                    ProceedType = (EProceedType)reader.GetInt(),
                    ItemId = reader.GetString(),
                    ItemTemplateId = reader.GetString(),
                    Amount = reader.GetFloat(),
                    AnimationVariant = reader.GetInt(),
                    Scheduled = reader.GetBool(),
                    BodyPart = (EBodyPart)reader.GetInt()
                };
            }
            public static void Serialize(NetDataWriter writer, ProceedPacket packet)
            {
                writer.Put((int)packet.ProceedType);
                writer.Put(packet.ItemId);
                writer.Put(packet.ItemTemplateId);
                writer.Put(packet.Amount);
                writer.Put(packet.AnimationVariant);
                writer.Put(packet.Scheduled);
                writer.Put((int)packet.BodyPart);
            }

        }

        public struct DropPacket
        {
            public bool FastDrop { get; set; }
            public bool HasItemId { get; set; }
            public string ItemId { get; set; }

            public static DropPacket Deserialize(NetDataReader reader)
            {
                DropPacket packet = new()
                {
                    FastDrop = reader.GetBool(),
                    HasItemId = reader.GetBool()
                };
                if (packet.HasItemId)
                    packet.ItemId = reader.GetString();

                return packet;
            }
            public static void Serialize(NetDataWriter writer, DropPacket packet)
            {
                writer.Put(packet.FastDrop);
                writer.Put(packet.ItemId);
                if (packet.HasItemId)
                    writer.Put(packet.ItemId);
            }
        }

        public struct StationaryPacket
        {
            public EStationaryCommand Command { get; set; }
            public string Id { get; set; }
            public enum EStationaryCommand : byte
            {
                Occupy,
                Leave,
                Denied
            }

            public static StationaryPacket Deserialize(NetDataReader reader)
            {
                StationaryPacket packet = new()
                {
                    Command = (EStationaryCommand)reader.GetByte()
                };

                if (packet.Command == EStationaryCommand.Occupy)
                    packet.Id = reader.GetString();

                return packet;
            }
            public static void Serialize(NetDataWriter writer, StationaryPacket packet)
            {
                writer.Put((byte)packet.Command);
                if (packet.Command == EStationaryCommand.Occupy && !string.IsNullOrEmpty(packet.Id))
                    writer.Put(packet.Id);
            }
        }

        public struct ShotInfoPacket()
        {

            public bool IsPrimaryActive { get; set; } = true;
            public EShotType ShotType { get; set; } = EShotType.Unknown;
            public int AmmoAfterShot { get; set; } = 0;
            public Vector3 ShotPosition { get; set; } = Vector3.zero;
            public Vector3 ShotDirection { get; set; } = Vector3.zero;
            public Vector3 FireportPosition { get; set; } = Vector3.zero;
            public int ChamberIndex { get; set; } = 0;
            public float Overheat { get; set; } = 0f;
            public bool UnderbarrelShot { get; set; } = false;
            public string AmmoTemplate { get; set; } = "null";

            public static ShotInfoPacket Deserialize(NetDataReader reader)
            {
                ShotInfoPacket packet = new()
                {
                    IsPrimaryActive = reader.GetBool(),
                    ShotType = (EShotType)reader.GetInt(),
                    AmmoAfterShot = reader.GetInt(),
                    ShotPosition = Vector3Utils.Deserialize(reader),
                    ShotDirection = Vector3Utils.Deserialize(reader),
                    FireportPosition = Vector3Utils.Deserialize(reader),
                    ChamberIndex = reader.GetInt(),
                    Overheat = reader.GetFloat(),
                    UnderbarrelShot = reader.GetBool(),
                    AmmoTemplate = reader.GetString()
                };

                return packet;
            }
            public static void Serialize(NetDataWriter writer, ShotInfoPacket packet)
            {
                writer.Put(packet.IsPrimaryActive);
                writer.Put((int)packet.ShotType);
                writer.Put(packet.AmmoAfterShot);
                Vector3Utils.Serialize(writer, packet.ShotPosition);
                Vector3Utils.Serialize(writer, packet.ShotDirection);
                Vector3Utils.Serialize(writer, packet.FireportPosition);
                writer.Put(packet.ChamberIndex);
                writer.Put(packet.Overheat);
                writer.Put(packet.UnderbarrelShot);
                writer.Put(packet.AmmoTemplate);
            }
        }

       

        public enum EProceedType
        {
            EmptyHands,
            FoodClass,
            GrenadeClass,
            MedsClass,
            QuickGrenadeThrow,
            QuickKnifeKick,
            QuickUse,
            Weapon,
            Knife,
            TryProceed
        }
    }
}