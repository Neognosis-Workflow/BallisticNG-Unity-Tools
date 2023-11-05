using System;
using System.Collections.Generic;
using System.Linq;
using BallisticUnityTools.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

[CustomEditor(typeof(TrmEditor))]
public class TrmEditorEd : Editor
{
    [MenuItem("BallisticNG/Utilities/Trm Editor", false, EditorMenuPriorities.UtilitiesMenu.TrmEditor)]
    public static void CreateEditor()
    {
        TrmEditor editor = FindObjectOfType<TrmEditor>();
        if (!editor)
        {
            GameObject newGo = new GameObject("Trm Editor");
            editor = newGo.AddComponent<TrmEditor>();
        }
        Selection.activeObject = editor;
    }

    #region Editor Data Definitions
    [Serializable]
    public class ManualEditorData
    {
        public enum CreateState
        {
            Middle,
            LeftFloorSpan,
            LeftWallSpan,
            RightFloorSpan,
            RightWallSpan
        }

        public CreateState ClickState = CreateState.Middle;
        public CreateState DragState = CreateState.LeftWallSpan;
        public EInteractionMode Mode = EInteractionMode.Click;
        public Vector3[] Points = new Vector3[5];
        public Vector3 DragModeLastVertex;

        public string[] ActionNames = {"Middle", "Left Floor", "Left Wall", "Right Floor", "Right Wall"};
        public string[] DragActionNames = {"Left Floor", "Left Wall", "Middle", "Right Floor", "Right Wall"};
    }

    public class RaycastHitData
    {
        public RaycastHit Hit;
        public Vector3 HitVertex;
    }

    public enum ECreateMode
    {
        Sections,
        Nexts,
        Vertices,
    }

    public enum EInteractionMode
    {
        Click,
        Drag
    }

    #endregion

    #region State Variables

    /// <summary>
    /// Contains data for keeping track of the manual editor state.
    /// </summary>
    public ManualEditorData ManualEditor = new ManualEditorData();

    /// <summary>
    /// Information regarding mouse-world raycasts.
    /// </summary>
    public RaycastHitData RaycastHit = new RaycastHitData();

    /// <summary>
    /// True if RaycastHit contains recent raycast data.
    /// </summary>
    public bool RaycastHasHit;

    /// <summary>
    /// The current editing mode.
    /// </summary>
    public ECreateMode CreateMode = ECreateMode.Sections;

    private Vector2 _previousMousePosition;
    private bool _repaintSceneView;
    private bool _autoCentre;

    private TrmEditor.TrackSection _highlightedSection;

    #endregion

    #region Cached Variables

    private TrmEditor _cachedTarget;
    public TrmEditor Target
    {
        get
        {
            if (!_cachedTarget) _cachedTarget = (TrmEditor) target;
            return _cachedTarget;
        }
    }

    private SerializedObject _targetSerialized;
    public SerializedObject TargetSerialized
    {
        get
        {
            if (_targetSerialized != null) return _targetSerialized;
            if (!Target) return null;

            _targetSerialized = new SerializedObject(Target);
            return _targetSerialized;
        }
    }
    #endregion


    #region Events

    private void Awake()
    {
        Undo.undoRedoPerformed -= OnUndoRedo;
        Undo.undoRedoPerformed += OnUndoRedo;
    }

    private void OnDestroy()
    {
        Undo.undoRedoPerformed -= OnUndoRedo;
    }

    private void OnUndoRedo()
    {
        if (ManualEditor.Mode == EInteractionMode.Drag) ManualEditor.DragState = ManualEditorData.CreateState.LeftWallSpan;
        Target.RebuildMesh();
    }

    /// <summary>
    /// Handles OnClick events from OnSceneGUI.
    /// </summary>
    /// <param name="button"></param>
    private void OnClick(int button)
    {
        _repaintSceneView = true;

        if (button == 0)
        {
            if (CreateMode == ECreateMode.Nexts) OnNextClick();
            else OnManualClick();
        }
    }

    private void OnNextClick()
    {
        TrmEditor.TrackSection section = FindClosestSection(RaycastHit.HitVertex);
        if (eInput.KeyHeld(KeyCode.Z))
        {
            if (_highlightedSection == null) return;
            _highlightedSection.NextSection = Target.TrackData.IndexOf(section);

            Target.RebuildFloor();
            Target.RebuildWall();
        }

        if (eInput.KeyHeld(KeyCode.X))
        {
            section.NextSection = -1;
            
            Target.RebuildFloor();
            Target.RebuildWall();

            return;
        }

        _highlightedSection = section;
    }

    private void OnManualClick()
    {
        if (ManualEditor.Mode == EInteractionMode.Drag)
        {
            if (ManualEditor.DragState == ManualEditorData.CreateState.LeftWallSpan)
            {
                RecordClickUndo();
                ManualEditor.Points[2] = RaycastHit.HitVertex;
                ManualEditor.DragModeLastVertex = RaycastHit.HitVertex;
                ManualEditor.DragState = ManualEditorData.CreateState.LeftFloorSpan;
            }
        }
        else
        {
            switch (ManualEditor.ClickState)
            {
                case ManualEditorData.CreateState.Middle:
                    RecordClickUndo();
                    ManualEditor.Points[0] = RaycastHit.HitVertex;
                    ManualEditor.ClickState = ManualEditorData.CreateState.LeftFloorSpan;
                    break;
                case ManualEditorData.CreateState.LeftFloorSpan:
                    RecordClickUndo();
                    ManualEditor.Points[1] = RaycastHit.HitVertex;
                    ManualEditor.ClickState = ManualEditorData.CreateState.LeftWallSpan;
                    break;
                case ManualEditorData.CreateState.LeftWallSpan:
                    RecordClickUndo();
                    ManualEditor.Points[2] = RaycastHit.HitVertex;
                    ManualEditor.ClickState = ManualEditorData.CreateState.RightFloorSpan;
                    break;
                case ManualEditorData.CreateState.RightFloorSpan:
                    RecordClickUndo();
                    ManualEditor.Points[3] = RaycastHit.HitVertex;
                    ManualEditor.ClickState = ManualEditorData.CreateState.RightWallSpan;
                    break;
                case ManualEditorData.CreateState.RightWallSpan:
                    RecordClickUndo();
                    ManualEditor.Points[4] = RaycastHit.HitVertex;
                    ManualEditor.ClickState = ManualEditorData.CreateState.Middle;
                    TrmEditor.TrackSection section = new TrmEditor.TrackSection(_autoCentre, ManualEditor.Points[0], ManualEditor.Points[1], ManualEditor.Points[2], ManualEditor.Points[3], ManualEditor.Points[4], Target);
                    break;
            }
        }
    }

    private void RecordClickUndo()
    {
        Undo.RecordObject(Target, "Click");
        Undo.RecordObject(this, "Click");
    }

    #endregion


    #region Per Frame Updates

    /// <summary>
    /// Performs a raycast and updates the information in RaycastHit.
    /// </summary>
    private void UpdateRaycastData()
    {
        Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
        RaycastHasHit = Physics.Raycast(ray, out RaycastHit.Hit);
        if (RaycastHasHit)
        {
            MeshCollider meshCollider = (MeshCollider)RaycastHit.Hit.collider;
            if (meshCollider) RaycastHit.HitVertex = FindClosetPoint(RaycastHit.Hit.transform, meshCollider.sharedMesh.vertices, RaycastHit.Hit.point);
        }
    }

    /// <summary>
    /// Updates the scene view to repaint on mouse inputs and keep selection passive.
    /// </summary>
    private void UpdateSceneView()
    {
        if (Target.FloorRenderer) EditorUtility.SetSelectedRenderState(Target.FloorRenderer, EditorSelectedRenderState.Hidden);
        if (Target.WallRenderer) EditorUtility.SetSelectedRenderState(Target.WallRenderer, EditorSelectedRenderState.Hidden);

        if (Event.current.mousePosition != _previousMousePosition || _repaintSceneView)
        {
            _previousMousePosition = Event.current.mousePosition;
            HandleUtility.Repaint();
        }

        eInput.Update();
        Tools.current = Tool.None;
        HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
    }

    private void DrawHighlightedVert()
    {
        if (CreateMode == ECreateMode.Nexts)
        {
            TrmEditor.TrackSection section = FindClosestSection(RaycastHit.HitVertex);

            Handles.color = Color.blue;
            Handles.zTest = CompareFunction.LessEqual;
            if (section != null && section != _highlightedSection) Handles.SphereHandleCap(0, section.Position, Quaternion.identity, Target.GizmoScale * 2.0f, EventType.Repaint);

            if (_highlightedSection != null)
            {
                Handles.color = Color.yellow;
                Handles.SphereHandleCap(-1, _highlightedSection.Position, Quaternion.identity, Target.GizmoScale * 2.0f, EventType.Repaint);
            }
        }
        else
        {
            Handles.color = Color.red;
            Handles.zTest = CompareFunction.LessEqual;
            if (RaycastHit.Hit.normal.magnitude > Mathf.Epsilon) Handles.SphereHandleCap(0, RaycastHit.HitVertex, Quaternion.LookRotation(RaycastHit.Hit.normal), Target.GizmoScale, EventType.Repaint);
        }
    }

    private void DrawVertTrail()
    {
        Handles.color = Color.green;

        CompareFunction origFunc = Handles.zTest;
        if (ManualEditor.Mode == EInteractionMode.Drag)
        {
            switch (ManualEditor.DragState)
            {
                case ManualEditorData.CreateState.Middle:
                    Handles.SphereHandleCap(1, ManualEditor.Points[2], Quaternion.identity, Target.GizmoScale * 0.5f, EventType.Repaint);
                    Handles.SphereHandleCap(2, ManualEditor.Points[1], Quaternion.identity, Target.GizmoScale * 0.5f, EventType.Repaint);

                    Handles.zTest = CompareFunction.Disabled;
                    if (RaycastHasHit) Handles.DrawLines(new[] { ManualEditor.Points[2], ManualEditor.Points[1], ManualEditor.Points[1], RaycastHit.HitVertex });
                    Handles.zTest = origFunc;
                    break;
                case ManualEditorData.CreateState.LeftFloorSpan:
                    Handles.SphereHandleCap(1, ManualEditor.Points[2], Quaternion.identity, Target.GizmoScale * 0.5f, EventType.Repaint);

                    Handles.zTest = CompareFunction.Disabled;
                    if (RaycastHasHit) Handles.DrawLines(new[] { ManualEditor.Points[2], RaycastHit.HitVertex });
                    Handles.zTest = origFunc;
                    break;
                case ManualEditorData.CreateState.LeftWallSpan:

                    break;
                case ManualEditorData.CreateState.RightFloorSpan:
                    Handles.SphereHandleCap(1, ManualEditor.Points[2], Quaternion.identity, Target.GizmoScale * 0.5f, EventType.Repaint);
                    Handles.SphereHandleCap(2, ManualEditor.Points[1], Quaternion.identity, Target.GizmoScale * 0.5f, EventType.Repaint);
                    Handles.SphereHandleCap(2, ManualEditor.Points[0], Quaternion.identity, Target.GizmoScale * 0.5f, EventType.Repaint);

                    Handles.zTest = CompareFunction.Disabled;
                    if (RaycastHasHit) Handles.DrawLines(new[] { ManualEditor.Points[2], ManualEditor.Points[1],
                        ManualEditor.Points[1], ManualEditor.Points[0], ManualEditor.Points[0], RaycastHit.HitVertex });
                    Handles.zTest = origFunc;
                    break;
                case ManualEditorData.CreateState.RightWallSpan:
                    Handles.SphereHandleCap(1, ManualEditor.Points[2], Quaternion.identity, Target.GizmoScale * 0.5f, EventType.Repaint);
                    Handles.SphereHandleCap(2, ManualEditor.Points[1], Quaternion.identity, Target.GizmoScale * 0.5f, EventType.Repaint);
                    Handles.SphereHandleCap(2, ManualEditor.Points[0], Quaternion.identity, Target.GizmoScale * 0.5f, EventType.Repaint);
                    Handles.SphereHandleCap(2, ManualEditor.Points[3], Quaternion.identity, Target.GizmoScale * 0.5f, EventType.Repaint);

                    Handles.zTest = CompareFunction.Disabled;
                    if (RaycastHasHit) Handles.DrawLines(new[] { ManualEditor.Points[2], ManualEditor.Points[1],
                        ManualEditor.Points[1], ManualEditor.Points[0], ManualEditor.Points[0], ManualEditor.Points[3],
                        ManualEditor.Points[3], RaycastHit.HitVertex });
                    Handles.zTest = origFunc;
                    break;
            }
        }
        else
        {
            switch (ManualEditor.ClickState)
            {
                case ManualEditorData.CreateState.Middle:
                    break;
                case ManualEditorData.CreateState.LeftFloorSpan:
                    Handles.SphereHandleCap(1, ManualEditor.Points[0], Quaternion.identity, Target.GizmoScale * 0.5f, EventType.Repaint);

                    Handles.zTest = CompareFunction.Disabled;
                    if (RaycastHasHit) Handles.DrawLine(ManualEditor.Points[0], RaycastHit.HitVertex);
                    Handles.zTest = origFunc;
                    break;
                case ManualEditorData.CreateState.LeftWallSpan:
                    Handles.SphereHandleCap(1, ManualEditor.Points[0], Quaternion.identity, Target.GizmoScale * 0.5f, EventType.Repaint);
                    Handles.SphereHandleCap(2, ManualEditor.Points[1], Quaternion.identity, Target.GizmoScale * 0.5f, EventType.Repaint);

                    Handles.zTest = CompareFunction.Disabled;
                    if (RaycastHasHit) Handles.DrawLines(new[] {ManualEditor.Points[0], ManualEditor.Points[1], ManualEditor.Points[1], RaycastHit.HitVertex});
                    Handles.zTest = origFunc;
                    break;
                case ManualEditorData.CreateState.RightFloorSpan:
                    Handles.SphereHandleCap(1, ManualEditor.Points[0], Quaternion.identity, Target.GizmoScale * 0.5f, EventType.Repaint);
                    Handles.SphereHandleCap(2, ManualEditor.Points[1], Quaternion.identity, Target.GizmoScale * 0.5f, EventType.Repaint);
                    Handles.SphereHandleCap(3, ManualEditor.Points[2], Quaternion.identity, Target.GizmoScale * 0.5f, EventType.Repaint);

                    Handles.zTest = CompareFunction.Disabled;
                    if (RaycastHasHit)
                        Handles.DrawLines(new[]
                        {
                            ManualEditor.Points[0], ManualEditor.Points[1], ManualEditor.Points[1], ManualEditor.Points[2],
                            ManualEditor.Points[0], RaycastHit.HitVertex
                        });
                    Handles.zTest = origFunc;
                    break;
                case ManualEditorData.CreateState.RightWallSpan:
                    Handles.SphereHandleCap(1, ManualEditor.Points[0], Quaternion.identity, Target.GizmoScale * 0.5f, EventType.Repaint);
                    Handles.SphereHandleCap(2, ManualEditor.Points[1], Quaternion.identity, Target.GizmoScale * 0.5f, EventType.Repaint);
                    Handles.SphereHandleCap(3, ManualEditor.Points[2], Quaternion.identity, Target.GizmoScale * 0.5f, EventType.Repaint);
                    Handles.SphereHandleCap(4, ManualEditor.Points[3], Quaternion.identity, Target.GizmoScale * 0.5f, EventType.Repaint);

                    Handles.zTest = CompareFunction.Disabled;
                    if (RaycastHasHit)
                        Handles.DrawLines(new[]
                        {
                            ManualEditor.Points[0], ManualEditor.Points[1], ManualEditor.Points[1], ManualEditor.Points[2],
                            ManualEditor.Points[0], ManualEditor.Points[3], ManualEditor.Points[3], RaycastHit.HitVertex
                        });
                    Handles.zTest = origFunc;
                    break;
            }
        }
    }

    private void UpdateDragMode()
    {
        Vector3 currentVertex = RaycastHit.HitVertex;
        if (currentVertex == ManualEditor.DragModeLastVertex || ManualEditor.DragState == ManualEditorData.CreateState.LeftWallSpan) return;

        switch (ManualEditor.DragState)
        {
            case ManualEditorData.CreateState.Middle:
                ManualEditor.Points[0] = RaycastHit.HitVertex;
                ManualEditor.DragState = ManualEditorData.CreateState.RightFloorSpan;
                ManualEditor.DragModeLastVertex = currentVertex;
                break;
            case ManualEditorData.CreateState.LeftFloorSpan:
                ManualEditor.Points[1] = RaycastHit.HitVertex;
                ManualEditor.DragState = ManualEditorData.CreateState.Middle;
                ManualEditor.DragModeLastVertex = currentVertex;
                break;
            case ManualEditorData.CreateState.LeftWallSpan:
                ManualEditor.Points[2] = RaycastHit.HitVertex;
                ManualEditor.DragState = ManualEditorData.CreateState.LeftFloorSpan;
                ManualEditor.DragModeLastVertex = currentVertex;
                break;
            case ManualEditorData.CreateState.RightFloorSpan:
                ManualEditor.Points[3] = RaycastHit.HitVertex;
                ManualEditor.DragState = ManualEditorData.CreateState.RightWallSpan;
                ManualEditor.DragModeLastVertex = currentVertex;
                break;
            case ManualEditorData.CreateState.RightWallSpan:
                RecordClickUndo();
                ManualEditor.Points[4] = RaycastHit.HitVertex;
                ManualEditor.DragState = ManualEditorData.CreateState.LeftWallSpan;
                ManualEditor.DragModeLastVertex = currentVertex;
                TrmEditor.TrackSection section = new TrmEditor.TrackSection(_autoCentre, ManualEditor.Points[0], ManualEditor.Points[1], ManualEditor.Points[2], ManualEditor.Points[3], ManualEditor.Points[4], Target);
                break;
        }
    }

    #endregion

    [DrawGizmo(GizmoType.InSelectionHierarchy | GizmoType.NotInSelectionHierarchy)]
    static void OnScene(TrmEditor editor, GizmoType gizmoType)
    {
        if (!Selection.Contains(editor.gameObject)) editor.OnDrawHandles(editor, false);
    }

    void OnSceneGUI()
    {
        UpdateSceneView();

        Target.OnDrawHandles(this, CreateMode == ECreateMode.Vertices);

        if (CreateMode != ECreateMode.Vertices)
        {
            UpdateRaycastData();
            DrawHighlightedVert();
            DrawVertTrail();

            if (ManualEditor.Mode == EInteractionMode.Drag) UpdateDragMode();
            if (Event.current.type == EventType.MouseDown) OnClick(Event.current.button);
        }

        GUI.Window(0, new Rect(10, 20, 350, 400), DrawWindow, "TRM Editor");
    }

    void DrawWindow(int windowID)
    {
        if (InspectorTemplates.DocumentationHeaderCloseButton("TRM Editor", "")) Selection.activeObject = null;

        GUI.color = new Color(0.7f, 1.0f, 0.7f, 1.0f);
        if (GUILayout.Button("Export mesh to TRM", EditorStyles.toolbarButton))
        {
            string exportFile = EditorUtility.SaveFilePanel("Save TRM", Application.dataPath, "", "trm");
            Target.TrmExport(exportFile);
        }
        GUI.color = Color.white;
        GUILayout.Space(10);

        InspectorTemplates.DocumentationHeader("Select Edit Mode", "");
        CreateMode = (ECreateMode) GUILayout.Toolbar((int) CreateMode, Enum.GetNames(typeof(ECreateMode)), EditorStyles.toolbarButton);
        if (CreateMode == ECreateMode.Nexts)
        {
            GUILayout.Label("Click to select a section.");
            GUILayout.Label("Z + click to set the clicked section as the selected sections next reference.");
            GUILayout.Label("X + click to clear a sections next reference");

        }
        else
        {
            InspectorTemplates.DocumentationHeader("Select Interaction Mode", "");
            ManualEditor.Mode = (EInteractionMode) GUILayout.Toolbar((int) ManualEditor.Mode, Enum.GetNames(typeof(EInteractionMode)), EditorStyles.toolbarButton);

            InspectorTemplates.DocumentationHeader("General Settings", "");
            Target.DoNotHookNextSection = GUILayout.Toggle(Target.DoNotHookNextSection, "Don't connect next section");
            _autoCentre = GUILayout.Toggle(_autoCentre, "Auto centre middle vert");

            if (ManualEditor.Mode == EInteractionMode.Drag)
            {
                EditorGUILayout.HelpBox("This mode only works when you have 5 points to setup. If you have more then you'll want to use the click mode.", MessageType.Info);
                if (ManualEditor.DragState == ManualEditorData.CreateState.LeftWallSpan)
                {
                    GUILayout.Label("Click on the left wall and begin dragging over the profile\n for the section you want. Undo to reset state.");
                }
                else GUILayout.Label("Continue dragging your mouse.");

                if (GUILayout.Button("Reset Drag State", EditorStyles.toolbarButton)) ManualEditor.DragState = ManualEditorData.CreateState.LeftWallSpan;
            }
            else
            {
                GUILayout.Label("Current state (only click these if you need to backtrack):");
                ManualEditor.ClickState = (ManualEditorData.CreateState) GUILayout.Toolbar((int) ManualEditor.ClickState, ManualEditor.ActionNames, EditorStyles.toolbarButton);

                GUILayout.Space(10);
                GUILayout.Label("Required Action: ");
                switch (ManualEditor.ClickState)
                {
                    case ManualEditorData.CreateState.Middle:
                        GUILayout.Label("Click on the middle vertex of the current section.");
                        break;
                    case ManualEditorData.CreateState.LeftFloorSpan:
                        GUILayout.Label("Click on the left side of the track floor.");
                        break;
                    case ManualEditorData.CreateState.LeftWallSpan:
                        GUILayout.Label("Click on the top left wall. Click the floor again to set no wall for this side.");
                        break;
                    case ManualEditorData.CreateState.RightFloorSpan:
                        GUILayout.Label("Click on the right side of the track floor.");
                        break;
                    case ManualEditorData.CreateState.RightWallSpan:
                        GUILayout.Label("Click on the top right wall. Click the floor again to set no wall for this side.");
                        break;
                }
            }
        }

    }

    #region Utility Functions
    /// <summary>
    /// Finds which point in points is closest to current.
    /// </summary>
    private static Vector3 FindClosetPoint(Transform t, Vector3[] points, Vector3 current)
    {
        float distance = Mathf.Infinity;
        Vector3 output = current;

        for (int i = 0; i < points.Length; ++i)
        {
            float newDistance = Vector3.Distance(t.TransformPoint(points[i]), current);
            if (!(newDistance < distance)) continue;

            distance = newDistance;
            output = t.TransformPoint(points[i]);
        }
        return output;
    }

    /// <summary>
    /// Finds the closest section to a given position.
    /// </summary>
    /// <returns></returns>
    private TrmEditor.TrackSection FindClosestSection(Vector3 position)
    {
        float distance = Mathf.Infinity;
        TrmEditor.TrackSection section = null;

        for (int i = 0; i < Target.TrackData.Count; ++i)
        {
            float newDistance = Vector3.Distance(position, Target.TrackData[i].Position);
            if (!(newDistance < distance)) continue;

            distance = newDistance;
            section = Target.TrackData[i];
        }
        return section;
    }

    #endregion
}
