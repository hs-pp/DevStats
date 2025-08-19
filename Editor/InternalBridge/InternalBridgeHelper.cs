using System;
using UnityEditor;

namespace DevStats.Editor
{
    public static class InternalBridgeHelper
    {
        public static Type GetSceneHierarchyWindowType()
        {
            return typeof(SceneHierarchyWindow);
        }
    }
}