using System;
using System.Collections.Generic;
using BallisticModding.Formats;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
public class TrmEditor : MonoBehaviour
{

    [System.Serializable]
    public class TrackSection
    {
        public enum Side
        {
            Left,
            Right
        }

        public TrackSection(bool autoCentre, Vector3 point, Vector3 leftFloor, Vector3 leftWall, Vector3 rightFloor, Vector3 rightWall, TrmEditor editor)
        {
            editor.TrackData.Add(this);

            // position and base rotation
            Position = autoCentre ? Vector3.Lerp(leftFloor, rightFloor, 0.5f) : point;

            TrackLeft = new SectonData();
            TrackRight = new SectonData();
            TrackLeft.FloorEnd = leftFloor;
            TrackLeft.WallEnd = leftWall;
            TrackRight.FloorEnd = rightFloor;
            TrackRight.WallEnd = rightWall;

            if (leftWall == leftFloor) LeftWall = false;
            if (rightWall == rightFloor) RightWall = false;

            if (editor.TrackData.Count > 1 && !editor.DoNotHookNextSection)
            {
                TrackSection previousSection = editor.TrackData[editor.TrackData.Count - 2];
                PreviousSection = editor.TrackData.Count - 2;
                previousSection.NextSection = editor.TrackData.Count - 1;


                if (leftWall == leftFloor) previousSection.LeftWall = false;
                if (rightWall == rightFloor) previousSection.RightWall = false;
            }

            editor.DoNotHookNextSection = false;
            editor.RebuildFloor();
            editor.RebuildWall();
        }

        public Vector3 Position;
        public SectonData TrackLeft;
        public SectonData TrackRight;
        public int NextSection = -1;
        public int PreviousSection = -1;
        public int JunctionSection = -1;
        public int ExitSection = -1;

        public Side JunctionSide;
        public Side ExitSide;

        public bool Connect = true;
        public bool LeftWall = true;
        public bool RightWall = true;

        public SectonData GetTrackSide(Side side)
        {
            return side == Side.Left ? TrackLeft : TrackRight;
        }
    }

    [System.Serializable]
    public struct SectonData
    {
        public Vector3 FloorEnd;
        public Vector3 WallEnd;
    }

    public List<TrackSection> TrackData = new List<TrackSection>();
    [HideInInspector] public bool DoNotHookNextSection;

    private GameObject _floorObject;
    private GameObject _wallObject;
    private MeshFilter _floorFilter;
    private MeshFilter _wallFilter;

    private MeshRenderer _floorRenderer;
    private MeshRenderer _wallRenderer;
    public MeshRenderer FloorRenderer { get { return _floorRenderer; } }
    public MeshRenderer WallRenderer { get { return _wallRenderer; } }

    public MeshFilter[] AutomatedMeshReferences;

    [HideInInspector] public bool ConnectEnd;
    [HideInInspector] public int ConnectEndIndex;
    [HideInInspector] public float GizmoScale = 0.1f;

    private readonly List<Vector3> _shapeLines = new List<Vector3>();
    private readonly List<Vector3> _junctionLines = new List<Vector3>();
    private readonly List<Vector3> _exitLines = new List<Vector3>();

    private void Awake()
    {
        name = "TRM Editor";
        _floorObject = new GameObject("Floor Object");
        _floorObject.transform.SetParent(transform);
        _floorObject.transform.position = Vector3.zero;
        _floorObject.transform.rotation = Quaternion.identity;

        _wallObject = new GameObject("Wall Object");
        _wallObject.transform.SetParent(transform);
        _wallObject.transform.position = Vector3.zero;
        _wallObject.transform.rotation = Quaternion.identity;

        _floorFilter = _floorObject.AddComponent<MeshFilter>();
        _wallFilter = _wallObject.AddComponent<MeshFilter>();
        _floorRenderer = _floorObject.AddComponent<MeshRenderer>();
        _wallRenderer = _wallObject.AddComponent<MeshRenderer>();

        Material newMat = new Material(Shader.Find("Standard"));
        newMat.SetColor("_Color", Color.grey);

        _floorRenderer.material = newMat;
        _wallRenderer.material = newMat;
    }

    public void TrmExport(string file)
    {
        TRM trm = new TRM
        {
            floorMesh = _floorFilter.mesh,
            wallMesh = _wallFilter.mesh
        };
        CustomContent trmContent = trm.Prepare(trm.floorMesh, trm.wallMesh, null);
        trm.WriteFile(trmContent, file);
    }

    [ContextMenu("Rebuild Mesh")]
    public void RebuildMesh()
    {
        RebuildFloor();
        RebuildWall();
    }

    public void RebuildFloor()
    {
        List<Vector3> verts = new List<Vector3>();
        List<int> tris = new List<int>();

        int tri = 0;
        for (int i = 0; i < TrackData.Count; ++i)
        {
            if (TrackData[i].NextSection != -1 && TrackData[i].Connect)
            {
                if (TrackData[i].JunctionSection != -1) AppendJunctionToFloor(TrackData[i].JunctionSide, TrackData[i], ref verts, ref tris, ref tri);

                Vector3 lefFloorCurrent = TrackData[i].TrackLeft.FloorEnd;
                Vector3 rightFloorCurrent = TrackData[i].TrackRight.FloorEnd;
                Vector3 centreCurrent = TrackData[i].Position;

                Vector3 lefFloorNext = TrackData[TrackData[i].NextSection].TrackLeft.FloorEnd;
                Vector3 rightFloorNext = TrackData[TrackData[i].NextSection].TrackRight.FloorEnd;
                Vector3 centreNext = TrackData[TrackData[i].NextSection].Position;

                // right tile
                verts.AddRange(new[] {centreCurrent, centreNext, rightFloorNext, rightFloorCurrent});
                tris.AddRange(new int[6] {tri + 3, tri + 0, tri + 1, tri + 3, tri + 1, tri + 2});
                tri += 4;

                // left tile
                verts.AddRange(new[] {lefFloorCurrent, lefFloorNext, centreNext, centreCurrent});
                tris.AddRange(new int[6] {tri + 3, tri + 0, tri + 1, tri + 3, tri + 1, tri + 2});
                tri += 4;
            }

            if (TrackData[i].ExitSection != -1) AppendExitToFloor(TrackData[i].ExitSide, TrackData[i], ref verts, ref tris, ref tri);
        }

        Mesh m = new Mesh();
        m.vertices = verts.ToArray();
        m.triangles = tris.ToArray();

        m.RecalculateNormals();
        _floorFilter.mesh = m;
    }

    public void AppendJunctionToFloor(TrackSection.Side side, TrackSection section, ref List<Vector3> verts, ref List<int> tris, ref int tri)
    {
        Vector3 currentLeft = section.TrackLeft.FloorEnd;
        Vector3 currentCentre = section.Position;
        Vector3 currentRight = section.TrackRight.FloorEnd;

        Vector3[] junctionSides = new Vector3[3];
        TrackSection nextSection = TrackData[section.JunctionSection];
        junctionSides[0] = nextSection.GetTrackSide(side).FloorEnd;

        nextSection = TrackData[nextSection.NextSection];
        junctionSides[1] = nextSection.GetTrackSide(side).FloorEnd;

        nextSection = TrackData[nextSection.NextSection];
        junctionSides[2] = nextSection.GetTrackSide(side).FloorEnd;

        // if this is on the right side then flip the vectors
        if (side == TrackSection.Side.Right) Array.Reverse(junctionSides);

        // right tile
        verts.AddRange(new[] { junctionSides[1], currentCentre, currentRight, junctionSides[2] });
        tris.AddRange(new int[6] { tri + 3, tri + 0, tri + 1, tri + 3, tri + 1, tri + 2 });
        tri += 4;

        // left tile
        verts.AddRange(new [] {junctionSides[0], currentLeft, currentCentre, junctionSides[1]});
        tris.AddRange(new int[6] { tri + 3, tri + 0, tri + 1, tri + 3, tri + 1, tri + 2 });
        tri += 4;

    }

    public void AppendExitToFloor(TrackSection.Side side, TrackSection section, ref List<Vector3> verts, ref List<int> tris, ref int tri)
    {
        Vector3 currentLeft = section.TrackLeft.FloorEnd;
        Vector3 currentCentre = section.Position;
        Vector3 currentRight = section.TrackRight.FloorEnd;

        Vector3[] junctionSides = new Vector3[3];
        TrackSection nextSection = TrackData[section.ExitSection];
        junctionSides[0] = nextSection.GetTrackSide(side).FloorEnd;

        nextSection = TrackData[nextSection.NextSection];
        junctionSides[1] = nextSection.GetTrackSide(side).FloorEnd;

        nextSection = TrackData[nextSection.NextSection];
        junctionSides[2] = nextSection.GetTrackSide(side).FloorEnd;

        if (side == TrackSection.Side.Left) Array.Reverse(junctionSides);

        // right tile
        verts.AddRange(new[] { currentRight, currentCentre, junctionSides[1], junctionSides[2] });
        tris.AddRange(new int[6] { tri + 0, tri + 1, tri + 2, tri + 0, tri + 2, tri + 3 });
        tri += 4;

        // left tile
        verts.AddRange(new[] { currentCentre, currentLeft, junctionSides[0], junctionSides[1] });
        tris.AddRange(new int[6] { tri + 0, tri + 1, tri + 2, tri + 0, tri + 2, tri + 3 });
        tri += 4;
    }

    public void RebuildWall()
    {
        List<Vector3> verts = new List<Vector3>();
        List<int> tris = new List<int>();

        int tri = 0;
        for (int i = 0; i < TrackData.Count; ++i)
        {
            if (TrackData[i].NextSection != -1 && TrackData[i].Connect)
            {
                if (TrackData[i].JunctionSection != -1) AppendJunctionToWall(TrackData[i].JunctionSide, TrackData[i], ref verts, ref tris, ref tri);

                Vector3 leftWallCurrent = TrackData[i].TrackLeft.WallEnd;
                Vector3 rightWallCurrent = TrackData[i].TrackRight.WallEnd;
                Vector3 leftFloorCurrent = TrackData[i].TrackLeft.FloorEnd;
                Vector3 rightFloorCurrent = TrackData[i].TrackRight.FloorEnd;

                Vector3 leftWallNext = TrackData[TrackData[i].NextSection].TrackLeft.WallEnd;
                Vector3 rightWallNext = TrackData[TrackData[i].NextSection].TrackRight.WallEnd;
                Vector3 leftFloorNext = TrackData[TrackData[i].NextSection].TrackLeft.FloorEnd;
                Vector3 rightFloorNext = TrackData[TrackData[i].NextSection].TrackRight.FloorEnd;

                if (TrackData[i].RightWall)
                {
                    // right tile
                    verts.AddRange(new[] {rightFloorNext, rightWallNext, rightWallCurrent, rightFloorCurrent});
                    tris.AddRange(new int[6] {tri + 2, tri + 3, tri + 0, tri + 2, tri + 0, tri + 1});
                    tri += 4;
                }

                if (TrackData[i].LeftWall)
                {
                    // left tile
                    verts.AddRange(new[] {leftFloorCurrent, leftWallCurrent, leftWallNext, leftFloorNext});
                    tris.AddRange(new int[6] {tri + 0, tri + 1, tri + 2, tri + 0, tri + 2, tri + 3});
                    tri += 4;
                }
            }
            if (TrackData[i].ExitSection != -1) AppendExitToWall(TrackData[i].ExitSide, TrackData[i], ref verts, ref tris, ref tri);
        }

        Mesh m = new Mesh();
        m.vertices = verts.ToArray();
        m.triangles = tris.ToArray();

        m.RecalculateNormals();
        _wallFilter.mesh = m;
    }

    public void AppendJunctionToWall(TrackSection.Side side, TrackSection section, ref List<Vector3> verts, ref List<int> tris, ref int tri)
    {
        Vector3 currentLeftBottom = section.TrackLeft.FloorEnd;
        Vector3 currentLeftTop = section.TrackLeft.WallEnd;
        Vector3 currentRightBottom = section.TrackRight.FloorEnd;
        Vector3 currentRightTop = section.TrackRight.WallEnd;

        Vector3[] junctionIndicies = new Vector3[4];

        TrackSection nextSection = TrackData[section.JunctionSection];
        junctionIndicies[0] = nextSection.GetTrackSide(side).FloorEnd;
        junctionIndicies[1] = nextSection.GetTrackSide(side).WallEnd;

        nextSection = TrackData[TrackData[nextSection.NextSection].NextSection];
        junctionIndicies[2] = nextSection.GetTrackSide(side).FloorEnd;
        junctionIndicies[3] = nextSection.GetTrackSide(side).WallEnd;

        // if this is on the right side then flip the vectors
        if (side == TrackSection.Side.Right)
        {
            Vector3[] oldVecs = new Vector3[4];
            Array.Copy(junctionIndicies, oldVecs, 4);

            junctionIndicies[0] = oldVecs[2];
            junctionIndicies[1] = oldVecs[3];
            junctionIndicies[2] = oldVecs[0];
            junctionIndicies[3] = oldVecs[1];
        }

        // right tile
        verts.AddRange(new[] { currentRightBottom, currentRightTop, junctionIndicies[3], junctionIndicies[2]});
        tris.AddRange(new int[6] { tri + 2, tri + 3, tri + 0, tri + 2, tri + 0, tri + 1 });
        tri += 4;

        // left tile
        verts.AddRange(new[] { junctionIndicies[0], junctionIndicies[1], currentLeftTop, currentLeftBottom });
        tris.AddRange(new int[6] { tri + 0, tri + 1, tri + 2, tri + 0, tri + 2, tri + 3 });
        tri += 4;
    }

    public void AppendExitToWall(TrackSection.Side side, TrackSection section, ref List<Vector3> verts, ref List<int> tris, ref int tri)
    {
        Vector3 currentLeftBottom = section.TrackLeft.FloorEnd;
        Vector3 currentLeftTop = section.TrackLeft.WallEnd;
        Vector3 currentRightBottom = section.TrackRight.FloorEnd;
        Vector3 currentRightTop = section.TrackRight.WallEnd;

        Vector3[] junctionIndicies = new Vector3[4];

        TrackSection nextSection = TrackData[section.ExitSection];
        junctionIndicies[0] = nextSection.GetTrackSide(side).FloorEnd;
        junctionIndicies[1] = nextSection.GetTrackSide(side).WallEnd;

        nextSection = TrackData[TrackData[nextSection.NextSection].NextSection];
        junctionIndicies[2] = nextSection.GetTrackSide(side).FloorEnd;
        junctionIndicies[3] = nextSection.GetTrackSide(side).WallEnd;

        // if this is on the right side then flip the vectors
        if (side == TrackSection.Side.Left)
        {
            Vector3[] oldVecs = new Vector3[4];
            Array.Copy(junctionIndicies, oldVecs, 4);

            junctionIndicies[0] = oldVecs[2];
            junctionIndicies[1] = oldVecs[3];
            junctionIndicies[2] = oldVecs[0];
            junctionIndicies[3] = oldVecs[1];
        }

        // right tile
        verts.AddRange(new[] { junctionIndicies[2], junctionIndicies[3], currentRightTop, currentRightBottom});
        tris.AddRange(new int[6] { tri + 2, tri + 3, tri + 0, tri + 2, tri + 0, tri + 1 });
        tri += 4;

        // left tile
        verts.AddRange(new[] { currentLeftBottom, currentLeftTop, junctionIndicies[1], junctionIndicies[0]});
        tris.AddRange(new int[6] { tri + 0, tri + 1, tri + 2, tri + 0, tri + 2, tri + 3 });
        tri += 4;
    }

    #if UNITY_EDITOR

    private int _selectedSection = -1;
    private int _selectedPoint = -1;

    [SerializeField]
    private Vector3 _selectedPosition;

    public void OnDrawHandles(Object undoTarget, bool interactive)
    {
        if (!Camera.current) return;

        int len = TrackData.Count;
        Vector3 camPos = Camera.current.transform.position;
        const float lodThreshold = 50.0f;

        _shapeLines.Clear();
        _junctionLines.Clear();
        _exitLines.Clear();

        Handles.color = Color.green;
        Handles.zTest = interactive ? CompareFunction.Always : CompareFunction.LessEqual;
        for (int i = 0; i < len; ++i)
        {
            TrackSection section = TrackData[i];

            float distance = Vector3.Distance(section.Position, camPos);
            Vector3 screenPoint = Camera.current.WorldToViewportPoint(section.Position);
            if (screenPoint.z < 0.0f) continue;

            _shapeLines.Add(section.TrackLeft.WallEnd);
            _shapeLines.Add(section.TrackLeft.FloorEnd);

            _shapeLines.Add(section.TrackLeft.FloorEnd);
            _shapeLines.Add(section.Position);

            _shapeLines.Add(section.Position);
            _shapeLines.Add(section.TrackRight.FloorEnd);

            _shapeLines.Add(section.TrackRight.FloorEnd);
            _shapeLines.Add(section.TrackRight.WallEnd);

            if (distance <= lodThreshold)
            {
                float size = Mathf.Lerp(0.05f + distance * 0.001f, 0.0f, distance / (lodThreshold * 2.0f));
                section.Position = DoPointHandle(section.Position, size, i, 0, interactive, undoTarget);
                section.TrackLeft.FloorEnd = DoPointHandle(section.TrackLeft.FloorEnd, size, i, 2, interactive, undoTarget);
                section.TrackLeft.WallEnd = DoPointHandle(section.TrackLeft.WallEnd, size, i, 1, interactive, undoTarget);
                section.TrackRight.FloorEnd = DoPointHandle(section.TrackRight.FloorEnd, size, i, 3, interactive, undoTarget);
                section.TrackRight.WallEnd = DoPointHandle(section.TrackRight.WallEnd, size, i, 4, interactive, undoTarget);
            }

            if (section.NextSection != -1)
            {
                _shapeLines.Add(section.Position);
                _shapeLines.Add(TrackData[TrackData[i].NextSection].Position);

                _shapeLines.Add(section.TrackLeft.FloorEnd);
                _shapeLines.Add(TrackData[TrackData[i].NextSection].TrackLeft.FloorEnd);

                if (TrackData[i].LeftWall && TrackData[TrackData[i].NextSection].LeftWall)
                {
                    _shapeLines.Add(section.TrackLeft.WallEnd);
                    _shapeLines.Add(TrackData[TrackData[i].NextSection].TrackLeft.WallEnd);
                }

                _shapeLines.Add(section.TrackRight.FloorEnd);
                _shapeLines.Add(TrackData[TrackData[i].NextSection].TrackRight.FloorEnd);

                if (TrackData[i].RightWall && TrackData[TrackData[i].NextSection].RightWall)
                {
                    _shapeLines.Add(section.TrackRight.WallEnd);
                    _shapeLines.Add(TrackData[TrackData[i].NextSection].TrackRight.WallEnd);
                }
            }

            if (section.JunctionSection != -1)
            {
                TrackSection refSection = TrackData[section.JunctionSection];
                _junctionLines.Add(section.Position);
                _junctionLines.Add(refSection.GetTrackSide(refSection.ExitSide).FloorEnd);
            }

            if (section.ExitSection != -1)
            {
                TrackSection refSection = TrackData[section.ExitSection];
                _exitLines.Add(section.Position);
                _exitLines.Add(refSection.GetTrackSide(section.ExitSide).FloorEnd);
            }
        }
        Handles.color = Color.red;
        Handles.DrawLines(_shapeLines.ToArray());

        Handles.color = Color.blue;
        Handles.DrawLines(_junctionLines.ToArray());

        Handles.color = Color.yellow;
        Handles.DrawLines(_exitLines.ToArray());

        Handles.color = Color.white;
    }

    private Vector3 DoPointHandle(Vector3 position, float size, int sectionIndex, int pointIndex, bool interactable, Object undoTarget = null)
    {
        if (interactable)
        {
            if (Handles.Button(position, Quaternion.LookRotation((position - Camera.current.transform.position).normalized, Camera.current.transform.up), size, size * 1.2f, Handles.RectangleHandleCap))
            {
                _selectedSection = sectionIndex;
                _selectedPoint = pointIndex;
                _selectedPosition = position;
            }

            if (sectionIndex == _selectedSection && pointIndex == _selectedPoint)
            {
                EditorGUI.BeginChangeCheck();
                Vector3 newPosition = Handles.DoPositionHandle(_selectedPosition, Quaternion.identity);

                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(undoTarget, "Move TRM Vertex Position");
                    Undo.RecordObject(this, "Move TRM Vertex Position");
                    position = newPosition;
                    _selectedPosition = position;

                    RebuildMesh();
                }
            }
            return position;
        }
        else
        {
            Handles.DotHandleCap(0, position, Quaternion.identity, size, EventType.Repaint);
            return position;
        }
    }

    #endif
}
