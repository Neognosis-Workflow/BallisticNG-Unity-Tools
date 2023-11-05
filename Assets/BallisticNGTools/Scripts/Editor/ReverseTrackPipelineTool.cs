using System;
using BallisticModding.Formats;
using BallisticUnityTools.Editor.Shaders;
using BallisticUnityTools.Placeholders;
using BallisticUnityTools.TrackTools;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Source.Editor.Utilities
{
    public class ReverseTrackPipelineTool
    {
        /// <summary>
        /// Flips a TRM mesh.
        /// </summary>
        public static Mesh FlipTrm(Mesh mesh)
        {
            /*---Original Mesh Data---*/
            Vector3[] originalVerts = mesh.vertices;
            int[] originalTri = mesh.triangles;

            /*---The new mesh data where every 6 indicies will be flipped---*/
            Vector3[] newVerts = new Vector3[originalVerts.Length];

            /*---Transfer vertices---*/
            for (int i = newVerts.Length - 1; i > -1; i -= 6)
            {
                int j = newVerts.Length - 1 - i;
                newVerts[j + 0] = originalVerts[i - 1];
                newVerts[j + 1] = originalVerts[i - 0];
                newVerts[j + 2] = originalVerts[i - 2];
                newVerts[j + 3] = originalVerts[i - 3];
                newVerts[j + 4] = originalVerts[i - 5];
                newVerts[j + 5] = originalVerts[i - 4];
            }

            Mesh newMesh = new Mesh();
            newMesh.vertices = newVerts;
            newMesh.uv = new Vector2[0];
            newMesh.triangles = originalTri;
            newMesh.RecalculateNormals();
            return newMesh;
        }

        [MenuItem("BallisticNG/Create Reverse Track", false, 0)]
        public static void Do()
        {
            string projDirectory = Environment.CurrentDirectory;

            /*---Valid Scene Check---*/
            TrackDataStorage data = Object.FindObjectOfType<TrackDataStorage>();
            if (!data)
            {
                ErrorMessage("Please run this tool in a track scene.");
                return;
            }

            /*---Save TRM---*/
            EditorUtility.DisplayDialog("Notice", "Please select a location to save the reversed TRM.\n\nDO NOT select the same folder as the forward TRM as they will conflict!", "Got it.");

            MeshRenderer trackRenderer = data.TrackFloor.GetComponent<MeshRenderer>();
            string trackShader = trackRenderer.sharedMaterial.shader.name;

            Mesh flippedFloor = FlipTrm(data.TrackFloor.sharedMesh);
            Mesh flippedWall = FlipTrm(data.TrackWall.sharedMesh);

            string trmPath = EditorUtility.SaveFilePanel("New TRM", "", "Reverse TRM", "trm");

            TRM trm = new TRM();
            CustomContent cc = trm.Prepare(flippedFloor, flippedWall, null);
            trm.WriteFile(cc, trmPath);

            /*---Save Scene---*/
            EditorUtility.DisplayDialog("Notice", "Please select a location to save the reverse scene.\n\nUnlike the TRM this can be in the same folder as your forward scene.", "Got it.");
            Scene currentScene = EditorSceneManager.GetActiveScene();
            string fwdScenePath = currentScene.path;
            string revScenePath = EditorUtility.SaveFilePanel("New Scene", "", $"{currentScene.name} Reverse", "unity");
            EditorSceneManager.SaveScene(currentScene, revScenePath, true);

            AssetDatabase.Refresh(ImportAssetOptions.Default);

            /*---Switch to reverse scene---*/
            EditorUtility.DisplayDialog("Notice", "Assets created. The reverse scene will now be opened.", "Got it.");
            EditorSceneManager.OpenScene(revScenePath, OpenSceneMode.Single);

            /*---Import TRM into the scene---*/
            string trmAssetPath = trmPath.Remove(0, projDirectory.Length + 1) + ".prefab";
            Debug.Log(trmAssetPath);
            GameObject reverseTrmPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(trmAssetPath);
            reverseTrmPrefab = PrefabUtility.InstantiatePrefab(reverseTrmPrefab) as GameObject;
            Debug.Log(reverseTrmPrefab);

            GameObject sceneObj = GameObject.Find("[ Track Configuration ]");
            if (sceneObj) reverseTrmPrefab.transform.SetParent(sceneObj.transform);

            /*---Get track/floor---*/
            GameObject reverseFloor = reverseTrmPrefab.transform.Find("Track Floor").gameObject;
            GameObject reverseWall = reverseTrmPrefab.transform.Find("Track Wall").gameObject;

            MeshRenderer floorRenderer = reverseFloor.GetComponent<MeshRenderer>();
            MeshRenderer wallRenderer = floorRenderer.GetComponent<MeshRenderer>();

            /*---Configure track/floor---*/
            floorRenderer.sharedMaterial.shader = Shader.Find(trackShader);
            StandardNgShaderEditor.TryConfigureStandardMaterial(floorRenderer.sharedMaterial);
            SetupTrmObject(reverseFloor, 0);
            SetupTrmObject(reverseWall, 1);

            /*---Cache forward references---*/
            TrackDataStorage revData = Object.FindObjectOfType<TrackDataStorage>();
            GameObject forwardFloor = revData.TrackFloor.gameObject;
            GameObject forwardWall = revData.TrackWall.gameObject;

            /*---Load the forward scene to get the track data storage from it---*/
            Scene fwdScene = EditorSceneManager.OpenScene(fwdScenePath, OpenSceneMode.Additive);
            GameObject[] fwdObjects = fwdScene.GetRootGameObjects();
            TrackDataStorage fwdData = null;
            foreach (GameObject obj in fwdObjects)
            {
                fwdData = obj.GetComponentInChildren<TrackDataStorage>();
                if (fwdData) break;
            }

            /*---Swap out references and generate new data---*/
            revData.TrackFloor = reverseFloor.GetComponent<MeshFilter>();
            revData.TrackWall = reverseWall.GetComponent<MeshFilter>();

            revData.GenerateTrackData();
            revData.UpdateTrackTextureAtlas(true, 0, revData.FloorTiles.Count, 0, revData.WallTiles.Count);
            TrackProcessors.RecalculateSectionPositions(revData.Sections);

            /*---Reverse it---*/
            ReverseTransfer(fwdData, revData, forwardFloor, reverseFloor, forwardWall, reverseWall, false, true);

            /*---Clean up old data---*/
            GameObject forwardPrefab = forwardFloor.transform.parent.gameObject;
            Object.DestroyImmediate(forwardPrefab);

            EditorSceneManager.UnloadSceneAsync(fwdScene);

            EditorUtility.DisplayDialog("Notice", "Reverse track has been created.\n\nPlease note that this tool does not automate everything. You will now need to configure sections and double check for misplaced tile textures.", "Got it.");
        }

        private static void SetupTrmObject(GameObject obj, byte surfaceType)
        {
            MeshCollider mc = obj.AddComponent<MeshCollider>();

            /*---SetupCollider---*/
            BallisticMeshCollider collider = mc.gameObject.AddComponent<BallisticMeshCollider>();
            collider.Type = surfaceType == 0 ? BallisticMeshCollider.CollisionType.TrackFloor : BallisticMeshCollider.CollisionType.TrackWall;
        }

        private static void ReverseTransfer(TrackDataStorage fwdData, TrackDataStorage revData, GameObject forwardFloor, GameObject reverseFloor, GameObject forwardWall, GameObject reverseWall, bool flipPads, bool autoFlipTiles)
        {
            MeshCollider forwardFloorCollider = forwardFloor.GetComponent<MeshCollider>();
            MeshCollider reverseFloorCollider = reverseFloor.GetComponent<MeshCollider>();

            MeshCollider forwardWallCollider = forwardWall.GetComponent<MeshCollider>();
            MeshCollider reverseWallCollider = reverseWall.GetComponent<MeshCollider>();

            /*---Exchange Tiles---*/
            for (int i = 0; i < fwdData.FloorTiles.Count; ++i)
            {
                Tile tile = fwdData.FloorTiles[i];
                Vector3 startPos = tile.Position + tile.Section.Normal;

                RaycastHit hit;

                /*---Exchange Floor--*/
                if (reverseFloorCollider.Raycast(new Ray(startPos, -tile.Section.Normal), out hit, 5.0f))
                {
                    Tile reverseTile = TrackProcessors.GetTileFromTriangleIndex(hit.triangleIndex, revData.MappedFloorTIles);
                    reverseTile.Type = tile.Type;
                    reverseTile.AtlasIndex = tile.AtlasIndex;
                    if (autoFlipTiles && (tile.Type != ETiletype.Boost && tile.Type != ETiletype.Weapon || flipPads)) reverseTile.UvFlip = EUvflip.Horizontal;
                    else reverseTile.UvFlip = tile.UvFlip;
                    reverseTile.UvOrder = tile.UvOrder;
                }
            }

            for (int i = 0; i < fwdData.Sections.Count; ++i)
            {

                Section section = fwdData.Sections[i];

                /*---Exchange Left Floor---*/
                RaycastHit hit;
                Ray exchangeRay = new Ray(section.Position + section.Normal * (section.Height * 0.2f), section.Rotation * -Vector3.right);
                if (forwardWallCollider.Raycast(exchangeRay, out hit, section.Width * 2.0f))
                {
                    Tile forwardTile = TrackProcessors.GetTileFromTriangleIndex(hit.triangleIndex, fwdData.MappedWallTiles);

                    RaycastHit reverseHit;
                    if (reverseWallCollider.Raycast(exchangeRay, out reverseHit, section.Width * 2.0f))
                    {
                        Tile reverseTile = TrackProcessors.GetTileFromTriangleIndex(reverseHit.triangleIndex, revData.MappedWallTiles);
                        reverseTile.AtlasIndex = forwardTile.AtlasIndex;
                        if (autoFlipTiles)
                        {
                            if (forwardTile.UvFlip == EUvflip.Vertical) reverseTile.UvFlip = EUvflip.Both;
                            else reverseTile.UvFlip = EUvflip.Horizontal;
                        }
                        else reverseTile.UvFlip = forwardTile.UvFlip;
                        reverseTile.UvOrder = forwardTile.UvOrder;
                    }
                }

                /*---Exchange Right Wall---*/
                exchangeRay = new Ray(section.Position + section.Normal * (section.Height * 0.2f), section.Rotation * Vector3.right);
                if (forwardWallCollider.Raycast(exchangeRay, out hit, section.Width * 2.0f))
                {
                    Tile forwardTile = TrackProcessors.GetTileFromTriangleIndex(hit.triangleIndex, fwdData.MappedWallTiles);

                    RaycastHit reverseHit;
                    if (reverseWallCollider.Raycast(exchangeRay, out reverseHit, section.Width * 2.0f))
                    {
                        Tile reverseTile = TrackProcessors.GetTileFromTriangleIndex(reverseHit.triangleIndex, revData.MappedWallTiles);
                        reverseTile.AtlasIndex = forwardTile.AtlasIndex;
                        if (autoFlipTiles)
                        {
                            if (forwardTile.UvFlip == EUvflip.Vertical) reverseTile.UvFlip = EUvflip.Both;
                            else reverseTile.UvFlip = EUvflip.Horizontal;
                        }
                        else reverseTile.UvFlip = forwardTile.UvFlip;
                        reverseTile.UvOrder = forwardTile.UvOrder;
                    }
                }
            }

            if (revData) revData.UpdateTrackTextureAtlas(true, 0, revData.FloorTiles.Count, 0, revData.WallTiles.Count);
        }

        private static void ErrorMessage(string details)
        {
            EditorUtility.DisplayDialog("Error", details, "OK");
        }
    }
}