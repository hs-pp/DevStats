using DevStatsSystem.Core;
using DevStatsSystem.Core.SerializedData;
using UnityEngine;
using UnityEngine.UIElements;

namespace DevStatsSystem.UI
{
    [UxmlElement]
    internal partial class TimespanStatsElement : VisualElement
    {
        private const string UXML_PATH = "DevStats/UXML/TimespanStatsElement";
        private const string TIMESPAN_LABELED_CONTAINER_TAG = "timespan-container";
        private const string TOTAL_TIME_LABEL_TAG = "total-time-label";
        private const string CODE_TO_ASSET_PERCENTAGE_ELEMENT_TAG = "code-to-asset-percentage-element";
        private const string DAILY_AVERAGE_LABEL_TAG = "daily-average-label";

        private LabeledContainerElement m_labeledContainerElement;
        private Label m_totalTimeLabel;
        private CodeToAssetPercentageElement m_codeToAssetPercentageElement;
        private Label m_dailyAverageLabel;

        public TimespanStatsElement()
        {
            CreateLayout();
        }

        private void CreateLayout()
        {
            var uxmlAsset = Resources.Load<VisualTreeAsset>(UXML_PATH);
            uxmlAsset.CloneTree(this);
            
            m_labeledContainerElement = this.Q<LabeledContainerElement>(TIMESPAN_LABELED_CONTAINER_TAG);
            m_totalTimeLabel = this.Q<Label>(TOTAL_TIME_LABEL_TAG);
            m_codeToAssetPercentageElement = this.Q<CodeToAssetPercentageElement>(CODE_TO_ASSET_PERCENTAGE_ELEMENT_TAG);
            m_dailyAverageLabel = this.Q<Label>(DAILY_AVERAGE_LABEL_TAG);
        }

        public void SetData(in TimespanStats timespanStats)
        {
            m_labeledContainerElement.TitleText = timespanStats.TimespanName;
            m_totalTimeLabel.text = DevStats.SecondsToFormattedTimePassed(timespanStats.TotalTime);
            m_codeToAssetPercentageElement.SetData(timespanStats.CodeTime, timespanStats.AssetTime);
            m_dailyAverageLabel.text = $"Average: {DevStats.SecondsToFormattedTimePassed(timespanStats.DailyAverageTime)}";
        }
    }
}