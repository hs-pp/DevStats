using DevStatsSystem.Core;
using DevStatsSystem.Core.SerializedData;
using UnityEngine;
using UnityEngine.UIElements;

namespace DevStatsSystem.UI
{
    [UxmlElement]
    internal partial class AllTimeStatsElement : VisualElement
    {
        private const string UXML_PATH = "DevStats/UXML/AllTimeStatsElement";
        private const string PROJECT_TOTAL_TIME_LABEL_TAG = "project-total-time-label";
        private const string GRAND_TOTAL_TIME_LABEL_TAG = "grand-total-time-label";
        private const string DAILY_AVERAGE_LABEL_TAG = "daily-average-label";
        
        private Label m_projectTotalTimeLabel;
        private Label m_grandTotalTimeLabel;
        private Label m_dailyAverageLabel;
        
        public AllTimeStatsElement()
        {
            CreateLayout();
        }

        private void CreateLayout()
        {
            VisualTreeAsset uxmlAsset = Resources.Load<VisualTreeAsset>(UXML_PATH);
            uxmlAsset.CloneTree(this);
            
            m_projectTotalTimeLabel = this.Q<Label>(PROJECT_TOTAL_TIME_LABEL_TAG);
            m_grandTotalTimeLabel = this.Q<Label>(GRAND_TOTAL_TIME_LABEL_TAG);
            m_dailyAverageLabel = this.Q<Label>(DAILY_AVERAGE_LABEL_TAG);
        }

        public void SetData(in AllTimeStats allTimeStats)
        {
            m_projectTotalTimeLabel.text = $"Project Total: {DevStats.SecondsToFormattedTimePassed(allTimeStats.ProjectTotalTime)}";
            m_grandTotalTimeLabel.text = $"Grand Total: {DevStats.SecondsToFormattedTimePassed(allTimeStats.GrandTotalTime)}";
            m_dailyAverageLabel.text = $"Daily Average: {DevStats.SecondsToFormattedTimePassed(allTimeStats.DailyAverageTime)}";
        }
    }
}