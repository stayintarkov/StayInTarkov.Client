﻿using Comfort.Common;
using UnityEngine;
using static EFT.ClientPlayer;

namespace StayInTarkov.Coop
{
    internal class CoopPlayerClient : CoopPlayer
    {
        public override void InitVoip(EVoipState voipState)
        {
            //base.InitVoip(voipState);
            SoundSettings settings = Singleton<SettingsManager>.Instance.Sound.Settings;
            var allVoipMethods = ReflectionHelpers.GetAllMethodsForType(typeof(PlayerVoipController), true);
            //var playerVOIP = (PlayerVoipController)(typeof(PlayerVoipController).GetMethods(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.CreateInstance).First(x => x.IsConstructor).Invoke(null, new object[] { this, settings }));
        }

        public override void Move(Vector2 direction)
        {
            base.Move(direction);
        }
    }
}
