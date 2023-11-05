using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Source.NeoEditorTools.Editor
{
    public class NeoSurfacePlacer : NeoEditorTool
    {
        [MenuItem("Neognosis/Surface Placer")]
        public static void Create()
        {
            NeoToolEditor.OpenTool<NeoSurfacePlacer>();
        }

        /// <summary>
        /// The parent of the reference object that we're using.
        /// </summary>
        public Transform ReferenceParent;

        /// <summary>
        /// The reference object.
        /// </summary>
        public GameObject ReferenceObject;

        private readonly List<UnityEditor.Editor> _referenceEditors = new List<UnityEditor.Editor>();

        /// <summary>
        /// The objects created for this editor.
        /// </summary>
        private List<GameObject> _createdObjects = new List<GameObject>();

        /// <summary>
        /// How far off of the hit face's normal the object will be offset.
        /// </summary>
        public float NormalOffset;

        /// <summary>
        /// Whether the object should also be rotated.
        /// </summary>
        public bool Rotate;

        /// <summary>
        /// The axis that the object should be rotated along.
        /// </summary>
        public ERotateAxis RotateAxis;

        /// <summary>
        /// How much to rotate the object once it has been placed.
        /// </summary>
        public Vector3 RotationOffset;

        /// <summary>
        /// The last point 
        /// </summary>
        private Vector3 _lastHitPoint;

        /// <summary>
        /// The last hit normal;
        /// </summary>
        private Vector3 _lastHitNormal;

        /// <summary>
        /// The last raycast hit.
        /// </summary>
        private RaycastHit _lastHit;

        /// <summary>
        /// Whether a mesh was hit.
        /// </summary>
        private bool _hitMesh;

        /// <summary>
        /// Whether the user confirmed the exit.
        /// </summary>
        private bool _confirmedExit;

        public override void OnAwake()
        {
            GameObject selection = Selection.activeGameObject;
            if (!selection)
            {
                EditorUtility.DisplayDialog("Error", "Please select an object before using this tool.", "OK");
                Finished();
            }

            SetupReference(selection);

        }

        public override void OnDestroy()
        {
            if (ReferenceObject) Object.DestroyImmediate(ReferenceObject);
            foreach (UnityEditor.Editor editor in _referenceEditors) Object.DestroyImmediate(editor);
            _referenceEditors.Clear();

            if (!_confirmedExit && _createdObjects.Count > 0)
            {
                bool keepObjects = EditorUtility.DisplayDialog("Warning", "Exiting tool without confirming. Keep or remove created objects?", "Keep", "Delete");
                if (!keepObjects)
                {
                    foreach (GameObject createdObject in _createdObjects) Object.DestroyImmediate(createdObject);
                }
            }
        }

        public override void OnSceneGUI()
        {
            Event e = Event.current;

            /*---Update Normal Offset---*/
            if (Camera.current && _hitMesh && ShiftHeld && e.type == EventType.MouseMove) NormalOffset += e.delta.x * 0.0065f;

            /*---Draw controls---*/
            Handles.BeginGUI();

            NeoToolEditor.BeginControlsGui();

            GUILayout.Label("Controls", EditorStyles.boldLabel);
            GUILayout.Space(10);

            GUILayout.BeginVertical(EditorStyles.helpBox);
            NeoToolEditor.DrawControlsEntry("Accept", "Enter");
            NeoToolEditor.DrawControlsEntry("Cancel", "Escape");
            GUILayout.EndVertical();

            GUILayout.BeginVertical(EditorStyles.helpBox);
            NeoToolEditor.DrawControlsEntry("Adjust Normal Offset", "Shift + Mouse X");
            GUILayout.EndVertical();

            NeoToolEditor.EndControlsGui();

            Handles.EndGUI();

            /*---Draw offset helper line---*/
            Handles.color = Color.green;
            Handles.DrawLine(_lastHitPoint, ReferenceObject.transform.position);
            Handles.color = Color.white;

            /*---Draw reference component GUIs---*/
            foreach (UnityEditor.Editor editor in _referenceEditors)
            {
                MethodInfo mi = editor.GetType().GetMethod("OnSceneGUI", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                mi?.Invoke(editor, null);
            }
        }

        public override void OnInspectorGUI()
        {
            /*---Draw tool inspector---*/
            NeoToolEditor.BeginTitledSection("Actions");
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Confirm Objects", EditorStyles.toolbarButton)) Confirm();
            if (GUILayout.Button("Cancel Objects", EditorStyles.toolbarButton)) Cancel();
            GUILayout.EndHorizontal();
            NeoToolEditor.EndTitledSection();

            GUILayout.Space(10);
            NeoToolEditor.BeginTitledSection("Tool Settings");
            NormalOffset = EditorGUILayout.FloatField("Normal Offset", NormalOffset);
            Rotate = EditorGUILayout.Toggle("Rotate To Normal", Rotate);

            GUI.enabled = Rotate;
            ++EditorGUI.indentLevel;
            RotateAxis = (ERotateAxis) EditorGUILayout.EnumPopup("Rotate Axis", RotateAxis);
            RotationOffset = EditorGUILayout.Vector3Field("Rotation Offset", RotationOffset);
            --EditorGUI.indentLevel;
            GUI.enabled = true;

            NeoToolEditor.EndTitledSection();

            /*---Draw reference insecptors---*/
            GUILayout.Space(10);

            NeoToolEditor.BeginTitledSection("Reference Inspector");
            foreach (UnityEditor.Editor editor in _referenceEditors)
            {
                NeoToolEditor.BeginTitledSection(editor.target.GetType().Name);
                editor.OnInspectorGUI();
                NeoToolEditor.EndTitledSection();
            }
            NeoToolEditor.EndTitledSection();
        }

        public override void OnMouseMove(Vector2 mousePosition, Vector2 mouseDelta)
        {
            if (!ShiftHeld) _hitMesh = NeoToolEditor.RaycastFromMouse(out _lastHit, new[] { ReferenceObject });

            if (_hitMesh)
            {
                Transform t = ReferenceObject.transform;
                ReferenceObject.transform.position = _lastHit.point + (_lastHit.normal.normalized * NormalOffset);

                if (Rotate)
                {
                    switch (RotateAxis)
                    {
                        case ERotateAxis.X:
                            ReferenceObject.transform.right = _lastHit.normal;
                            break;
                        case ERotateAxis.Y:
                            ReferenceObject.transform.up = _lastHit.normal;
                            break;
                        case ERotateAxis.Z:
                            ReferenceObject.transform.forward = _lastHit.normal;
                            break;
                    }

                    ReferenceObject.transform.rotation *= Quaternion.AngleAxis(RotationOffset.x, Vector3.right);
                    ReferenceObject.transform.rotation *= Quaternion.AngleAxis(RotationOffset.y, Vector3.up);
                    ReferenceObject.transform.rotation *= Quaternion.AngleAxis(RotationOffset.z, Vector3.forward);
                }


                _lastHitPoint = _lastHit.point;
                _lastHitNormal = _lastHit.normal.normalized;
            }
        }

        public override void OnMouseButtonDown(int button)
        {
            if (button == 0)
            {
                GameObject newObject = Object.Instantiate(ReferenceObject);
                if (ReferenceParent) newObject.transform.SetParent(ReferenceParent);
                newObject.transform.position = ReferenceObject.transform.position;
                newObject.transform.rotation = ReferenceObject.transform.rotation;
                newObject.transform.localScale = ReferenceObject.transform.localScale;
                _createdObjects.Add(newObject);
                Undo.RegisterCreatedObjectUndo(newObject, $"Created {newObject.name}");
            }
        }

        public override void OnKeyDown(KeyCode key)
        {
            if (key == KeyCode.Return) Confirm();
            if (key == KeyCode.Escape) Cancel();
        }

        private void SetupReference(GameObject reference)
        {
            if (ReferenceObject) Object.DestroyImmediate(ReferenceObject);

            ReferenceParent = reference.transform.parent;
            ReferenceObject = Object.Instantiate(reference);
            ReferenceObject.hideFlags = HideFlags.DontSave;

            _referenceEditors.Clear();
            Component[] components = ReferenceObject.GetComponents<Component>();
            foreach (Component c in components)
            {
                if (c.GetType() == typeof(Transform)) continue;

                _referenceEditors.Add(UnityEditor.Editor.CreateEditor(c));
            }

            if (ReferenceParent) ReferenceObject.transform.SetParent(ReferenceParent);
        }

        private void Confirm()
        {
            _confirmedExit = true;
            Finished();
        }

        private void Cancel()
        {
            if (_createdObjects.Count > 0)
            {
                bool confirmed = EditorUtility.DisplayDialog("Warning", "This will delete all objects that have been created. Are you sure you wish to continue?", "Yes", "No");
                if (!confirmed) return;

                foreach (GameObject obj in _createdObjects) Object.DestroyImmediate(obj);
            }

            Confirm();
        }

        public enum ERotateAxis
        {
            X,
            Y,
            Z
        }
    }
}
