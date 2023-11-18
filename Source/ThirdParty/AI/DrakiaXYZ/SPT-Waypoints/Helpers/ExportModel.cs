using System.Collections.Generic;
using UnityEngine;

namespace DrakiaXYZ.Waypoints.Helpers
{
    internal class ExportModel
    {
        public Dictionary<string, ExportZoneModel> zones = new Dictionary<string, ExportZoneModel>();
    }

    internal class ExportZoneModel
    {
        public List<CustomPatrol> patrols = new List<CustomPatrol>();
        public List<ExportNavigationPoint> coverPoints = new List<ExportNavigationPoint>();
        public List<ExportNavigationPoint> ambushPoints = new List<ExportNavigationPoint>();
    }

    internal class ExportNavigationPoint
    {
        public Vector3 AltPosition;
        public bool HaveAltPosition;
        public Vector3 BasePosition;
        public Vector3 ToWallVector;
        public Vector3 FirePosition;
        public int TiltType;
        public int CoverLevel;
        public bool AlwaysGood;
        public bool BordersLightHave;
        public Vector3 LeftBorderLight;
        public Vector3 RightBorderLight;
        public bool CanLookLeft;
        public bool CanLookRight;
        public int HideLevel;
    }
}
