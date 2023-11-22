using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Globalization;
using System.Reflection;
using UnityEngine;

namespace StayInTarkov
{
    /// <summary>
    /// Created by Paulov
    /// Help Json Convert back and forth between extra data types
    /// </summary>
    internal class PaulovJsonConverters
    {
        internal class DateTimeOffsetJsonConverter : JsonConverter<DateTimeOffset>
        {
            public override DateTimeOffset ReadJson(JsonReader reader, Type objectType, DateTimeOffset existingValue, bool hasExistingValue, JsonSerializer serializer)
            {
                return DateTimeOffset.ParseExact(reader.ReadAsString()!,
                        "MM/dd/yyyy", CultureInfo.InvariantCulture);
            }

            public override void WriteJson(JsonWriter writer, DateTimeOffset value, JsonSerializer serializer)
            {
                writer.WriteValue(value.ToString(
                       "MM/dd/yyyy", CultureInfo.InvariantCulture));
            }
        }

        internal class SimpleCharacterControllerJsonConverter : JsonConverter<SimpleCharacterController>
        {
            public override bool CanRead => false;
            public override bool CanWrite => false;

            public override SimpleCharacterController ReadJson(JsonReader reader, Type objectType, SimpleCharacterController existingValue, bool hasExistingValue, JsonSerializer serializer)
            {
                return null;
            }

            public override void WriteJson(JsonWriter writer, SimpleCharacterController value, JsonSerializer serializer)
            {

            }
        }

        internal class CollisionFlagsJsonConverter : JsonConverter<CollisionFlags>
        {
            public override bool CanRead => true;
            public override bool CanWrite => true;

            public override CollisionFlags ReadJson(JsonReader reader, Type objectType, CollisionFlags existingValue, bool hasExistingValue, JsonSerializer serializer)
            {
                if (Enum.TryParse<CollisionFlags>(reader.ReadAsString(), out var collisionFlags))
                    return collisionFlags;

                return CollisionFlags.None;
            }

            public override void WriteJson(JsonWriter writer, CollisionFlags value, JsonSerializer serializer)
            {
                writer.WriteValue(value.ToString());
            }
        }

        public class PlayerJsonConverter : JsonConverter
        {
            public override bool CanConvert(Type objectType)
            {
                return objectType == typeof(EFT.Player);
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                throw new NotImplementedException();
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                serializer.Converters.Add(new CollisionFlagsJsonConverter());
                foreach (var c in StayInTarkovHelperConstants.JsonConverterDefault)
                    serializer.Converters.Add(c);

                JObject jo = new();
                Type type = value.GetType();
                jo.Add("type", type.Name);
                foreach (PropertyInfo prop in type.GetProperties())
                {
                    if (prop.CanRead)
                    {
                        object propVal = prop.GetValue(value, null);
                        if (propVal != null)
                        {
                            jo.Add(prop.Name, JToken.FromObject(propVal, serializer));
                        }
                    }
                }
                jo.WriteTo(writer);
            }
        }

    }
}
