using BallisticUnityTools.Placeholders;
using UnityEngine;

namespace NgData.NameData.Actions.Zones
{
    [RegisterNameDataAction("recharge")]
    public class RechargeZoneActionProvider : ZoneNameDataAction
    {
        public override void Execute(GameObject targetObject)
        {
            Collider collider = GetCollider(targetObject);
            if (!collider) return;

            ModRechargeZone rz = targetObject.GetComponent<ModRechargeZone>();
            if (!rz) rz = targetObject.gameObject.AddComponent<ModRechargeZone>();
        }

        public override void Clear(GameObject targetObject)
        {
            Collider collider = targetObject.GetComponent<Collider>();
            if (collider) Object.DestroyImmediate(collider);

            ModRechargeZone rz = targetObject.GetComponent<ModRechargeZone>();
            if (rz) Object.DestroyImmediate(rz);
        }
    }
}