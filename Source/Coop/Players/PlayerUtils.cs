using EFT.AssetsManager;
using StayInTarkov.Coop.Controllers.HandControllers;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.ParticleSystem.PlaybackState;

namespace StayInTarkov.Coop.Players
{
    public static class PlayerUtils
    {
        public static void MakeVisible(EFT.Player player, bool isVisible)
        {
            // Turn off all weapon lights
            if (!isVisible && player.HandsController is SITFirearmController fac)
            {
                var lights = new List<LightsStates>();

                foreach (var x in fac.GetAllLightMods())
                {
                    var state = x.GetLightState();
                    state.IsActive = false;
                    lights.Add(state);
                }

                fac.SetLightsState([.. lights], force: true);
            }

            // Turn off helmet lights
            if (!isVisible && player is CoopPlayer p)
            {
                foreach (var x in p.HelmetLightControllers)
                {
                    if (x.LightMod.GetLightState().IsActive)
                    {
                        player.SwitchHeadLights(togglesActive: true, changesState: false);
                    }
                }
            }

            // Toggle any animators and colliders
            if (player.HealthController.IsAlive)
            {
                IAnimator bodyAnimatorCommon = player.GetBodyAnimatorCommon();
                if (bodyAnimatorCommon.enabled != isVisible)
                {
                    bool flag = !bodyAnimatorCommon.enabled;
                    bodyAnimatorCommon.enabled = isVisible;
                    FirearmsAnimator firearmsAnimator = player.HandsController.FirearmsAnimator;
                    if (firearmsAnimator != null && firearmsAnimator.Animator.enabled != isVisible)
                    {
                        firearmsAnimator.Animator.enabled = isVisible;
                    }
                }

                PlayerPoolObject component = player.gameObject.GetComponent<PlayerPoolObject>();
                foreach (Collider collider in component.Colliders)
                {
                    if (collider.enabled != isVisible)
                    {
                        collider.enabled = isVisible;
                    }
                }

                player._characterController.GetCollider().enabled = isVisible;
            }

            // Build a list of renderers for this player object and set their rendering state
            List<Renderer> rendererList = new(256);
            player.PlayerBody.GetRenderersNonAlloc(rendererList);

            var firearmsController = player.gameObject.GetComponent<EFT.Player.FirearmController>();
            if (firearmsController != null)
            {
                var weaponPrefab = (WeaponPrefab)ReflectionHelpers.GetFieldFromType(typeof(EFT.Player.FirearmController), "weaponPrefab_0").GetValue(firearmsController);
                if (weaponPrefab != null)
                {
                    rendererList.AddRange(weaponPrefab.Renderers);
                }
            }

            rendererList.ForEach(renderer => renderer.forceRenderingOff = !isVisible);
        }
    }
}
