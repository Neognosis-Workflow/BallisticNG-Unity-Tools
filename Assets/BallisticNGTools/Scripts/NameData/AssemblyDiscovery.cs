using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace NgLib
{
    public struct AssemblyDiscovery
    {
        /// <summary>
        /// Searches for all instances of a root type with a specific attribute type in all loaded assemblies.
        /// </summary>
        /// <typeparam name="TAttr">The attribute to search for.</typeparam>
        /// <typeparam name="TFc">The type to search for.</typeparam>
        /// <returns>A collection of instances of the provided type.</returns>
        public static Dictionary<TAttr, TFc> FindAttributedInterface<TAttr, TFc>() where TAttr : class where TFc : class
        {
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            return FindAttributedInterface<TAttr, TFc>(assemblies);
        }

        /// <summary>
        /// Searches for all instances of a root type with a specific attribute type in the provided assemblies.
        /// </summary>
        /// <typeparam name="TAttr">The attribute to search for.</typeparam>
        /// <typeparam name="TFc">The type to search for.</typeparam>
        /// <param name="assemblies">The assemblies to look in.</param>
        /// <returns>A collection of instances of the provided type.</returns>
        public static Dictionary<TAttr, TFc> FindAttributedInterface<TAttr, TFc>(ICollection<Assembly> assemblies) where TAttr : class where TFc : class
        {
            Dictionary<TAttr, TFc> foundTypes = new Dictionary<TAttr, TFc>();

            foreach (Assembly assembly in assemblies)
            {
                foreach (Type type in GetAssemblyTypes(assembly))
                {
                    object[] attributes = type.GetCustomAttributes(typeof(TAttr), false);
                    if (attributes.Length > 0)
                    {
                        TAttr attribute = attributes[0] as TAttr;
                        if (attribute == null) continue;

                        Type[] interfaces = type.GetInterfaces();
                        foreach (Type i in interfaces)
                        {
                            if (i == typeof(TFc))
                            {
                                object obj = Activator.CreateInstance(type);

                                TFc ifc = obj as TFc;
                                if (ifc != null) foundTypes.Add(attribute, ifc);
                            }
                        }
                    }
                }
            }

            return foundTypes;
        }

        /// <summary>
        /// Searches for all instances of an interface in the provided assemblies.
        /// </summary>
        public static List<T> FindInterfaces<T>(ICollection<Assembly> assemblies) where T : class
        {
            List<T> foundInterfaces = new List<T>();

            foreach (Assembly assembly in assemblies)
            {
                foreach (Type type in GetAssemblyTypes(assembly))
                {
                    Type[] interfaces = type.GetInterfaces();
                    foreach (Type i in interfaces)
                    {
                        if (i == typeof(T))
                        {
                            object obj = Activator.CreateInstance(type);

                            T ifc = obj as T;
                            if (ifc != null) foundInterfaces.Add(ifc);
                        }
                    }
                }
            }

            return foundInterfaces;
        }

        /// <summary>
        /// Searches for all instances of a root type with a specific attribute type in the provided assemblies.
        /// </summary>
        /// <typeparam name="TAttr">The attribute to search for.</typeparam>
        /// <typeparam name="TFc">The type to search for.</typeparam>
        /// <param name="assemblies">The assemblies to look in.</param>
        /// <returns>A collection of instances of the provided type.</returns>
        public static Dictionary<Type, TAttr> FindAttributedObjects<TAttr>(ICollection<Assembly> assemblies) where TAttr : Attribute
        {
            Dictionary<Type, TAttr> foundTypes = new Dictionary<Type, TAttr>();
            foreach (Assembly assembly in assemblies)
            {
                foreach (Type type in GetAssemblyTypes(assembly))
                {
                    object[] attributes = type.GetCustomAttributes(typeof(TAttr), false);
                    if (attributes.Length > 0)
                    {
                        TAttr attribute = attributes[0] as TAttr;
                        if (attribute == null) continue;

                        foundTypes.Add(type, attribute);
                    }
                }
            }

            return foundTypes;
        }

        /// <summary>
        /// Returns all types in an assembly.
        /// </summary
        public static Type[] GetAssemblyTypes(Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                return e.Types.Where(t => t != null).ToArray();
            }
        }
    }
}
