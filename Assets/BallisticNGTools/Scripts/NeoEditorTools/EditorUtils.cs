#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Source.NeoEditorTools
{
    public class EditorUtils
    {
        /// <summary>
        /// The style for drawing label backgrounds.
        /// </summary>
        public static GUIStyle LabelBgStyle
        {
            get
            {
                if (_labelBgStyle != null) return _labelBgStyle;

                _labelBgStyle = new GUIStyle(GUI.skin.GetStyle("AnimationEventTooltip"))
                {
                    normal = {textColor = Color.white}, 
                    alignment = TextAnchor.MiddleCenter, 
                    padding = new RectOffset(0, 0, 0, 0)
                };
                return _labelBgStyle;
            }
        }
        private static GUIStyle _labelBgStyle;

        /// <summary>
        /// The material for drawing mesh highlights.
        /// </summary>
        public static Material HighlightMaterial
        {
            get
            {
                if (_highlightMaterial) return _highlightMaterial;

                _highlightMaterial = Object.Instantiate(AssetDatabase.LoadAssetAtPath<Material>("Assets/Neo Unity Editor/Mesh Highlights/Mesh Highlight.mat"));
                return _highlightMaterial;
            }   
        }
        private static Material _highlightMaterial;

        /// <summary>
        /// Draws a label.
        /// </summary>
        public static void DrawLabel(Vector3 position, string text, float maxDistance = 1000.0f)
        {
            const int maxFontSize = 12;

            Vector3 offsetFromCam = Camera.current.WorldToViewportPoint(position);
            float t = 1 - (offsetFromCam.z / maxDistance);

            Color bgColor = GUI.backgroundColor;
            bgColor.a = t * 0.8f / 0.8f;
            GUI.backgroundColor = bgColor;

            LabelBgStyle.fontSize = Mathf.RoundToInt(t * maxFontSize);

            Color txtColor = LabelBgStyle.normal.textColor;
            txtColor.a = t;
            LabelBgStyle.normal.textColor = txtColor;
            
            Handles.Label(position, text, LabelBgStyle);
            
            GUI.backgroundColor = Color.white;
        }

        /// <summary>
        /// Draws a highlight for the provided mesh filter.
        /// </summary>
        public static void DrawMeshHighlight(MeshFilter mf, Texture texture = null)
        {
            HighlightMaterial.SetPass(0);
            HighlightMaterial.mainTexture = texture;
            Graphics.DrawMeshNow(mf.sharedMesh, mf.transform.localToWorldMatrix, 0);
        }

        /// <summary>
        /// Draws a dot with a button that confirms if a user wants to delete an object.
        /// </summary>
        /// <returns></returns>
        public static bool DrawDeleteCap(string name, Vector3 pos, Quaternion cameraRot, bool drawOnly = false)
        {
            Handles.RectangleHandleCap(0, pos, cameraRot, 1.0f, EventType.Repaint);

            if (!Handles.Button(pos, cameraRot, 0.5f, 1.0f, Handles.DotHandleCap)) return false;
            if (drawOnly) return false;
            
            bool accepted = EditorUtility.DisplayDialog($"Delete {name}", $"You are about to delete {name}. Are you sure?", "Yes", "No");
            return accepted;
        }
    }
}
#endif