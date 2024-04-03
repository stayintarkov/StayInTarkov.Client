using Google.FlatBuffers;
using StayInTarkov.FlatBuffers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace StayInTarkov.Coop.NetworkPacket.FlatBuffers
{   
    public static class Extensions
    {
        public static bool Empty(this PlayerState x) => x.ByteBuffer == null;
        public static Vector2 Unity(this Vec2 x) => new(x.X, x.Y);
        public static Vector3 Unity(this Vec3 x) => new(x.X, x.Y, x.Z);
    }
}
