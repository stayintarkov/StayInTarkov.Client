using EFT;
using UnityEngine;

namespace StayInTarkov.AI
{
    /// <summary>
    /// Created by: DrakiaXYZ 
    /// This is a replacement for a GClass that does this process.
    /// Used by RoamingLogic from DrakiaXYZ
    /// </summary>
    internal class AIProcessLookAtPoints
    {
        private int int_0;

        private Vector3? nullable_0;

        private float float_0;

        public void Update(BotOwner _owner)
        {
            if (!_owner.Memory.GoalTarget.HavePlaceTarget() || !_owner.Memory.GoalTarget.GoalTarget.IsCome || float_0 > Time.time)
            {
                return;
            }
            _owner.BotLight.TurnOn();
            if (_owner.Memory.LastEnemy != null && Time.time - _owner.Memory.LastEnemyTimeSeen < _owner.Settings.FileSettings.Cover.LOOK_LAST_ENEMY_POS_LOOKAROUND)
            {
                _owner.Steering.LookToPoint(_owner.Memory.LastEnemy.EnemyLastPosition);
                return;
            }
            if (nullable_0.HasValue && int_0 >= 0 && (double)Time.time - _owner.Memory.GoalTarget.CreatedTime > _owner.Settings.FileSettings.Look.MIN_LOOK_AROUD_TIME)
            {
                _owner.Memory.GoalTarget.PointLookComplete(int_0);
                nullable_0 = null;
            }
            if (nullable_0.HasValue)
            {
                return;
            }
            int_0 = _owner.Memory.GoalTarget.GoalTarget.GetPointToLook(out nullable_0);
            if (!nullable_0.HasValue)
            {
                if ((double)Time.time - _owner.Memory.GoalTarget.CreatedTime > 15.0)
                {
                    _owner.BotsGroup.PointChecked(_owner.Memory.GoalTarget.GoalTarget);
                    _owner.Memory.GoalTarget.Clear();
                }
                nullable_0 = null;
                int_0 = -1;
                float_0 = 0f;
            }
            else
            {
                _owner.Steering.LookToPoint(nullable_0.Value, 30f);
                float num = _owner.Settings.FileSettings.Look.LOOK_AROUND_DELTA * (UnityEngine.Random.value + 0.5f);
                float_0 = num * _owner.Settings.FileSettings.Look.LOOK_AROUND_DELTA + Time.time;
            }
        }
    }
}
