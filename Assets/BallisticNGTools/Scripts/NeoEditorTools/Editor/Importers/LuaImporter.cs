using System.IO;
using System.Reflection;
using UnityEditor;

using UnityEngine;
using Type = System.Type;

namespace Source.NeoEditorTools.Editor.Importers
{
    [UnityEditor.AssetImporters.ScriptedImporter(1, "lua")]
    public class LuaImporter : UnityEditor.AssetImporters.ScriptedImporter
    {
        [MenuItem("Assets/Create/Lua Script", false, 0)]
        public static void CreateLuaAsset()
        {
            string path = Path.Combine(GetPathForCreate(), "New Lua Script.lua");
            TextAsset luaTemplate = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Templates/Lua Template.lua");

            File.WriteAllText(path, luaTemplate.text);
            AssetDatabase.ImportAsset(path);
        }

        private static string GetPathForCreate()
        {
            if (Selection.activeObject)
            {
                string path = AssetDatabase.GetAssetPath(Selection.activeObject);
                if (Directory.Exists(path)) return path;
            }

            Type t = typeof(ProjectWindowUtil);
            MethodInfo getActiveFolderPath = t.GetMethod("GetActiveFolderPath", BindingFlags.Static | BindingFlags.NonPublic);

            object obj = getActiveFolderPath.Invoke(null, new object[0]);
            return obj.ToString();
        }

        public override void OnImportAsset(UnityEditor.AssetImporters.AssetImportContext ctx)
        {
            TextAsset ta = new TextAsset(File.ReadAllText(ctx.assetPath));
            ctx.AddObjectToAsset("Lua Script", ta);
        }
    }
}
