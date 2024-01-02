using Comfort.Common;
using EFT;
using EFT.Interactive;
using StayInTarkov.Core.Player;
using UnityEngine;
using static EFT.ClientPlayer;
using static EFT.UI.CharacterSelectionStartScreen;

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

        void Update()
        {
            //BepInLogger.LogDebug("Update");

            var prc = GetComponent<PlayerReplicatedComponent>();
            if (prc == null || !prc.IsClientDrone)
                return;

            prc.UpdateTick();
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

            var prc = GetComponent<PlayerReplicatedComponent>();
            if (prc == null || !prc.IsClientDrone)
                return;

            prc.UpdateTick();
        }

        public override void UpdateTick()
        {
            base.UpdateTick();

            var prc = GetComponent<PlayerReplicatedComponent>();
            if (prc == null || !prc.IsClientDrone)
                return;

            prc.UpdateTick();
        }
    }
}
