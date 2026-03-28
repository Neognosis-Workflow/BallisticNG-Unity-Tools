#if UNITY_EDITOR
using BallisticUnityTools.Placeholders;
using UnityEngine;

namespace NgData.NameData.Actions.Zones
{
    [RegisterNameDataAction("turbo")]
    public class TurboTriggerZoneActionProvider : ZoneNameDataAction
    {
        public override void Execute(GameObject targetObject)
        {
            Collider collider = GetCollider(targetObject);
            if (!collider) return;

            ModTurboTrigger tt = targetObject.GetComponent<ModTurboTrigger>();
            if (!tt) tt = targetObject.AddComponent<ModTurboTrigger>();

            if (HasFlag("nosound")) tt.PlaySound = false;
            if (GetFloat("time", out float time)) tt.TimeMult = time;
        }

        public override void Clear(GameObject targetObject)
        {
            Collider collider = targetObject.GetComponent<Collider>();
            if (collider) Object.DestroyImmediate(collider);

            ModTurboTrigger tt = targetObject.GetComponent<ModTurboTrigger>();
            if (tt) Object.DestroyImmediate(tt);
        }
    }
}
#endif