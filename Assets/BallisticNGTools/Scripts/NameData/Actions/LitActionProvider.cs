#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using Vlm;

namespace NgData.NameData.Actions
{
    [RegisterNameDataAction("lit")]
    public class LitActionProvider : NameDataAction
    {
        public override void Execute(GameObject targetObject)
        {
            if (HasFlag("ignore")) DisableContributeGi(targetObject);
            else EnableContributeGi(targetObject);
            
            if (HasFlag("tan"))
                GetOptions(targetObject).EncodeInTangents = true;

            if (HasFlag("shworldup"))
                GetOptions(targetObject).WorldUpShadows = true;

            if (HasFlag("noshcast"))
                GetOptions(targetObject).CastShadows = false;

            if (HasFlag("noshrecieve"))
                GetOptions(targetObject).RecieveShadows = false;
        }

        private VlmBakeOptions GetOptions(GameObject targetObject)
        {
            VlmBakeOptionsComponent lmo = targetObject.GetComponent<VlmBakeOptionsComponent>();
            if (!lmo) lmo = targetObject.AddComponent<VlmBakeOptionsComponent>();
            
            lmo.BakeOptions ??= new VlmBakeOptions();
            return lmo.BakeOptions;
        }

        public override void Clear(GameObject targetObject)
        {
            DisableContributeGi(targetObject);

            VlmBakeOptionsComponent lmo = targetObject.GetComponent<VlmBakeOptionsComponent>();
            if (lmo) Object.DestroyImmediate(lmo);
        }

        private void EnableContributeGi(GameObject targetObject)
        {
            StaticEditorFlags flags = GameObjectUtility.GetStaticEditorFlags(targetObject);
            if (flags.HasFlag(StaticEditorFlags.ContributeGI)) return;
            
            flags |= StaticEditorFlags.ContributeGI;
            
            GameObjectUtility.SetStaticEditorFlags(targetObject, flags);
        }

        private void DisableContributeGi(GameObject targetObject)
        {
            StaticEditorFlags flags = GameObjectUtility.GetStaticEditorFlags(targetObject);
            if (!flags.HasFlag(StaticEditorFlags.ContributeGI)) return;
            
            flags &= ~StaticEditorFlags.ContributeGI;
            
            GameObjectUtility.SetStaticEditorFlags(targetObject, flags);
        }
    }
}
#endif