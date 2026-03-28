#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NgData.NameData
{
    public class NameDataAction
    {
        private static Dictionary<string, Type> _registeredActions = new Dictionary<string, Type>();

        /// <summary>
        /// Registers a new action.
        /// </summary>
        public static void RegisterAction<T>(string name) where T : NameDataAction
        {
            name = name.ToLower();
            if (_registeredActions.ContainsKey(name)) return;
             _registeredActions.Add(name, typeof(T));
        }

        /// <summary>
        /// Registers a new action.
        /// </summary>
        public static void RegisterAction(string name, Type actionType)
        {
            name = name.ToLower();
            if (_registeredActions.ContainsKey(name)) return;
            _registeredActions.Add(name, actionType);
        }

        /// <summary>
        /// Attempts to run an action if it's available.
        /// </summary>
        public static void RunActionIfAvailable(ObjectNameData[] parsedData, GameObject targetObject, bool storeActionHistory, ref string report)
        {
            report = null;
            
            parsedData = parsedData.Where(data => !string.IsNullOrEmpty(data.ObjectType)).ToArray();
            string[] newActions = parsedData.Select(data => data.ObjectType).ToArray();
            
            // try to clear the previous action
            NameDataActionStore actionStore = TryClearActions(newActions, targetObject);
            foreach (ObjectNameData data in parsedData)
            {
                if (string.IsNullOrEmpty(data.ObjectType)) continue;
                string actionName = data.ObjectType.ToLower();
                
                bool hasAction = _registeredActions.TryGetValue(actionName, out Type t);
                if (!hasAction)
                {
                    Debug.LogWarning($"Unable to run name data action. {actionName} doesn't exist!");
                    return;
                }

                if (storeActionHistory)
                {
                    // store invisible action data so we can keep track of how an object has been changed
                    if (!actionStore) actionStore = targetObject.AddComponent<NameDataActionStore>();
                    actionStore.hideFlags = HideFlags.DontSaveInBuild | HideFlags.HideInInspector;
                    actionStore.LastActions.Add(actionName);
                }

                // run the action
                report += $"running {actionName} on {targetObject.name}\n";
            
                NameDataAction action = (NameDataAction) Activator.CreateInstance(t);
                action.Flags = data.Flags.ToArray();
                action.Variables = data.Variables.ToArray();
                action.Execute(targetObject);
                
            }
        }

        private static NameDataActionStore TryClearActions(string[] newActions, GameObject targetObject)
        {
            NameDataActionStore actionStore = targetObject.GetComponent<NameDataActionStore>();
            if (!actionStore) return null;
            
            List<string> newActionList = actionStore.LastActions.ToList();
            
            foreach (string lastAction in actionStore.LastActions)
            {
                bool hasAction = _registeredActions.TryGetValue(lastAction, out Type t);
                if (!hasAction) newActionList.Remove(lastAction);

                if (!newActions.Contains(lastAction))
                {
                    NameDataAction clearAction = (NameDataAction) Activator.CreateInstance(t);
                    clearAction.Clear(targetObject);
                    newActionList.Remove(lastAction);
                }
            }

            actionStore.LastActions = newActionList;
            
            return actionStore;
        }
        
        public string[] Flags = Array.Empty<string>();
        public ObjectNameData.VariableData[] Variables = Array.Empty<ObjectNameData.VariableData>();
        
        /// <summary>
        /// Called wdhen the action has been executed.
        /// </summary>
        public virtual void Execute(GameObject targetObject) { }
        
        /// <summary>
        /// Called when the action should clear up what it's previous done.
        /// </summary>
        public virtual void Clear(GameObject targetObject) { }

        /// <summary>
        /// Returns whether a flag is present in this action.
        /// </summary>
        public bool HasFlag(string flag)
        {
            return Flags != null && Flags.Any(f => string.Equals(f, flag, StringComparison.InvariantCultureIgnoreCase));
        }

        /// <summary>
        /// Returns whether an attempt to get a flag at the given index is succesful. if it is, the flag is returned through the flag out.
        /// </summary>
        public bool GetFlagAtIndex(int index, out string flag)
        {
            if (Flags == null || index < 0 || index > Flags.Length - 1)
            {
                flag = null;
                return false;
            }

            flag = Flags[index].ToLower();
            return true;
        }

        /// <summary>
        /// Tries to fetch a variable with the given name from the actions variable list.
        /// </summary>
        public ObjectNameData.VariableData GetVariableOfName(string name)
        {
            return Variables?.FirstOrDefault(var => string.Equals(name, var.Name, StringComparison.InvariantCultureIgnoreCase));
        }
        
        /// <summary>
        /// Returns whether an attempt to get a variable as a float was succesful. If it is, the value is returned through the value out.
        /// </summary>
        public bool GetFloat(string name, out float value)
        {
            ObjectNameData.VariableData variable = GetVariableOfName(name);

            value = 0.0f;
            if (variable == null) return false;
            
            bool parsed = float.TryParse(variable.Value, out value);
            return parsed;
        }
        
        /// <summary>
        /// Returns whether an attempt to get a variable as a double was succesful. If it is, the value is returned through the value out.
        /// </summary>
        public bool GetDouble(string name, out double value)
        {
            ObjectNameData.VariableData variable = GetVariableOfName(name);

            value = 0.0f;
            if (variable == null) return false;
            
            bool parsed = double.TryParse(variable.Value, out value);
            return parsed;
        }
        
        /// <summary>
        /// Returns whether an attempt to get a variable as a int was succesful. If it is, the value is returned through the value out.
        /// </summary>
        public bool GetInt(string name, out int value)
        {
            ObjectNameData.VariableData variable = GetVariableOfName(name);

            value = 0;
            if (variable == null) return false;
            
            bool parsed = int.TryParse(variable.Value, out value);
            return parsed;
        }
        
        /// <summary>
        /// Returns whether an attempt to get a variable as a uint was succesful. If it is, the value is returned through the value out.
        /// </summary>
        public bool GetUint(string name, out uint value)
        {
            ObjectNameData.VariableData variable = GetVariableOfName(name);

            value = 0;
            if (variable == null) return false;
            
            bool parsed = uint.TryParse(variable.Value, out value);
            return parsed;
        }
        
        /// <summary>
        /// Returns whether an attempt to get a variable as a boolean was succesful. If it is, the value is returned through the value out.
        /// </summary>
        public bool GetBool(string name, out bool value)
        {
            ObjectNameData.VariableData variable = GetVariableOfName(name);

            value = false;
            if (variable == null) return false;
            
            bool parsed = bool.TryParse(variable.Value, out value);
            return parsed;
        }
        
        /// <summary>
        /// Returns whether an attempt to get a variable as a float was succesful. If it is, the value is returned through the value out.
        /// </summary>
        public bool GetString(string name, out string value)
        {
            ObjectNameData.VariableData variable = GetVariableOfName(name);

            value = null;
            if (variable == null) return false;

            value = variable.Value;
            return true;
        }
    }
}
#endif