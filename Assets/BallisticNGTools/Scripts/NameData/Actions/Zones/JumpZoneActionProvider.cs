#if UNITY_EDITOR
using BallisticUnityTools.Placeholders;
using UnityEngine;

namespace NgData.NameData.Actions.Zones
{
    [RegisterNameDataAction("jump")]
    public class JumpZoneActionProvider : NameDataAction
    {
        public override void Execute(GameObject targetObject)
        {
            ModJumpZone jz = targetObject.GetComponent<ModJumpZone>();
            if (!jz) jz = targetObject.AddComponent<ModJumpZone>();

            jz.ZoneExtents = (targetObject.transform.localScale * 2.0f) / ZoneNameDataAction.Scaler;
            targetObject.transform.localScale = Vector3.one;
            
            if (GetFloat("yaw", out float yaw)) jz.PushYawOffset = yaw;
            if (GetFloat("pitch", out float pitch)) jz.PushPitchOffset = pitch;

            if (GetFloat("speedtoxic", out float speedToxic)) jz.PushSpeedToxic = speedToxic;
            if (GetFloat("acceltoxic", out float acceltoxic)) jz.AccelAmountToxic = acceltoxic;
            
            if (GetFloat("speedapex", out float speedApex)) jz.PushSpeedApex = speedApex;
            if (GetFloat("accelapex", out float accelApex)) jz.AccelAmountApex = accelApex;
            
            if (GetFloat("speedhalberd", out float speedHalberd)) jz.PushSpeedHalberd = speedHalberd;
            if (GetFloat("accelhalberd", out float accelHalberd)) jz.AccelAmountHalberd = accelHalberd;
            
            if (GetFloat("speedspectre", out float speedSpectre)) jz.PushSpeedSpectre = speedSpectre;
            if (GetFloat("accelspectre", out float accelSpectre)) jz.AccelAmountSpectre = accelSpectre;
            
            if (GetFloat("speedzen", out float speedZen)) jz.PushSpeedZen = speedZen;
            if (GetFloat("accelzen", out float accelZen)) jz.AccelAmountZen = accelZen;
        }

        public override void Clear(GameObject targetObject)
        {
            ModJumpZone jz = targetObject.GetComponent<ModJumpZone>();
            if (jz) Object.DestroyImmediate(jz);
        }
    }
}
#endif