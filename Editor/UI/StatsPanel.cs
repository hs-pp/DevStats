using DevStatsSystem.Editor.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace DevStatsSystem.Editor.UI
{
    internal class StatsPanel : ADevStatsPanel
    {
        private const string UXML_PATH = "DevStats/UXML/StatsPanel";
        private const string TEST_BUTTON_TAG = "test-button";
        
        private Button m_testButton;

        public StatsPanel()
        {
            CreateLayout();
        }

        private void CreateLayout()
        {
            var uxmlAsset = Resources.Load<VisualTreeAsset>(UXML_PATH);
            uxmlAsset.CloneTree(this);
            
            m_testButton = this.Q<Button>(TEST_BUTTON_TAG);
            m_testButton.clicked += () =>
            {
                //WakatimeWebRequests.GetSummariesRequest(7, null);
                //WakatimeWebRequests.GetStatsRequest(null);
                WakatimeWebRequests.GetDayDurationRequest(null);
            };
        }
        
        public override void OnShow()
        {
            
        }

        public override void OnHide()
        {
            
        }
    }
}