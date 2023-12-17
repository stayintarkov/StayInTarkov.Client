using EFT;
using EFT.InventoryLogic;
using LiteNetLib.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StayInTarkov.Coop.Player.Health;
using System.Diagnostics;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using static Physical;

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

        public class DamageInfoUtils
        {
            public static void Serialize(NetDataWriter writer, DamageInfo dInfo)
            {
            }

            //public static DamageInfo Deserialize(NetDataReader reader)
            //{
            //    return null;
            //}
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
            public byte Type { get; set; }
            public EBodyPart BodyPartType { get; set; }
            public float? DelayTime { get; set; }
            public float? BuildUpTime { get; set; }
            public float? WorkTime { get; set; }
            public float? ResidueTime { get; set; }
            public float? Strength { get; set; }
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
                packet.Type = reader.GetByte();
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
                if (packet.DelayTime != null)
                    writer.Put((float)packet.DelayTime);
                if (packet.BuildUpTime != null)
                    writer.Put((float)packet.BuildUpTime);
                if (packet.WorkTime != null)
                    writer.Put((float)packet.WorkTime);
                if (packet.ResidueTime != null)
                    writer.Put((float)packet.ResidueTime);
                if (packet.Strength != null)
                    writer.Put((float)packet.Strength);
                writer.Put((int)packet.ExtraDataTypeValue);
                if (packet.ExtraDataTypeValue == ExtraDataType.MedEffect)
                    ExtraData.MedEffect.Serialize(writer, packet.ExtraDataValue.MedEffectValue);
                else if (packet.ExtraDataTypeValue == ExtraDataType.Stimulator)
                    ExtraData.Stimulator.Serialize(writer, packet.ExtraDataValue.StimulatorValue);
            }
        }


        //public struct ReloadBarrelsPacket : INetSerializable
        //{
        //    public bool Reload { get; set; }
        //    public string AmmoId { get; set; }
        //    public byte[] LocationDescription { get; set; }

        //    public void Deserialize(NetDataReader reader)
        //    {
        //        throw new NotImplementedException();
        //    }

        //    public void Serialize(NetDataWriter writer)
        //    {
        //        throw new NotImplementedException();
        //    }
        //}
        //public struct LauncherReloadPacket : INetSerializable
        //{
        //    public bool Reload { get; set; }
        //    public string[] AmmoIds { get; set; }

        //    public void Deserialize(NetDataReader reader)
        //    {
        //        throw new NotImplementedException();
        //    }

        //    public void Serialize(NetDataWriter writer)
        //    {
        //        throw new NotImplementedException();
        //    }
        //}
        //public struct CompassPacket : INetSerializable
        //{
        //    public bool Toggle { get; set; }
        //    public bool Status { get; set; }

        //    public void Deserialize(NetDataReader reader)
        //    {
        //        throw new NotImplementedException();
        //    }

        //    public void Serialize(NetDataWriter writer)
        //    {
        //        throw new NotImplementedException();
        //    }
        //}
    }
}
