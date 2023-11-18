using System.Collections.Generic;

namespace StayInTarkov.AkiSupport.Singleplayer.Models.Healing
{
    /// <summary>
    /// SPT-Aki PlayerHealth - https://dev.sp-tarkov.com/SPT-AKI/Modules/src/branch/master/project/Aki.SinglePlayer/Models/Healing/PlayerHealth.cs
    /// </summary>
    public class PlayerHealth
    {
        private readonly Dictionary<EBodyPart, BodyPartHealth> _health = new()
        {
            { EBodyPart.Head, new BodyPartHealth() },
            { EBodyPart.Chest, new BodyPartHealth() },
            { EBodyPart.Stomach, new BodyPartHealth() },
            { EBodyPart.LeftArm, new BodyPartHealth() },
            { EBodyPart.RightArm, new BodyPartHealth() },
            { EBodyPart.LeftLeg, new BodyPartHealth() },
            { EBodyPart.RightLeg, new BodyPartHealth() }
        };

        public bool IsAlive { get; set; } = true;

        public IReadOnlyDictionary<EBodyPart, BodyPartHealth> Health => _health;

        public float Hydration { get; set; } = 100;

        public float Energy { get; set; } = 100;

        public float Temperature { get; set; } = 40;
    }
}
