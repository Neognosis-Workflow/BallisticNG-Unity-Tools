#if UNITY_EDITOR
using BallisticUnityTools.Placeholders;
using UnityEngine;

namespace NgData.NameData.Actions
{
    [RegisterNameDataAction("col")]
    public class SceneryColliderProvider : NameDataAction
    {
        public override void Execute(GameObject targetObject)
        {
            BallisticMeshCollider collider = targetObject.GetComponent<BallisticMeshCollider>();
            if (!collider) collider = targetObject.AddComponent<BallisticMeshCollider>();

            collider.Type = HasFlag("wall") 
                ? BallisticMeshCollider.CollisionType.SceneryWall :
                BallisticMeshCollider.CollisionType.SceneryFloor;
        }
        public override void Clear(GameObject targetObject)
        {
            BallisticMeshCollider collider = targetObject.GetComponent<BallisticMeshCollider>();
            if (collider) Object.DestroyImmediate(collider);
        }
    }
}
#endif