using UnityEngine;
using UnityEngine.UIElements;

namespace DevStatsSystem.UI
{
    internal class AboutPanel : ADevStatsPanel
    {
        private const string UXML_PATH = "DevStats/UXML/AboutPanel";

        public AboutPanel()
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