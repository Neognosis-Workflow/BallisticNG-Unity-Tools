#if UNITY_EDITOR
using UnityEngine;

namespace NgData.NameData.Actions.Zones
{
    public class ZoneNameDataAction : NameDataAction
    {
        /// <summary>
        /// Scaler for controlling how much created collider dimensions are scaled on creation.
        /// The default value of 100 is setup to accomodate for Blender.
        /// </summary>
        public static float Scaler = 100.0f;
        
        protected Collider GetCollider(GameObject targetObject, int sizeMode = -1)
        {
            Collider collider = targetObject.GetComponent<Collider>();
            
            if (collider) return collider;
            
            if (HasFlag("sphere"))
            {
                Vector3 center = Vector3.zero;
                float radius = 1.0f;

                if (GetFloat("cx", out float cx)) center.x = cx;
                if (GetFloat("cy", out float cy)) center.y = cy;
                if (GetFloat("cz", out float cz)) center.z = cz;
                
                if (GetFloat("r", out float r)) radius = r;

                SphereCollider sc = targetObject.AddComponent<SphereCollider>();
                sc.center = center / Scaler;
                sc.radius = radius / Scaler;
                
                if (sizeMode == 1)
                {
                    Vector3 size = targetObject.transform.localScale / Scaler;
                    radius = (size.x + size.y + size.z) / 3.0f;
                    if (GetFloat("r", out float rt)) radius *= rt;
                    sc.radius = 1.0f;
                    targetObject.transform.localScale = Vector3.one * radius;
                }

                if (sizeMode == 2)
                {
                    Vector3 size = targetObject.transform.localScale / Scaler;
                    sc.radius *= (size.x + size.y + size.z) / 3.0f;
                    targetObject.transform.localScale = Vector3.one;
                }

                return sc;
            } 
            
            if (HasFlag("mesh"))
            {
                MeshCollider mc = targetObject.AddComponent<MeshCollider>();
                mc.convex = true;
                return mc;
            }

            {
                Vector3 size = Vector3.one * 2.0f;
                Vector3 center = Vector3.zero;

                if (GetFloat("sx", out float sx)) size.x = sx;
                if (GetFloat("sy", out float sy)) size.y = sy;
                if (GetFloat("sz", out float sz)) size.z = sz;

                if (GetFloat("cx", out float cx)) center.x = cx;
                if (GetFloat("cy", out float cy)) center.y = cy;
                if (GetFloat("cz", out float cz)) center.z = cz;

                BoxCollider bc = targetObject.AddComponent<BoxCollider>();
                bc.size = size / Scaler;
                bc.center = center / Scaler;
                
                if (sizeMode == 1)
                {
                    size = targetObject.transform.localScale * 2.0f;
                    size /= Scaler;
                    if (GetFloat("sx", out float stx)) size.x *= stx;
                    if (GetFloat("sy", out float sty)) size.y *= sty;
                    if (GetFloat("sz", out float stz)) size.z *= stz;
                    targetObject.transform.localScale = size;
                    bc.size = Vector3.one;
                }

                if (sizeMode == 2)
                {
                    size = bc.size;
                    Vector3 tSize = targetObject.transform.localScale * 2.0f;
                    size.x *= tSize.x;
                    size.y *= tSize.y;
                    size.z *= tSize.z;

                    bc.size = size;
                    targetObject.transform.localScale = Vector3.one;
                }
                
                bc.isTrigger = true;

                return bc;
            }
        }
    }
}
#endif