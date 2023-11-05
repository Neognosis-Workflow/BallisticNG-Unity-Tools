using System;
using System.Collections.Generic;
using System.Linq;
using BallisticUnityTools.TrackTools;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Source.NeoEditorTools.Editor.Importers
{
    [CustomEditor(typeof(LuaContainer))]
    public class LuaContainerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            LuaContainer container = (LuaContainer) target;

            container.LuaScript = (TextAsset) EditorGUILayout.ObjectField("Lua Script", container.LuaScript, typeof(TextAsset), false);

            GUILayout.Space(10);

            container.TickRate = (ELuaTickRate) EditorGUILayout.EnumPopup("Update Tick Rate", container.TickRate);

            GUILayout.Space(10);
            if (GUILayout.Button("Create Variables From Script"))
            {
                string[] typeNames = Enum.GetNames(typeof(LuaContainer.ELuaDataType));
                if (container.LuaScript) ParseScriptForVariables(typeNames, container.LuaScript.text.Split('\n'), container);
            }
            container.Variables = NeoToolEditor.DrawArray(container, "Variables", container.Variables, GetElementTitle, GetNewElement, DrawElement);
        }

        private void ParseScriptForVariables(string[] typeNames, string[] lines, LuaContainer container)
        {
            foreach (string line in lines)
            {
                /*---Split the line to parse it as individual symbols---*/
                string[] symbols = line.Split(' ');
                string prevSymbol = "";

                /*---Enumerate the symbols---*/
                string varName = "";
                string varType = "";
                bool lastWasVarName = false;
                foreach (string symbol in symbols)
                {
                    if (prevSymbol == "var")
                    {
                        varName = symbol;
                        lastWasVarName = true;
                    } else if (lastWasVarName)
                    {
                        varType = symbol;
                        break;
                    }

                    prevSymbol = symbol;
                }

                /*---Create the variable definition---*/
                if (!string.IsNullOrEmpty(varName) && !string.IsNullOrEmpty(varType))
                {
                    bool parsedType = Enum.TryParse(varType, true, out LuaContainer.ELuaDataType type);
                    if (!parsedType) continue;

                    LuaContainer.LuaVariable existingVar = container.Variables.FirstOrDefault(v => v.Name == varName);

                    if (existingVar != null) existingVar.Type = type;
                    else
                    {
                        LuaContainer.LuaVariable newVar = new LuaContainer.LuaVariable();
                        newVar.Name = varName;
                        newVar.Type = type;
                        container.Variables.Add(newVar);
                    }
                }
            }
        }

        private void DrawElement(List<LuaContainer.LuaVariable> array, int i)
        {
            string oldName = array[i].Name;
            string newName = EditorGUILayout.TextField("Name", oldName);
            if (oldName != newName)
            {
                Undo.RecordObject((LuaContainer)target, "Changed Name");
                array[i].Name = newName;
            }

            LuaContainer.ELuaDataType oldType = array[i].Type;
            LuaContainer.ELuaDataType newType = (LuaContainer.ELuaDataType) EditorGUILayout.EnumPopup("Type", oldType);
            if (oldType != newType)
            {
                Undo.RecordObject((LuaContainer)target, "Changed Type");
                array[i].Type = newType;
            }

            GUILayout.Space(10);
            Type luaType = LuaContainer.GetTypeFromLuaType(array[i].Type);
            switch (array[i].Type)
            {
                case LuaContainer.ELuaDataType.Integer:
                {
                    int oldValue = array[i].IntValue;
                    int newValue = EditorGUILayout.IntField("Value", oldValue);

                    if (oldValue != newValue)
                    {
                        Undo.RecordObject((LuaContainer) target, "Changed Value");
                        array[i].IntValue = newValue;
                    }
                }
                    break;
                case LuaContainer.ELuaDataType.Float:
                {
                    float oldValue = array[i].FloatValue;
                    float newValue = EditorGUILayout.FloatField("Value", oldValue);

                    if (oldValue != newValue)
                    {
                        Undo.RecordObject((LuaContainer) target, "Changed Value");
                        array[i].FloatValue = newValue;
                    }
                }
                    break;
                case LuaContainer.ELuaDataType.Boolean:
                {
                    bool oldValue = array[i].BoolValue;
                    bool newValue = EditorGUILayout.Toggle("Value", oldValue);

                    if (oldValue != newValue)
                    {
                        Undo.RecordObject((LuaContainer) target, "Changed Value");
                        array[i].BoolValue = newValue;
                    }
                }
                    break;
                case LuaContainer.ELuaDataType.String:
                {
                    string oldValue = array[i].StringValue;
                    string newValue = EditorGUILayout.TextField("Value", oldValue);

                    if (oldValue != newValue)
                    {
                        Undo.RecordObject((LuaContainer) target, "Changed Value");
                        array[i].StringValue = newValue;
                    }
                }
                    break;
                case LuaContainer.ELuaDataType.Curve:
                {
                    if (array[i].CurveValue == null) array[i].CurveValue = new AnimationCurve();
                    AnimationCurve oldValue = array[i].CurveValue;
                    AnimationCurve newValue = EditorGUILayout.CurveField("Value", oldValue);

                    if (!oldValue.Equals(newValue))
                    {
                        Undo.RecordObject((LuaContainer) target, "Changed Value");
                        array[i].CurveValue = newValue;
                    }
                }
                    break;

                case LuaContainer.ELuaDataType.Gradient:
                {
                    if (array[i].GradientValue == null) array[i].GradientValue = new Gradient();
                    Gradient oldValue = array[i].GradientValue;
                    Gradient newValue = EditorGUILayout.GradientField("Value", oldValue);

                    if (!oldValue.Equals(newValue))
                    {
                        Undo.RecordObject((LuaContainer) target, "Changed Value");
                        array[i].GradientValue = newValue;
                    }
                }
                    break;
                case LuaContainer.ELuaDataType.Vector2:
                {
                    Vector2 oldValue = array[i].Vec2Value;
                    Vector2 newValue = EditorGUILayout.Vector2Field("Value", oldValue);

                    if (oldValue != newValue)
                    {
                        Undo.RecordObject((LuaContainer) target, "Changed Value");
                        array[i].Vec2Value = newValue;
                    }
                }
                    break;
                case LuaContainer.ELuaDataType.Vector2Int:
                {
                    Vector2Int oldValue = array[i].Vec2IntValue;
                    Vector2Int newValue = EditorGUILayout.Vector2IntField("Value", oldValue);

                    if (oldValue != newValue)
                    {
                        Undo.RecordObject((LuaContainer) target, "Changed Value");
                        array[i].Vec2IntValue = newValue;
                    }
                }
                    break;
                case LuaContainer.ELuaDataType.Vector3:
                {
                    Vector3 oldValue = array[i].Vec3Value;
                    Vector3 newValue = EditorGUILayout.Vector3Field("Value", oldValue);

                    if (oldValue != newValue)
                    {
                        Undo.RecordObject((LuaContainer) target, "Changed Value");
                        array[i].Vec3Value = newValue;
                    }
                }
                    break;
                case LuaContainer.ELuaDataType.Vector3Int:
                {
                    Vector3Int oldValue = array[i].Vec3IntValue;
                    Vector3Int newValue = EditorGUILayout.Vector3IntField("Value", oldValue);

                    if (oldValue != newValue)
                    {
                        Undo.RecordObject((LuaContainer) target, "Changed Value");
                        array[i].Vec3IntValue = newValue;
                    }
                }
                    break;
                case LuaContainer.ELuaDataType.Vector4:
                {
                    Vector4 oldValue = array[i].Vec4Value;
                    Vector4 newValue = EditorGUILayout.Vector4Field("Value", oldValue);

                    if (oldValue != newValue)
                    {
                        Undo.RecordObject((LuaContainer) target, "Changed Value");
                        array[i].Vec4Value = newValue;
                    }
                }
                    break;
                case LuaContainer.ELuaDataType.Quaternion:
                {
                    Quaternion oldValue = array[i].QuaternionValue;

                    Vector4 vec = new Vector4(oldValue.x, oldValue.y, oldValue.z, oldValue.w);
                    Vector4 newVec = EditorGUILayout.Vector4Field("Value", vec);

                    if (vec != newVec)
                    {
                        Undo.RecordObject((LuaContainer) target, "Changed Value");

                        oldValue.x = newVec.x;
                        oldValue.y = newVec.y;
                        oldValue.z = newVec.z;
                        oldValue.w = newVec.w;
                        array[i].QuaternionValue = oldValue;
                    }
                }
                    break;
                case LuaContainer.ELuaDataType.Color:
                {
                    Color oldValue = array[i].ColorValue;
                    Color newValue = EditorGUILayout.ColorField("Value", oldValue);

                    if (oldValue != newValue)
                    {
                        Undo.RecordObject((LuaContainer) target, "Changed Value");
                        array[i].ColorValue = newValue;
                    }
                }
                    break;
                case LuaContainer.ELuaDataType.Color32:
                {
                    Color oldValue = array[i].Color32Value;
                    Color newValue = EditorGUILayout.ColorField("Value", oldValue);

                    if (oldValue != newValue)
                    {
                        Undo.RecordObject((LuaContainer) target, "Changed Value");
                        array[i].Color32Value = new Color32((byte) (newValue.r * 255), (byte) (newValue.g * 255), (byte) (newValue.b * 255), (byte) (newValue.a * 255));
                    }
                }
                    break;
                default:
                {
                    Object oldValue = array[i].ObjectValue;
                    Object newValue = EditorGUILayout.ObjectField("Value", oldValue, luaType, true);

                    if (oldValue != newValue)
                    {
                        Undo.RecordObject((LuaContainer) target, "Changed Value");
                        array[i].ObjectValue = newValue;
                    }
                }
                    break;
            }
        }

        private object GetNewElement()
        {
            return new LuaContainer.LuaVariable();
        }

        private string GetElementTitle(int i)
        {
            return ((LuaContainer) target).Variables[i].Name;
        }
    }
}