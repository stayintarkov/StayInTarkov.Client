using EFT.Communications;
using System;
using System.Reflection;
using UnityEngine;

namespace StayInTarkov.UI
{
    internal static class DisplayMessageNotifications
    {
        public static Type MessageNotificationType { get; set; }

        public static void DisplayMessageNotification(
            string message
            , ENotificationDurationType duration = ENotificationDurationType.Default
            , ENotificationIconType icon = ENotificationIconType.Default
            , Color? color = null
            )
        {
            if (MessageNotificationType == null)
            {
                return;
            }

            var o = MessageNotificationType.GetMethod("DisplayMessageNotification", BindingFlags.Static | BindingFlags.Public);
            if (o != null)
            {
                o.Invoke("DisplayMessageNotification", new object[] { message, duration, icon, color });
            }

        }
    }
}
