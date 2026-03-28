using BallisticUnityTools.Placeholders;
using UnityEngine;

namespace NgData.NameData.Actions.Zones
{
    [RegisterNameDataAction("noantiskip")]
    public class NoAntiSkipActionProvider : ZoneNameDataAction
    {
        public override void Execute(GameObject targetObject)
        {
            Collider collider = GetCollider(targetObject);
            if (!collider) return;

            NasVolume nas = targetObject.GetComponent<NasVolume>();
            if (!nas) nas = targetObject.AddComponent<NasVolume>();
        }

        public override void Clear(GameObject targetObject)
        {
            Collider collider = targetObject.GetComponent<Collider>();
            if (collider) Object.DestroyImmediate(collider);

            NasVolume nas = targetObject.GetComponent<NasVolume>();
            if (nas) Object.DestroyImmediate(nas);
        }
    }
}