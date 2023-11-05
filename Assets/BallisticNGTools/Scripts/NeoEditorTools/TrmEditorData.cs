#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Source.NeoEditorTools
{
    [AddComponentMenu("Neognosis/Editors/Trm Vertex Editor")]
    [ExecuteInEditMode]
    public class TrmEditorData : MonoBehaviour
    {
        /// <summary>
        /// The track floor mesh filter.
        /// </summary>
        public MeshFilter TrackFloor;

        /// <summary>
        /// The track wall mesh filter.
        /// </summary>
        public MeshFilter TrackWall;

        /// <summary>
        /// Whether the editor has started. 
        /// </summary>
        public bool HasBegun;

        public bool SmoothSelection;

        public float SmoothSelectionDistance = 10.0f;

        /// <summary>
        /// The vertices of the trm floor.
        /// </summary>
        [SerializeField]
        private Vector3[] _floorVertices = new Vector3[0];

        /// <summary>
        /// The normals of the trm floor.
        /// </summary>
        [SerializeField]
        private Vector3[] _floorNormals = new Vector3[0];

        /// <summary>
        /// The vertices of the trm wall.
        /// </summary>
        [SerializeField]
        private Vector3[] _wallVertices = new Vector3[0];

        /// <summary>
        /// The normals of the trm wall.
        /// </summary>
        [SerializeField]
        private Vector3[] _wallNormals = new Vector3[0];

        /// <summary>
        /// The shared vertex definitions.
        /// </summary>
        public List<TrmVertexLinker> SharedVertices = new List<TrmVertexLinker>();

        /// <summary>
        /// The current vertex selection.
        /// </summary>
        public List<TrmVertex> Vertices = new List<TrmVertex>();

        /// <summary>
        /// Returns whether the shared vertices list has a specific vertex.
        /// </summary>
        public bool HasVertex(Vector3 vertex)
        {
            return SharedVertices.Any(vert => vert.Vertex == vertex);
        }

        /// <summary>
        /// Returns or creates a new shared vertex.
        /// </summary>
        public TrmVertexLinker GetSharedVertex(Vector3 vertex, Vector3 normal)
        {
            foreach (TrmVertexLinker linker in SharedVertices)
            {
                if (linker.Vertex == vertex) return linker;
            }

            TrmVertexLinker newLinker = new TrmVertexLinker(vertex, normal);
            SharedVertices.Add(newLinker);
            return newLinker;
        }
        
        public void CreateVertexData()
        {
            SharedVertices.Clear();

            /*---Populate Arrays---*/
            _floorVertices = TrackFloor.sharedMesh.vertices;
            _wallVertices = TrackWall.sharedMesh.vertices;

            TrackFloor.sharedMesh.RecalculateNormals();
            TrackWall.sharedMesh.RecalculateNormals();

            _floorNormals = TrackFloor.sharedMesh.normals;
            _wallNormals = TrackWall.sharedMesh.normals;

            /*---Floor Verts---*/
            for (int i = 0; i < _floorVertices.Length; ++i)
            {
                Vector3 floor = _floorVertices[i];

                TrmVertexLinker linker = GetSharedVertex(floor, _floorNormals[i]);
                linker.Links.Add(new TrmVertexLink(0, i));
            }

            /*---Wall Verts---*/
            for (int i = 0; i < _wallVertices.Length; ++i)
            {
                Vector3 wall = _wallVertices[i];

                TrmVertexLinker linker = GetSharedVertex(wall, _wallNormals[i]);
                linker.Links.Add(new TrmVertexLink(1, i));
            }
            
            CreateLinkerObject();
        }

        public void CreateLinkerObject()
        {
            TrmVertex[] verts = FindObjectsOfType<TrmVertex>();
            foreach (TrmVertex vert in verts) DestroyImmediate(vert); 
            Vertices.Clear();

            for (int i = 0; i < SharedVertices.Count; ++i)
            {
                GameObject linkerObj = new GameObject("Linker Vert");
                linkerObj.transform.SetParent(transform);
                linkerObj.transform.position = SharedVertices[i].Vertex;
                
                TrmVertex newVertex = linkerObj.AddComponent<TrmVertex>();
                newVertex.Linker = SharedVertices[i];
                newVertex.Data = this;
                
                Vertices.Add(newVertex);
            }
        }

        public void UpdateTrack()
        {
            /*---Apply verts to arrays---*/
            foreach (TrmVertexLinker linker in SharedVertices)
            {
                foreach (TrmVertexLink link in linker.Links)
                {
                    if (link.Array == 0) _floorVertices[link.Index] = linker.Vertex;
                    if (link.Array == 1) _wallVertices[link.Index] = linker.Vertex;
                }
            }

            /*---Update Mesh---*/
            TrackFloor.sharedMesh.vertices = _floorVertices;
            TrackWall.sharedMesh.vertices = _wallVertices;
        }

        private void Update()
        {
            if (TrackFloor)
            {
                Transform t = TrackFloor.transform;
                t.position = Vector3.zero;
                t.rotation = Quaternion.identity;
                t.localScale = Vector3.one;
            }

            if (TrackWall)
            {
                Transform t = TrackWall.transform;
                t.position = Vector3.zero;
                t.rotation = Quaternion.identity;
                t.localScale = Vector3.one;
            }
            
            if (TrmVertex.AnyDirty)
            {
                Undo.RecordObject(this, "Updated Vertex");
                
                TrmVertex.AnyDirty = false;
                UpdateTrack();
            }
        }

        private void OnDestroy()
        {
            TrmVertex[] verts = FindObjectsOfType<TrmVertex>();
            foreach (TrmVertex vert in verts) DestroyImmediate(vert); 
        }

        [System.Serializable]
        public class TrmVertexLinker
        {
            public TrmVertexLinker(Vector3 vertex, Vector3 normal)
            {
                Vertex = vertex;
            }
            
            public Vector3 Vertex;
            public Vector3 Normal;
            public List<TrmVertexLink> Links = new List<TrmVertexLink>();
        }

        [System.Serializable]
        public class TrmVertexLink
        {
            public TrmVertexLink(int array, int index)
            {
                Array = array;
                Index = index;
            }

            /// <summary>
            /// The array that the vertex is linked in.
            /// 0 - Floor
            /// 1 - Wall
            /// </summary>
            public int Array;

            /// <summary>
            /// The index of the vertex in the array.
            /// </summary>
            public int Index;
        }
    }
}
#endif