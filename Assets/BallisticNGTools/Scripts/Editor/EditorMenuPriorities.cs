using UnityEngine;
using UnityEditor;

public struct EditorMenuPriorities
{
    public struct UtilitiesMenu
    {
        public const int SceneUpgrader = 1;
        public const int AffineActivator = 2;
        public const int TrmEditor = 21;
        public const int TrmDataTransfer = 22;
        public const int ExportReverseTrm = 41;
        public const int ExportTrackDiffuse = 42;
        public const int ExportTrackIllim = 43;
    }
}