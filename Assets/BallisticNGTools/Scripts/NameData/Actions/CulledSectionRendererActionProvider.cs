#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using BallisticUnityTools.TrackTools.SectionRendering;
using UnityEngine;

namespace NgData.NameData.Actions
{
    [RegisterNameDataAction("secrend")]
    public class CulledSectionRendererActionProvider : NameDataAction
    {
        public override void Execute(GameObject targetObject)
        {
            NgSectionCulledRenderer renderer = targetObject.GetComponent<NgSectionCulledRenderer>();
            if (!renderer) renderer = targetObject.AddComponent<NgSectionCulledRenderer>();

            if (Flags == null) return;
            List<string> renderGroups = Flags.ToList();
            if (renderGroups.Count > 0) renderer.RenderGroups = renderGroups.ToArray();
        }

        public override void Clear(GameObject targetObject)
        {
            NgSectionCulledRenderer renderer = targetObject.GetComponent<NgSectionCulledRenderer>();
            if (renderer) Object.DestroyImmediate(renderer);
        }
    }
}
#endif