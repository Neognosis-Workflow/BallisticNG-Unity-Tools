using BallisticUnityTools.Placeholders;
using UnityEngine;

namespace NgData.NameData.Actions.Zones
{
    [RegisterNameDataAction("dustzone")]
    public class DustZoneActionProvider : ZoneNameDataAction
    {
        public override void Execute(GameObject targetObject)
        {
            Collider collider = GetCollider(targetObject);
            if (!collider) return;

            ModDustZone zone = targetObject.GetComponent<ModDustZone>();
            if (!zone) zone = targetObject.AddComponent<ModDustZone>();

            if (GetString("set", out string set)) zone.ParticleSet = set;
        }

        public override void Clear(GameObject targetObject)
        {
            Collider collider = targetObject.GetComponent<Collider>();
            if (collider) Object.DestroyImmediate(collider);

            ModDustZone zone = targetObject.GetComponent<ModDustZone>();
            if (zone) Object.DestroyImmediate(zone);
        }
    }
}