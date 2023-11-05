using System.Collections.Generic;
using System.Linq;
using BallisticUnityTools.Editor;
using BallisticUnityTools.Placeholders;
using BallisticUnityTools.TrackTools;
using UnityEditor;
using UnityEngine;

namespace NgEditorTools.ModernPads.Editor
{
    [CustomEditor(typeof(ModernPadEditorMono))]
    public class ModernPadEditor : UnityEditor.Editor 
    {

        [MenuItem("BallisticNG/Track Configuration/3d Pad Editor")]
        public static void CreateModernPadEditor()
        {
            ModernPadEditorMono mono = FindObjectOfType<ModernPadEditorMono>();
            if (!mono)
            {
                GameObject newGo = new GameObject("3d Pad Editor");
                mono = newGo.AddComponent<ModernPadEditorMono>();
            }

            Selection.activeObject = mono.gameObject;
        }
        
        public ModernPadEditorMono Target
        {
            get { return (ModernPadEditorMono) target; }
        }

        private TrackDataStorage _tData;
        private TrackDataStorage TrackData
        {
            get
            {
                if (_tData) return _tData;
                _tData = FindObjectOfType<TrackDataStorage>();
                return _tData;
            }
        }

        private bool _confirmingPlacement;
        private GameObject _confirmPlaceObject;
        private Quaternion _confirmPlaceRot;

        private Dictionary<TrackPad, MeshRenderer[]> _deletable = new Dictionary<TrackPad, MeshRenderer[]>();
        
        /// <summary>
        /// Calculates the barycentric normal for the provided raycast hit.
        /// </summary>
        /// <param name="normals">The mesh normals.</param>
        /// <param name="triangles">The mesh triangles.</param>
        /// <param name="trackTransform">The meshes transform component.</param>
        /// <param name="hit">The raycasts hit result.</param>
        public static Vector3 GetBaryCentricNormal(Vector3[] normals, int[] triangles, Transform trackTransform, RaycastHit hit)
        {
            // get triangle indicies
            Vector3 n0 = Vector3.zero;
            Vector3 n1 = Vector3.zero;
            Vector3 n2 = Vector3.zero;
            Vector3 baryCenter = hit.barycentricCoordinate;

            int tri1 = hit.triangleIndex * 3 + 0;
            int tri2 = hit.triangleIndex * 3 + 1;
            int tri3 = hit.triangleIndex * 3 + 2;

            n0 = normals[triangles[tri1]];
            n1 = normals[triangles[tri2]];
            n2 = normals[triangles[tri3]];

            Transform hitTransform = hit.collider.transform;
            return hitTransform.TransformDirection((n0 * baryCenter.x + n1 * baryCenter.y + n2 * baryCenter.z).normalized);
        }

        public SerializedProperty PrefabSpeed;
        public SerializedProperty PrefabWeapon;
        public SerializedProperty GroundOffset;
        public SerializedProperty PadType;

        public List<MeshCollider> CreatedColliders = new List<MeshCollider>();

        private bool _adjustingGroundOffset;
        private RaycastHit _groundOffsetHit;
        private bool _groundOffsetHitFloor;

        private void Awake()
        {
            InspectorPropertySetup.Setup(this, GetType(), serializedObject);
            
            if (Target.PrefabSpeed || Target.PrefabWeapon) return;
            
            Target.PrefabSpeed = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/BallisticNGTools/BallisticNG Assets/2280 Pads/Prefabs/Flat Speed Pad.prefab");
            Target.PrefabWeapon = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/BallisticNGTools/BallisticNG Assets/2280 Pads/Prefabs/Flat Weapon Pad.prefab");
            
            Target.SetupPrefabs();
        }

        protected void OnEnable()
        {
            Target.ShowPreviews(false);
            Undo.undoRedoPerformed += UndoRedoPerformed;

            BallisticMeshCollider[] colliders = FindObjectsOfType<BallisticMeshCollider>();
            foreach (BallisticMeshCollider collider in colliders)
            {
                if (collider.Type == BallisticMeshCollider.CollisionType.TrackFloor || collider.Type == BallisticMeshCollider.CollisionType.TrackWall) continue;

                MeshCollider newCol = collider.gameObject.AddComponent<MeshCollider>();
                newCol.hideFlags = HideFlags.DontSave;
                CreatedColliders.Add(newCol);
            }
        }

        protected void OnDisable()
        {
            Target.HidePreviews(false);
            Undo.undoRedoPerformed += UndoRedoPerformed;

            foreach (MeshCollider collider in CreatedColliders)
            {
                if (collider) DestroyImmediate(collider);
            }
            CreatedColliders.Clear();
        }
        
        private void UndoRedoPerformed()
        {
            RefreshDeleteCache();
        }

        public override void OnInspectorGUI()
        {
            ModernPadEditorMono editor = (ModernPadEditorMono)target;
            
            InspectorTemplates.DocumentationHeader("Prefabs", "");
            
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.HelpBox("Usage:\n\n * Setup prefabs below and click the \"Update Prefab\" to change the prefabs to place.\n\n * Click to place pad. Use the mouse to adjust the rotation or press escape to skip. Hold Shift when placing to use world space. Press T to switch pad type.\n\n* Hold CTRL to enter delete mode. Hover over a pad and click to delete it.\n\n* Hold CTRL + Shift + Mouse Drag to adjust the placement ground offset.", MessageType.Info);
            EditorGUILayout.PropertyField(PrefabSpeed, new GUIContent("Speed Pad Prefab"));
            EditorGUILayout.PropertyField(PrefabWeapon, new GUIContent("Weapon Pad Prefab"));
            if (GUILayout.Button("Update Prefab")) editor.SetupPrefabs();
            if (GUILayout.Button("Replace Prefabs")) editor.ReplacePrefabs();

            GUILayout.Space(10);
            InspectorTemplates.DocumentationHeader("Placement Settings", "");
            EditorGUILayout.PropertyField(GroundOffset, new GUIContent("Ground Offset"));
            if (GUILayout.Button("Reset Ground Offset")) editor.GroundOffset = 0.05f;
            
            EditorGUILayout.PropertyField(PadType, new GUIContent("Pad Type"));
            
            if (GUILayout.Button("Finished")) DestroyImmediate(editor.gameObject);

            if (EditorGUI.EndChangeCheck())
            {
                if (serializedObject.targetObject) serializedObject.ApplyModifiedProperties();
            }

        }
        
        private void OnSceneGUI()
        {
            if (!target) return;

            // don't allow editor selections
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));

            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.T)
            {
                if (Target.PadType == TrackPad.TrackPadType.Speed) Target.PadType = TrackPad.TrackPadType.Weapon;
                else if (Target.PadType == TrackPad.TrackPadType.Weapon) Target.PadType = TrackPad.TrackPadType.Speed;
                
                Target.ShowPreviews(false, true);
            }
            
            if (_confirmingPlacement) ConfirmPlaceMode();
            else if (Event.current.control)
            {
                if (!AdjustPadGroundOffset())
                {
                    if (Event.current.type == EventType.KeyDown) RefreshDeleteCache();
                    PadDeleteMode();
                }
            }
            else SetPlaceMode();
        }

        private void PadDeleteMode()
        {
            Target.HidePreviews(true);
            Event e = Event.current;
            HandleUtility.Repaint();
            
            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            
            TrackPad hitPad = null;
            MeshRenderer hitMr = null;
            foreach (KeyValuePair<TrackPad,MeshRenderer[]> deletable in _deletable)
            {
                foreach (MeshRenderer meshRenderer in deletable.Value)
                {
                    bool hit = meshRenderer.bounds.IntersectRay(ray, out float distance);
                    if (hit)
                    {
                        hitPad = deletable.Key;
                        hitMr = meshRenderer;
                        break;
                    }
                }
            }
            
            if (hitPad)
            {
                Handles.color = Color.red;
                if (hitMr)
                {
                    Transform t = hitMr.transform;
                    Matrix4x4 prevMatrix = Handles.matrix;
                    Handles.matrix = Matrix4x4.TRS(t.position, t.rotation, Vector3.one);

                    Handles.DrawWireCube(Vector3.zero, hitMr.bounds.size);
                    Handles.matrix = prevMatrix;
                }
                Handles.color = Color.white;
            }

            if (hitPad && e.type == EventType.MouseDown && e.button == 0)
            {
                Undo.DestroyObjectImmediate(hitPad.gameObject);
                RefreshDeleteCache();
            }
        }
        
        private void SetPlaceMode()
        { 
            Event e = Event.current;
            if (!e.isMouse) return;
            HandleUtility.Repaint();
            
            // raycast to get track geometry
            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, 1, QueryTriggerInteraction.Ignore))
            {
                Target.ShowPreviews(true);
                
                // get closest section and build transform data
                Vector3 point = hit.point;

                Section closestSection = TrackProcessors.GetClosestSectionFast(point, TrackData);
                Vector3 up = BarycentricNormal(hit);
                Vector3 fwd = e.shift ? Vector3.ProjectOnPlane(Vector3.forward, up): closestSection.Rotation * Vector3.forward;
                Quaternion rot = Quaternion.LookRotation(fwd, up);
                
                // move the template objects to the mouse cursor
                if (Target.PadType == TrackPad.TrackPadType.Speed && Target.TemplateSpeed) SetPadTransform(Target.TemplateSpeed.transform, point, rot);
                if (Target.PadType == TrackPad.TrackPadType.Weapon && Target.TemplateWeapon) SetPadTransform(Target.TemplateWeapon.transform, point, rot);

                // button clicks
                if (e.type == EventType.MouseDown && e.button == 0)
                {
                    // instance prefab
                    if (Target.PadType == TrackPad.TrackPadType.Speed && Target.TemplateSpeed)
                    {
                        _confirmPlaceObject = Target.GetPrefabInstance(Target.PrefabSpeed, false);
                        _confirmPlaceObject.transform.position = Target.TemplateSpeed.transform.position;
                        _confirmPlaceObject.transform.rotation = Target.TemplateSpeed.transform.rotation;
                    }

                    if (Target.PadType == TrackPad.TrackPadType.Weapon && Target.TemplateWeapon)
                    {
                        _confirmPlaceObject = Target.GetPrefabInstance(Target.PrefabWeapon, false);
                        _confirmPlaceObject.transform.position = Target.TemplateWeapon.transform.position;
                        _confirmPlaceObject.transform.rotation = Target.TemplateWeapon.transform.rotation;
                    }
                    
                    Undo.RegisterCreatedObjectUndo(_confirmPlaceObject, "Created Track Pad");

                    // switch mode
                    _confirmPlaceRot = _confirmPlaceObject.transform.rotation;
                    _confirmingPlacement = true;
                    Target.HidePreviews(true);
                    Target.SetCanTogglePreviews(false);
                }
            } else Target.HidePreviews(true);
        }

        private bool AdjustPadGroundOffset()
        {
            Event e = Event.current;
            if (!e.shift)
            {
                _groundOffsetHitFloor = false;
                return false;
            }

            if (e.type == EventType.MouseDown && e.button == 0)
            {
                Undo.RegisterCompleteObjectUndo(Target, "Adjust Pad Placer Ground Offset");

                Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
                _groundOffsetHitFloor = Physics.Raycast(ray, out _groundOffsetHit, Mathf.Infinity, 1, QueryTriggerInteraction.Ignore);
            }

            if (e.type == EventType.MouseUp && e.button == 0)
            {
                _groundOffsetHitFloor = false;
            }

            if (_groundOffsetHitFloor)
            {
                Target.ShowPreviews(true);
                if (e.type == EventType.MouseDrag) Target.GroundOffset += e.delta.x / Screen.width;

                if (Target.PadType == TrackPad.TrackPadType.Speed && Target.TemplateSpeed)
                {
                    SetPadTransform(Target.TemplateSpeed.transform, _groundOffsetHit.point, Target.TemplateSpeed.transform.rotation);
                    Handles.DrawLine(_groundOffsetHit.point, Target.TemplateSpeed.transform.position);
                }

                if (Target.PadType == TrackPad.TrackPadType.Weapon && Target.TemplateWeapon)
                {
                    SetPadTransform(Target.TemplateWeapon.transform, _groundOffsetHit.point, Target.TemplateWeapon.transform.rotation);
                    Handles.DrawLine(_groundOffsetHit.point, Target.TemplateWeapon.transform.position);
                }

                return true;
            }

            return false;
        }
        
        private void ConfirmPlaceMode()
        {
            Event e = Event.current;
            HandleUtility.Repaint();
            
            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            Vector3 floorPoint = Vector3.zero;
            bool hitFloor = Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, 1, QueryTriggerInteraction.Ignore);
            
            if (!hitFloor)
            {
                Plane p = new Plane(_confirmPlaceObject.transform.up, _confirmPlaceObject.transform.position);
                p.Raycast(ray, out float enter);

                floorPoint = ray.GetPoint(enter);
            }
            else floorPoint = hit.point;

            Handles.DrawLine(floorPoint, _confirmPlaceObject.transform.position);
            
            // update rotation offset
            if (e.type == EventType.MouseMove)
            {
                Vector3 dir = floorPoint - _confirmPlaceObject.transform.position;
                dir = Vector3.ProjectOnPlane(dir, _confirmPlaceRot * Vector3.up);
                Quaternion rot = Quaternion.LookRotation(dir, _confirmPlaceRot * Vector3.up);

                _confirmPlaceObject.transform.rotation = rot;
            }

            // user rotation confirm / cancel
            bool endMode = e.type == EventType.MouseDown && e.button == 0;
            if (e.type == EventType.KeyDown && e.keyCode == KeyCode.Escape)
            {
                _confirmPlaceObject.transform.rotation = _confirmPlaceRot;
                endMode = true;
            }

            // back out of mode if we're done
            if (!endMode) return;
            
            _confirmingPlacement = false;
            Target.SetCanTogglePreviews(true);
            Target.ShowPreviews(true);
        }

        /// <summary>
        /// Calculates the barycentric normal for the given raycast hit.
        /// </summary>
        private Vector3 BarycentricNormal(RaycastHit hit)
        {
            MeshCollider meshCollider = hit.collider as MeshCollider;
            if (!meshCollider || !meshCollider.sharedMesh) return hit.normal;
            
            Mesh mesh = meshCollider.sharedMesh;
            Vector3[] normals = mesh.normals;
            int[] triangles = mesh.triangles;

            return GetBaryCentricNormal(normals, triangles, hit.collider.transform, hit);
        }

        /// <summary>
        /// Sets the transform of the provided pads transform, taking into account the GroundOffset from the editor.
        /// </summary>
        private void SetPadTransform(Transform t, Vector3 position, Quaternion rotation)
        {
            t.position = position + (rotation * Vector3.up * Target.GroundOffset);
            t.rotation = rotation;
        }
        
        private void RefreshDeleteCache()
        {
            _deletable.Clear();
            TrackPad[] pads = FindObjectsOfType<TrackPad>();

            foreach (TrackPad pad in pads) _deletable.Add(pad, pad.GetComponentsInChildren<MeshRenderer>());
        }
    }
}
