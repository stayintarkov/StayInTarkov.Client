using LiteNetLib.Utils;
using UnityEngine;
using static Physical;

namespace StayInTarkov.Networking
{
    internal class StructUtils
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
    }
}
