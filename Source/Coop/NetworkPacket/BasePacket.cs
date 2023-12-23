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
            return this.DeserializePacketSIT(bytes);
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

        public static Stopwatch swDeserializerDebug = new Stopwatch();

        public static ISITPacket DeserializePacketSIT<ISITPacket>(this ISITPacket obj, byte[] serializedPacket)
        {
            //StayInTarkovHelperConstants.Logger.LogInfo("DeserializePacketSIT<ISITPacket>");

            var stringOfSP = Encoding.UTF8.GetString(serializedPacket);
            //StayInTarkovHelperConstants.Logger.LogInfo(stringOfSP);
            var indexOfQuestionMark = stringOfSP.IndexOf('?');
            //File.WriteAllBytes("_sit_D_DeserializePacketSIT.bin", serializedPacket);

            long headerReaderEndPosition = indexOfQuestionMark != -1 ? ReadPacketHeader(ref obj, serializedPacket, indexOfQuestionMark) : 0;

            BinaryReader bodyReader = new BinaryReader(new MemoryStream(serializedPacket));
            bodyReader.BaseStream.Position = headerReaderEndPosition;
            var bodyPacketBytes = bodyReader.ReadBytes((int)bodyReader.BaseStream.Length - (int)headerReaderEndPosition);
            bodyReader.Close();
            bodyReader.Dispose();
            bodyReader = null;
            DeserializePacketIntoObj(ref obj, bodyPacketBytes);

            //StayInTarkovHelperConstants.Logger.LogInfo(obj.ToJson());

            return obj;
        }

        private static long ReadPacketHeader<ISITPacket>(ref ISITPacket obj, byte[] serializedPacket, int indexOfQuestionMark)
        {
            var playerPacket = obj as BasePlayerPacket;

            BinaryReader headerReader = new BinaryReader(new MemoryStream(serializedPacket));
            headerReader.ReadBytes(3); // SIT
            var profileIdBytes = headerReader.ReadBytes(27); // ProfileId
            if (playerPacket != null)
                playerPacket.ProfileId = Encoding.UTF8.GetString(profileIdBytes);

            var methodBytes = headerReader.ReadBytes(indexOfQuestionMark - 30); // Method
            if (playerPacket != null)
            {
                playerPacket.Method = Encoding.UTF8.GetString(methodBytes);
                //StayInTarkovHelperConstants.Logger.LogInfo(playerPacket.Method);
            }
            var headerReaderEndPosition = headerReader.BaseStream.Position + 1; // remove the ?
            headerReader.Close();
            headerReader.Dispose();
            headerReader = null;
            return headerReaderEndPosition;
        }

        public static T DeserializePacketSIT<T>(this T obj, string serializedPacket)
        {
            if (serializedPacket == null)
                throw new NullReferenceException(nameof(serializedPacket));

            var indexOfQuestionMark = serializedPacket.IndexOf('?');
            long headerReaderEndPosition = indexOfQuestionMark != -1 ? ReadPacketHeader(ref obj, Encoding.UTF8.GetBytes(serializedPacket), indexOfQuestionMark) : 0;
            var bodyString = serializedPacket.Substring((int)headerReaderEndPosition);
            //StayInTarkovHelperConstants.Logger.LogInfo(bodyString);
            DeserializePacketIntoObj(ref obj, Encoding.UTF8.GetBytes(bodyString));
            return obj;
            //string method = new string([]);
            //if (serializedPacket.Length == 0)
            //    return obj;

            //var indexOfQuestionMark = serializedPacket.IndexOf('?');
            //if (indexOfQuestionMark != -1)
            //{
            //    var headerFromPacket = serializedPacket.Substring(0, indexOfQuestionMark);

            //    StreamReader streamReaderHeader = new StreamReader(new MemoryStream());
            //    //StayInTarkovHelperConstants.Logger.LogDebug($"{headerFromPacket}");
            //    var sit = streamReaderHeader.ReadToEnd();
            //    //StayInTarkovHelperConstants.Logger.LogDebug($"{sit}");
            //    if (sit.Length < 3)
            //        return obj;
            //    var serverId = sit.Substring(2, 27);

            //    if (sit.Length < 31)
            //        return obj;

            //    method = sit.Substring(30, sit.Length - 30);
            //    //StayInTarkovHelperConstants.Logger.LogDebug($"{method}");
            //    sit = sit.Substring(0, 3);
            //    streamReaderHeader.Close();
            //    streamReaderHeader.Dispose();
            //    streamReaderHeader = null;
            //    sit.Clear();
            //    StayInTarkovHelperConstants.Logger.LogDebug($"{method}");
            //}

            //var bodyFromPacket = serializedPacket.Contains("?") ? serializedPacket.Split('?')[1] : serializedPacket;
            //var index = 0;
            //var bodyPacketBytes = Encoding.UTF8.GetBytes(bodyFromPacket);
            //DeserializePacketIntoObj(ref obj, bodyPacketBytes);

            //bodyPacketBytes = null;
            //bodyFromPacket = null;

            //if (((ISITPacket)obj).TimeSerializedBetter == null)
            //    ((ISITPacket)obj).TimeSerializedBetter = DateTime.Now.Ticks.ToString();

            //if (string.IsNullOrEmpty(((ISITPacket)obj).Method) && method != null)
            //    ((ISITPacket)obj).Method = method;


            //return obj;
        }

        private static void DeserializePacketIntoObj<T>(ref T obj, byte[] bodyPacketBytes)
        {
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
                    //Array array = Array.CreateInstance(ReflectionHelpers.SearchForType(arrayType), arrayCount);

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

                    packetBytes = null;
                    continue;
                }
                else if (prop.PropertyType == typeof(bool))
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
            }
            binaryReader.Close();
            binaryReader.Dispose();
            binaryReader = null;

        }
    }

}
