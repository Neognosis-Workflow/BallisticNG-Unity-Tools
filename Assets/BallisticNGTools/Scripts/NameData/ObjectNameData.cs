#if UNITY_EDITOR
using System.Collections.Generic;

namespace NgData.NameData
{
    public class ObjectNameData
    {
        /// <summary>
        /// The object type definition.
        /// </summary>
        public string ObjectType;

        /// <summary>
        /// The flags given for the argument.
        /// </summary>
        public List<string> Flags = new List<string>();

        /// <summary>
        /// The variables that were found during the name parsing.
        /// </summary>
        public List<VariableData> Variables = new List<VariableData>();
        
        public class VariableData
        {
            public string Name;
            public string Value;
        }

    }
}
#endif