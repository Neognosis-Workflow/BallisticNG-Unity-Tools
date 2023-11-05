using UnityEditor;

namespace Source.Editor.CustomAssets
{
    /// <summary>
    /// Processor to automatically mark meshes as readable for lightmapping. 
    /// </summary>
    public class MeshPostProcessor : AssetPostprocessor
    {
        private void OnPreprocessModel()
        {
            ModelImporter modelImporter = assetImporter as ModelImporter;
            if (!modelImporter) return;
            
            modelImporter.isReadable = true;
        }
    }
}
