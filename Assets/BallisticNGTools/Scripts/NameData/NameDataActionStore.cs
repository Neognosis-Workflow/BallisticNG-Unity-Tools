using System.Collections.Generic;
using UnityEngine;

namespace NgData.NameData
{
    [ExecuteAlways]
    public class NameDataActionStore : MonoBehaviour
    {
        [HideInInspector] 
        public List<string> LastActions = new List<string>();

        private void Awake()
        {
            hideFlags = HideFlags.DontSaveInBuild;
        }
    }
}