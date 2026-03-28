#if UNITY_EDITOR
using UnityEngine;

namespace NgData.NameData.Actions
{
    [RegisterNameDataAction("hidden")]
    public class HiddenActionProvider : NameDataAction
    {
        public override void Execute(GameObject targetObject)
        {
            MeshRenderer mr = targetObject.GetComponent<MeshRenderer>();
            if (mr) mr.enabled = false;
        }
    }
}
#endif