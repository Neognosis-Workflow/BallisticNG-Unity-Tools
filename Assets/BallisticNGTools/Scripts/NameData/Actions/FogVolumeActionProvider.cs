#if UNITY_EDITOR
using BallisticUnityTools.Placeholders;
using UnityEngine;

namespace NgData.NameData.Actions
{
    [RegisterNameDataAction("fog")]
    public class FogVolumeActionProvider : NameDataAction
    {
        /// <summary>
        /// Scaler for controlling how much created volume  dimensions are scaled on creation.
        /// The default value of 100 is setup to accomodate for Blender.
        /// </summary>
        public static float Scaler = 100.0f;
        
        public override void Execute(GameObject targetObject)
        {
            ModFogVolume fog = targetObject.GetComponent<ModFogVolume>();
            if (!fog) fog = targetObject.AddComponent<ModFogVolume>();

            if (HasFlag("sphere")) fog.VolumeMode = ModFogVolume.FogVolumeMode.Sphere;
            else fog.VolumeMode = ModFogVolume.FogVolumeMode.Box;

            Vector3 scale = targetObject.transform.localScale;
            targetObject.transform.localScale = Vector3.one;
            fog.Extents = (scale / Scaler) * 2.0f;
            fog.Radius = ((scale.x + scale.y + scale.z) / 3.0f) / Scaler;

            if (GetString("color", out string color))
            {
                if (ColorUtility.TryParseHtmlString(color, out Color actualColor))
                    fog.FogColor = actualColor;
            }
            if (GetFloat("ds", out float ds)) fog.StartDistance = ds;
            if (GetFloat("de", out float de)) fog.EndDistance = de;
            if (GetFloat("it", out float it)) fog.TransitionInTime = it;
            if (GetFloat("ot", out float ot)) fog.TransitionOutTime = ot;

        }

        public override void Clear(GameObject targetObject)
        {
            ModFogVolume fog = targetObject.GetComponent<ModFogVolume>();
            if (fog) Object.DestroyImmediate(fog);
        }
    }
}
#endif