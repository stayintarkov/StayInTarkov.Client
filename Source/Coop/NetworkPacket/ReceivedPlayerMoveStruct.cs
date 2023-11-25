using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StayInTarkov.Coop.NetworkPacket
{
    internal struct ReceivedPlayerMoveStruct
    {
        public float pX { get; set; }

        public float pY { get; set; }

        public float pZ { get; set; }

        public float dX { get; set; }
        public float dY { get; set; }
        public float spd { get; set; }

        public ReceivedPlayerMoveStruct(float px, float py, float pz, float dx, float dy, float speed)
        {
            pX = px;
            pY = py;
            pZ = pz;
            dX = dx;
            dY = dy;
            spd = speed;
        }

    }
}
