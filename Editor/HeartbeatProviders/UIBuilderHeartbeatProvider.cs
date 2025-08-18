using UnityEditor;

namespace DevStats.Editor
{
    public class UIBuilderHeartbeatProvider : AHeartbeatProvider
    {
        public override void Initialize()
        {
            EditorWindow.windowFocusChanged += OnWindowFocusChanged;
        }

        public override void Deinitialize()
        {
            EditorWindow.windowFocusChanged -= OnWindowFocusChanged;
        }
        
        private void OnWindowFocusChanged()
        {
            var focusedWindow = EditorWindow.focusedWindow;
            UIBuilderExtensions a;
        }
    }
}