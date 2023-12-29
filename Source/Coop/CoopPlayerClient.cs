using Comfort.Common;
using EFT;
using EFT.Interactive;
using UnityEngine;
using static EFT.ClientPlayer;

namespace StayInTarkov.Coop
{
    public class CoopPlayerClient : CoopPlayer
    {
        public override void InitVoip(EVoipState voipState)
        {
            //base.InitVoip(voipState);
            SoundSettings settings = Singleton<SettingsManager>.Instance.Sound.Settings;
        }

        public override void Move(Vector2 direction)
        {
            //base.Move(direction);
        }

        new void Update()
        {
            //BepInLogger.LogDebug("Update");
        }

        new void LateUpdate()
        {
            //base.LateUpdate();
            //BepInLogger.LogDebug("LateUpdate");
            MovementContext?.AnimatorStatesLateUpdate();
            DistanceDirty = true;
            OcclusionDirty = true;
            if (HealthController != null && HealthController.IsAlive)
            {
                Physical.LateUpdate();
                VisualPass();
                _armsupdated = false;
                _bodyupdated = false;
                if (_nFixedFrames > 0)
                {
                    _nFixedFrames = 0;
                    _fixedTime = 0f;
                }
                ProceduralWeaponAnimation.StartFovCoroutine(this);
                PropUpdate();
            }
            ComplexLateUpdate(EUpdateQueue.Update, DeltaTime);
        }
    }
}
