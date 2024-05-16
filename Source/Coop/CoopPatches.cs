﻿using BepInEx.Logging;
using StayInTarkov.Coop.AI;
using StayInTarkov.Coop.Components.CoopGameComponents;
//using StayInTarkov.Coop.ItemControllerPatches;
using StayInTarkov.Coop.Matchmaker;
using StayInTarkov.Coop.Player;
using StayInTarkov.Coop.Session;
//using StayInTarkov.Coop.Sounds;
using StayInTarkov.Coop.World;
//using StayInTarkov.Core.Player;
using StayInTarkov.Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

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
            //new LoadLocationLootPatch().Enable();


            // ------ MATCHMAKER -------------------------
            SITMatchmaking.Run();

        }

        internal static List<ModulePatch> NoMRPPatches { get; } = new List<ModulePatch>();

        internal static void EnableDisablePatches()
        {
            // Paulov: There is no reason to disable these anymore as all games are now MP
            var enablePatches = true;

            if (!NoMRPPatches.Any())
            {
                NoMRPPatches.Add(new LootableContainer_Interact_Patch());
                NoMRPPatches.Add(new BotDespawnPatch());
            }

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
                coopGameComponent.RunAsyncTasks = false;
                GameObject.DestroyImmediate(coopGameComponent);
            }

            AkiBackendCommunication.Instance.WebSocketClose();

            EnableDisablePatches();
        }
    }
}
