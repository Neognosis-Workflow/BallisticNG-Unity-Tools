using System;
using System.IO;
using BallisticModding.Formats;
using BallisticUnityTools.Lightmapping;
using BallisticUnityTools.TrackTools;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Debug = System.Diagnostics.Debug;
using Object = UnityEngine.Object;

namespace BallisticUnityTools.Editor
{
    public class TrmImporter : AssetPostprocessor
    {
        private static readonly string ExtTrm = ".trm";

        public static string ExternalToInternalPath(string asset, string ext)
        {
            string path = asset.Substring(0, asset.Length - ext.Length);
            return path + ext + ".prefab";
        }

        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            for (int i = 0; i < importedAssets.Length; ++i)
            {
                if (importedAssets[i].EndsWith(ExtTrm)) ImportTrm(importedAssets[i]);
            }
        }

        static void ImportTrm(string path)
        {
            // load trm
            TRM trm = new TRM();
            trm.LoadFile(path);

            // create meshes
            Mesh floor = trm.floorMesh;
            Mesh wall = trm.wallMesh;

            // get file path information
            FileInfo fi = new FileInfo(path);

            // get internal asset path and check if it exists
            string newPath = ExternalToInternalPath(path, ExtTrm);
            string fileName = fi.Name.Replace(".trm", "");

            fi = new FileInfo(newPath);
            string directory = fi.Directory.FullName.Replace(Environment.CurrentDirectory, "").Remove(0, 1);

            string floorMeshPath = string.Format("{0}/TRM/{1}_floor.asset", directory, fileName);
            string wallMeshPath = string.Format("{0}/TRM/{1}_wall.asset", directory, fileName);

            Mesh floorAsset = AssetDatabase.LoadAssetAtPath(floorMeshPath, typeof(Mesh)) as Mesh;
            Mesh wallAsset = AssetDatabase.LoadAssetAtPath(wallMeshPath, typeof(Mesh)) as Mesh;
            if (floorAsset && wallAsset)
            {
                UnityEngine.Debug.Log("TRM Import: Data found, updating...");

                if (floorAsset)
                {
                    floorAsset.vertices = floor.vertices;
                    floorAsset.triangles = floor.triangles;
                    floorAsset.uv = new Vector2[floor.vertices.Length];
                    floorAsset.colors32 = floor.colors32;
                    floorAsset.RecalculateNormals();
                }

                if (wallAsset)
                {
                    wallAsset.vertices = wall.vertices;
                    wallAsset.triangles = wall.triangles;
                    wallAsset.colors32 = wall.colors32;
                    wallAsset.uv = new Vector2[wall.vertices.Length];
                    wallAsset.RecalculateNormals();
                }

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                TrackDataStorage storage = Object.FindObjectOfType<TrackDataStorage>();
                if (storage)
                {
                    storage.UpdateTrackTextureAtlas(false, 0, storage.FloorTiles.Count, 0, storage.WallTiles.Count);
                    UnityEngine.Debug.Log("TRM Import: Scene is loaded, refreshing the track atlas.");

                    if (storage.TrackFloor)
                    {
                        VlmData vlm = storage.TrackFloor.GetComponent<VlmData>();
                        vlm.Apply();

                        UnityEngine.Debug.Log("TRM Import: Reapplied Track Floor Lighting.");
                    }

                    if (storage.TrackWall)
                    {
                        VlmData vlm = storage.TrackWall.GetComponent<VlmData>();
                        vlm.Apply();

                        UnityEngine.Debug.Log("TRM Import: Reapplied Track Wall Lighting.");
                    }
                }

                UnityEngine.Debug.Log("TRM Import: TRM Meshes Updated.");
            }
            else
            {

                UnityEngine.Debug.Log("TRM Import: Data not found, generating assets...");

                // create objects
                GameObject trackParent = new GameObject(fileName);
                GameObject floorObject = new GameObject("Track Floor");
                GameObject wallObject = new GameObject("Track Wall");

                // setup hiearchy
                floorObject.transform.SetParent(trackParent.transform);
                wallObject.transform.SetParent(trackParent.transform);

                // setup meshes
                Material trackMat = new Material(Shader.Find("BallisticNG/VertexLit Cutout"));
                if (trackMat.HasProperty("_AffineBlend")) trackMat.SetFloat("_AffineBlend", 0.5f);

                MeshFilter mf = floorObject.AddComponent<MeshFilter>();
                mf.mesh = floor;
                MeshRenderer mr = floorObject.AddComponent<MeshRenderer>();
                mr.material = trackMat;

                mf = wallObject.AddComponent<MeshFilter>();
                mf.mesh = wall;
                mr = wallObject.AddComponent<MeshRenderer>();
                mr.material = trackMat;

                // create meshes in asset database
                if (!AssetDatabase.IsValidFolder(fi.Directory.FullName + "/TRM/")) AssetDatabase.CreateFolder(directory, "TRM");
                if (floor) AssetDatabase.CreateAsset(floor, string.Format("{0}/TRM/{1}_floor.asset", directory, fileName));
                if (wall) AssetDatabase.CreateAsset(wall, string.Format("{0}/TRM/{1}_wall.asset", directory, fileName));
                AssetDatabase.CreateAsset(trackMat, string.Format("{0}/TRM/{1}_tempmat.mat", directory, fileName));

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                // create new prefab
                PrefabUtility.SaveAsPrefabAsset(trackParent, newPath);

                // destroy temp gameobject
                Object.DestroyImmediate(trackParent);

                UnityEngine.Debug.Log("TRM Import: TRM Data Created.");
            }
        }
    }
}
