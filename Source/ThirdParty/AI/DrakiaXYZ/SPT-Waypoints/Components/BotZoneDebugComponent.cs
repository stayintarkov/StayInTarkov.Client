//using Comfort.Common;
//using DrakiaXYZ.Waypoints.Helpers;
//using EFT;
//using EFT.Game.Spawning;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using UnityEngine;

//namespace DrakiaXYZ.Waypoints.Components
//{
//    internal class BotZoneDebugComponent : MonoBehaviour, IDisposable
//    {
//        private static List<UnityEngine.Object> gameObjects = new List<UnityEngine.Object>();

//        private List<SpawnPointMarker> spawnPoints = new List<SpawnPointMarker>();
//        private List<BotZone> botZones = new List<BotZone>();

//        public void Awake()
//        {
//            Console.WriteLine("BotZoneDebug::Awake");

//            // Cache spawn points so we don't constantly need to re-fetch them
//            CachePoints(true);

//            // Create static game objects
//            createSpawnPointObjects();
//            createBotZoneObjects();
//        }

//        public void Dispose()
//        {
//            Console.WriteLine("BotZoneDebugComponent::Dispose");
//            gameObjects.ForEach(Destroy);
//            gameObjects.Clear();
//            spawnPoints.Clear();
//            botZones.Clear();
//        }

//        private void createSpawnPointObjects()
//        {
//            // Draw spawn point markers
//            if (spawnPoints.Count > 0)
//            {
//                Console.WriteLine($"Found {spawnPoints.Count} SpawnPointMarkers");
//                foreach (SpawnPointMarker spawnPointMarker in spawnPoints)
//                {
//                    var spawnPoint = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
//                    spawnPoint.GetComponent<Renderer>().material.color = Color.blue;
//                    spawnPoint.GetComponent<Collider>().enabled = false;
//                    spawnPoint.transform.localScale = new Vector3(0.1f, 1.0f, 0.1f);
//                    spawnPoint.transform.position = new Vector3(spawnPointMarker.Position.x, spawnPointMarker.Position.y + 1.0f, spawnPointMarker.Position.z);

//                    gameObjects.Add(spawnPoint);
//                }
//            }
//        }

//        private void createBotZoneObjects()
//        {
//            foreach (BotZone botZone in botZones)
//            {
//                Console.WriteLine($"Drawing BotZone {botZone.NameZone}");
//                Console.WriteLine($"BushPoints (Green): {botZone.BushPoints.Length}");
//                Console.WriteLine($"CoverPoints (Blue): {botZone.CoverPoints.Length}");
//                Console.WriteLine($"AmbushPoints (Red): {botZone.AmbushPoints.Length}");
//                Console.WriteLine($"PatrolWays: {botZone.PatrolWays.Length}");
//                foreach (PatrolWay patrol in botZone.PatrolWays)
//                {
//                    Console.WriteLine($"    {patrol.name}");
//                }

//                // Bushpoints are green
//                foreach (CustomNavigationPoint bushPoint in botZone.BushPoints)
//                {
//                    gameObjects.Add(GameObjectHelper.drawSphere(bushPoint.Position, 0.5f, Color.green));
//                }

//                // Coverpoints are blue
//                var coverPoints = botZone.CoverPoints;
//                foreach (CustomNavigationPoint coverPoint in coverPoints)
//                {
//                    gameObjects.Add(GameObjectHelper.drawSphere(coverPoint.Position, 0.5f, Color.blue));
//                }

//                // Ambushpoints are red
//                var ambushPoints = botZone.AmbushPoints;
//                foreach (CustomNavigationPoint ambushPoint in ambushPoints)
//                {
//                    gameObjects.Add(GameObjectHelper.drawSphere(ambushPoint.Position, 0.5f, Color.red));
//                }

//                // Patrol points are yellow
//                var patrolWays = botZone.PatrolWays;
//                foreach (PatrolWay patrolWay in patrolWays)
//                {
//                    foreach (PatrolPoint patrolPoint in patrolWay.Points)
//                    {
//                        gameObjects.Add(GameObjectHelper.drawSphere(patrolPoint.Position, 0.5f, Color.yellow));

//                        //// Sub-points are purple
//                        //foreach (PatrolPoint subPoint in patrolPoint.subPoints)
//                        //{
//                        //    gameObjects.Add(GameObjectHelper.drawSphere(subPoint.Position, 0.25f, Color.magenta));
//                        //}
//                    }
//                }
//            }
//        }

//        private void CachePoints(bool forced)
//        {
//            if (forced || spawnPoints.Count == 0)
//            {
//                spawnPoints = FindObjectsOfType<SpawnPointMarker>().ToList();
//            }

//            if (forced || botZones.Count == 0)
//            {
//                botZones = LocationScene.GetAll<BotZone>().ToList();
//            }
//        }

//        public static void Enable()
//        {
//            if (Singleton<IBotGame>.Instantiated && Settings.DebugEnabled.Value)
//            {
//                var gameWorld = Singleton<GameWorld>.Instance;
//                gameObjects.Add(gameWorld.GetOrAddComponent<BotZoneDebugComponent>());
//            }
//        }

//        public static void Disable()
//        {
//            if (Singleton<IBotGame>.Instantiated)
//            {
//                var gameWorld = Singleton<GameWorld>.Instance;
//                gameWorld.GetComponent<BotZoneDebugComponent>()?.Dispose();
//            }
//        }
//    }
//}
