using DevStatsSystem.Core;
using DevStatsSystem.Core.SerializedData;
using UnityEngine;
using UnityEngine.UIElements;

namespace DevStatsSystem.UI
{
    [UxmlElement]
    internal partial class TodayStatsWidget : VisualElement
    {
        private const string UXML_PATH = "DevStats/UXML/TodayStatsWidget";
        private const string HOURLY_GRAPH_TAG = "hourly-graph";
        private const string TOTAL_TIME_LABEL_TAG = "total-time-label";
        private const string CODE_PERCENTAGE_ELEMENT_TAG = "code-percentage-element";
        private const string ASSET_PERCENTAGE_ELEMENT_TAG = "asset-percentage-element";

        private HourlyGraphWidget m_hourlyGraph;
        private Label m_totalTimeLabel;
        private VisualElement m_codePercentageElement;
        private VisualElement m_assetPercentageElement;

        public TodayStatsWidget()
        {
            CreateLayout();
        }

        private void CreateLayout()
        {
            var uxmlAsset = Resources.Load<VisualTreeAsset>(UXML_PATH);
            uxmlAsset.CloneTree(this);

            m_hourlyGraph = this.Q<HourlyGraphWidget>(HOURLY_GRAPH_TAG);
            m_totalTimeLabel = this.Q<Label>(TOTAL_TIME_LABEL_TAG);
            m_codePercentageElement = this.Q<VisualElement>(CODE_PERCENTAGE_ELEMENT_TAG);
            m_assetPercentageElement = this.Q<VisualElement>(ASSET_PERCENTAGE_ELEMENT_TAG);
        }

        public void SetData(in TodayStats data)
        {
            m_hourlyGraph.SetData(data.DayTimeSegments);
            
            m_totalTimeLabel.text = $"Total Time: {DevStats.SecondsToFormattedTime(data.TotalTime)}";
            float totalPercentage = data.CodePercentage + data.UnityAssetPercentage;

            m_codePercentageElement.style.flexGrow = data.CodePercentage / totalPercentage;
            m_assetPercentageElement.style.flexGrow = data.UnityAssetPercentage / totalPercentage;
        }
    }
}
