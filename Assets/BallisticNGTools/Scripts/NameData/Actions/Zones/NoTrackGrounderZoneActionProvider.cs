using BallisticUnityTools.Placeholders;
using UnityEngine;

namespace NgData.NameData.Actions.Zones
{
    [RegisterNameDataAction("nogrounding")]
    public class NoTrackGrounderZoneActionProvider : ZoneNameDataAction
    {
        public override void Execute(GameObject targetObject)
        {
            Collider collider = GetCollider(targetObject);
            if (!collider) return;

            ModNoForcedGroundingZone ntgz = targetObject.GetComponent<ModNoForcedGroundingZone>();
            if (!ntgz) targetObject.AddComponent<ModNoForcedGroundingZone>();
        }

        public override void Clear(GameObject targetObject)
        {
            ModNoForcedGroundingZone ntgz = targetObject.GetComponent<ModNoForcedGroundingZone>();
            if (!ntgz) Object.DestroyImmediate(ntgz);
        }
    }
}