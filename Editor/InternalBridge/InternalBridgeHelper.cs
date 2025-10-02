using System;
using UnityEditor;

namespace DevStatsSystem.Core
{
    public static class InternalBridgeHelper
    {
        public static Type GetSceneHierarchyWindowType()
        {
            return typeof(SceneHierarchyWindow);
        }
    }
}