#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Reflection;
using NgLib;
using UnityEditor;

namespace NgData.NameData
{
    public static class NameDataActionRegister
    {
        [InitializeOnLoadMethod]
        public static void Register()
        {
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            Dictionary<Type, RegisterNameDataActionAttribute> actions = AssemblyDiscovery.FindAttributedObjects<RegisterNameDataActionAttribute>(assemblies);
            foreach (KeyValuePair<Type,RegisterNameDataActionAttribute> action in actions)
                NameDataAction.RegisterAction(action.Value.Name, action.Key);
        }
    }
}
#endif