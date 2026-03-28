#if UNITY_EDITOR
using System;

namespace NgData.NameData
{
    public class RegisterNameDataActionAttribute : Attribute
    {
        public RegisterNameDataActionAttribute(string name)
        {
            Name = name;
        }
        
        public string Name;
    }
}
#endif