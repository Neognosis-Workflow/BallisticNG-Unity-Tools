using UnityEngine;

namespace Source.NeoEditorTools
{
    public class NeoToolMono : MonoBehaviour
    {
        /// <summary>
        /// The tool that is being used on this object.
        /// </summary>
        [HideInInspector]
        public NeoEditorTool Tool;
    }
}
