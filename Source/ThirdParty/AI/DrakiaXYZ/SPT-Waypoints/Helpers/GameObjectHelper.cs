using UnityEngine;

namespace DrakiaXYZ.Waypoints.Helpers
{
    public class GameObjectHelper
    {
        public static GameObject drawSphere(Vector3 position, float size, Color color)
        {
            var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.GetComponent<Renderer>().material.color = color;
            sphere.GetComponent<Collider>().enabled = false;
            sphere.transform.position = new Vector3(position.x, position.y, position.z); ;
            sphere.transform.localScale = new Vector3(size, size, size);

            return sphere;
        }
    }
}
