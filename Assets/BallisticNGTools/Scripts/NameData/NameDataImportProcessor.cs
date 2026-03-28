#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace NgData.NameData
{
    public class NameDataImportProcessor : AssetPostprocessor
    {
        public float Test;
        private void OnPostprocessModel(GameObject g)
        {
            List<Transform> transforms = new List<Transform> { g.transform };
            transforms.AddRange(g.GetComponentsInChildren<Transform>());
            Transform[] tArray = transforms.ToArray();

            HashSet<string> importFlags = GetImportFlags(tArray, out Transform importFlagsT);
            if (!importFlagsT) return;
            
            if (importFlags.Contains("lightall")) LightAll(tArray);
            if (importFlags.Contains("autoconfig"))
            {
                AutoConfig(new[] {g.transform});
                if (!importFlags.Contains("keepnames")) StripNames(tArray);
            }
            if (!importFlags.Contains("keepme")) Object.DestroyImmediate(importFlagsT.gameObject);
        }

        private void LightAll(Transform[] transforms)
        {
            foreach (Transform t in transforms)
            {
                StaticEditorFlags flags = GameObjectUtility.GetStaticEditorFlags(t.gameObject);
                flags |= StaticEditorFlags.ContributeGI;
            
                GameObjectUtility.SetStaticEditorFlags(t.gameObject, flags);
            }
        }

        private void AutoConfig(Transform[] transforms)
        {
            StringBuilder sb = new StringBuilder();
            EditorNameDataActions.RunNameDataActons(transforms, false, sb);
            EditorNameDataActions.LogReport(sb);
        }
        
        private HashSet<string> GetImportFlags(Transform[] transforms, out Transform importObject)
        {
            importObject = null;
            const string prefix = "_import:";
            foreach (Transform t in transforms)
            {
                string name = t.name.ToLower();
                if (!name.StartsWith(prefix)) continue;

                importObject = t;
                name = name.Replace(prefix, "");

                HashSet<string> flagSet = new HashSet<string>();
                
                string[] flags = name.CommaSeparate(true, true);
                foreach (string flag in flags) flagSet.Add(flag.ToLower());

                return flagSet;
            }

            return new HashSet<string>();
        }

        private void StripNames(Transform[] transforms)
        {
            foreach (Transform t in transforms)
            {
                string name = t.name;
                int indexOfDollar = name.IndexOf("$", StringComparison.InvariantCulture);
                if (indexOfDollar < 0) continue;

                t.name = name.Substring(0, indexOfDollar);
            }
        }
    }
}
#endif