using System.Collections.Generic;
using UnityEngine;

namespace DrakiaXYZ.Waypoints
{
    public class CustomPatrol
    {
        public string name;
        public List<CustomWaypoint> waypoints;

        public PatrolType? patrolType;
        public int? maxPersons;
        public int? blockRoles;
    }

    public class CustomWaypoint
    {
        public Vector3 position;
        public bool canUseByBoss;
        public PatrolPointType patrolPointType;
        public bool shallSit;
        public List<CustomWaypoint> waypoints;
    }
}
