using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using Exiled.API.Interfaces;
using Exiled.API.Features;
using MapEditorReborn.Events.Handlers;
using MapEditorReborn.Events.EventArgs;
using Interactables;
using Interactables.Interobjects;
using Interactables.Interobjects.DoorUtils;
using UnityEngine;
using Exiled.Events.EventArgs.Player;
using Exiled.Events.EventArgs.Scp173;

namespace Scp650Plugin
{
    public class Scp650Plugin : Plugin<Config>
    {
        public static Scp650Plugin Instance;

        public static List<Scp650ai> scp650s = new List<Scp650ai> { };

        private EventHandler eventHandler;

        public override void OnEnabled()
        {
            Instance = this;
            Config.LoadPoses();
            RegisterEvents();
        }

        public override void OnDisabled()
        {
            Instance = null;
            UnRegisterEvents();
        }

        void RegisterEvents()
        {
            eventHandler = new EventHandler();

            Schematic.SchematicSpawned += eventHandler.OnSpawn;
            Schematic.SchematicDestroyed += eventHandler.OnDespawn;
            Exiled.Events.Handlers.Server.RoundStarted += OnRoundStart;
            Exiled.Events.Handlers.Scp173.Blinking += eventHandler.OnBlinking;
            if (!Config.FollowTargetToPocketDimension)
            {
                Exiled.Events.Handlers.Player.EnteringPocketDimension += eventHandler.OnPocketed;
            }
        }

        void UnRegisterEvents()
        {
            Schematic.SchematicSpawned -= eventHandler.OnSpawn;
            Schematic.SchematicDestroyed -= eventHandler.OnDespawn;
            Exiled.Events.Handlers.Server.RoundStarted -= OnRoundStart;
            Exiled.Events.Handlers.Scp173.Blinking -= eventHandler.OnBlinking;
            if (!Config.FollowTargetToPocketDimension)
            {
                Exiled.Events.Handlers.Player.EnteringPocketDimension -= eventHandler.OnPocketed;
            }

            eventHandler = null;
        }

        void OnRoundStart()
        {
            for (int i = 0; i < Config.MaximumSpawnNumber; i++)
            {
                List<DoorVariant> possibleDoors = new List<DoorVariant> { };
                foreach (DoorVariant variant in DoorVariant.AllDoors)
                {
                    if (variant.Rooms == null || variant.Rooms.Length == 0) continue;
                    if (variant is BreakableDoor && Config.SpawnableZone.Contains(variant.Rooms[0].Zone) && variant.Rooms.Length > 1)
                    {
                        possibleDoors.Add(variant);
                    }
                }
                if (possibleDoors.Count == 0)
                {
                    continue;
                }
                Transform transform = possibleDoors.RandomItem().transform;
                Vector3 pos = transform.position + 0.75f * (UnityEngine.Random.Range(0, 1) * 2f - 1f) * transform.forward;
                if (Config.LogsItslocation)
                {
                    ServerConsole.AddLog(Scp650Plugin.Instance.Config.SchematicName + " Spawned in: " + MapGeneration.RoomIdUtils.RoomAtPosition(pos).name);
                }
                MapEditorReborn.API.Features.ObjectSpawner.SpawnSchematic(Scp650Plugin.Instance.Config.SchematicName, pos, Quaternion.Euler(new Vector3(0f, UnityEngine.Random.Range(0f, 360f), 0f)), isStatic: false);
            }
        }
    }

    public class EventHandler
    {
        public void OnSpawn(SchematicSpawnedEventArgs ev)
        {
            if (ev.Name.Equals(Scp650Plugin.Instance.Config.SchematicName, StringComparison.InvariantCultureIgnoreCase))
            {
                ev.Schematic.IsStatic = false;
                Scp650Plugin.scp650s.Add(ev.Schematic.gameObject.AddComponent<Scp650ai>());
            }
        }

        public void OnDespawn(SchematicDestroyedEventArgs ev)
        {
            if (ev.Name.Equals(Scp650Plugin.Instance.Config.SchematicName, StringComparison.InvariantCultureIgnoreCase) && ev.Schematic.gameObject.TryGetComponent<Scp650ai>(out Scp650ai ai))
            {
                Scp650Plugin.scp650s.Remove(ai);
            }
        }

        public void OnPlayerDead(DiedEventArgs ev)
        {
            foreach (Scp650ai ai in Scp650Plugin.scp650s)
            {
                if (ai.Target == ev.Player.ReferenceHub)
                {
                    ai.TargetKilled(ev.Attacker.ReferenceHub);
                }
            }
        }

        public void OnBlinking(BlinkingEventArgs ev)
        {
            foreach (Scp650ai ai in Scp650Plugin.scp650s)
            {
                if (ev.Targets.Contains(Player.Get(ai.Target)))
                {
                    ai.TryTeleport(ev.Targets);
                }
            }
        }

        public void OnPocketed(EnteringPocketDimensionEventArgs ev)
        {
            foreach (Scp650ai ai in Scp650Plugin.scp650s)
            {
                if (ai.Target == ev.Player.ReferenceHub)
                {
                    ai.Target = null;
                }
            }
        }
    }
}
