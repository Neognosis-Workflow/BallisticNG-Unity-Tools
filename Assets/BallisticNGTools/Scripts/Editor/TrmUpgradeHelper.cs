using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using BallisticUnityTools.Lightmapping;
using BallisticUnityTools.Placeholders;
using BallisticUnityTools.TrackTools;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Helper class to automate track updating (designed for 5.6 to 2017.1 asset reimport issues)
/// </summary>
public class TrmUpgradeHelper
{
    #if UNITY_2017_1_OR_NEWER
    [MenuItem("BallisticNG/Utilities/5.6 to 2017 Scene Updater", false, EditorMenuPriorities.UtilitiesMenu.SceneUpgrader)]
    public static void TrmUpdate()
    {
        /*---Make sure there is track data present---*/
        TrackDataStorage trackData = UnityEngine.Object.FindObjectOfType<TrackDataStorage>();
        if (!trackData)
        {
            EditorUtility.DisplayDialog("Error", "There is no track data present in the current scene!", "Close");
            return;
        }

        if (trackData.TrackFloor && trackData.TrackWall)
        {
            EditorUtility.DisplayDialog("Error", "Tool might have already been ran! Aborting.", "Close");
            return;
        }

        /*---Make sure track objects exists---*/
        GameObject sceneFloor = GameObject.Find("Track Floor");
        GameObject sceneWall = GameObject.Find("Track Wall");
        if (!sceneFloor || !sceneWall)
        {
            EditorUtility.DisplayDialog("Error", "Couldn't find track object(s)! Make sure they are called Track Floor and Track Wall!", "Close");
            return;
        }

        /*---Update References---*/
        trackData.TrackFloor = sceneFloor.GetComponent<MeshFilter>();
        trackData.TrackWall = sceneWall.GetComponent<MeshFilter>();

        /*---Re-configure tracks---*/
        ConfigureTrackSurface(sceneFloor, false);
        ConfigureTrackSurface(sceneWall, true);

        EditorUtility.DisplayDialog("Notice", "The track atlas and scene lighting is about to be re-baked, this could take a moment depending on the complexity of your scene.", "Okay");
        trackData.UpdateTrackTextureAtlas(true, 0, trackData.FloorTiles.Count, 0, trackData.WallTiles.Count);
        VlmData.RebakeLighting();
    }

    private static void ConfigureTrackSurface(GameObject trackObject, bool isWall)
    {
        BallisticMeshCollider bcm = trackObject.GetComponent<BallisticMeshCollider>();
        if (!bcm) bcm = trackObject.AddComponent<BallisticMeshCollider>();
        bcm.Type = isWall ? BallisticMeshCollider.CollisionType.TrackWall : BallisticMeshCollider.CollisionType.TrackFloor;

        MeshCollider meshCollider = trackObject.GetComponent<MeshCollider>();
        if (!meshCollider) trackObject.AddComponent<MeshCollider>();

        MeshFilter mf = trackObject.GetComponent<MeshFilter>();
        if (!mf) return;

        Mesh m = mf.sharedMesh;
        Vector2[] uvs = new Vector2[m.vertexCount];
        for (int i = 0; i < uvs.Length; ++i) uvs[i] = Vector2.zero;
        m.uv = uvs;
        mf.sharedMesh = m;
    }
    #endif
}
