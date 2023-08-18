using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using PlayerRoles.FirstPersonControl;
using PlayerRoles;
using PlayerRoles.PlayableScps;
using InventorySystem.Items;
using InventorySystem.Items.Usables.Scp244;
using Exiled.API.Features;

namespace Scp650Plugin
{
    public class Scp650ai : MonoBehaviour
    {
        void Start()
        {
            ChildRegister(gameObject);
            ChangePose();
        }

        void ChildRegister(GameObject @object)
        {
            for (int i = 0; i < @object.transform.childCount; i++)
            {
                GameObject game = @object.transform.GetChild(i).gameObject;
                if (game.TryGetComponent<MapEditorReborn.API.Features.Objects.PrimitiveObject>(out MapEditorReborn.API.Features.Objects.PrimitiveObject @object1))
                {
                    @object1.Primitive.MovementSmoothing = 0;
                }
                if (game.name.Contains("mixamorig"))
                {
                    if (object1 != null)
                    {
                        JointHelper.Add(game.name, game);
                    }
                    else
                    {
                        Joints.Add(game.name, game);
                        ChildRegister(game);
                    }
                }
            }
        }

        public void ChangePose()
        {
            List<Poseture> posetures = new List<Poseture> { };
            posetures.AddRange(Scp650Plugin.Instance.Config.poses.posetures);
            posetures.Remove(poseture);
            poseture = posetures.RandomItem();
            foreach (KeyValuePair<string, GameObject> pair in Joints)
            {
                if (poseture.TransfromPerJoint.TryGetValue(pair.Key.Replace("mixamorig:", ""), out Vector3[] vector3s))
                {
                    if (JointHelper.TryGetValue(pair.Key, out GameObject @object))
                    {
                        pair.Value.transform.localPosition = @object.transform.localPosition;
                    }
                    else
                    {
                        pair.Value.transform.localPosition = vector3s[0];
                    }
                    pair.Value.transform.localRotation = Quaternion.Euler(vector3s[1]);
                }
            }
        }

        void Update()
        {
            SeeingPlayers.Clear();
            foreach (ReferenceHub hub in ReferenceHub.AllHubs)
            {
                if (hub.isLocalPlayer)
                {
                    continue;
                }
                if (IsWatching(this.transform.GetChild(0).transform.position, hub))
                {
                    SeeingPlayers.Add(hub);
                }
            }
            if (SeeingPlayers.Count != 0)
            {
                if (LookingForTarget)
                {
                    List<ReferenceHub> targets = SeeingPlayers;
                    foreach (ReferenceHub hub in targets)
                    {
                        if (!IsTargetable(hub))
                        {
                            targets.Remove(hub);
                        }
                    }
                    if (targets.Count == 0)
                    {
                        return;
                    }
                    Target = targets.RandomItem();
                    if (config.TargetingAmbient != -1)
                    {
                        Exiled.API.Extensions.MirrorExtensions.SendFakeTargetRpc(Player.Get(Target), ReferenceHub.HostHub.networkIdentity, typeof(AmbientSoundPlayer), "RpcPlaySound", new object[]
                        {
                            config.TargetingAmbient
                        });
                    }
                    LookingForTarget = false;
                    FollowTime = UnityEngine.Random.Range(config.TargetFollowingMinTime, config.TargetFollowingMaxTime);
                    TeleportTime = UnityEngine.Random.Range(config.TeleportMinCoolTime, config.TeleportMaxCoolTime);
                    TargetFollowTimer = 0f;
                    TargetTeleportTimer = 0f;
                }
            }
            if (Target != null)
            {
                TargetFollowTimer += Time.deltaTime;
                if (TargetFollowTimer >= FollowTime)
                {
                    LookingForTarget = true;
                    if (!config.SmoothChangeTarget)
                    {
                        return;
                    }
                }
                TargetTeleportTimer += Time.deltaTime;
                if (TargetTeleportTimer >= TeleportTime && SeeingPlayers.Count == 0)
                {
                    TryTeleport();
                }
            }
        }

        public void TargetKilled(ReferenceHub killer)
        {
            Target = null;
            if (killer == null)
            {
                return;
            }
            if (!config.ChangeTargetToKillerWhenTargetKilled)
            {
                LookingForTarget = true;
            }
            if (IsTargetable(killer))
            {
                Target = killer;
                LookingForTarget = false;
                FollowTime = UnityEngine.Random.Range(config.TargetFollowingMinTime, config.TargetFollowingMaxTime);
                TeleportTime = UnityEngine.Random.Range(config.TeleportMinCoolTime, config.TeleportMaxCoolTime);
                TargetFollowTimer = 0f;
                TargetTeleportTimer = 0f;
            }
        }

        bool IsTargetable(ReferenceHub hub)
        {
            PlayerRoleBase roleBase = hub.roleManager.CurrentRole;
            if (!config.OverlapTarget)
            {
                foreach (Scp650ai ai in Scp650Plugin.scp650s)
                {
                    if (ai != this && ai.Target == hub)
                    {
                        return false;
                    }
                }
            }
            return hub != Target && config.TargetableFactions.Contains(roleBase.RoleTypeId.GetFaction()) && !config.TargetBlacklistRoles.Contains(roleBase.RoleTypeId);
        }

        bool IsWatching(Vector3 pos, ReferenceHub hub)
        {
            PlayerRoleBase roleBase = hub.roleManager.CurrentRole;
            if (roleBase.RoleTypeId == RoleTypeId.Spectator || !config.ObserveEffectableFactions.Contains(roleBase.RoleTypeId.GetFaction()) || config.ObserveEffectableBlacklistRoles.Contains(roleBase.RoleTypeId))
            {
                return false;
            }
            Vector3 vector = (pos - hub.PlayerCameraReference.position).normalized;
            Vector3 vector1 = hub.PlayerCameraReference.forward;
            if (Vector3.Dot(vector, vector1) >= 0.1f && GetVisionInformation(hub, hub.PlayerCameraReference, pos, 0.8f, 60f, true, true, 0).IsLooking)
            {
                return true;
            }
            if (Vector3.Distance(pos, hub.PlayerCameraReference.position) <= 1.2f)
            {
                return true;
            }
            return false;
        }

        public void TryTeleport(List<Player> blinker = null)
        {
            if (Physics.Raycast(Target.PlayerCameraReference.position, -Target.PlayerCameraReference.forward.NormalizeIgnoreY(), out RaycastHit _, 3f))
            {
                return;
            }
            if (!Physics.Raycast(Target.PlayerCameraReference.position - Target.PlayerCameraReference.forward.NormalizeIgnoreY() * 2f, Vector3.down, out RaycastHit hit, 3f))
            {
                return;
            }
            foreach (Scp650ai ai in Scp650Plugin.scp650s)
            {
                if (ai != this && Vector3.Distance(ai.transform.position, hit.point) <= 2f)
                {
                    return;
                }
            }
            foreach (ReferenceHub hub in ReferenceHub.AllHubs)
            {
                if (hub.isLocalPlayer || hub == Target)
                {
                    continue;
                }
                if (blinker != null)
                {
                    if (blinker.Contains(Player.Get(hub)))
                    {
                        continue;
                    }
                }
                if (IsWatching(hit.point + new Vector3(0f, 1.2f, 0f), hub))
                {
                    return;
                }
            }
            Vector3 pos = hit.point;
            this.transform.position = pos;
            Vector3 normalized = (Target.PlayerCameraReference.position - pos).normalized;
            float b = Vector3.Angle(normalized, Vector3.forward) * Mathf.Sign(Vector3.Dot(normalized, Vector3.right));
            this.transform.rotation = Quaternion.Euler(0f, b, 0f);
            ChangePose();
            TeleportTime = UnityEngine.Random.Range(config.TeleportMinCoolTime, config.TeleportMaxCoolTime);
            TargetTeleportTimer = 0f;
        }

        VisionInformation GetVisionInformation(ReferenceHub source, Transform sourceCam, Vector3 target, float targetRadius = 0f, float visionTriggerDistance = 0f, bool checkFog = true, bool checkLineOfSight = true, int maskLayer = 0)
        {
            bool isOnSameFloor = false;
            bool flag = false;
            if (Mathf.Abs(target.y - sourceCam.position.y) < 100f)
            {
                isOnSameFloor = true;
                flag = true;
            }
            bool flag2 = visionTriggerDistance == 0f;
            Vector3 vector = target - sourceCam.position;
            float magnitude = vector.magnitude;
            if (flag && visionTriggerDistance > 0f)
            {
                float num = checkFog ? ((target.y > 980f) ? visionTriggerDistance : (visionTriggerDistance / 2f)) : visionTriggerDistance;
                if (magnitude <= num)
                {
                    flag2 = true;
                }
                flag = flag2;
            }
            float lookingAmount = 1f;
            if (flag)
            {
                flag = false;
                if (magnitude < targetRadius)
                {
                    if (Vector3.Dot(source.transform.forward, (target - source.transform.position).normalized) > -2.1f)
                    {
                        flag = true;
                        lookingAmount = 1f;
                    }
                }
                else if (Scp244Utils.CheckVisibility(sourceCam.position, target))
                {
                    Vector3 vector2 = sourceCam.InverseTransformPoint(target);
                    if (targetRadius != 0f)
                    {
                        vector2.x = Mathf.MoveTowards(vector2.x, 0f, targetRadius);
                        vector2.y = Mathf.MoveTowards(vector2.y, 0f, targetRadius);
                    }
                    AspectRatioSync aspectRatioSync = source.aspectRatioSync;
                    float num2 = Vector2.Angle(Vector2.up, new Vector2(vector2.x, vector2.z));
                    if (num2 < aspectRatioSync.XScreenEdge)
                    {
                        float num3 = Vector2.Angle(Vector2.up, new Vector2(vector2.y, vector2.z));
                        if (num3 < AspectRatioSync.YScreenEdge)
                        {
                            lookingAmount = (num2 + num3) / aspectRatioSync.XplusY;
                            flag = true;
                        }
                    }
                }
            }
            bool flag3 = !checkLineOfSight;
            if (flag && checkLineOfSight)
            {
                if (maskLayer == 0)
                {
                    maskLayer = VisionInformation.VisionLayerMask;
                }
                flag3 = (Physics.RaycastNonAlloc(new Ray(sourceCam.position, vector.normalized), VisionInformation.RaycastResult, flag2 ? magnitude : vector.magnitude, maskLayer) == 0);
                if (VisionInformation.RaycastResult[0].collider.transform.root.gameObject.TryGetComponent<Scp650ai>(out _))
                {
                    flag3 = true;
                }
                flag = flag3;
            }
            bool flag4 = !CheckAttachments(source) && RoomLightController.IsInDarkenedRoom(target);
            flag &= !flag4;
            return new VisionInformation(source, target, flag, isOnSameFloor, lookingAmount, magnitude, flag2, flag3, flag4);
        }

        private bool CheckAttachments(ReferenceHub source)
        {
            ItemBase curInstance = source.inventory.CurInstance;
            ILightEmittingItem lightEmittingItem;
            return curInstance != null && (lightEmittingItem = (curInstance as ILightEmittingItem)) != null && lightEmittingItem.IsEmittingLight;
        }

        static LayerMask _mask = 0;

        public static LayerMask WallMask
        {
            get
            {
                if (Scp650ai._mask == 0)
                {
                    _mask = FpcStateProcessor.Mask;
                    _mask ^= 1 << LayerMask.NameToLayer("Glass");
                }
                return Scp650ai._mask;
            }
        }

        readonly Config config = Scp650Plugin.Instance.Config;

        public Poseture poseture;

        public ReferenceHub Target;

        bool LookingForTarget = true;

        float TargetFollowTimer;

        float FollowTime;

        float TargetTeleportTimer;

        float TeleportTime;
        readonly List<ReferenceHub> SeeingPlayers = new List<ReferenceHub> { };
        readonly Dictionary<string, GameObject> Joints = new Dictionary<string, GameObject> { };
        readonly Dictionary<string, GameObject> JointHelper = new Dictionary<string, GameObject> { };
    }
}
