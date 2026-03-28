#if UNITY_EDITOR
using System;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NgData.NameData
{
    public static class EditorNameDataActions
    {
        public static void RunNameDataActionsAll()
        {
            Scene scene = SceneManager.GetActiveScene();
            if (EditorSceneManager.IsPreviewScene(scene)) return;

            GameObject[] gos = scene.GetRootGameObjects();
            Transform[] transforms = new Transform[gos.Length];
            for (int i = 0; i < transforms.Length; ++i) transforms[i] = gos[i].transform;

            StringBuilder sb = new StringBuilder();
            try
            {
                RunNameDataActons(transforms, true, sb);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            LogReport(sb);
        }

        public static void RunNameDataActionsSelected()
        {
            if (EditorSceneManager.IsPreviewScene(SceneManager.GetActiveScene())) return;
            
            StringBuilder sb = new StringBuilder();
            try
            {
                RunNameDataActons(Selection.GetTransforms(SelectionMode.Unfiltered), true, sb);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            LogReport(sb);
        }

        public static void RunNameDataActons(Transform[] transforms, bool storeHistory, StringBuilder reportBuilder)
        {
            foreach (Transform t in transforms)
            {
                int childCount = t.childCount;

                if (childCount > 0)
                {
                    Transform[] children = new Transform[childCount];
                    for (int i = 0; i < childCount; ++i) children[i] = t.GetChild(i);
                    
                    RunNameDataActons(children, storeHistory, reportBuilder);
                }
                
                string report = "";
                NameDataAction.RunActionIfAvailable(ObjectNameDataParser.Parse(t.name), t.gameObject, storeHistory, ref report);
                    
                if (!string.IsNullOrEmpty(report)) reportBuilder.AppendLine(report);
            }
        }

        public static void LogReport(StringBuilder sb)
        {
            if (sb.Length == 0) return;
            
            Debug.Log($"Configured objects:\n{sb}");
        }
    }
}
#endif