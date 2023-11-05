#if UNITY_EDITOR
using System;
using BallisticUnityTools.Editor;
using BallisticUnityTools.TrackTools;
using UnityEditor;
using UnityEngine;

namespace NgEditorTools.ModernPads
{
    [AddComponentMenu("")]
    [ExecuteInEditMode]
    public class ModernPadEditorMono : MonoBehaviour
    {

        [HideInInspector] public GameObject TemplateSpeed;
        [HideInInspector] public GameObject TemplateWeapon;
        
        public GameObject PrefabSpeed;
        public GameObject PrefabWeapon;
        public float GroundOffset = 0.05f;
        public TrackPad.TrackPadType PadType;

        [HideInInspector]
        public GameObject PrefabParent;

        private bool _canTogglePreviews = true;

        private void Awake()
        {
            /*---this cannot be saved---*/
            hideFlags = HideFlags.DontSave;
            
            CheckParent();
        }

        private void OnDestroy()
        {
            if (TemplateSpeed) DestroyImmediate(TemplateSpeed);
            if (TemplateWeapon) DestroyImmediate(TemplateWeapon);
        }

        /// <summary>
        /// Configures the PrefabSpeed and PrefabWeapon prefabs to be used by the editor.
        /// </summary>
        public void SetupPrefabs()
        {
            // destroy old templates
            if (TemplateSpeed) DestroyImmediate(TemplateSpeed);
            if (TemplateWeapon) DestroyImmediate(TemplateWeapon);
            
            // create new templates
            if (PrefabSpeed) TemplateSpeed = GetPrefabInstance(PrefabSpeed, true);
            if (PrefabWeapon) TemplateWeapon = GetPrefabInstance(PrefabWeapon, true);
        }

        /// <summary>
        /// Replaces all prefabs in the scene with the currently set template.
        /// </summary>
        public void ReplacePrefabs()
        {
            bool cont = EditorUtility.DisplayDialog("Notice", "This will replace any enabled pads in the scene with the current selected prefabs. You can exclude pads by disabling their game objects. This action is undoable.", "Continue", "Cancel");
            if (!cont) return;

            int value = EditorUtility.DisplayDialogComplex("Selection", "What kind of pads do you want to replace?", "Speed", "Weapon", "Both");
            bool replaceSpeed = value == 0 || value == 2;
            bool replaceWeapon = value == 1 || value == 2;
            
            TrackPad[] pads = FindObjectsOfType<TrackPad>(false);
            foreach (TrackPad pad in pads)
            {
                if (!pad) continue;
                
                TrackPad.TrackPadType type = pad.Type;
                if (type == TrackPad.TrackPadType.Speed)
                {
                    if (!replaceSpeed) continue;

                    GameObject newSpeed = GetPrefabInstance(PrefabSpeed, false);
                    Undo.RegisterCreatedObjectUndo(newSpeed, "Replaced Speed Pad");
                    
                    newSpeed.transform.SetParent(pad.transform.parent);
                    newSpeed.transform.position = pad.transform.position;
                    newSpeed.transform.rotation = pad.transform.rotation;
                    newSpeed.transform.localScale = pad.transform.localScale;
                    
                    Undo.DestroyObjectImmediate(pad.gameObject);
                } else if (type == TrackPad.TrackPadType.Weapon)
                {
                    if (!replaceWeapon) continue;
                    
                    GameObject newWeapon = GetPrefabInstance(PrefabWeapon, false);
                    Undo.RegisterCreatedObjectUndo(newWeapon, "Replaced Weapon Pad");
                    newWeapon.transform.SetParent(pad.transform.parent);
                    newWeapon.transform.position = pad.transform.position;
                    newWeapon.transform.rotation = pad.transform.rotation;
                    newWeapon.transform.localScale = pad.transform.localScale;
                    
                    Undo.DestroyObjectImmediate(pad.gameObject);
                }
            }
        }

        /// <summary>
        /// Returns an instantiated prefab with the appropiate hide flags for the editor.
        /// </summary>
        public GameObject GetPrefabInstance(GameObject prefab, bool hide)
        {
            CheckParent();
            
            GameObject instance = PrefabUtility.InstantiatePrefab(prefab, PrefabParent.transform) as GameObject;
            if (instance && hide) instance.hideFlags = HideFlags.HideInInspector | HideFlags.DontSave;
            
            return instance;
        }

        /// <summary>
        /// Hides the preview objects.
        /// </summary>
        public void HidePreviews(bool editorForced)
        {
            if (_canTogglePreviews || editorForced)
            {
                if (TemplateSpeed && TemplateSpeed.activeSelf) TemplateSpeed.gameObject.SetActive(false);
                if (TemplateWeapon && TemplateWeapon.activeSelf) TemplateWeapon.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Shows the preview objects
        /// </summary>
        public void ShowPreviews(bool editorForced, bool forceUpdate = false)
        {
            if (_canTogglePreviews || editorForced)
            {
                if (TemplateSpeed && (!TemplateSpeed.activeSelf || forceUpdate)) TemplateSpeed.gameObject.SetActive(PadType == TrackPad.TrackPadType.Speed);
                if (TemplateWeapon && (!TemplateWeapon.activeSelf || forceUpdate)) TemplateWeapon.gameObject.SetActive(PadType == TrackPad.TrackPadType.Weapon);
            }
        }

        /// <summary>
        /// Sets whether the previews can be toggled on or off.
        /// </summary>
        public void SetCanTogglePreviews(bool value)
        {
            _canTogglePreviews = value;
        }

        private void CheckParent()
        {
            const string parentName = "3d Pads";
            if (!PrefabParent) PrefabParent = GameObject.Find(parentName);
            if (!PrefabParent) PrefabParent = new GameObject(parentName);
        }
    }
}
#endif