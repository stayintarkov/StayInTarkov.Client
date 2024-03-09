using Aki.Custom.Airdrops.Models;
using Newtonsoft.Json;
using System.Numerics;

namespace StayInTarkov.AkiSupport.Airdrops.Models
{
    /// <summary>
    /// Created by: SPT-Aki team
    /// Link: https://dev.sp-tarkov.com/SPT-AKI/Modules/src/branch/master/project/Aki.Custom/Airdrops/Models
    /// Paulov: Property instead of Fields so I can easily Json the Model
    /// </summary>
    public class AirdropParametersModel
    {
        public AirdropConfigModel Config { get; set; }

        public bool AirdropAvailable { get; set; }
        public bool PlaneSpawned { get; set; }
        public bool BoxSpawned { get; set; }
        public float DistanceTraveled { get; set; }
        public float DistanceToTravel { get; set; }
        public float DistanceToDrop { get; set; }
        public float Timer { get; set; }
        public int DropHeight { get; set; }
        public int TimeToStart { get; set; }
        public UnityEngine.Vector3 PlaneSpawnPoint { get; set; }
        public UnityEngine.Vector3 PlaneLookAt { get; set; }

        [JsonIgnore]
        public UnityEngine.Vector3 RandomAirdropPoint { get; set; } = UnityEngine.Vector3.zero;

        public float RandomAirdropPointX { get { return RandomAirdropPoint.x; } set { RandomAirdropPoint = new UnityEngine.Vector3(value, RandomAirdropPoint.y, RandomAirdropPoint.z); } }
        public float RandomAirdropPointY { get { return RandomAirdropPoint.y; } set { RandomAirdropPoint = new UnityEngine.Vector3(RandomAirdropPoint.x, value, RandomAirdropPoint.z); } }
        public float RandomAirdropPointZ { get { return RandomAirdropPoint.z; } set { RandomAirdropPoint = new UnityEngine.Vector3(RandomAirdropPoint.x, RandomAirdropPoint.y, value); } }
    }
}