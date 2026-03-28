#if UNITY_EDITOR
using BallisticUnityTools.Placeholders;
using UnityEngine;

namespace NgData.NameData.Actions
{
    [RegisterNameDataAction("flare")]
    public class FlareActionProvider : NameDataAction
    {
        public override void Execute(GameObject targetObject)
        {
            if (HasFlag("pass"))
            {
                ModFlarePassthrough pt = targetObject.GetComponent<ModFlarePassthrough>();
                if (!pt) targetObject.AddComponent<ModFlarePassthrough>();
            }
            else if (HasFlag("block"))
            {
                BallisticFlareOccluder fo = targetObject.GetComponent<BallisticFlareOccluder>();
                if (!fo) targetObject.AddComponent<BallisticFlareOccluder>();
            }
        }

        public override void Clear(GameObject targetObject)
        {
            ModFlarePassthrough pt = targetObject.GetComponent<ModFlarePassthrough>();
            if (pt) Object.DestroyImmediate(pt);

            BallisticFlareOccluder oc = targetObject.GetComponent<BallisticFlareOccluder>();
            if (oc) Object.DestroyImmediate(oc);
        }
    }
}
#endif