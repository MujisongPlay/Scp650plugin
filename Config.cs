using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Exiled.API.Interfaces;
using UnityEngine;
using System.IO;
using Exiled.API.Features;
using Exiled.Loader;
using MapGeneration;
using PlayerRoles;
using System.ComponentModel;

namespace Scp650Plugin
{
    public sealed class Config : IConfig
    {
        //Setting
        public bool IsEnabled { get; set; } = true;
        public bool Debug { get; set; } = false;
        public string PoseFolderPath { get; set; } = Path.Combine(Paths.Configs, "SCP-650 poses");
        public string PoseFile { get; set; } = "global.yml";

        //Spawning
        public int MaximumSpawnNumber { get; set; } = 1;
        public FacilityZone[] SpawnableZone { get; set; } = new FacilityZone[]
        {
            FacilityZone.LightContainment,
            FacilityZone.HeavyContainment,
            FacilityZone.Entrance
        };
        public bool LogsItslocation { get; set; } = true;

        //Targeting
        public Faction[] ObserveEffectableFactions { get; set; } = new Faction[]
        {
            Faction.FoundationEnemy,
            Faction.FoundationStaff
        };
        public RoleTypeId[] ObserveEffectableBlacklistRoles { get; set; } = new RoleTypeId[] { };
        public Faction[] TargetableFactions { get; set; } = new Faction[]
        {
            Faction.FoundationEnemy,
            Faction.FoundationStaff
        };
        public RoleTypeId[] TargetBlacklistRoles { get; set; } = new RoleTypeId[] { };
        public bool OverlapTarget { get; set; } = false;


        //Target
        public float TargetFollowingMinTime { get; set; } = 50f;
        public float TargetFollowingMaxTime { get; set; } = 120f;
        [Description("If following time passes, select whether SCP-650 will follow original target until next targetable player appears. Or it will just freeze at that point and wait.")]
        public bool SmoothChangeTarget { get; set; } = true;
        public bool ChangeTargetToKillerWhenTargetKilled { get; set; } = true;
        public float TeleportMinCoolTime { get; set; } = 5f;
        public float TeleportMaxCoolTime { get; set; } = 10f;
        public bool FollowTargetToPocketDimension { get; set; } = true;

        public void LoadPoses()
        {
            if (!Directory.Exists(PoseFolderPath))
            {
                Directory.CreateDirectory(PoseFolderPath);
            }
            string filePath = Path.Combine(PoseFolderPath, PoseFile);
            Log.Info($"{filePath}");
            if (!File.Exists(filePath))
            {
                poses = new Poses();
                File.WriteAllText(filePath, Loader.Serializer.Serialize(poses));
            }
            else
            {
                poses = Loader.Deserializer.Deserialize<Poses>(File.ReadAllText(filePath));
                File.WriteAllText(filePath, Loader.Serializer.Serialize(poses));
            }
        }
        public Poses poses;
    }

    public class Poses
    {
        public List<Poseture> posetures { get; set; } = new List<Poseture>
        {
            new Poseture()
        };
    }

    public class Poseture
    {
        public string PoseName { get; set; } = "Standing";

        public Dictionary<string, Vector3[]> TransfromPerJoint { get; set; }
    }
}
