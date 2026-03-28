#if UNITY_EDITOR
using BallisticUnityTools.Placeholders;
using UnityEngine;

namespace NgData.NameData.Actions
{
    [RegisterNameDataAction("vrender")]
    public class SurvivalRenderActionProvider : NameDataAction
    {
        public override void Execute(GameObject targetObject)
        {
            if (HasFlag("disable")) AttachComponent<ZoneDisable>(targetObject);
            if (HasFlag("enable")) AttachComponent<ZoneEnable>(targetObject);

            if (HasFlag("trackeq")) 
                AttachComponent<ZoneTrackSurface>(targetObject)
                    .SurfaceType = ZoneTrackSurface.EZoneTrackSurfaceType.EqOverlay;

            if (HasFlag("track"))
                AttachComponent<ZoneTrackSurface>(targetObject)
                    .SurfaceType = ZoneTrackSurface.EZoneTrackSurfaceType.BaseFloorOrWall;

            if (HasFlag("ignore")) AttachComponent<ZoneIgnore>(targetObject);

            if (HasFlag("sky")) AttachComponent<ZoneSky>(targetObject);
        }

        private T AttachComponent<T>(GameObject target) where T : Component
        {
            T t = target.GetComponent<T>();
            if (!t) t = target.AddComponent<T>();

            return t;
        }

        private void RemoveComponent<T>(GameObject target) where T : Component
        {
            T t = target.GetComponent<T>();
            if (t) Object.DestroyImmediate(t);
        }

        public override void Clear(GameObject targetObject)
        {
            RemoveComponent<ZoneDisable>(targetObject);
            RemoveComponent<ZoneEnable>(targetObject);
            RemoveComponent<ZoneTrackSurface>(targetObject);
            RemoveComponent<ZoneIgnore>(targetObject);
            RemoveComponent<ZoneSky>(targetObject);
        }
    }
}
#endif