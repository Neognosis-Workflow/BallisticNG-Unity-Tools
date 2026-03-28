#if UNITY_EDITOR
using BallisticUnityTools.Placeholders;
using UnityEngine;

namespace NgData.NameData.Actions.Zones
{
    [RegisterNameDataAction("damage")]
    public class DamageZoneActionProvider : ZoneNameDataAction
    {
        public override void Execute(GameObject targetObject)
        {
            Collider collider = GetCollider(targetObject);
            if (!collider) return;

            ModDamageZone dz = targetObject.GetComponent<ModDamageZone>();
            if (!dz) dz = targetObject.AddComponent<ModDamageZone>();

            Configure(0, dz);
            Configure(1, dz);
        }

        private void Configure(int dataVal, ModDamageZone zone)
        {
            ModDamageZone.DamageZoneDamageData damageData = dataVal == 0 ? zone.OnEnterDamage : zone.OnStayDamage;
            
            string varName = dataVal == 0 ? "enter" : "stay";

            damageData.Enabled = false;
            if (GetBool($"{varName}instant", out bool instant))
            {
                damageData.Enabled = true;
                damageData.IsInstantKill = true;
            }
            
            if (GetFloat($"{varName}amount", out float amount))
            {
                damageData.Enabled = true;
                damageData.DamageAmount = amount;
            }

            if (GetFloat($"{varName}interval", out float interval))
            {
                damageData.Enabled = true;
                damageData.DamageInterval = interval;
            }
            
            if (dataVal == 0) zone.OnEnterDamage = damageData;
            else zone.OnStayDamage = damageData;
            
        }

        public override void Clear(GameObject targetObject)
        { 
            ModDamageZone dz = targetObject.GetComponent<ModDamageZone>(); 
            if (dz) Object.DestroyImmediate(dz);
            
            Collider collider = targetObject.GetComponent<Collider>(); 
            if (collider) Object.DestroyImmediate(collider);
        }
    }
}
#endif