using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class eInputMono : MonoBehaviour
{

}

public class eInput : Editor
{
    private static eInputMono _editorObject;
    private static eInputMono EditorMono
    {
        get
        {
            if (_editorObject) return _editorObject;
            _editorObject = FindObjectOfType<eInputMono>();
            if (_editorObject) return _editorObject;

            _editorObject = new GameObject("Editor Input Manager (Unconfigured)").AddComponent<eInputMono>();
            _editorObject.gameObject.hideFlags = HideFlags.DontSave | HideFlags.HideInInspector | HideFlags.HideInHierarchy;
            return _editorObject;
        }
    }

    private static bool[] _downArray;
    private static bool[] _upArray;
    private static bool[] _heldArray;
    private static bool _isInit;

    public static void Update()
    {
        Init();

        for (int i = 0; i < _downArray.Length; ++i) _downArray[i] = false;
        for (int i = 0; i < _upArray.Length; ++i) _upArray[i] = false;

        Event e = Event.current;

        switch (e.type)
        {
            case EventType.MouseDown:
                break;
            case EventType.MouseUp:
                break;
            case EventType.MouseMove:
                break;
            case EventType.MouseDrag:
                break;
            case EventType.KeyDown:
                _downArray[(int) e.keyCode] = true;
                _heldArray[(int) e.keyCode] = true;
                break;
            case EventType.KeyUp:
                _upArray[(int) e.keyCode] = true;
                _heldArray[(int) e.keyCode] = false;
                break;
            case EventType.ScrollWheel:
                break;
            case EventType.Repaint:
                break;
            case EventType.Layout:
                break;
            case EventType.DragUpdated:
                break;
            case EventType.DragPerform:
                break;
            case EventType.DragExited:
                break;
            case EventType.Ignore:
                break;
            case EventType.Used:
                break;
            case EventType.ValidateCommand:
                break;
            case EventType.ExecuteCommand:
                break;
            case EventType.ContextClick:
                break;
            case EventType.MouseEnterWindow:
                break;
            case EventType.MouseLeaveWindow:
                break;
        }
    }

    private static void Init()
    {
        if (_isInit) return;
        eInputMono editorMono = EditorMono;
        editorMono.name = "Editor Input Manager (Configured)";

        int len = Enum.GetNames(typeof(KeyCode)).Length;
        _downArray = new bool[len];
        _upArray = new bool[len];
        _heldArray = new bool[len];

        _isInit = true;
    }

    public static bool KeyDown(KeyCode key)
    {
        Init();
        return _downArray[(int) key];
    }

    public static bool KeyUp(KeyCode key)
    {
        Init();
        return _upArray[(int) key];
    }

    public static bool KeyHeld(KeyCode key)
    {
        Init();
        return _heldArray[(int) key];
    }
}
