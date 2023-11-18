using BepInEx.Logging;
using Comfort.Common;
using DrakiaXYZ.Waypoints.Helpers;
using EFT;
using EFT.Interactive;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace DrakiaXYZ.Waypoints.Components
{
    internal class DoorBlockAdderComponent : MonoBehaviour
    {
        List<DoorContainer> doorList = new List<DoorContainer>();
        float nextUpdate = 0f;
        protected ManualLogSource Logger = null;

        public void Awake()
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(GetType().Name);

            var gameWorld = Singleton<GameWorld>.Instance;
            string mapName = gameWorld.MainPlayer.Location.ToLower();

            FindObjectsOfType<Collider>().ExecuteForEach(collider =>
            {
                // We don't support doors that aren't on the "Door" layer
                if (collider.gameObject.layer != LayerMaskClass.DoorLayer)
                {
                    return;
                }

                // We don't support doors that don't have an "Interactive" parent
                GameObject doorObject = collider.transform.parent.gameObject;
                WorldInteractiveObject door = doorObject.GetComponent<WorldInteractiveObject>();

                // If we don't have a door object, and the layer isn't interactive, skip
                // Note: We have to do a door null check here because Factory has some non-interactive doors that bots can use...
                if (door == null)
                {
                    // Note: Labs is a special case where fake doors don't always have an interactive parent...
                    if (mapName.StartsWith("laboratory"))
                    {
                        if (doorObject.layer != 0 && doorObject.layer != LayerMaskClass.InteractiveLayer)
                        {
                            //Logger.LogDebug($"Skipping labs door ({doorObject.name}) due to layer {doorObject.layer} != 0 && != {LayerMaskClass.InteractiveLayer.value}");
                            return;
                        }
                    }
                    else if (doorObject.layer != LayerMaskClass.InteractiveLayer)
                    {
                        //Logger.LogDebug($"Skipping door ({doorObject.name}) due to layer {doorObject.layer} != {LayerMaskClass.InteractiveLayer.value}");
                        return;
                    }
                }

                // If the door is an interactive object, and it's open or shut, we don't need to worry about it
                if (door != null && door.enabled && (door.DoorState == EDoorState.Open || door.DoorState == EDoorState.Shut))
                {
                    drawDebugSphere(collider.bounds.center, 0.5f, Color.blue);
                    //Logger.LogDebug($"Found an open/closed door, skipping");
                    return;
                }

                // Make sure the door is tall, otherwise it's probably not a real door
                if (collider.bounds.size.y < 1.5f)
                {
                    drawDebugSphere(collider.bounds.center, 0.5f, Color.yellow);
                    //Logger.LogDebug($"Skipping door ({meshCollider.name}) that's not tall enough ({meshCollider.bounds.center}) ({meshCollider.bounds.size})");
                    return;
                }

                GameObject obstacleObject = new GameObject("ObstacleObject");
                NavMeshObstacle navMeshObstacle = obstacleObject.AddComponent<NavMeshObstacle>();

                // We use a small cube, to avoid cutting into the hallway mesh
                navMeshObstacle.size = collider.bounds.size;
                navMeshObstacle.carving = true;
                navMeshObstacle.carveOnlyStationary = false;

                // Position the new gameObject
                obstacleObject.transform.SetParent(collider.transform);
                obstacleObject.transform.position = collider.bounds.center;

                // If the door was locked, we want to keep track of it to remove the blocker when it's unlocked
                if (door != null && door.DoorState == EDoorState.Locked)
                {
                    DoorContainer doorContainer = new DoorContainer();
                    doorContainer.door = door;
                    doorContainer.collider = collider;
                    doorContainer.navMeshObstacle = navMeshObstacle;
                    doorContainer.sphere = drawDebugSphere(obstacleObject.transform.position, 0.5f, Color.red);
                    doorList.Add(doorContainer);
                }
                else
                {
                    drawDebugSphere(obstacleObject.transform.position, 0.5f, Color.magenta);
                }
            });
        }

        public void Update()
        {
            if (Time.time > nextUpdate)
            {
                for (int i = doorList.Count - 1; i >= 0; i--)
                {
                    DoorContainer doorContainer = doorList[i];

                    // If the door has been unlocked, delete the blocker
                    if (doorContainer.door.DoorState != EDoorState.Locked && doorContainer.door.DoorState != EDoorState.Interacting)
                    {
                        //Logger.LogDebug($"DoorState is now {doorContainer.door.DoorState}, removing blocker");
                        if (doorContainer.sphere != null)
                        {
                            Destroy(doorContainer.sphere);
                        }

                        Destroy(doorContainer.navMeshObstacle);
                        doorList.RemoveAt(i);
                    }
                }

                nextUpdate = Time.time + 0.5f;
            }
        }

        private GameObject drawDebugSphere(Vector3 position, float size, Color color)
        {
            if (Settings.DebugEnabled.Value)
            {
                return GameObjectHelper.drawSphere(position, size, color);
            }
            else
            {
                return null;
            }
        }

        public static void Enable()
        {
            if (Singleton<IBotGame>.Instantiated)
            {
                var gameWorld = Singleton<GameWorld>.Instance;
                gameWorld.GetOrAddComponent<DoorBlockAdderComponent>();
            }
        }
    }

    internal struct DoorContainer
    {
        public WorldInteractiveObject door;
        public Collider collider;
        public NavMeshObstacle navMeshObstacle;
        public GameObject sphere;
    }
}
