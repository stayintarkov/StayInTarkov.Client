//using Comfort.Common;
//using DrakiaXYZ.Waypoints.Helpers;
//using EFT;
//using HarmonyLib;
//using Newtonsoft.Json;
//using SIT.Tarkov.Core;
//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.IO;
//using System.Linq;
//using System.Reflection;
//using UnityEngine;
//using UnityEngine.AI;
//using Random = UnityEngine.Random;

//namespace DrakiaXYZ.Waypoints.Patches
//{
//    public class WaypointPatch : ModulePatch
//    {
//        private static int customWaypointCount = 0;
//        private static FieldInfo _doorLinkListField;

//        protected override MethodBase GetTargetMethod()
//        {
//            _doorLinkListField = AccessTools.Field(typeof(BotCellController), "navMeshDoorLink_0");

//            return typeof(BotControllerClass).GetMethod("Init");
//        }

//        /// <summary>
//        /// 
//        /// </summary>
//        [PatchPrefix]
//        private static void PatchPrefix(BotZone[] botZones)
//        {
//            var gameWorld = Singleton<GameWorld>.Instance;
//            if (gameWorld == null)
//            {
//                Logger.LogError("BotController::Init called, but GameWorld doesn't exist");
//                return;
//            }

//            if (botZones != null)
//            {
//                InjectWaypoints(gameWorld, botZones);
//            }

//            if (Settings.EnableCustomNavmesh.Value)
//            {
//                InjectNavmesh(gameWorld);
//            }
//        }

//        /// <summary>
//        /// Re-calculate the doorlink navmesh carvers, since these are baked into the map, but we've
//        /// changed the navmesh
//        /// </summary>
//        [PatchPostfix]
//        private static void PatchPostfix(BotControllerClass __instance)
//        {
//            NavMeshDoorLink[] doorLinkList = _doorLinkListField.GetValue(__instance.GetCellController()) as NavMeshDoorLink[];
//            if (doorLinkList != null)
//            {
//                foreach (NavMeshDoorLink doorLink in doorLinkList)
//                {
//                    doorLink.CheckAfterCreatedCarver();
//                }
//            }
//            else
//            {
//                Logger.LogError($"Error finding doorLinkList");
//            }

//            var gameWorld = Singleton<GameWorld>.Instance;
//            FixMap(gameWorld);
//        }

//        private static void InjectWaypoints(GameWorld gameWorld, BotZone[] botZones)
//        {
//            string mapName = gameWorld.MainPlayer.Location.ToLower();
//            customWaypointCount = 0;

//            var stopwatch = new Stopwatch();
//            stopwatch.Start();

//            // Inject our loaded patrols
//            foreach (BotZone botZone in botZones)
//            {
//                Dictionary<string, CustomPatrol> customPatrols = CustomWaypointLoader.Instance.getMapZonePatrols(mapName, botZone.NameZone);
//                if (customPatrols != null)
//                {
//                    Logger.LogDebug($"Found custom patrols for {mapName} / {botZone.NameZone}");
//                    foreach (string patrolName in customPatrols.Keys)
//                    {
//                        AddOrUpdatePatrol(botZone, customPatrols[patrolName]);
//                    }
//                }
//            }

//            stopwatch.Stop();
//            Logger.LogDebug($"Loaded {customWaypointCount} custom waypoints in {stopwatch.ElapsedMilliseconds}ms!");

//            // If enabled, dump the waypoint data
//            if (Settings.ExportMapPoints.Value)
//            {
//                // If we haven't written out the Waypoints for this map yet, write them out now
//                Directory.CreateDirectory(WaypointsPlugin.PointsFolder);
//                string exportFile = $"{WaypointsPlugin.PointsFolder}\\{mapName}.json";
//                if (!File.Exists(exportFile))
//                {
//                    ExportWaypoints(exportFile, botZones);
//                }
//            }
//        }

//        private static void InjectNavmesh(GameWorld gameWorld)
//        {
//            // First we load the asset from the bundle
//            string mapName = gameWorld.MainPlayer.Location.ToLower();

//            // Standardize Factory
//            if (mapName.StartsWith("factory4"))
//            {
//                mapName = "factory4";
//            }

//            string navMeshFilename = mapName + "-navmesh.bundle";
//            string navMeshPath = Path.Combine(new string[] { WaypointsPlugin.NavMeshFolder, navMeshFilename });
//            if (!File.Exists(navMeshPath))
//            {
//                return;
//            }

//            var bundle = AssetBundle.LoadFromFile(navMeshPath);
//            if (bundle == null)
//            {
//                Logger.LogError($"Error loading navMeshBundle: {navMeshPath}");
//                return;
//            }

//            var assets = bundle.LoadAllAssets(typeof(NavMeshData));
//            if (assets == null || assets.Length == 0)
//            {
//                Logger.LogError($"Bundle did not contain a NavMeshData asset: {navMeshPath}");
//                return;
//            }

//            // Then inject the new navMeshData, while blowing away the old data
//            var navMeshData = assets[0] as NavMeshData;
//            NavMesh.RemoveAllNavMeshData();
//            NavMesh.AddNavMeshData(navMeshData);

//            // Unload the bundle, leaving behind currently in use assets, so we can reload it next map
//            bundle.Unload(false);

//            Logger.LogDebug($"Injected custom navmesh: {navMeshPath}");

//            // For Streets, we want to inject a mesh into Chek15, so bots can get inside
//            if (mapName == "tarkovstreets")
//            {
//                Logger.LogDebug("Injecting custom box colliders to expand Streets bot access");

//                GameObject chek15LobbyAddonRamp = new GameObject("chek15LobbyAddonRamp");
//                chek15LobbyAddonRamp.layer = LayerMaskClass.LowPolyColliderLayer;
//                chek15LobbyAddonRamp.transform.position = new Vector3(126.88f, 2.96f, 229.91f);
//                chek15LobbyAddonRamp.transform.localScale = new Vector3(1.0f, 0.1f, 1.0f);
//                chek15LobbyAddonRamp.transform.Rotate(new Vector3(0f, 23.65f, 25.36f));
//                chek15LobbyAddonRamp.transform.SetParent(gameWorld.transform);
//                chek15LobbyAddonRamp.AddComponent<BoxCollider>();

//                GameObject chek15BackAddonRamp = new GameObject("Chek15BackAddonRamp");
//                chek15BackAddonRamp.layer = LayerMaskClass.LowPolyColliderLayer;
//                chek15BackAddonRamp.transform.position = new Vector3(108.31f, 3.32f, 222f);
//                chek15BackAddonRamp.transform.localScale = new Vector3(1.0f, 0.1f, 1.0f);
//                chek15BackAddonRamp.transform.Rotate(new Vector3(-40f, 0f, 0f));
//                chek15BackAddonRamp.transform.SetParent(gameWorld.transform);
//                chek15BackAddonRamp.AddComponent<BoxCollider>();
//            }
//        }

//        // Some maps need special treatment, to fix bad map data
//        private static void FixMap(GameWorld gameWorld)
//        {
//            string mapName = gameWorld.MainPlayer.Location.ToLower();

//            if (mapName.StartsWith("factory"))
//            {
//                var doorLinks = UnityEngine.Object.FindObjectsOfType<NavMeshDoorLink>();

//                // Gate 1 door, open carver is angled wrong, disable the secondary carver
//                var gate1DoorLink = doorLinks.Single(x => x.name == "DoorLink_7");
//                if (gate1DoorLink != null)
//                {
//                    gate1DoorLink.Carver_2.enabled = false;
//                }
//            }
//        }

//        public static void AddOrUpdatePatrol(BotZone botZone, CustomPatrol customPatrol)
//        {
//            // If the map already has this patrol, update its values
//            PatrolWay mapPatrol = botZone.PatrolWays.FirstOrDefault(p => p.name == customPatrol.name);
//            if (mapPatrol != null)
//            {
//                Console.WriteLine($"PatrolWay {customPatrol.name} exists, updating");
//                UpdatePatrol(mapPatrol, customPatrol);
//            }
//            // Otherwise, add a full new patrol
//            else
//            {
//                Console.WriteLine($"PatrolWay {customPatrol.name} doesn't exist, creating");
//                AddPatrol(botZone, customPatrol);
//            }
//        }

//        private static void UpdatePatrol(PatrolWay mapPatrol, CustomPatrol customPatrol)
//        {
//            //mapPatrol.BlockRoles = (WildSpawnType?)customPatrol.blockRoles ?? mapPatrol.BlockRoles;
//            mapPatrol.MaxPersons = customPatrol.maxPersons ?? mapPatrol.MaxPersons;
//            mapPatrol.PatrolType = customPatrol.patrolType ?? mapPatrol.PatrolType;

//            // Exclude any points that already exist in the map PatrolWay
//            var customWaypoints = customPatrol.waypoints.Where(
//                p => (mapPatrol.Points.Where(w => w.position == p.position).ToList().Count == 0)
//            ).ToList();

//            if (customWaypoints.Count > 0)
//            {
//                mapPatrol.Points.AddRange(processWaypointsToPatrolPoints(mapPatrol, customWaypoints));
//            }
//        }

//        private static List<PatrolPoint> processWaypointsToPatrolPoints(PatrolWay mapPatrol, List<CustomWaypoint> waypoints)
//        {
//            List<PatrolPoint> patrolPoints = new List<PatrolPoint>();
//            if (waypoints == null)
//            {
//                return patrolPoints;
//            }

//            foreach (CustomWaypoint waypoint in waypoints)
//            {
//                var newPatrolPointObject = new GameObject("CustomWaypoint_" + (customWaypointCount++));
//                //Logger.LogDebug($"Injecting custom PatrolPoint({newPatrolPointObject.name}) at {waypoint.position.x}, {waypoint.position.y}, {waypoint.position.z}");
//                newPatrolPointObject.AddComponent<PatrolPoint>();
//                var newPatrolPoint = newPatrolPointObject.GetComponent<PatrolPoint>();

//                newPatrolPoint.Id = (new System.Random()).Next();
//                newPatrolPoint.transform.position = new Vector3(waypoint.position.x, waypoint.position.y, waypoint.position.z);
//                newPatrolPoint.CanUseByBoss = waypoint.canUseByBoss;
//                newPatrolPoint.PatrolPointType = waypoint.patrolPointType;
//                newPatrolPoint.ShallSit = waypoint.shallSit;
//                newPatrolPoint.PointWithLookSides = null;
//                newPatrolPoint.SubManual = false;
//                if (mapPatrol != null && waypoint.waypoints == null)
//                {
//                    // CreateSubPoints has a very annoying debug log message, so disable debug logging to avoid it
//                    bool previousLogEnabled = UnityEngine.Debug.unityLogger.logEnabled;
//                    UnityEngine.Debug.unityLogger.logEnabled = false;

//                    newPatrolPoint.CreateSubPoints(mapPatrol);

//                    UnityEngine.Debug.unityLogger.logEnabled = previousLogEnabled;
//                }
//                else
//                {
//                    newPatrolPoint.subPoints = processWaypointsToPatrolPoints(null, waypoint.waypoints);
//                }
//                patrolPoints.Add(newPatrolPoint);
//            }

//            return patrolPoints;
//        }

//        private static void AddPatrol(BotZone botZone, CustomPatrol customPatrol)
//        {
//            //Logger.LogDebug($"Creating custom patrol {customPatrol.name} in {botZone.NameZone}");
//            // Validate some data
//            //if (customPatrol.blockRoles == null)
//            //{
//            //    Logger.LogError("Invalid custom Patrol, blockRoles is null");
//            //    return;
//            //}
//            if (customPatrol.maxPersons == null)
//            {
//                Logger.LogError("Invalid custom Patrol, maxPersons is null");
//                return;
//            }
//            if (customPatrol.patrolType == null)
//            {
//                Logger.LogError("Invalid custom Patrol, patrolTypes is null");
//                return;
//            }

//            // Create the Patrol game object
//            var mapPatrolObject = new GameObject(customPatrol.name);
//            mapPatrolObject.AddComponent<PatrolWayCustom>();
//            var mapPatrol = mapPatrolObject.GetComponent<PatrolWayCustom>();

//            // Add the waypoints to the Patrol object
//            UpdatePatrol(mapPatrol, customPatrol);

//            // Add the patrol to our botZone
//            botZone.PatrolWays = botZone.PatrolWays.Append(mapPatrol).ToArray();
//        }

//        static void ExportWaypoints(string exportFile, BotZone[] botZones)
//        {
//            ExportModel exportModel = new ExportModel();

//            foreach (BotZone botZone in botZones)
//            {
//                exportModel.zones.Add(botZone.name, new ExportZoneModel());

//                List<CustomPatrol> customPatrolWays = new List<CustomPatrol>();
//                foreach (PatrolWay patrolWay in botZone.PatrolWays)
//                {
//                    CustomPatrol customPatrolWay = new CustomPatrol();
//                    //customPatrolWay.blockRoles = patrolWay.BlockRoles.GetInt();
//                    customPatrolWay.maxPersons = patrolWay.MaxPersons;
//                    customPatrolWay.patrolType = patrolWay.PatrolType;
//                    customPatrolWay.name = patrolWay.name;
//                    customPatrolWay.waypoints = CreateCustomWaypoints(patrolWay.Points);

//                    customPatrolWays.Add(customPatrolWay);
//                }

//                exportModel.zones[botZone.name].patrols = customPatrolWays;

//                exportModel.zones[botZone.name].coverPoints = botZone.CoverPoints.Select(p => customNavPointToExportNavPoint(p)).ToList();
//                exportModel.zones[botZone.name].ambushPoints = botZone.AmbushPoints.Select(p => customNavPointToExportNavPoint(p)).ToList();
//            }

//            string jsonString = JsonConvert.SerializeObject(exportModel, Formatting.Indented);
//            if (File.Exists(exportFile))
//            {
//                File.Delete(exportFile);
//            }
//            File.Create(exportFile).Dispose();
//            StreamWriter streamWriter = new StreamWriter(exportFile);
//            streamWriter.Write(jsonString);
//            streamWriter.Flush();
//            streamWriter.Close();
//        }

//        static ExportNavigationPoint customNavPointToExportNavPoint(CustomNavigationPoint customNavPoint)
//        {
//            ExportNavigationPoint exportNavPoint = new ExportNavigationPoint();
//            exportNavPoint.AltPosition = customNavPoint.AltPosition;
//            exportNavPoint.HaveAltPosition = customNavPoint.HaveAltPosition;
//            exportNavPoint.BasePosition = customNavPoint.BasePosition;
//            exportNavPoint.ToWallVector = customNavPoint.ToWallVector;
//            exportNavPoint.FirePosition = customNavPoint.FirePosition;
//            exportNavPoint.TiltType = customNavPoint.TiltType.GetInt();
//            exportNavPoint.CoverLevel = customNavPoint.CoverLevel.GetInt();
//            exportNavPoint.AlwaysGood = customNavPoint.AlwaysGood;
//            exportNavPoint.BordersLightHave = customNavPoint.BordersLightHave;
//            exportNavPoint.LeftBorderLight = customNavPoint.LeftBorderLight;
//            exportNavPoint.RightBorderLight = customNavPoint.RightBorderLight;
//            exportNavPoint.CanLookLeft = customNavPoint.CanLookLeft;
//            exportNavPoint.CanLookRight = customNavPoint.CanLookRight;
//            exportNavPoint.HideLevel = customNavPoint.HideLevel;

//            return exportNavPoint;
//        }

//        static List<CustomWaypoint> CreateCustomWaypoints(List<PatrolPoint> patrolPoints)
//        {
//            List<CustomWaypoint> customWaypoints = new List<CustomWaypoint>();
//            if (patrolPoints == null)
//            {
//                //Logger.LogDebug("patrolPoints is null, skipping");
//                return customWaypoints;
//            }

//            foreach (PatrolPoint patrolPoint in patrolPoints)
//            {
//                CustomWaypoint customWaypoint = new CustomWaypoint();
//                customWaypoint.canUseByBoss = patrolPoint.CanUseByBoss;
//                customWaypoint.patrolPointType = patrolPoint.PatrolPointType;
//                customWaypoint.position = patrolPoint.Position;
//                customWaypoint.shallSit = patrolPoint.ShallSit;

//                customWaypoints.Add(customWaypoint);
//            }

//            return customWaypoints;
//        }
//    }

//    public class BotOwnerRunPatch : ModulePatch
//    {
//        protected override MethodBase GetTargetMethod()
//        {
//            return AccessTools.Method(typeof(BotOwner), "CalcGoal");
//        }

//        [PatchPostfix]
//        public static void PatchPostfix(BotOwner __instance)
//        {
//            // Wrap the whole thing in a try/catch, so we don't accidentally kill bot interactions
//            try
//            {
//                // If the bot doesn't have patrolling data, don't do anything
//                if (__instance.PatrollingData == null)
//                {
//                    return;
//                }

//                // If we're not patrolling, don't do anything
//                if (!__instance.Memory.IsPeace || __instance.PatrollingData.Status != PatrolStatus.go)
//                {
//                    //Logger.LogInfo($"({Time.time})BotOwner::RunPatch[{__instance.name}] - Bot not in peace, or not patrolling");
//                    return;
//                }

//                // If we're already running, check if our stamina is too low (< 30%), or we're close to our end point and stop running
//                if (__instance.Mover.Sprinting)
//                {
//                    if (__instance.GetPlayer.Physical.Stamina.NormalValue < 0.3f)
//                    {
//                        //Logger.LogInfo($"({Time.time})BotOwner::RunPatch[{__instance.name}] - Bot was sprinting but stamina hit {Math.Floor(__instance.GetPlayer.Physical.Stamina.NormalValue * 100)}%. Stopping sprint");
//                        __instance.Sprint(false);
//                    }

//                    // TODO: Get BotOwner.PatrollingData.PatrolPathControl.
//                }

//                // If we aren't running, and our stamina is near capacity (> 80%), allow us to run
//                if (!__instance.Mover.Sprinting && __instance.GetPlayer.Physical.Stamina.NormalValue > 0.8f)
//                {
//                    //Logger.LogInfo($"({Time.time})BotOwner::RunPatch[{__instance.name}] - Bot wasn't sprinting but stamina hit {Math.Floor(__instance.GetPlayer.Physical.Stamina.NormalValue * 100)}%. Giving bot chance to run");
//                    if (Random.Range(0, 1000) < __instance.Settings.FileSettings.Patrol.SPRINT_BETWEEN_CACHED_POINTS)
//                    {
//                        //Logger.LogInfo($"({Time.time})BotOwner::RunPatch[{__instance.name}] - Bot decided to run");
//                        __instance.Sprint(true);
//                    }
//                }
//            }
//            catch (Exception e)
//            {
//                Logger.LogDebug($"Something broke in the patch. We caught it so everything should be fine: {e.ToString()}");
//            }
//        }
//    }
//}
