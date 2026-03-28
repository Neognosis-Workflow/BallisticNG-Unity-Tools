#if UNITY_EDITOR
using BallisticUnityTools.Placeholders;
using UnityEngine;

namespace NgData.NameData.Actions
{
    [RegisterNameDataAction("dustsurface")]
    public class DustSurfaceActionProvider : NameDataAction
    {
        public override void Execute(GameObject targetObject)
        {
            ModDustSurface surface = targetObject.GetComponent<ModDustSurface>();
            if (!surface) surface = targetObject.AddComponent<ModDustSurface>();

            if (GetString("set", out string set)) surface.ParticleSet = set;
        }

        public override void Clear(GameObject targetObject)
        {
            ModDustSurface surface = targetObject.GetComponent<ModDustSurface>();
            if (surface) Object.DestroyImmediate(surface);
        }
    }
}
#endif