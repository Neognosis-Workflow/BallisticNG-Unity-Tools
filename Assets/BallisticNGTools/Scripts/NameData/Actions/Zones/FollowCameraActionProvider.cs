#if UNITY_EDITOR
using BallisticUnityTools.Animation.Transformation;
using UnityEngine;

namespace NgData.NameData.Actions.Zones
{
    [RegisterNameDataAction("follow")]
    public class FollowCameraActionProvider : NameDataAction
    {
        public override void Execute(GameObject targetObject)
        {
            UtObjectShipFollow follow = targetObject.GetComponent<UtObjectShipFollow>();
            if (!follow) follow = targetObject.AddComponent<UtObjectShipFollow>();

            if (GetFloat("fx", out float fx)) follow.FollowBlend.x = fx;
            if (GetFloat("fy", out float fy)) follow.FollowBlend.y = fy;
            if (GetFloat("fz", out float fz)) follow.FollowBlend.z = fz;

            if (GetFloat("ox", out float ox)) follow.FollowOffset.x = ox;
            if (GetFloat("oy", out float oy)) follow.FollowOffset.y = oy;
            if (GetFloat("oz", out float oz)) follow.FollowOffset.z = oz;
        }

        public override void Clear(GameObject targetObject)
        {
            UtObjectShipFollow follow = targetObject.GetComponent<UtObjectShipFollow>();
            if (follow) Object.DestroyImmediate(follow);
        }
    }
}
#endif