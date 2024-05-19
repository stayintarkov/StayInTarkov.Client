using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StayInTarkov.Multiplayer.BTR.Models
{
    public class BTRConfigModel
    {
        [JsonProperty("moveSpeed")]
        public float MoveSpeed { get; set; }

        [JsonProperty("coverFireTime")]
        public float CoverFireTime { get; set; }

        [JsonProperty("pointWaitTime")]
        public BtrMinMaxValue PointWaitTime { get; set; }

        [JsonProperty("taxiWaitTime")]
        public float TaxiWaitTime { get; set; }

        [JsonProperty("machineGunAimDelay")]
        public float MachineGunAimDelay { get; set; }

        [JsonProperty("machineGunBurstCount")]
        public BtrMinMaxValue MachineGunBurstCount { get; set; }

        [JsonProperty("machineGunRecoveryTime")]
        public BtrMinMaxValue MachineGunRecoveryTime { get; set; }
    }

    public class BtrMinMaxValue
    {
        [JsonProperty("min")]
        public float Min { get; set; }

        [JsonProperty("max")]
        public float Max { get; set; }
    }
}
