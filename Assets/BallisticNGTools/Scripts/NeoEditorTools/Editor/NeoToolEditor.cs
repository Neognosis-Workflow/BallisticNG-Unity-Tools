using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Type = System.Type;

namespace Source.NeoEditorTools.Editor
{
    [CustomEditor(typeof(NeoToolMono))]
    public class NeoToolEditor : UnityEditor.Editor
    {
        public delegate void DelegateOnArrayAction<T>(List<T> array, int i);
        public delegate string DelegateGetArrayTitle(int i);
        public delegate object DelegateGetNewArrayElement();

        public static Color CloseButtonColor = new Color(1.0f, 0.6f, 0.6f, 1.0f);
        public static Color HeaderCloseButtonColor = new Color(1.0f, 0.7f, 0.7f, 1.0f);
        public static Color HeaderGreenButtonColor = new Color(0.7f, 1.0f, 0.7f, 1.0f);

        static NeoToolEditor()
        {
            Type t = typeof(HandleUtility);
            _unityIntersectRayMesh = t.GetMethod("IntersectRayMesh", BindingFlags.Static | BindingFlags.NonPublic);
        }

        private static MethodInfo _unityIntersectRayMesh;

        /// <summary>
        /// Creates a new neo tool mono object.
        /// </summary>
        public static void OpenTool<T>() where T : NeoEditorTool
        {
            if (FindObjectOfType<NeoToolMono>()) return;

            GameObject go = new GameObject("Neognosis Editor Tool Manager") {hideFlags = HideFlags.DontSave};
            NeoToolMono mono = go.AddComponent<NeoToolMono>();

            T tool = (T) Activator.CreateInstance(typeof(T));
            tool.Target = mono;
            mono.Tool = tool;

            mono.Tool.OnAwake();
            Selection.activeObject = go;
        }

        /// <summary>
        /// Raycasts from the current mouse position.
        /// </summary>
        public static bool RaycastFromMouse(out RaycastHit hit, GameObject[] ignore)
        {
            if (ignore == null) ignore = new GameObject[0];

            Vector2 mousePos = Event.current.mousePosition;
            Ray ray = HandleUtility.GUIPointToWorldRay(mousePos);
            GameObject pickedObject = HandleUtility.PickGameObject(mousePos, false, ignore);

            if (!pickedObject)
            {
                hit = new RaycastHit();
                return false;
            }

            MeshFilter mf = pickedObject.GetComponent<MeshFilter>();
            if (!mf)
            {
                hit = new RaycastHit();
                return false;
            }


            Matrix4x4 matrix = pickedObject.transform.localToWorldMatrix;
            return EditorRaycast(ray, mf.sharedMesh, matrix, out hit);
        }

        /// <summary>
        /// Calls Unity's internal instersect ray mesh method.
        /// </summary>
        private static bool EditorRaycast(Ray ray, Mesh mesh, Matrix4x4 matrix, out RaycastHit hit)
        {
            object[] p = {ray, mesh, matrix, null};
            bool result = (bool)_unityIntersectRayMesh.Invoke(null, p);
            hit = (RaycastHit) p[3];
            return result;
        }

        public static GUIStyle DocumentationHeaderStylePersonal = new GUIStyle
        {
            alignment = TextAnchor.MiddleLeft,
            normal = new GUIStyleState { textColor = new Color(0.2f, 0.2f, 0.2f, 1.0f) }
        };

        public static GUIStyle DocumentationHeaderStyleProffesional = new GUIStyle
        {
            alignment = TextAnchor.MiddleLeft,
            normal = new GUIStyleState { textColor = new Color(0.8f, 0.8f, 0.8f, 1.0f) }
        };

        /// <summary>
        /// Draws the header for an inspector that can link to documentation.
        /// </summary>
        public static void DocumentationHeader(string headerTitle, string documentationUrl)
        {
            GUILayout.Space(10);
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbarButton);
            GUILayout.Label(headerTitle, EditorGUIUtility.isProSkin ? DocumentationHeaderStyleProffesional : DocumentationHeaderStylePersonal, GUILayout.MaxWidth(Screen.width * 0.8f));
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// Draws the header for an inspector that can link to documentation.
        /// </summary>
        public static bool DocumentationHeaderButton(string headerTitle, string documentationUrl, string buttonText, Color buttonColor)
        {
            GUILayout.Space(10);
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbarButton);
            GUILayout.Label(headerTitle, EditorGUIUtility.isProSkin ? DocumentationHeaderStyleProffesional : DocumentationHeaderStylePersonal, GUILayout.MaxWidth(Screen.width * 0.8f));

            GUI.color = buttonColor;
            if (GUILayout.Button(buttonText, EditorStyles.toolbarButton)) return true;
            GUI.color = Color.white;
            EditorGUILayout.EndHorizontal();
            return false;
        }

        /// <summary>
        /// Draws the header for an inspector that can link to documentation.
        /// </summary>
        public static bool DocumentationHeaderCloseButton(string headerTitle, string documentationUrl)
        {
            GUILayout.Space(10);
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbarButton);
            GUILayout.Label(headerTitle, EditorGUIUtility.isProSkin ? DocumentationHeaderStyleProffesional : DocumentationHeaderStylePersonal, GUILayout.MaxWidth(Screen.width * 0.8f));

            GUI.color = HeaderCloseButtonColor;
            if (GUILayout.Button("close", EditorStyles.toolbarButton)) return true;
            GUI.color = Color.white;
            EditorGUILayout.EndHorizontal();
            return false;
        }

        /// <summary>
        /// Begins a new controls GUI.
        /// </summary>
        public static void BeginControlsGui()
        {
            GUI.color = Color.black;
            GUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.MaxWidth(400));
            GUI.color = Color.white;
        }

        public static void DrawControlsEntry(string action, string input)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(action, GUILayout.Width(180));
            GUILayout.Label(":::", GUILayout.Width(20));
            GUILayout.Label(input, GUILayout.Width(180));

            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// Ends a previous controls GUI.
        /// </summary>
        public static void EndControlsGui()
        {
            GUILayout.EndVertical();
        }

        /// <summary>
        /// Begins a titled area for organized interfaces.
        /// </summary>
        public static void BeginTitledSection(string title)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label(title, EditorStyles.boldLabel);
            ++EditorGUI.indentLevel;
        }

        /// <summary>
        /// Ends a titled area.
        /// </summary>
        public static void EndTitledSection()
        {
            --EditorGUI.indentLevel;
            EditorGUILayout.EndVertical();
        }

        public static List<T> DrawArray<T>(UnityEngine.Object target, string title, IEnumerable<T> array, DelegateGetArrayTitle getElementTitle = null, DelegateGetNewArrayElement getNewElement = null, DelegateOnArrayAction<T> drawElement = null)
        {
            /*---Init Array---*/
            List<T> arrayList = array.ToList();
            int len = arrayList.Count;

            ++EditorGUI.indentLevel;
            if (DocumentationHeaderButton(title, "", "Add", HeaderGreenButtonColor))
            {
                if (getNewElement != null)
                {
                    Undo.RecordObject(target, "Add new array element");
                    arrayList.Add((T) getNewElement());
                }
            }

            GUILayout.BeginVertical();
            {
                for (int i = 0; i < len; ++i)
                {
                    GUILayout.BeginVertical(EditorStyles.helpBox);
                    {
                        string elementName = getElementTitle == null ? $"Element {i}" : getElementTitle(i);
                        GUILayout.BeginHorizontal(EditorStyles.toolbarButton);
                        {
                            GUILayout.Label(elementName);
                            if (GUILayout.Button("up", EditorStyles.toolbarButton, GUILayout.Width(50)))
                            {
                                if (i > 0)
                                {
                                    Undo.RecordObject(target, "Add new array element");

                                    T obj = arrayList[i];
                                    arrayList.RemoveAt(i);
                                    arrayList.Insert(i - 1, obj);
                                }

                                break;
                            }

                            if (GUILayout.Button("down", EditorStyles.toolbarButton, GUILayout.Width(50)))
                            {
                                if (i < len - 1)
                                {
                                    Undo.RecordObject(target, "Add new array element");

                                    T obj = arrayList[i];
                                    arrayList.RemoveAt(i);
                                    arrayList.Insert(i + 1, obj);
                                }
                            }

                            GUI.color = HeaderCloseButtonColor;
                            if (GUILayout.Button("X", EditorStyles.toolbarButton, GUILayout.Width(25)))
                            {
                                Undo.RecordObject(target, "Add new array element");

                                arrayList.RemoveAt(i);
                                break;
                            }

                            GUI.color = Color.white;
                        }
                        GUILayout.EndHorizontal();
                        drawElement?.Invoke(arrayList, i);
                    }
                    GUILayout.EndVertical();
                }
            }
            GUILayout.EndVertical();

            --EditorGUI.indentLevel;
            return arrayList;
        }


        private bool _holdingCtrl;
        private bool _holdingShift;
        private bool _holdingAlt;

        private void OnDisable()
        {
            NeoToolMono t = (NeoToolMono)target;
            if (t != null) DestroyImmediate(t.gameObject);
        }

        private void OnDestroy()
        {
            NeoToolMono t = (NeoToolMono)target;
            t.Tool.OnDestroy();
        }

        private void OnSceneGUI()
        {
            NeoToolMono t = (NeoToolMono)target;
            Event e = Event.current;

            /*---Update Modifiers---*/
            t.Tool.CtrlHeld = e.control || e.command;
            t.Tool.ShiftHeld = e.shift;
            t.Tool.AltHeld = e.alt;

            /*---Trigger Mouse Events---*/
            if (e.type == EventType.MouseDown) t.Tool.OnMouseButtonDown(e.button);
            if (e.type == EventType.MouseUp) t.Tool.OnMouseButtonUp(e.button);
            if (e.type == EventType.ScrollWheel) t.Tool.OnMouseScroll(e.delta);
            if (e.type == EventType.MouseMove) t.Tool.OnMouseMove(e.mousePosition, e.delta);

            /*---Trigger Key Events---*/
            if (e.type == EventType.KeyDown) t.Tool.OnKeyDown(e.keyCode);
            if (e.type == EventType.KeyUp) t.Tool.OnKeyUp(e.keyCode);

            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
            t.Tool.OnSceneGUI();
        }

        public override void OnInspectorGUI()
        {
            NeoToolMono t = (NeoToolMono)target;
            t.Tool.OnInspectorGUI();
        }
    }
}
