#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace NgTrackData
{
    [ExecuteInEditMode]
    public class TrackTodoList : MonoBehaviour
    {
        #if UNITY_EDITOR
        /// <summary>
        /// Creates a new editor to do list.
        /// </summary>
        [MenuItem("BallisticNG/Create Track Todo List")]
        public static void Create()
        {
            TrackTodoList existingList = FindObjectOfType<TrackTodoList>();
            if (existingList)
            {
                Selection.activeGameObject = existingList.gameObject;
                return;
            };
            
            GameObject newGo = new GameObject();
            newGo.AddComponent<TrackTodoList>();

            Selection.activeGameObject = newGo;
        }
        #endif
        
        [Header("Section Data")]
        public bool SectionsSetup;
        public bool StartMidLasersSetup;
        public bool RacingLinesSetup;
        
        [Header("Tile Data")]
        public bool AtlasSetup;
        public bool TilesSetup;

        [Header("Material Data")]
        public bool TrackAffineBlendSetup;

        [Header("Scene Objects")]
        public bool CountdownBoardsSetup;
        public bool ScenerySetup;
        public bool SkySetup;
        public bool EnvironmentAffineSetup;
        
        [Header("Reflection Objects")]
        public bool TrackReflectionsSetup;
        public bool SceneryReflectionsSetup;

        [Header("Cinematic Objects")]
        public bool TrackCamerasSetup;
        public bool OverviewsSetup;
        public bool StartGridDroidsSetup;

        [Header("Configuration Objects")]
        public bool FlareOccludersSetuo;
        public bool SurvivalTogglersSetup;
        public bool SurvivalDetailsSetup;
        public bool SurvivalVisualizersSetup;
        public bool PostProcessingSetup;

        [Header("Frontend Data")]
        public bool FrontendImage;
        public bool FrontendLocation;
        
        private void Awake()
        {
            gameObject.name = "TODO LIST";
            gameObject.tag = "EditorOnly";
            gameObject.hideFlags = HideFlags.DontSaveInBuild;
        }
    }
}
#endif