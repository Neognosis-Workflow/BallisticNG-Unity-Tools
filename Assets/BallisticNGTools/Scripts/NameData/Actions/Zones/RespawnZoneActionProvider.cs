using BallisticUnityTools.Placeholders;
using UnityEngine;

namespace NgData.NameData.Actions.Zones
{
    [RegisterNameDataAction("respawn")]
    public class RespawnZoneActionProvider : ZoneNameDataAction
    {
        public override void Execute(GameObject targetObject)
        {
            Collider collider = GetCollider(targetObject, 1);
            if (!collider) return;

            ModRespawnZone rz = targetObject.GetComponent<ModRespawnZone>();
            if (!rz) targetObject.AddComponent<ModRespawnZone>();
        }

        public override void Clear(GameObject targetObject)
        {
            Collider collider = targetObject.GetComponent<Collider>();
            if (collider) Object.DestroyImmediate(collider);

            ModRespawnZone rz = targetObject.GetComponent<ModRespawnZone>();
            if (rz) Object.DestroyImmediate(rz);
        }
    }
}