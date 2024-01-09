using StayInTarkov.Coop.Components.CoopGameComponents;
using StayInTarkov.Coop.NetworkPacket;
using StayInTarkov.Core.Player;
using StayInTarkov.Networking;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace StayInTarkov.Coop.Player.FirearmControllerPatches
{
    public class FirearmController_SetTriggerPressed_Patch : ModuleReplicationPatch
    {
        public override Type InstanceType => typeof(EFT.Player.FirearmController);
        public override string MethodName => "SetTriggerPressed";

        protected override MethodBase GetTargetMethod()
        {
            var method = ReflectionHelpers.GetMethodForType(InstanceType, MethodName);
            return method;
        }

        public override void Enable()
        {
            base.Enable();
            LastPress.Clear();
        }

        public static List<string> CallLocally = new();

        public static Dictionary<string, bool> LastPress = new();


        [PatchPrefix]
        public static bool PrePatch(
            EFT.Player.FirearmController __instance
            , EFT.Player ____player
            , bool pressed
            )
        {
            if (CoopGameComponent.GetCoopGameComponent() == null)
                return false;

            if (AkiBackendCommunication.Instance.HighPingMode && ____player.IsYourPlayer)
            {
                return true;
            }

            var player = ____player;
            if (player == null)
                return false;

            var result = false;
            if (CallLocally.Contains(player.ProfileId))
                result = true;

            return result;
        }

        [PatchPostfix]
        public static void PostPatch(
            EFT.Player.FirearmController __instance
            , bool pressed
            , EFT.Player ____player
            )
        {
            var player = ____player;
            if (player == null)
                return;

            if (player.IsSprintEnabled)
                return;

            if (CallLocally.Contains(player.ProfileId))
            {
                CallLocally = CallLocally.Where(x => x != player.ProfileId).ToList();
                return;
            }

            if (player.TryGetComponent<PlayerReplicatedComponent>(out var prc) && prc.IsClientDrone)
                return;

            // Handle LastPress
            if (LastPress.ContainsKey(player.ProfileId) && LastPress[player.ProfileId] == pressed)
                return;

            if (!LastPress.ContainsKey(player.ProfileId))
                LastPress.Add(player.ProfileId, pressed);

            LastPress[player.ProfileId] = pressed;

            TriggerPressedPacket triggerPressedPacket = new(player.ProfileId);
            triggerPressedPacket.pr = pressed;
            triggerPressedPacket.rX = player.Rotation.x;
            triggerPressedPacket.rY = player.Rotation.y;
            var serialized = triggerPressedPacket.Serialize();
            AkiBackendCommunication.Instance.SendDataToPool(serialized);
        }


        public override void Replicated(EFT.Player player, Dictionary<string, object> dict)
        {
            if (AkiBackendCommunication.Instance.HighPingMode && player.IsYourPlayer)
            {
                // You would have already run this. Don't bother
                return;
            }

            if (!player.PlayerHealthController.IsAlive)
            {
                if (player.HandsController is EFT.Player.FirearmController fc)
                {
                    fc.SetTriggerPressed(false);
                }
                return;
            }
            TriggerPressedPacket tpp = new(player.ProfileId);

            //Logger.LogInfo("Pressed:Replicated");
            if (!dict.ContainsKey("data"))
            {
                Logger.LogError($"{nameof(FirearmController_SetTriggerPressed_Patch)}:{nameof(Replicated)} data is missing");
                return;
            }

            tpp.DeserializePacketSIT(dict["data"].ToString());

            if (HasProcessed(GetType(), player, tpp))
                return;

            if (!player.TryGetComponent<PlayerReplicatedComponent>(out var prc))
                return;

            if (CallLocally.Contains(player.ProfileId))
                return;

            CallLocally.Add(player.ProfileId);

            bool pressed = tpp.pr; // bool.Parse(dict["pr"].ToString());

            if (player.HandsController is EFT.Player.FirearmController firearmCont)
            {
                try
                {
                    //if (pressed && dict.ContainsKey("rX"))
                    if (prc.IsClientDrone && tpp.rX != 0)
                    {
                        var rotat = new Vector2(tpp.rX, tpp.rY);
                        player.Rotation = rotat;
                    }
                    //firearmCont.SetTriggerPressed(pressed);
                    firearmCont.StartCoroutine(SetTriggerPressedCR(player, firearmCont, pressed));

                    //ReplicatedShotEffects(player, pressed);


                }
                catch (Exception e)
                {
                    Logger.LogInfo(e);
                }
            }
            //});
        }

        private IEnumerator SetTriggerPressedCR(EFT.Player player
           , EFT.Player.FirearmController firearmCont
           , bool pressed)
        {
            while (!firearmCont.CanPressTrigger() && pressed)
            {
                yield return new WaitForEndOfFrame();
            }

            firearmCont.SetTriggerPressed(pressed);
            yield break;
        }

        



    }
}
