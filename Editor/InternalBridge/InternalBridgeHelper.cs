using System;
using Unity.UI.Builder;
using UnityEditor;

namespace DevStatsSystem.Core
{
    public static class InternalBridgeHelper
    {
        public static Type GetSceneHierarchyWindowType()
        {
            return typeof(SceneHierarchyWindow);
        }

        public static Type GetUIBuilderWindowType()
        {
            return typeof(Builder);
        }
    }
}