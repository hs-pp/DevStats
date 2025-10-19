using System;
using UnityEditor;

namespace DevStatsSystem.Core
{
    public static class InternalBridgeHelper
    {
        /// <summary>
        /// We need this for one of our heartbeat scenarios.
        /// </summary>
        public static Type GetSceneHierarchyWindowType()
        {
            return typeof(SceneHierarchyWindow);
        }
    }
}