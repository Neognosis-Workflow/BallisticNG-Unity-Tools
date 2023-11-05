#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Source.NeoEditorTools
{
    [ExecuteInEditMode]
    public class TrmVertex : MonoBehaviour
    {
        public static bool AnyDirty = false;

        [HideInInspector]
        public TrmEditorData Data;
        
        [HideInInspector]
        public TrmEditorData.TrmVertexLinker Linker;

        [HideInInspector]
        private Vector3 _prevPos;

        [HideInInspector]
        public bool IgnoreSmoothingNextUpdate;

        private Vector3 _smoothOrigin;
        private List<TrmVertex> _smoothCache = new List<TrmVertex>();
        private readonly List<Vector3> _smoothCacheOrigins = new List<Vector3>();
        private readonly List<float> _smoothUpdateFactors = new List<float>();
        
        public bool SmoothDragging;

        private void Awake()
        {
            _prevPos = transform.position;
        }

        private void Update()
        {
            if (_prevPos != transform.position)
            {
                Vector3 moveDIff = transform.position - _prevPos;
                Vector3 moveAbsoluteDiff = transform.position - _smoothOrigin;
                
                _prevPos = transform.position;

                Linker.Vertex = _prevPos;
                AnyDirty = true;

                if (IgnoreSmoothingNextUpdate) IgnoreSmoothingNextUpdate = false;
                else if (Data.SmoothSelection)
                {
                    int i = 0;
                    foreach (TrmVertex vertex in _smoothCache)
                    {
                        ApplySmoothMove(vertex, _smoothCacheOrigins[i], _smoothUpdateFactors[i], moveAbsoluteDiff);
                        ++i;
                    }
                }
            }
        }
        
        private void OnDrawGizmos()
        {
            if (Selection.Contains(gameObject)) return;
            
            Gizmos.DrawIcon(transform.position, "d_PreMatQuad", false, Color.black);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.DrawIcon(transform.position, "d_PreMatQuad", false, Color.cyan);
            
            if (Data.SmoothSelection)
            {
                Handles.CircleHandleCap(0, transform.position, Camera.current.transform.rotation, Data.SmoothSelectionDistance, EventType.Repaint);
            }
        }

        public void UpdateSmoothCache()
        {
            if (!Data.SmoothSelection) return;
            
            _smoothOrigin = transform.position;
            _smoothCache = Data.Vertices.Where(v => !Selection.Contains(v.gameObject) 
                                            && Vector3.Distance(v.transform.position, transform.position) <= Data.SmoothSelectionDistance).ToList();
            
            _smoothUpdateFactors.Clear();
            for (int i = 0; i < _smoothCache.Count; ++i)
            {
                float distance = Vector3.Distance(_smoothCache[i].transform.position, transform.position) - 1.0f;
                if (distance < 0.0f) distance = 0.0f;
                _smoothUpdateFactors.Add(1.0f - (distance / Data.SmoothSelectionDistance));
            }
            
            _smoothCacheOrigins.Clear();
            for (int i = 0; i < _smoothCache.Count; ++i) _smoothCacheOrigins.Add(_smoothCache[i].transform.position);
        }

        public void ApplySmoothMove(TrmVertex vert, Vector3 origin, float factor, Vector3 diff)
        {
            vert.IgnoreSmoothingNextUpdate = true;
            vert.transform.position = origin + (diff * factor);
        }
    }
}
#endif