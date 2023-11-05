using UnityEngine;
using UnityEditor;

public class MaterialAffineActivator : MonoBehaviour
{
    [MenuItem("BallisticNG/Utilities/Activate Affine Material Keyword", false, EditorMenuPriorities.UtilitiesMenu.AffineActivator)]
    static void UpdateMaterials()
    {
        bool proceed = EditorUtility.DisplayDialog("Info", "Running this tool will run through every material in your project and activate the affine mapping keyword if the option is enabled in the material. Depending on when you made a track you might have to manually toggle the affine option on the track floor/wall materials. All material edits will be logged to Unity's console. Continue?", "Yes", "No");
        if (!proceed) return;

        StackTraceLogType originalLogType = Application.GetStackTraceLogType(LogType.Log);
        Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);

        int materialUpdateCount = 0;
        string[] assets = AssetDatabase.FindAssets("t: Material");

        foreach (string asset in assets)
        {
            string path = AssetDatabase.GUIDToAssetPath(asset);

            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (!mat) continue;

            if (mat.HasProperty("_AllowAfineMapping") && mat.GetFloat("_AllowAfineMapping") > 0.0f && !mat.IsKeywordEnabled("_ALLOW_AFFINE_MAPPING"))
            {
                mat.EnableKeyword("_ALLOW_AFFINE_MAPPING");
                EditorUtility.SetDirty(mat);

                ++materialUpdateCount;
                Debug.Log(string.Format("Affine Activator Utility: Enabled Affine Keyword on {0}", mat.name));
            }
        }
        if (materialUpdateCount > 0) AssetDatabase.SaveAssets();

        EditorUtility.DisplayDialog("Info", string.Format("Affine keyword activation complete. {0} materials were updated. See the Untiy console for details.", materialUpdateCount), "Close");

        Application.SetStackTraceLogType(LogType.Log, originalLogType);
    }
}
