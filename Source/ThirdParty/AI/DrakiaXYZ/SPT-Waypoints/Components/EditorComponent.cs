//using Comfort.Common;
//using DrakiaXYZ.Waypoints.Helpers;
//using DrakiaXYZ.Waypoints.Patches;
//using EFT;
//using Newtonsoft.Json;
//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using UnityEngine;
//using UnityEngine.AI;

//namespace DrakiaXYZ.Waypoints.Components
//{
//    public class EditorComponent : MonoBehaviour, IDisposable
//    {
//        private static List<UnityEngine.Object> gameObjects = new List<UnityEngine.Object>();
//        private GameWorld gameWorld;
//        private Player player;
//        private IBotGame botGame;
//        private List<BotZone> botZones = new List<BotZone>();

//        private GUIContent guiContent;
//        private GUIStyle guiStyle;

//        // Used to only update the nearest zone periodically to avoid performance issues
//        private float lastLocationUpdate;
//        private float locationUpdateFrequency = 0.5f;

//        private int currentZoneIndex = -1;
//        private BotZone nearestBotZone = null;

//        private float distanceToZone;
//        private NavMeshHit navMeshHit;

//        // Dictionary is [zone][patrol]
//        private Dictionary<string, Dictionary<string, CustomPatrol>> zoneWaypoints = new Dictionary<string, Dictionary<string, CustomPatrol>>();
//        private string filename;


//        private EditorComponent()
//        {
//            // Empty
//        }

//        public void Dispose()
//        {
//            gameObjects.ForEach(Destroy);
//            gameObjects.Clear();
//        }

//        public void Awake()
//        {
//            Console.WriteLine("Editor::OnEnable");

//            // Setup access to game objects
//            gameWorld = Singleton<GameWorld>.Instance;
//            botGame = Singleton<IBotGame>.Instance;
//            player = gameWorld.MainPlayer;

//            // Cache list of botZones
//            botZones = LocationScene.GetAll<BotZone>().ToList();
//            UpdateLocation();

//            // Generate the filename to use for output
//            string datetime = DateTime.Now.ToString("MM-dd-yyyy.HH-mm");
//            filename = $"{WaypointsPlugin.CustomFolder }\\{gameWorld.MainPlayer.Location.ToLower()}_{datetime}.json";
//        }

//        public void OnGUI()
//        {
//            if (Time.time > (lastLocationUpdate + locationUpdateFrequency))
//            {
//                UpdateLocation();
//                lastLocationUpdate = Time.time;
//            }

//            // Setup GUI objects if they're not setup yet
//            if (guiStyle == null)
//            {
//                guiStyle = new GUIStyle(GUI.skin.box);
//                guiStyle.alignment = TextAnchor.MiddleRight;
//                guiStyle.fontSize = 36;
//                guiStyle.margin = new RectOffset(3, 3, 3, 3);
//            }

//            if (guiContent == null)
//            {
//                guiContent = new GUIContent();
//            }

//            string zoneName = getCurrentZoneName();
//            string patrolName = getCurrentPatrolName();

//            // Build the data to show in the GUI
//            string guiText = "Waypoint Editor\n";
//            guiText += "-----------------------\n";

//            guiText += $"Current Zone: {zoneName}\n";
//            guiText += $"Current Patrol: {patrolName}\n";
//            //guiText += $"Distance To Nearest Waypoint: {distanceToZone}\n";
//            guiText += $"{(navMeshHit.hit ? "Inside" : "Outside")} of Navmesh\n";
//            guiText += $"Loc: {player.Position.x}, {player.Position.y}, {player.Position.z}\n";

//            // Draw the GUI
//            guiContent.text = guiText;
//            Vector2 guiSize = guiStyle.CalcSize(guiContent);
//            UnityEngine.Rect guiRect = new (
//                Screen.width - guiSize.x - 5f,
//                Screen.height - guiSize.y - 30f,
//                guiSize.x,
//                guiSize.y);

//            GUI.Box(guiRect, guiContent, guiStyle);
//        }

//        public void Update()
//        {
//            if (Settings.AddWaypointKey.Value.IsDown())
//            {
//                string zoneName = getCurrentZone().NameZone;
//                string patrolName = getCurrentPatrolName();

//                // Verify that our dictionary has our zone/patrol in it
//                if (!zoneWaypoints.ContainsKey(zoneName))
//                {
//                    zoneWaypoints.Add(zoneName, new Dictionary<string, CustomPatrol>());
//                }

//                if (!zoneWaypoints[zoneName].ContainsKey(patrolName))
//                {
//                    CustomPatrol patrolWay = new CustomPatrol();
//                    patrolWay.name = patrolName;
//                    patrolWay.patrolType = PatrolType.patrolling;
//                    patrolWay.maxPersons = 10;
//                    patrolWay.blockRoles = 0;
//                    patrolWay.waypoints = new List<CustomWaypoint>();
//                    zoneWaypoints[zoneName].Add(patrolName, patrolWay);
//                }

//                // Create and add a waypoint
//                CustomWaypoint waypoint = new CustomWaypoint();
//                waypoint.position = player.Position;
//                waypoint.canUseByBoss = true;
//                waypoint.patrolPointType = PatrolPointType.checkPoint;
//                waypoint.shallSit = false;
//                zoneWaypoints[zoneName][patrolName].waypoints.Add(waypoint);

//                // Add the waypoint to the map
//                WaypointPatch.AddOrUpdatePatrol(getCurrentZone(), zoneWaypoints[zoneName][patrolName]);
//                gameObjects.Add(GameObjectHelper.drawSphere(player.Position, 0.5f, new Color(1.0f, 0.41f, 0.09f)));

//                // Write output to file
//                Save();
//            }
            
//            if (Settings.RemoveWaypointKey.Value.IsDown())
//            {
//                if (DeleteNearestAddedWaypoint(player.Position))
//                {
//                    Save();
//                }
//            }

//            if (Settings.NextBotZoneKey.Value.IsDown())
//            {
//                currentZoneIndex++;

//                // Loop from the end of the zone list to -1
//                if (currentZoneIndex >= botZones.Count)
//                {
//                    currentZoneIndex = -1;
//                }
//            }

//            if (Settings.PrevBotZoneKey.Value.IsDown())
//            {
//                currentZoneIndex--;

//                // Loop from -1 to the end of the zone list
//                if (currentZoneIndex < -1)
//                {
//                    currentZoneIndex = botZones.Count - 1;
//                }
//            }
//        }

//        private bool DeleteNearestAddedWaypoint(Vector3 position)
//        {
//            string zoneName = getCurrentZone().NameZone;
//            string patrolName = getCurrentPatrolName();

//            // If there are no custom waypoints, just return false
//            if (!zoneWaypoints[zoneName].ContainsKey(patrolName) || zoneWaypoints[zoneName][patrolName].waypoints.Count == 0)
//            {
//                return false;
//            }

//            // Find the nearest waypoint we've created this session, and make sure we're near it
//            FindNearestAddedWaypointSphere(position, out GameObject sphere, out float dist);
//            if (dist > 10)
//            {
//                Console.WriteLine($"Nearest added waypoint is too far away {dist}");
//                return false;
//            }

//            // Remove the visible sphere
//            Vector3 waypointPosition = sphere.transform.position;
//            gameObjects.Remove(sphere);
//            Destroy(sphere);

//            // Remove the waypoint from our local data
//            CustomWaypoint waypoint = zoneWaypoints[zoneName][patrolName].waypoints.FirstOrDefault(w => w.position == waypointPosition);
//            if (waypoint != null)
//            {
//                zoneWaypoints[zoneName][patrolName].waypoints.Remove(waypoint);
//            }

//            // Remove the waypoint from the map data
//            PatrolWay patrolWay = getCurrentZone().PatrolWays.FirstOrDefault(p => p.name == patrolName);
//            if (patrolWay != null)
//            {
//                PatrolPoint patrolPoint = patrolWay.Points.FirstOrDefault(p => p.position == waypointPosition);
//                if (patrolPoint != null)
//                {
//                    patrolWay.Points.Remove(patrolPoint);
//                    Destroy(patrolPoint.gameObject);
//                }
//            }

//            return true;
//        }

//        private void FindNearestAddedWaypointSphere(Vector3 position, out GameObject sphere, out float dist)
//        {
//            sphere = null;
//            dist = float.MaxValue;

//            foreach (UnityEngine.Object obj in gameObjects)
//            {
//                if (!(obj is GameObject))
//                {
//                    continue;
//                }
//                GameObject gameObject = (GameObject)obj;
//                float sqrMagnitude = (gameObject.transform.position - position).sqrMagnitude;
//                if (sqrMagnitude < dist)
//                {
//                    dist = sqrMagnitude;
//                    sphere = gameObject;
//                }
//            }
//        }

//        private void UpdateLocation()
//        {
//            Vector3 currentPosition = player.Position;
//            nearestBotZone = botGame.BotsController.GetClosestZone(currentPosition, out distanceToZone);

//            NavMesh.SamplePosition(currentPosition, out navMeshHit, 1f, NavMesh.AllAreas);
//        }

//        private void Save()
//        {
//            // Dump the data to file
//            string jsonString = JsonConvert.SerializeObject(zoneWaypoints, Formatting.Indented);
//            File.Create(filename).Dispose();
//            StreamWriter streamWriter = new StreamWriter(filename);
//            streamWriter.Write(jsonString);
//            streamWriter.Flush();
//            streamWriter.Close();
//        }

//        public static void Enable()
//        {
//            if (Singleton<IBotGame>.Instantiated && Settings.EditorEnabled.Value)
//            {
//                var gameWorld = Singleton<GameWorld>.Instance;
//                gameObjects.Add(gameWorld.GetOrAddComponent<EditorComponent>());
//            }
//        }

//        public static void Disable()
//        {
//            if (Singleton<IBotGame>.Instantiated)
//            {
//                var gameWorld = Singleton<GameWorld>.Instance;
//                gameWorld.GetComponent<EditorComponent>()?.Dispose();
//            }
//        }

//        public BotZone getCurrentZone()
//        {
//            return (currentZoneIndex >= 0) ? botZones[currentZoneIndex] : nearestBotZone;
//        }

//        public string getCurrentZoneName()
//        {
//            BotZone currentZone = getCurrentZone();
//            if (currentZoneIndex < 0)
//            {
//                return $"Nearest ({currentZone.NameZone})";
//            }

//            return currentZone.NameZone;
//        }

//        public string getCurrentPatrolName()
//        {
//            return Settings.CustomPatrolName.Value;
//        }
//    }
//}
