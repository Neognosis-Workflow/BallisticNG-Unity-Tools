using BallisticModding.Formats;
using UnityEditor;
using UnityEngine;

namespace Source.NeoEditorTools.Editor.Inspectors
{
    [CustomEditor(typeof(TrmEditorData))]
    public class TrmEditorInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            TrmEditorData t = (TrmEditorData) target;

            EditorGUILayout.HelpBox("This is a prototype tool and no where near finished!", MessageType.Info);
            
            NeoToolEditor.BeginTitledSection("Settings");
            {
                t.TrackFloor = (MeshFilter) EditorGUILayout.ObjectField("Floor Mesh", t.TrackFloor, typeof(MeshFilter), true);
                t.TrackWall = (MeshFilter) EditorGUILayout.ObjectField("Wall Mesh", t.TrackWall, typeof(MeshFilter), true);

                if (!t.HasBegun && t.TrackFloor && t.TrackWall && GUILayout.Button("Start"))
                {
                    t.TrackFloor.sharedMesh = Instantiate(t.TrackFloor.sharedMesh);
                    t.TrackWall.sharedMesh = Instantiate(t.TrackWall.sharedMesh);
                    
                    t.CreateVertexData();
                    t.HasBegun = true;
                }
            }
            NeoToolEditor.EndTitledSection();

            if (!t.HasBegun) return;
            
            NeoToolEditor.BeginTitledSection("Actions");
            {
                if (GUILayout.Button("Refresh Vertex Objects")) t.CreateLinkerObject();
                if (GUILayout.Button("Export") && t.TrackFloor && t.TrackFloor) ExportDialogue(t);
            }
            NeoToolEditor.EndTitledSection();
        }

        private void ExportDialogue(TrmEditorData data)
        {
            string path = EditorUtility.SaveFilePanel("Save TRM", "", "New TRM", "trm");

            TRM trm = new TRM();
            CustomContent cc = trm.Prepare(data.TrackFloor.sharedMesh, data.TrackWall.sharedMesh, new Mesh());
            trm.WriteFile(cc, path);
        }
    }
}