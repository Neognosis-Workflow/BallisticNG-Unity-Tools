using BallisticUnityTools.Placeholders;
using UnityEngine;

namespace NgData.NameData.Actions
{
    [RegisterNameDataAction("eqviz")]
    public class VirtualEqActionProvider : NameDataAction
    {
        public override void Execute(GameObject targetObject)
        {
            ModSurvivalVis id = targetObject.GetComponent<ModSurvivalVis>();
            if (!id) id = targetObject.AddComponent<ModSurvivalVis>();

            if (HasFlag("sliced")) id.Type = ModSurvivalVis.VisType.BandsSliced;
            else id.Type = ModSurvivalVis.VisType.Bands;
        }

        public override void Clear(GameObject targetObject)
        {
            ModSurvivalVis id = targetObject.GetComponent<ModSurvivalVis>();
            if (id) Object.DestroyImmediate(id);
        }
    }
}