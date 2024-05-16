using ChatShared;
using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using EFT.UI;
using LiteNetLib.Utils;
using Mono.Cecil;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StayInTarkov.Coop.Components.CoopGameComponents;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Networking;
using UnityEngine.UIElements;
using UnityStandardAssets.Water;
using WebSocketSharp;

namespace StayInTarkov.Coop.NetworkPacket
{
    public class BasePacket : ISITPacket, IDisposable
    {
        [JsonProperty(PropertyName = "serverId")]
        public string ServerId { get; set; }

        [JsonProperty(PropertyName = "t")]
        public string TimeSerializedBetter { get; set; }

        [JsonProperty(PropertyName = "m")]
        public virtual string Method { get; set; }

        public TimeSpan GetTimeSinceSent()
        {
            if (!string.IsNullOrEmpty(TimeSerializedBetter) && TimeSerializedBetter != "0")
            {
                var ticks = long.Parse(TimeSerializedBetter);
                var result = DateTime.UtcNow - new DateTime(ticks);
                return result;
            }

            return TimeSpan.Zero;
        }

        public BasePacket(string method)
        {
            Method = method;
            ServerId = SITGameComponent.GetServerId();
            TimeSerializedBetter = DateTime.UtcNow.Ticks.ToString("G");
        }

        public static Dictionary<Type, PropertyInfo[]> TypeProperties = new();
        protected bool disposedValue;

        public static PropertyInfo[] GetPropertyInfos(ISITPacket packet)
        {
            return GetPropertyInfos(packet.GetType());
        }

        public static PropertyInfo[] GetPropertyInfos(Type t)
        {
            if (!TypeProperties.ContainsKey(t))
            {
                var allProps = ReflectionHelpers.GetAllPropertiesForType(t);
                var allPropsFiltered = allProps
                  .Where(x => x.Name != nameof(ServerId) && x.Name != "Method")
                  .OrderByDescending(x => x.Name == "ProfileId").ToArray();
                TypeProperties.Add(t, allPropsFiltered.Distinct(x => x.Name).ToArray());
            }

            return TypeProperties[t];
        }

        public virtual void WriteHeader(BinaryWriter writer)
        {
            // Prefix SIT
            writer.WriteNonPrefixedString("SIT");
            // 0.14 is 24 profile Id
            writer.WriteNonPrefixedString(ServerId);
            writer.Write(Method);
            writer.WriteNonPrefixedString("?");
        }

        public virtual void ReadHeader(BinaryReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));

            var sitPrefix = reader.ReadBytes(3); // SIT
            if (Encoding.UTF8.GetString(sitPrefix) != "SIT")
            {
                reader.BaseStream.Position = 0;
                return;
            }
            //reader.ReadBytes(27); // Server Id
            // 0.14 is 24 bytes profile Id
            reader.ReadBytes(24); // Server Id
            Method = reader.ReadString();

            // A check for the ?
            var bQ = reader.ReadBytes(1); // ?
            if (Encoding.UTF8.GetString(bQ) != "?")
                reader.BaseStream.Position -= 1;
        }

        /// <summary>
        /// Serializes the BasePacket header. All inherited Packet classes must override this.
        /// </summary>
        /// <returns></returns>
        public virtual byte[] Serialize()
        {
            var ms = new MemoryStream();
            using BinaryWriter writer = new(ms);
            WriteHeader(writer);
            return ms.ToArray();
        }

        /// <summary>
        /// Deserializes the BasePacket header. All inherited Packet classes must override this.
        /// </summary>
        /// <returns></returns>
        public virtual ISITPacket Deserialize(byte[] bytes)
        {
            using BinaryReader reader = new(new MemoryStream(bytes));
            ReadHeader(reader);
            return this;
        }

        public override string ToString()
        {
            return Encoding.UTF8.GetString(Serialize());
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                    //ServerId.Clear();
                    //ServerId = null;

                    //Method.Clear();
                    //Method = null;

                    //TimeSerializedBetter.Clear();
                    //TimeSerializedBetter = null;
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Process this packet. Overridable for other packets to provide their own handlers.
        /// </summary>
        public virtual void Process()
        {
            Dispose();
        }
    }

    public interface ISITPacket
    {
        public string ServerId { get; set; }
        public string TimeSerializedBetter { get; set; }
        public string Method { get; set; }
        byte[] Serialize();
        ISITPacket Deserialize(byte[] bytes);

        void Process();

    }

    public static class SerializerExtensions
    {
        public const char SIT_SERIALIZATION_PACKET_SEPERATOR = '£';
        private static Dictionary<Type, PropertyInfo[]> TypeToPropertyInfos { get; } = new();

        static SerializerExtensions()
        {
            // Use the static Serializer Extensions to pre populate all Network Packet Property Infos
            var sitPacketTypes = Assembly.GetAssembly(typeof(ISITPacket))
                .GetTypes()
                .Where(x => x.GetInterface("ISITPacket") != null);
            foreach (var packetType in sitPacketTypes)
            {
                TypeToPropertyInfos.Add(packetType, BasePacket.GetPropertyInfos(packetType));
            }

            StayInTarkovHelperConstants.Logger.LogDebug($"{TypeToPropertyInfos.Count} ISITPacket types found");
        }

        public unsafe static void Clear(this string s)
        {
            if (s == null)
                return;

            fixed (char* ptr = s)
            {
                for (int i = 0; i < s.Length; i++)
                {
                    ptr[i] = '\0';
                }
            }
        }

        public static void WriteNonPrefixedString(this BinaryWriter binaryWriter, string value)
        {
            binaryWriter.Write(Encoding.UTF8.GetBytes(value));
        }

        public static void WriteLengthPrefixedBytes(this BinaryWriter binaryWriter, byte[] value)
        {
            binaryWriter.Write((int)value.Length);

            //StayInTarkovHelperConstants.Logger.LogDebug($"{nameof(SerializerExtensions)},{nameof(WriteLengthPrefixedBytes)},Write Length {value.Length}");

            binaryWriter.Write(value);
        }

        public static byte[] ReadLengthPrefixedBytes(this BinaryReader binaryReader)
        {
            var length = binaryReader.ReadInt32();

            //StayInTarkovHelperConstants.Logger.LogDebug($"{nameof(SerializerExtensions)},{nameof(ReadLengthPrefixedBytes)},Read Length {length}");

            if (length + binaryReader.BaseStream.Position <= binaryReader.BaseStream.Length)
                return binaryReader.ReadBytes(length);
            else
                return null;
        }

        public static float ReadFloat(this BinaryReader binaryReader)
        {
            return binaryReader.ReadSingle();
        }

        public static int ReadInt(this BinaryReader binaryReader)
        {
            return binaryReader.ReadInt32();
        }

        public static int ReadShort(this BinaryReader binaryReader)
        {
            return binaryReader.ReadInt16();
        }

        public static bool ReadBool(this BinaryReader binaryReader)
        {
            return binaryReader.ReadBoolean();
        }

        // INetSerializable Extensions


        public static void Put(this BinaryWriter binaryWriter, int value)
        {
            binaryWriter.Write(value);
        }

        public static void Put(this BinaryWriter binaryWriter, string value)
        {
            binaryWriter.Write(value);
        }

        public static void Put(this BinaryWriter binaryWriter, bool value)
        {
            binaryWriter.Write(value);
        }

        public static void Put(this BinaryWriter binaryWriter, byte[] value)
        {
            binaryWriter.Write(value);
        }

        public static int GetInt(this BinaryReader binaryReader)
        {
            return binaryReader.ReadInt32();
        }

        public static string GetString(this BinaryReader binaryReader)
        {
            return binaryReader.ReadString();
        }

        public static bool GetBool(this BinaryReader binaryReader)
        {
            return binaryReader.ReadBoolean();
        }

        public static byte[] GetBytes(this BinaryReader binaryReader, byte[] bytes, int length)
        {
            bytes = binaryReader.GetBytes(length);
            return bytes;
        }

        public static byte[] GetBytes(this BinaryReader binaryReader, int length)
        {
            return binaryReader.ReadBytes(length);
        }


        public static Stopwatch swDeserializerDebug = new();


    }


}
