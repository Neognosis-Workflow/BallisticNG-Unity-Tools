using UnityEngine;

namespace Source.NeoEditorTools
{
    public class NeoEditorTool
    {
        /// <summary>
        /// The target object that this tool is attached to.
        /// </summary>
        public NeoToolMono Target;

        /// <summary>
        /// Whether the control key is currently being held down.
        /// </summary>
        public bool CtrlHeld;

        /// <summary>
        /// Whether the shift key is currently being held down.
        /// </summary>
        public bool ShiftHeld;

        /// <summary>
        /// Whether the alt key is currently being held down.
        /// </summary>
        public bool AltHeld;

        public virtual void OnAwake()
        {

        }

        public virtual void OnDestroy()
        {

        }

        public virtual void OnSceneGUI()
        {

        }

        public virtual void OnInspectorGUI()
        {

        }

        public virtual void OnMouseMove(Vector2 mousePosition, Vector2 mouseDelta)
        {

        }

        public virtual void OnMouseButtonDown(int button)
        {

        }

        public virtual void OnMouseButtonUp(int button)
        {

        }

        public virtual void OnMouseScroll(Vector2 delta)
        {

        }

        public virtual void OnKeyDown(KeyCode key)
        {

        }

        public virtual void OnKeyUp(KeyCode key)
        {

        }

        public void Finished()
        {
            Object.DestroyImmediate(Target.gameObject);
        }
    }
}
