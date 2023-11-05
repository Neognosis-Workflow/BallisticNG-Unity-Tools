using System;
using UnityEditor;
using UnityEngine;

namespace Source.NeoEditorTools.Editor.Inspectors
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(TrmVertex))]
    public class TrmVertexInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            /*
            TrmVertex t = (TrmVertex)target;
            
            NeoToolEditor.BeginTitledSection("Settings");
            {
                t.Data.SmoothSelection = GUILayout.Toggle(t.Data.SmoothSelection, "Smooth Selections");
                t.Data.SmoothSelectionDistance = EditorGUILayout.FloatField("Smooth Selection Distance", t.Data.SmoothSelectionDistance);
            }
            NeoToolEditor.EndTitledSection();
            */
        }

        private void OnSceneGUI()
        {
            /*
            TrmVertex t = (TrmVertex)target;
        
            Event e = Event.current;
            if (e.type == EventType.MouseDown && e.button == 0 && Selection.Contains(t.gameObject))
            {
                t.SmoothDragging = true;
                t.UpdateSmoothCache();
            }

            if (e.type == EventType.MouseUp && e.button == 0 && t.SmoothDragging) t.SmoothDragging = false;
            */
        }
    }
}