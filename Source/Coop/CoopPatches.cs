using BepInEx.Logging;
using StayInTarkov.Coop.Session;
//using StayInTarkov.Coop.ItemControllerPatches;
using StayInTarkov.Coop.Matchmaker;
using StayInTarkov.Coop.Player;
//using StayInTarkov.Coop.Sounds;
using StayInTarkov.Coop.World;
//using StayInTarkov.Core.Player;
using StayInTarkov.Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using StayInTarkov.Coop.Components.CoopGameComponents;

namespace StayInTarkov.Coop
{
    internal class CoopPatches
    {
        internal static ManualLogSource Logger { get; private set; }

        private static BepInEx.Configuration.ConfigFile m_Config;

        internal static void Run(BepInEx.Configuration.ConfigFile config)
        {
            m_Config = config;

            if (Logger == null)
                Logger = BepInEx.Logging.Logger.CreateLogSource("Coop");

            var enabled = config.Bind<bool>("Coop", "Enable", true);
            if (!enabled.Value) // if it is disabled. stop all Coop stuff.
            {
                Logger.LogInfo("Coop has been disabled! Ignoring Patches.");
                return;
            }

            Logger.LogInfo("Stay in Tarkov - Enabling Coop Patches");

            new TarkovApplication_LocalGameCreator_Patch().Enable();
            new LoadLocationLootPatch().Enable();


            // ------ MATCHMAKER -------------------------
            SITMatchmaking.Run();

        }

        internal static List<ModulePatch> NoMRPPatches { get; } = new List<ModulePatch>();

        internal static GameObject CoopGameComponentParent { get; set; }

        internal static void EnableDisablePatches()
        {
            var enablePatches = true;

            var coopGC = SITGameComponent.GetCoopGameComponent();
            if (coopGC == null)
            {
                Logger.LogDebug($"CoopPatches:CoopGameComponent is null, Patches wont be Applied");
                enablePatches = false;
            }

            if (coopGC != null && !coopGC.enabled)
            {
                Logger.LogDebug($"CoopPatches:CoopGameComponent is not enabled, Patches wont be Applied");
                enablePatches = false;
            }

            if (string.IsNullOrEmpty(SITGameComponent.GetServerId()))
            {
                Logger.LogDebug($"CoopPatches:CoopGameComponent ServerId is not set, Patches wont be Applied");
                enablePatches = false;
            }

            if (!NoMRPPatches.Any())
            {
                //NoMRPPatches.Add(new Player_Init_Coop_Patch(m_Config));
                //NoMRPPatches.Add(new WeaponSoundPlayer_FireSonicSound_Patch());
                //NoMRPPatches.Add(new ItemControllerHandler_Move_Patch());
                NoMRPPatches.Add(new LootableContainer_Interact_Patch());
            }

            //Logger.LogInfo($"{NoMRPPatches.Count()} Non-MR Patches found");
            foreach (var patch in NoMRPPatches)
            {
                if (enablePatches)
                    patch.Enable();
                else
                    patch.Disable();
            }

            var moduleReplicationPatches = Assembly.GetAssembly(typeof(ModuleReplicationPatch))
                .GetTypes()
                .Where(x => x.GetInterface("IModuleReplicationPatch") != null);
            foreach (var module in moduleReplicationPatches)
            {
                if (module.IsAbstract
                    || module == typeof(ModuleReplicationPatch)
                    || module.Name.Contains(typeof(ModuleReplicationPatch).Name)
                    )
                    continue;

                ModuleReplicationPatch mrp = null;
                if (!ModuleReplicationPatch.Patches.Any(x => x.GetType() == module))
                {
                    mrp = (ModuleReplicationPatch)Activator.CreateInstance(module);
                    if (mrp.DisablePatch)
                    {
                        mrp = null;
                    }
                }
                else
                    mrp = ModuleReplicationPatch.Patches.Values.SingleOrDefault(x => x.GetType() == module);

                if (mrp == null)
                    continue;

                if (!mrp.DisablePatch && enablePatches)
                {
                    //Logger.LogInfo($"Enabled {mrp.GetType()}");
                    mrp.Enable();
                }
                else
                    mrp.Disable();
            }
        }

        internal static void LeftGameDestroyEverything()
        {
            EnableDisablePatches();

            if (SITGameComponent.TryGetCoopGameComponent(out var coopGameComponent))
            {
                //foreach (var p in coopGameComponent.Players)
                //{
                //    if (p.Value == null)
                //        continue;

                //    if (p.Value.TryGetComponent<PlayerReplicatedComponent>(out var prc))
                //    {
                //        GameObject.Destroy(prc);
                //    }
                //}

                //foreach (var pl in GameObject.FindObjectsOfType<CoopPlayer>())
                //{
                //    GameObject.DestroyImmediate(pl);
                //}

                coopGameComponent.RunAsyncTasks = false;
                GameObject.DestroyImmediate(coopGameComponent);
            }

            //foreach (var prc in GameObject.FindObjectsOfType<PlayerReplicatedComponent>())
            //{
            //    GameObject.DestroyImmediate(prc);
            //}


            if (CoopGameComponentParent != null)
                GameObject.DestroyImmediate(CoopGameComponentParent);

            //GCHelpers.DisableGC(true);
            //GCHelpers.ClearGarbage(true, true);

            AkiBackendCommunication.Instance.WebSocketClose();

            EnableDisablePatches();
        }
    }
}
