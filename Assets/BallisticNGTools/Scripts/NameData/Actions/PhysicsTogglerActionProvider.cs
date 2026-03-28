#if UNITY_EDITOR
using BallisticUnityTools.Placeholders;
using UnityEngine;

namespace NgData.NameData.Actions
{
    [RegisterNameDataAction("physics")]
    public class PhysicsTogglerActionProvider : NameDataAction
    {
        public override void Execute(GameObject targetObject)
        {
            PhysicsToggler pt = targetObject.GetComponent<PhysicsToggler>();
            if (!pt) pt = targetObject.AddComponent<PhysicsToggler>();

            pt.EnableIn2159 = HasFlag("2159");
            pt.EnableIn2280 = HasFlag("2280");
            pt.EnabledInFloorHugger = HasFlag("floorhugger");
        }

        public override void Clear(GameObject targetObject)
        {
            PhysicsToggler pt = targetObject.GetComponent<PhysicsToggler>();
            if (pt) Object.DestroyImmediate(pt);
        }
    }
}
#endif