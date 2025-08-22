using UnityEngine;
using UnityEngine.UIElements;

namespace DevStatsSystem.Editor.UI
{
    internal class HeartbeatPanel : ADevStatsPanel
    {
        private const string UXML_PATH = "DevStats/UXML/HeartbeatPanel";

        public HeartbeatPanel()
        {
            CreateLayout();
        }

        private void CreateLayout()
        {
            var uxmlAsset = Resources.Load<VisualTreeAsset>(UXML_PATH);
            uxmlAsset.CloneTree(this);
        }
        
        public override void OnShow()
        {
            
        }

        public override void OnHide()
        {
            
        }
    }
}