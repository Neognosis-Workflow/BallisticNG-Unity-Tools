using System;
using BallisticUnityTools.Placeholders;
using UnityEngine;
using Object = UnityEngine.Object;

namespace NgData.NameData.Actions
{
    [RegisterNameDataAction("vcolor")]
    public class SurvivalColorizeActionProvider : NameDataAction
    {
        public override void Execute(GameObject targetObject)
        {
            ModSurvivalColorizer c = targetObject.GetComponent<ModSurvivalColorizer>();
            if (!c) c = targetObject.AddComponent<ModSurvivalColorizer>();

            ModSurvivalColorizer.ModZoneColorTarget target = new ModSurvivalColorizer.ModZoneColorTarget();
            if (GetInt("index", out int index)) target.MaterialIndex = index;
            if (GetString("color", out string color)) 
                if (Enum.TryParse(color, true, out EZoneColorTarget colorTarget)) target.ColorTarget = colorTarget;

            if (GetString("props", out string props)) target.ColorProperties = props.CommaSeparate(true, true);
            
            c.Targets.Add(target);
        }

        public override void Clear(GameObject targetObject)
        {
            ModSurvivalColorizer c = targetObject.GetComponent<ModSurvivalColorizer>();
            if (c) Object.DestroyImmediate(c);
        }
    }
}