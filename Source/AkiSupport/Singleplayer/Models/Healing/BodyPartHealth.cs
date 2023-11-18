using System.Collections.Generic;

namespace StayInTarkov.AkiSupport.Singleplayer.Models.Healing
{
    /// <summary>
    /// Credit SPT-Aki team
    /// https://dev.sp-tarkov.com/SPT-AKI/Modules/src/branch/master/project/Aki.SinglePlayer/Models/Healing/BodyPartHealth.cs
    /// </summary>
    public class BodyPartHealth
    {
        private Dictionary<BodyPartEffect, float> _effects = new();

        public float Maximum { get; private set; }
        public float Current { get; private set; }

        public IReadOnlyDictionary<BodyPartEffect, float> Effects => _effects;

        public void Initialize(float current, float maximum)
        {
            Maximum = maximum;
            Current = current;
        }

        public void ChangeHealth(float diff)
        {
            Current += diff;
        }

        public void AddEffect(BodyPartEffect bodyPartEffect, float time = -1)
        {
            _effects[bodyPartEffect] = time;
        }

        public void RemoveEffect(BodyPartEffect bodyPartEffect)
        {
            if (_effects.ContainsKey(bodyPartEffect))
                _effects.Remove(bodyPartEffect);
        }
    }
    public enum BodyPartEffect
    {
        BreakPart
    }
}
