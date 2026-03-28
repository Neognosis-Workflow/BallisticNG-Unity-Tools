using System;
using BallisticUnityTools.Placeholders;
using UnityEngine;
using Object = UnityEngine.Object;

namespace NgData.NameData.Actions.Zones
{
    [RegisterNameDataAction("push")]
    public class PushZoneActionProvider : ZoneNameDataAction
    {
        public override void Execute(GameObject targetObject)
        {
            Collider collider = GetCollider(targetObject);
            if (!collider) return;

            ModPushZone pz = targetObject.GetComponent<ModPushZone>();
            if (!pz) pz = targetObject.AddComponent<ModPushZone>();

            if (HasFlag("noai")) pz.AffectsAi = false;
            if (HasFlag("noplayer")) pz.AffectsPlayers = false;
            if (HasFlag("velocity")) pz.DoVelociity = true;

            if (GetString("space", out string space))
            {
                if (Enum.TryParse(space, true, out ModPushZone.ModPushZoneSpace pushSpace)) 
                    pz.PushSpace = pushSpace;
            }

            if (GetFloat("dx", out float dx)) pz.Direction.x = dx;
            if (GetFloat("dy", out float dy)) pz.Direction.y = dy;
            if (GetFloat("dz", out float dz)) pz.Direction.z = dz;

            if (GetFloat("force", out float force)) pz.PushForce = force;
            if (GetString("forcemode", out string forceMode))
            {
                if (Enum.TryParse(forceMode, true, out ForceMode mode))
                    pz.PushForceMode = mode;
            }

            if (GetFloat("gain", out float gain))
            {
                pz.UseForceGain = true;
                pz.ForceGain = gain;
            }

            if (GetFloat("falloff", out float falloff))
            {
                pz.UseForceFalloff = true;
                pz.ForceFalloff = falloff;
            }

            if (GetBool("dovelocity", out bool doVelocity))
                pz.DoVelociity = true;
            
            Vector3 forceVelocity = Vector3.zero;
            if (GetFloat("vx", out float vx)) forceVelocity.x = vx;
            if (GetFloat("vy", out float vy)) forceVelocity.y = vy;
            if (GetFloat("vz", out float vz)) forceVelocity.z = vz;
            pz.VelocityMultiplier = forceVelocity;
        }

        public override void Clear(GameObject targetObject)
        {
            Collider collider = targetObject.GetComponent<Collider>();
            if (collider) Object.DestroyImmediate(collider);

            ModPushZone zone = targetObject.GetComponent<ModPushZone>();
            if (zone) Object.DestroyImmediate(zone);
        }
    }
}