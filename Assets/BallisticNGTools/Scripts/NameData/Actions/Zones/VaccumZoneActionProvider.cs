using BallisticUnityTools.Placeholders;
using UnityEngine;

namespace NgData.NameData.Actions.Zones
{
    [RegisterNameDataAction("vaccum")]
    public class VaccumZoneActionProvider : ZoneNameDataAction
    {
        public override void Execute(GameObject targetObject)
        {
            Collider collider = GetCollider(targetObject);
            if (!collider) return;

            ModVacuumZone vz = targetObject.GetComponent<ModVacuumZone>();
            if (!vz) vz = targetObject.AddComponent<ModVacuumZone>();

            if (HasFlag("invert")) vz.Inverted = true;
        }

        public override void Clear(GameObject targetObject)
        {
            Collider collider = targetObject.GetComponent<Collider>();
            if (collider) Object.DestroyImmediate(collider);

            ModVacuumZone zone = targetObject.GetComponent<ModVacuumZone>();
            if (zone) Object.DestroyImmediate(zone);
        }
    }
}