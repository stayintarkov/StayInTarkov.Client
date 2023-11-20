using Aki.Custom.Airdrops.Models;

namespace StayInTarkov.AkiSupport.Airdrops.Models
{
    /// <summary>
    /// Created by: SPT-Aki team
    /// Link: https://dev.sp-tarkov.com/SPT-AKI/Modules/src/branch/master/project/Aki.Custom/Airdrops/Models
    /// </summary>
    public class AirdropParametersModel
    {
        public AirdropConfigModel Config;

        public bool AirdropAvailable;
        public bool PlaneSpawned;
        public bool BoxSpawned;
        public float DistanceTraveled;
        public float DistanceToTravel;
        public float DistanceToDrop;
        public float Timer;
        public int DropHeight;
        public int TimeToStart;

        public UnityEngine.Vector3 RandomAirdropPoint;
    }
}