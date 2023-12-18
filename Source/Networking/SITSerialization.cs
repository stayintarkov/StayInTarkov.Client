using EFT;
using LiteNetLib.Utils;
using System.Collections.Generic;
using UnityEngine;
using static Physical;

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
        }

        public class PhysicalUtils
        {
            public static void Serialize(NetDataWriter writer, PhysicalStamina physicalStamina)
            {
                writer.Put(physicalStamina.StaminaExhausted);
                writer.Put(physicalStamina.OxygenExhausted);
                writer.Put(physicalStamina.HandsExhausted);
            }

            public static PhysicalStamina Deserialize(NetDataReader reader)
            {
                return new PhysicalStamina() { StaminaExhausted = reader.GetBool(), OxygenExhausted = reader.GetBool(), HandsExhausted = reader.GetBool() };
            }
        }

        public class AddressUtils
        {
            public static void SerializeGridItemAddressDescriptor(NetDataWriter writer, GridItemAddressDescriptor gridItemAddressDescriptor)
            {
                SerializeLocationInGrid(writer, gridItemAddressDescriptor.LocationInGrid);

            }

            public static GridItemAddressDescriptor DeserializeGridItemAddressDescriptor(NetDataReader reader)
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

            public static ContainerDescriptor DeserializeContainerDescriptor(NetDataReader reader)
            {
                return new ContainerDescriptor()
                {
                    ParentId = reader.GetString(),
                    ContainerId = reader.GetString(),
                };
            }

            public static void SerializeItemInGridDescriptor(NetDataWriter writer, ItemInGridDescriptor itemInGridDescriptor)
            {
                GClass1040 polyWriter = new();
                SerializeLocationInGrid(writer, itemInGridDescriptor.Location);
                SerializeItemDescriptor(polyWriter, itemInGridDescriptor.Item);
                writer.Put(polyWriter.ToArray());
            }

            public static ItemInGridDescriptor DeserializeItemInGridDescriptor(NetDataReader reader)
            {
                GClass1035 polyReader = new(reader.RawData);
                return new ItemInGridDescriptor()
                {
                    Location = DeserializeLocationInGrid(reader),
                    Item = DeserializeItemDescriptor(polyReader)
                };
            }

            public static void SerializeLocationInGrid(NetDataWriter writer, LocationInGrid locationInGrid)
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

            public static void SerializeItemDescriptor(GClass1040 writer, ItemDescriptor itemDescriptor)
            {
                writer.WriteString(itemDescriptor.Id);
                writer.WriteString(itemDescriptor.TemplateId);
                writer.WriteInt(itemDescriptor.StackCount);
                writer.WriteBool(itemDescriptor.SpawnedInSession);
                writer.WriteByte(itemDescriptor.ActiveCamora);
                writer.WriteBool(itemDescriptor.IsUnderBarrelDeviceActive);
                for (int i = 0; i < itemDescriptor.Components.Count; i++)
                {
                    writer.WritePolymorph(itemDescriptor.Components[i]);
                }
                writer.WriteInt(itemDescriptor.Slots.Count);
                for (int j = 0; j < itemDescriptor.Slots.Count; j++)
                {
                    writer.WriteEFTSlotDescriptor(itemDescriptor.Slots[j]);
                }
                writer.WriteInt(itemDescriptor.ShellsInWeapon.Count);
                for (int k = 0; k < itemDescriptor.ShellsInWeapon.Count; k++)
                {
                    writer.WriteEFTShellTemplateDescriptor(itemDescriptor.ShellsInWeapon[k]);
                }
                writer.WriteInt(itemDescriptor.ShellsInUnderbarrelWeapon.Count);
                for (int l = 0; l < itemDescriptor.ShellsInUnderbarrelWeapon.Count; l++)
                {
                    writer.WriteEFTShellTemplateDescriptor(itemDescriptor.ShellsInUnderbarrelWeapon[l]);
                }
                writer.WriteInt(itemDescriptor.Grids.Count);
                for (int m = 0; m < itemDescriptor.Grids.Count; m++)
                {
                    writer.WriteEFTGridDescriptor(itemDescriptor.Grids[m]);
                }
                writer.WriteInt(itemDescriptor.StackSlots.Count);
                for (int n = 0; n < itemDescriptor.StackSlots.Count; n++)
                {
                    writer.WriteEFTStackSlotDescriptor(itemDescriptor.StackSlots[n]);
                }
                writer.WriteInt(itemDescriptor.Malfunction.Count);
                for (int num = 0; num < itemDescriptor.Malfunction.Count; num++)
                {
                    writer.WriteEFTMalfunctionDescriptor(itemDescriptor.Malfunction[num]);
                }
            }

            public static ItemDescriptor DeserializeItemDescriptor(GClass1035 reader)
            {
                ItemDescriptor itemDescriptor = new();
                itemDescriptor.Id = reader.ReadString();
                itemDescriptor.TemplateId = reader.ReadString();
                itemDescriptor.StackCount = reader.ReadInt();
                itemDescriptor.SpawnedInSession = reader.ReadBool();
                itemDescriptor.ActiveCamora = reader.ReadByte();
                itemDescriptor.IsUnderBarrelDeviceActive = reader.ReadBool();
                int num = reader.ReadInt();
                itemDescriptor.Components = new List<AbstractDescriptor2>(num);
                for (int i = 0; i < num; i++)
                {
                    itemDescriptor.Components.Add(reader.ReadPolymorph<AbstractDescriptor2>());
                }
                int num2 = reader.ReadInt();
                itemDescriptor.Slots = new List<SlotDescriptor>(num2);
                for (int j = 0; j < num2; j++)
                {
                    itemDescriptor.Slots.Add(reader.ReadEFTSlotDescriptor());
                }
                int num3 = reader.ReadInt();
                itemDescriptor.ShellsInWeapon = new List<ShellTemplateDescriptor>(num3);
                for (int k = 0; k < num3; k++)
                {
                    itemDescriptor.ShellsInWeapon.Add(reader.ReadEFTShellTemplateDescriptor());
                }
                int num4 = reader.ReadInt();
                itemDescriptor.ShellsInUnderbarrelWeapon = new List<ShellTemplateDescriptor>(num4);
                for (int l = 0; l < num4; l++)
                {
                    itemDescriptor.ShellsInUnderbarrelWeapon.Add(reader.ReadEFTShellTemplateDescriptor());
                }
                int num5 = reader.ReadInt();
                itemDescriptor.Grids = new List<GridDescriptor>(num5);
                for (int m = 0; m < num5; m++)
                {
                    itemDescriptor.Grids.Add(reader.ReadEFTGridDescriptor());
                }
                int num6 = reader.ReadInt();
                itemDescriptor.StackSlots = new List<StackSlotDescriptor>(num6);
                for (int n = 0; n < num6; n++)
                {
                    itemDescriptor.StackSlots.Add(reader.ReadEFTStackSlotDescriptor());
                }
                int num7 = reader.ReadInt();
                itemDescriptor.Malfunction = new List<MalfunctionDescriptor>(num7);
                for (int num8 = 0; num8 < num7; num8++)
                {
                    itemDescriptor.Malfunction.Add(reader.ReadEFTMalfunctionDescriptor());
                }
                return itemDescriptor;
            }

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

        public struct ApplyDamageInfoPacket
        {
            public EDamageType DamageType { get; set; }
            public float Damage { get; set; }
            public EBodyPart BodyPartType { get; set; }
            public float Absorbed { get; set; }

            public static ApplyDamageInfoPacket Deserialize(NetDataReader reader)
            {
                ApplyDamageInfoPacket packet = new();
                packet.DamageType = (EDamageType)reader.GetInt();
                packet.Damage = reader.GetFloat();
                packet.BodyPartType = (EBodyPart)reader.GetInt();
                packet.Absorbed = reader.GetFloat();
                return packet;
            }
            public static void Serialize(NetDataWriter writer, ApplyDamageInfoPacket packet)
            {
                writer.Put((int)packet.DamageType);
                writer.Put(packet.Damage);
                writer.Put((int)packet.BodyPartType);
                writer.Put(packet.Absorbed);
            }
        }

        public struct RestoreBodyPartPacket
        {
            public EBodyPart BodyPartType { get; set; }
            public float HealthPenalty { get; set; }

            public static RestoreBodyPartPacket Deserialize(NetDataReader reader)
            {
                RestoreBodyPartPacket packet = new();
                packet.BodyPartType = (EBodyPart)reader.GetInt();
                packet.HealthPenalty = reader.GetFloat();
                return packet;
            }
            public static void Serialize(NetDataWriter writer, RestoreBodyPartPacket packet)
            {
                writer.Put((int)packet.BodyPartType);
                writer.Put(packet.HealthPenalty);
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

        public struct ItemMovementHandlerMovePacket
        {
            public string ItemId { get; set; }
            public AbstractDescriptor Descriptor { get; set; }
            public static ItemMovementHandlerMovePacket Deserialize(NetDataReader reader)
            {
                GClass1035 polyReader = new(reader.RawData);
                return new ItemMovementHandlerMovePacket()
                {
                    ItemId = polyReader.ReadString(),
                    Descriptor = polyReader.ReadPolymorph<AbstractDescriptor>()
                };
            }
            public static void Serialize(NetDataWriter writer, ItemMovementHandlerMovePacket packet)
            {
                GClass1040 polyWriter = new();
                polyWriter.WriteString(packet.ItemId);
                polyWriter.WritePolymorph(packet.Descriptor);
                writer.Put(polyWriter.ToArray());
            }

        }

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
    }
}
