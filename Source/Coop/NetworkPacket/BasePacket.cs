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
    public class BasePacket : ISITPacket, IDisposable, INetSerializable
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

        public static Dictionary<Type, PropertyInfo[]> TypeProperties = new ();
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

        //public virtual void WriteHeader(NetworkWriter writer)
        //{
        //    // Prefix SIT
        //    writer.Write("SIT");
        //    // 0.14 is 24 profile Id
        //    writer.Write(ServerId);
        //    writer.Write(Method);
        //    writer.Write("?");
        //}

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
            var bQ = reader.ReadBytes(1) ; // ?
            if (Encoding.UTF8.GetString(bQ) != "?")
                reader.BaseStream.Position -= 1;
        }

        //public byte[] AutoSerialize()
        //{
        //    if (string.IsNullOrEmpty(ServerId))
        //    {
        //        throw new ArgumentNullException($"{GetType()}:{nameof(ServerId)}");
        //    }

        //    if (string.IsNullOrEmpty(Method))
        //    {
        //        throw new ArgumentNullException($"{GetType()}:{nameof(Method)}");
        //    }

        //    byte[] result = null;
        //    BinaryWriter binaryWriter = new(new MemoryStream());
        //    WriteHeader(binaryWriter);

        //    var allPropsFiltered = GetPropertyInfos(this);
        //    if (allPropsFiltered == null)
        //        return null;

        //    // Extremely useful tool for discovering what is calling something else...
        //    //#if DEBUG
        //    //            System.Diagnostics.StackTrace t = new System.Diagnostics.StackTrace(true);
        //    //                StayInTarkovHelperConstants.Logger.LogInfo($"{t.ToString()}");
        //    //#endif


        //    for (var i = 0; i < allPropsFiltered.Count(); i++)
        //    {
        //        var prop = allPropsFiltered[i];
        //        var propValue = prop.GetValue(this);
        //        //StayInTarkovHelperConstants.Logger.LogInfo($"{prop.Name} is {propValue}");

        //        // Process an Array type
        //        if (prop.PropertyType.IsArray)
        //        {
        //            Array array = (Array)propValue;
        //            binaryWriter.Write(prop.PropertyType.FullName);
        //            binaryWriter.Write(array.Length);

        //            foreach (var item in array)
        //            {
        //                if (item.GetType().GetInterface(nameof(ISITPacket)) != null)
        //                {
        //                    var serializedSITPacket = ((ISITPacket)item).Serialize();
        //                    binaryWriter.Write((int)serializedSITPacket.Length);
        //                    binaryWriter.Write(serializedSITPacket);
        //                }
        //                else
        //                    binaryWriter.Write(item.ToString());
        //            }

        //        }
        //        // Process an ISITPacket
        //        else if (prop.PropertyType.GetInterface(nameof(ISITPacket)) != null)
        //        {
        //            binaryWriter.Write(prop.PropertyType.FullName);
        //            var serializedSITPacket = ((ISITPacket)propValue).Serialize();
        //            binaryWriter.Write(serializedSITPacket.Length);
        //            binaryWriter.Write(serializedSITPacket);
        //        }
        //        else if (prop.PropertyType == typeof(bool))
        //        {
        //            binaryWriter.Write((bool)propValue);
        //        }
        //        else
        //        {
        //            binaryWriter.Write(propValue.ToString());
        //        }

        //    }

        //    if (binaryWriter != null)
        //    {
        //        result = ((MemoryStream)binaryWriter.BaseStream).ToArray();
        //        binaryWriter.Close();
        //        binaryWriter.Dispose();
        //        binaryWriter = null;
        //    }

        //    return result;
        //}

        /// <summary>
        /// Serializes the BasePacket header. All inherited Packet classes must override this.
        /// </summary>
        /// <returns></returns>
        public virtual byte[] Serialize()
        {
            var ms = new MemoryStream();
            using BinaryWriter writer = new BinaryWriter(ms);
            WriteHeader(writer);
            return ms.ToArray();
        }

        /// <summary>
        /// Deserializes the BasePacket header. All inherited Packet classes must override this.
        /// </summary>
        /// <returns></returns>
        public virtual ISITPacket Deserialize(byte[] bytes)
        {
            using BinaryReader reader = new BinaryReader(new MemoryStream(bytes));
            ReadHeader(reader);
            return this;
        }

        //public virtual ISITPacket AutoDeserialize(byte[] data)
        //{
        //    //StayInTarkovHelperConstants.Logger.LogInfo($"{nameof(AutoDeserialize)}:{Encoding.UTF8.GetString(data)}");

        //    using BinaryReader reader = new BinaryReader(new MemoryStream(data));
        //    var headerExists = Encoding.UTF8.GetString(reader.ReadBytes(3)) == "SIT";
        //    reader.BaseStream.Position = 0;
        //    if (headerExists)
        //        ReadHeader(reader);

        //    //StayInTarkovHelperConstants.Logger.LogInfo(nameof(AutoDeserialize));
        //    //StayInTarkovHelperConstants.Logger.LogInfo($"headerExists:{headerExists}");
        //    //StayInTarkovHelperConstants.Logger.LogInfo($"reader:Position:{reader.BaseStream.Position}");
        //    //StayInTarkovHelperConstants.Logger.LogInfo($"reader:Length:{reader.BaseStream.Length}");
        //    //StayInTarkovHelperConstants.Logger.LogInfo(this.ToJson());

        //    DeserializePacketIntoObj(reader);

        //    //StayInTarkovHelperConstants.Logger.LogInfo(obj.ToJson());

        //    return this;
        //}

        //public ISITPacket DeserializePacketSIT(string serializedPacket)
        //{
        //    AutoDeserialize(Encoding.UTF8.GetBytes(serializedPacket));
        //    return this;
        //}

        //public ISITPacket DeserializePacketSIT(byte[] data)
        //{
        //    AutoDeserialize(data);
        //    return this;
        //}

        private void DeserializePacketIntoObj(BinaryReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));

            if (reader.BaseStream.Position >= reader.BaseStream.Length)
                return;

            try
            {
                var allPropsFiltered = GetPropertyInfos(this);
                if (allPropsFiltered == null)
                    return;

                foreach (var prop in allPropsFiltered)
                {
                    if (reader.BaseStream.Position >= reader.BaseStream.Length)
                        return;

                    //StayInTarkovHelperConstants.Logger.LogInfo($"{nameof(DeserializePacketIntoObj)}");
                    //StayInTarkovHelperConstants.Logger.LogInfo($"reader:Position:{reader.BaseStream.Position}");
                    //StayInTarkovHelperConstants.Logger.LogInfo($"reader:Length:{reader.BaseStream.Length}");
                    //StayInTarkovHelperConstants.Logger.LogInfo($"reader:Reading Prop:{prop.Name}");


                    // Is an array
                    if (prop.PropertyType.IsArray)
                    {
                        //StayInTarkovHelperConstants.Logger.LogDebug($"{prop.Name} is Array");
                        var arrayType = reader.ReadString().Replace("[", "").Replace("]", "");
                        var arrayCount = reader.ReadInt32();
                        //StayInTarkovHelperConstants.Logger.LogDebug($"{arrayType}");

                        // Find Array Type to Instantiate
                        var arrayTypeToInstantiate = StayInTarkovHelperConstants
                            .SITTypes
                            .Union(ReflectionHelpers.EftTypes)
                            .FirstOrDefault(x => x.FullName == arrayType);
                        if (arrayTypeToInstantiate == null)
                        {
                            StayInTarkovHelperConstants.Logger.LogError($"Failed to find: {arrayTypeToInstantiate}");
                            continue;
                        }
                        //StayInTarkovHelperConstants.Logger.LogDebug($"{arrayTypeToInstantiate}");
                        Array array = Array.CreateInstance(arrayTypeToInstantiate, arrayCount);
                        if (array == null)
                        {
                            StayInTarkovHelperConstants.Logger.LogError($"Failed to create array: {arrayTypeToInstantiate}");
                            continue;
                        }

                        for (var i = 0; i < arrayCount; i++)
                        {
                            if (arrayTypeToInstantiate.GetInterface(nameof(ISITPacket)) != null)
                            {
                                var packetType = reader.ReadString();
                                var packetByteLength = reader.ReadInt32();
                                StayInTarkovHelperConstants.Logger.LogDebug($"{packetByteLength}");
                                var packetBytes = reader.ReadBytes(packetByteLength);

                                //StayInTarkovHelperConstants.Logger.LogDebug($"{arrayTypeToInstantiate}");
                                //StayInTarkovHelperConstants.Logger.LogDebug($"{Encoding.UTF8.GetString(packetBytes)}");
                                //File.WriteAllBytes("t.bin", packetBytes);

                                //var sitPacket = Activator.CreateInstance(arrayTypeToInstantiate);
                                //DeserializePacketSIT(sitPacket, packetBytes);
                                packetBytes = null;
                            }
                            else
                            {

                            }
                        }
                        //array = null;

                        continue;
                    }
                    // Is a SITPacket
                    else if (prop.PropertyType.GetInterface(nameof(ISITPacket)) != null)
                    {
                        //StayInTarkovHelperConstants.Logger.LogDebug($"{prop.Name} is SIT Packet");

                        var packetType = reader.ReadString();
                        //StayInTarkovHelperConstants.Logger.LogDebug($"{packetType}");
                        var packetByteLength = reader.ReadInt32();
                        //StayInTarkovHelperConstants.Logger.LogDebug($"{packetByteLength}");
                        var packetBytes = reader.ReadBytes(packetByteLength);

                        var typeToInstantiate = StayInTarkovHelperConstants.SITTypes.FirstOrDefault(x => x.FullName == packetType);
                        if (typeToInstantiate != null)
                        {
                            var sitPacket = (BasePacket)Activator.CreateInstance(typeToInstantiate);
                            //StayInTarkovHelperConstants.Logger.LogDebug($"{prop.Name} is {Encoding.UTF8.GetString(packetBytes)}");
                            //sitPacket.DeserializePacketSIT(packetBytes);
                            //StayInTarkovHelperConstants.Logger.LogDebug($"{prop.Name} is SIT Packet and Deserialized");
                        }

                        packetBytes = null;
                        continue;
                    }
                    else if (prop.PropertyType == typeof(bool))
                    {
                        prop.SetValue(this, reader.ReadBoolean());
                        continue;
                    }


                    //StayInTarkovHelperConstants.Logger.LogDebug($"{prop.Name}");
                    var readString = reader.ReadString();
                    //StayInTarkovHelperConstants.Logger.LogDebug($"{prop.Name} to {readString}");

                    switch (prop.PropertyType.Name)
                    {
                        case "Float":
                            prop.SetValue(this, float.Parse(readString));
                            break;
                        case "Single":
                            prop.SetValue(this, Single.Parse(readString));
                            break;
                        //case "Boolean":
                        //    prop.SetValue(obj, Boolean.Parse(readString));
                        //    break;
                        case "String":
                            prop.SetValue(this, readString);
                            break;
                        case "Integer":
                        case "Int":
                        case "Int32":
                            prop.SetValue(this, int.Parse(readString));
                            break;
                        case "Double":
                            prop.SetValue(this, double.Parse(readString));
                            break;
                        case "Byte":
                            prop.SetValue(this, byte.Parse(readString));
                            break;
                        default:

                            // Process an Enum
                            if (prop.PropertyType.IsEnum)
                                prop.SetValue(this, Enum.Parse(prop.PropertyType, readString));


                            else
                            {
                                var jobj = JObject.Parse(readString);
                                var instance = jobj.ToObject(prop.PropertyType);
                                prop.SetValue(this, instance);
                                //StayInTarkovHelperConstants.Logger.LogError($"{prop.Name} of type {prop.PropertyType.Name} could not be parsed by SIT Deserializer!");
                            }
                            break;
                    }
                }
            }
            finally
            {
                reader.Close();
                reader.Dispose();
                reader = null;
            }

        }

        public Dictionary<string, object> ToDictionary(byte[] data)
        {
            Deserialize(data);

            // create a json obj from this obj
            var obj = JObject.FromObject(this);

            // read the header to discover SIT/ServerId/Method
            using BinaryReader reader = new BinaryReader(new MemoryStream(data));
            var headerExists = Encoding.UTF8.GetString(reader.ReadBytes(3)) == "SIT";
            reader.BaseStream.Position = 0;
            if (headerExists)
                ReadHeader(reader);

            if (!obj.ContainsKey("data"))
                obj.Add("data", data);

            if (!obj.ContainsKey("m"))
                obj.Add("m", Method);

            return JsonConvert.DeserializeObject<Dictionary<string, object>>(obj.SITToJson());
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

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~BasePacket()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        void INetSerializable.Serialize(NetDataWriter writer)
        {
            var serializedSIT = Serialize();
            writer.Put(serializedSIT.Length);
            writer.Put(serializedSIT);
        }

        void INetSerializable.Deserialize(NetDataReader reader)
        {
            var length = reader.GetInt();
            byte[] bytes = new byte[length];
            reader.GetBytes(bytes, length);
            Deserialize(bytes);
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


        public static Stopwatch swDeserializerDebug = new Stopwatch();

        
    }


}
