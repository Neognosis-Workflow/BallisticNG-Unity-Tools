using BallisticUnityTools.Placeholders;
using UnityEngine;

namespace NgData.NameData.Actions.Zones
{
    [RegisterNameDataAction("physmod")]
    public class ShipPhysicsModZoneActionProvider : ZoneNameDataAction
    {
        public override void Execute(GameObject targetObject)
        {
            Collider collider = GetCollider(targetObject, 1);
            if (!collider) return;

            ModPhysicsModZone modZone = targetObject.GetComponent<ModPhysicsModZone>();
            if (!modZone) modZone = targetObject.AddComponent<ModPhysicsModZone>();

            if (HasFlag("zerog")) modZone.ZeroGravity = true;
            if (HasFlag("spring")) modZone.SpringToTrack = true;

            if (GetFloat("zgtracking", out float zgtracking)) modZone.ZeroGTrackingMultiplier = zgtracking;
            if (GetFloat("gravity", out float gravity)) modZone.GravityMultiplier = gravity;
            if (GetFloat("zgpitchrot", out float zgpitchrot)) modZone.ZeroGravityPitchRotationMultiplier = zgpitchrot;
            if (GetFloat("zgpitchforce", out float zgpitchforce)) modZone.ZeroGravityPitchForce = zgpitchforce;
            if (GetFloat("zgpitchspeed", out float zgpitchspeed)) modZone.ZeroGravityPitchSpeed = zgpitchspeed;

            if (GetFloat("sforce", out float springForce)) modZone.TrackSpringForce = springForce;
            if (GetFloat("smin", out float springmin)) modZone.TrackSpringMinThreshold = springmin;
            if (GetFloat("smax", out float springMax)) modZone.TrackSpringMaxThreshold = springMax;
            if (GetFloat("sdamp", out float springdamp)) modZone.TrackSpringDamping = springdamp;
            if (GetFloat("stracking", out float springtraacking)) modZone.TrackSpringTrackingMultiplier = springtraacking;
            if (GetFloat("sbyspeed", out float springbyspeed)) modZone.TrackSpringSpeedMultiplier = springbyspeed;

            if (GetFloat("tracking", out float tracking)) modZone.TrackTrackingMultiplier = tracking;
            if (GetFloat("grip", out float grip)) modZone.GripMultiplier = grip;
            if (GetFloat("air", out float resistence)) modZone.AirResistenceMultiplier = resistence;
        }

        public override void Clear(GameObject targetObject)
        {
            Collider collider = GetCollider(targetObject);
            if (collider) Object.DestroyImmediate(collider);

            ModPhysicsModZone modZone = targetObject.GetComponent<ModPhysicsModZone>();
            if (modZone) Object.DestroyImmediate(modZone);
        }
    }
}