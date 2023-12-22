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

        public virtual byte[] Serialize()
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
            binaryWriter.WriteNonPrefixedString("SIT"); // 3
            binaryWriter.WriteNonPrefixedString(ServerId); // pmc + 24 chars
            binaryWriter.WriteNonPrefixedString(Method); // Unknown
            binaryWriter.WriteNonPrefixedString("?");

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
                            binaryWriter.Write(serializedSITPacket.Length);
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

        public virtual ISITPacket Deserialize(byte[] bytes)
        {
            var resultString = Encoding.UTF8.GetString(bytes);
            //StayInTarkovHelperConstants.Logger.LogInfo(resultString);   
            return this.DeserializePacketSIT(resultString);
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

        public static Stopwatch swDeserializerDebug = new Stopwatch();

        public static T DeserializePacketSIT<T>(this T obj, string serializedPacket)
        {
            //File.WriteAllText("_serializedPacket.bin", serializedPacket);

            //swDeserializerDebug.Restart();

            //StayInTarkovHelperConstants.Logger.LogDebug($"{nameof(DeserializePacketSIT)}");
            //StayInTarkovHelperConstants.Logger.LogDebug($"{serializedPacket}");

            string method = new string([]);
            if (serializedPacket.Contains("?"))
            {
                var headerFromPacket = serializedPacket.Split('?')[0];
                StreamReader streamReaderHeader = new StreamReader(new MemoryStream());
                //StayInTarkovHelperConstants.Logger.LogDebug($"{headerFromPacket}");
                var sit = streamReaderHeader.ReadToEnd();
                //StayInTarkovHelperConstants.Logger.LogDebug($"{sit}");
                var serverId = sit.Substring(2, 27);
                method = sit.Substring(30, sit.Length - 30);
                //StayInTarkovHelperConstants.Logger.LogDebug($"{method}");
                sit = sit.Substring(0, 3);
                streamReaderHeader.Close();
                streamReaderHeader.Dispose();
                streamReaderHeader = null;
                sit.Clear();
                //StayInTarkovHelperConstants.Logger.LogDebug($"{method}");
            }

            var bodyFromPacket = serializedPacket.Contains("?") ? serializedPacket.Split('?')[1] : serializedPacket;
            //serializedPacket.Clear();
            //serializedPacket = null;
            //StayInTarkovHelperConstants.Logger.LogDebug($"{bodyFromPacket}");
            var separatedPacket = bodyFromPacket.Split(SIT_SERIALIZATION_PACKET_SEPERATOR);
            var index = 0;

            var bodyPacketBytes = Encoding.UTF8.GetBytes(bodyFromPacket);
            //bodyFromPacket.Clear();
            //bodyFromPacket = null;

            //File.WriteAllBytes("t.bin", bodyPacketBytes);

            var binaryReader = new BinaryReader(new MemoryStream(bodyPacketBytes));

            foreach (var prop in BasePacket.GetPropertyInfos(obj.GetType()))
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
                    index++;
                    StayInTarkovHelperConstants.Logger.LogDebug($"{prop.Name} is NULL");
                    continue;
                }
                
                // Is an array
                if (prop.PropertyType.IsArray)
                {
                    //StayInTarkovHelperConstants.Logger.LogDebug($"{prop.Name} is Array");
                    //Array array = new Array();
                    //binaryWriter.Write(prop.PropertyType.Name);
                    //binaryWriter.Write(array.Length);
                    var arrayType = binaryReader.ReadString().Replace("[", "").Replace("]", "");
                    var arrayCount = binaryReader.ReadInt32();
                    StayInTarkovHelperConstants.Logger.LogDebug($"{arrayType}");
                    Array array = Array.CreateInstance(ReflectionHelpers.SearchForType(arrayType), arrayCount);

                    //foreach (var item in array)
                    //{
                    //    if (item.GetType().GetInterface(nameof(ISITPacket)) != null)
                    //    {
                    //        var serializedSITPacket = ((ISITPacket)item).Serialize();
                    //        binaryWriter.Write(serializedSITPacket.Length);
                    //        binaryWriter.Write(serializedSITPacket);
                    //    }
                    //    else
                    //        binaryWriter.Write(item.ToString());
                    //}
                    array = null;
                    index++;
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

                    packetBytes = null;
                    index++;
                    continue;
                }
                else if(prop.PropertyType == typeof(bool))
                {
                    prop.SetValue(obj, binaryReader.ReadBoolean());
                    continue;
                }


                //StayInTarkovHelperConstants.Logger.LogDebug($"{prop.Name}");
                var readString = binaryReader.ReadString();
                //StayInTarkovHelperConstants.Logger.LogDebug($"{prop.Name} to {readString}");

                switch (prop.PropertyType.Name)
                {
                    case "Float":
                        prop.SetValue(obj, float.Parse(readString));
                        break;
                    case "Single":
                        prop.SetValue(obj, Single.Parse(readString));
                        break;
                    //case "Boolean":
                    //    prop.SetValue(obj, Boolean.Parse(readString));
                    //    break;
                    case "String":
                        prop.SetValue(obj, readString);
                        break;
                    case "Integer":
                    case "Int":
                    case "Int32":
                        prop.SetValue(obj, int.Parse(readString));
                        break;
                    case "Double":
                        prop.SetValue(obj, double.Parse(readString));
                        break;
                    case "Byte":
                        prop.SetValue(obj, byte.Parse(readString));
                        break;
                    default:

                        // Process an Enum
                        if (prop.PropertyType.IsEnum)
                            prop.SetValue(obj, Enum.Parse(prop.PropertyType, readString));
                        
                        
                        else
                        {
                            var jobj = JObject.Parse(readString);
                            var instance = jobj.ToObject(prop.PropertyType);
                            prop.SetValue(obj, instance);
                            //StayInTarkovHelperConstants.Logger.LogError($"{prop.Name} of type {prop.PropertyType.Name} could not be parsed by SIT Deserializer!");
                        }
                        break;
                }
                index++;
            }

//#if DEBUG
//            if (swDeserializerDebug.ElapsedMilliseconds > 1)
//                StayInTarkovHelperConstants.Logger.LogDebug($"DeserializePacketSIT {obj.GetType()} took {swDeserializerDebug.ElapsedMilliseconds}ms to process!");
//#endif

            bodyPacketBytes = null;
            bodyFromPacket = null;
            separatedPacket = null;
            binaryReader.Close();
            binaryReader.Dispose();
            binaryReader = null;

            if (((ISITPacket)obj).TimeSerializedBetter == null)
                ((ISITPacket)obj).TimeSerializedBetter = DateTime.Now.Ticks.ToString();

            if (string.IsNullOrEmpty(((ISITPacket)obj).Method) && method != null)
                ((ISITPacket)obj).Method = method;


            return obj;
        }
    }

}
