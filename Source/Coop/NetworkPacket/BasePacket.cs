using Comfort.Common;
using EFT.InventoryLogic;
using EFT.UI;
using Mono.Cecil;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
using UnityStandardAssets.Water;
using WebSocketSharp;

namespace StayInTarkov.Coop.NetworkPacket
{
    public abstract class BasePacket : ISITPacket, IDisposable
    {
        [JsonProperty(PropertyName = "serverId")]
        public string ServerId { get; set; }

        [JsonProperty(PropertyName = "t")]
        public string TimeSerializedBetter { get; set; }

        [JsonProperty(PropertyName = "m")]
        public virtual string Method { get; set; }

        //[JsonProperty(PropertyName = "pong")]
        //public virtual string Pong { get; set; } = DateTime.UtcNow.Ticks.ToString("G");

        public BasePacket(string method)
        {
            Method = method;
            ServerId = CoopGameComponent.GetServerId();
            TimeSerializedBetter = DateTime.Now.Ticks.ToString("G");
        }

        public static Dictionary<Type, PropertyInfo[]> TypeProperties = new ();
        private bool disposedValue;

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
            writer.WriteNonPrefixedString("SIT");
            writer.WriteNonPrefixedString(ServerId);
            writer.Write(Method);
        }

        public virtual void ReadHeader(BinaryReader reader)
        {
            reader.ReadBytes(3); // SIT
            reader.ReadBytes(27); // Server Id
            Method = reader.ReadString();
        }

        public byte[] AutoSerialize()
        {
            if (string.IsNullOrEmpty(ServerId))
            {
                throw new ArgumentNullException($"{GetType()}:{nameof(ServerId)}");
            }

            if (string.IsNullOrEmpty(Method))
            {
                throw new ArgumentNullException($"{GetType()}:{nameof(Method)}");
            }

            byte[] result = null;
            BinaryWriter binaryWriter = new(new MemoryStream());
            //binaryWriter.WriteNonPrefixedString("SIT"); // 3
            //binaryWriter.WriteNonPrefixedString(ServerId); // pmc + 24 chars
            //binaryWriter.WriteNonPrefixedString(Method); // Unknown
            //binaryWriter.WriteNonPrefixedString("?");
            WriteHeader(binaryWriter);

            var allPropsFiltered = GetPropertyInfos(this);
            if (allPropsFiltered == null)
                return null;

            // Extremely useful tool for discovering what is calling something else...
            //#if DEBUG
            //            System.Diagnostics.StackTrace t = new System.Diagnostics.StackTrace(true);
            //                StayInTarkovHelperConstants.Logger.LogInfo($"{t.ToString()}");
            //#endif


            for (var i = 0; i < allPropsFiltered.Count(); i++)
            {
                var prop = allPropsFiltered[i];
                var propValue = prop.GetValue(this);
                //StayInTarkovHelperConstants.Logger.LogInfo($"{prop.Name} is {propValue}");

                // write is not null
                binaryWriter.Write(propValue != null);
                if (propValue == null)
                    continue;

                // Process an Array type
                if (prop.PropertyType.IsArray)
                {
                    Array array = (Array)propValue;
                    binaryWriter.Write(prop.PropertyType.FullName);
                    binaryWriter.Write(array.Length);

                    foreach (var item in array)
                    {
                        if (item.GetType().GetInterface(nameof(ISITPacket)) != null)
                        {
                            var serializedSITPacket = ((ISITPacket)item).Serialize();
                            binaryWriter.Write((int)serializedSITPacket.Length);
                            binaryWriter.Write(serializedSITPacket);
                        }
                        else
                            binaryWriter.Write(item.ToString());
                    }

                }
                // Process an ISITPacket
                else if (prop.PropertyType.GetInterface(nameof(ISITPacket)) != null)
                {
                    //StayInTarkovHelperConstants.Logger.LogInfo(prop.PropertyType);
                    //StayInTarkovHelperConstants.Logger.LogInfo(propValue.SITToJson());
                    binaryWriter.Write(prop.PropertyType.FullName);
                    var serializedSITPacket = ((ISITPacket)propValue).Serialize();
                    binaryWriter.Write(serializedSITPacket.Length);
                    binaryWriter.Write(serializedSITPacket);
                }
                else if (prop.PropertyType == typeof(bool))
                {
                    binaryWriter.Write((bool)propValue);
                }
                else
                {
                    //StayInTarkovHelperConstants.Logger.LogInfo($"Write {prop.Name} as (string){propValue}");
                    binaryWriter.Write(propValue.ToString());
                }

                //if (i != allPropsFiltered.Count() - 1)
                //    binaryWriter.WriteNonPrefixedString(SerializerExtensions.SIT_SERIALIZATION_PACKET_SEPERATOR.ToString());
            }

            if (binaryWriter != null)
            {
                result = ((MemoryStream)binaryWriter.BaseStream).ToArray();
                binaryWriter.Close();
                binaryWriter.Dispose();
                binaryWriter = null;
            }

            return result;
        }

        public virtual byte[] Serialize()
        {
            return AutoSerialize();
        }

        public virtual ISITPacket Deserialize(byte[] bytes)
        {
            return this.AutoDeserialize(bytes);
        }

        public virtual ISITPacket AutoDeserialize(byte[] serializedPacket)
        {
            BinaryReader reader = new BinaryReader(new MemoryStream(serializedPacket));
            ReadHeader(reader);
            var bodyPacketBytes = reader.ReadBytes((int)reader.BaseStream.Length - (int)reader.BaseStream.Position);
            reader.Close();
            reader.Dispose();
            reader = null;
            DeserializePacketIntoObj(bodyPacketBytes);

            //StayInTarkovHelperConstants.Logger.LogInfo(obj.ToJson());

            return this;
        }

        public ISITPacket DeserializePacketSIT(string serializedPacket)
        {
            AutoDeserialize(Encoding.UTF8.GetBytes(serializedPacket));
            return this;
        }

        private void DeserializePacketIntoObj(byte[] bodyPacketBytes)
        {
            var binaryReader = new BinaryReader(new MemoryStream(bodyPacketBytes));

            try
            {

                foreach (var prop in BasePacket.GetPropertyInfos(this.GetType()))
                {
                    if (binaryReader.BaseStream.Position >= binaryReader.BaseStream.Length)
                    {
                        //ConsoleScreen.LogError($"{nameof(DeserializePacketSIT)} stream cannot be read beyond the length. Failed on property {prop.Name}");
                        //StayInTarkovHelperConstants.Logger.LogError($"{nameof(DeserializePacketSIT)} stream cannot be read beyond the length. Failed on property {prop.Name}");
                        continue;
                    }

                    //StayInTarkovHelperConstants.Logger.LogDebug($"PropName:{prop.Name}");
                    //StayInTarkovHelperConstants.Logger.LogDebug($"Pos:{binaryReader.BaseStream.Position}");
                    //StayInTarkovHelperConstants.Logger.LogDebug($"ExpectedType:{prop.PropertyType.Name}");

                    var isNotNull = binaryReader.ReadBoolean();
                    if (!isNotNull)
                    {
                        StayInTarkovHelperConstants.Logger.LogDebug($"{prop.Name} is NULL");
                        continue;
                    }

                    // Is an array
                    if (prop.PropertyType.IsArray)
                    {
                        //StayInTarkovHelperConstants.Logger.LogDebug($"{prop.Name} is Array");
                        var arrayType = binaryReader.ReadString().Replace("[", "").Replace("]", "");
                        var arrayCount = binaryReader.ReadInt32();
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
                                var packetType = binaryReader.ReadString();
                                var packetByteLength = binaryReader.ReadInt32();
                                StayInTarkovHelperConstants.Logger.LogDebug($"{packetByteLength}");
                                var packetBytes = binaryReader.ReadBytes(packetByteLength);

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

                        var packetType = binaryReader.ReadString();
                        //StayInTarkovHelperConstants.Logger.LogDebug($"{packetType}");
                        var packetByteLength = binaryReader.ReadInt32();
                        //StayInTarkovHelperConstants.Logger.LogDebug($"{packetByteLength}");
                        var packetBytes = binaryReader.ReadBytes(packetByteLength);

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
                        prop.SetValue(this, binaryReader.ReadBoolean());
                        continue;
                    }


                    //StayInTarkovHelperConstants.Logger.LogDebug($"{prop.Name}");
                    var readString = binaryReader.ReadString();
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
                binaryReader.Close();
                binaryReader.Dispose();
                binaryReader = null;
            }

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
    }

    public interface ISITPacket
    {
        public string ServerId { get; set; }
        public string TimeSerializedBetter { get; set; }
        public string Method { get; set; }
        byte[] Serialize();

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
            binaryWriter.Write(value.Length);
            binaryWriter.Write(value);
        }

        public static byte[] ReadLengthPrefixedBytes(this BinaryReader binaryReader)
        {
            var length = binaryReader.ReadInt32();
           return binaryReader.ReadBytes(length);
        }


        public static Stopwatch swDeserializerDebug = new Stopwatch();

        
    }

}
