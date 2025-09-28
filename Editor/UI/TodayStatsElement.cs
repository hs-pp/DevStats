using DevStatsSystem.Core;
using DevStatsSystem.Core.SerializedData;
using UnityEngine;
using UnityEngine.UIElements;

namespace DevStatsSystem.UI
{
    [UxmlElement]
    internal partial class TodayStatsElement : VisualElement
    {
        private const string UXML_PATH = "DevStats/UXML/TodayStatsElement";
        private const string HOURLY_GRAPH_TAG = "hourly-graph";
        private const string TOTAL_TIME_LABEL_TAG = "total-time-label";
        private const string CODE_TO_ASSET_PERCENTAGE_ELEMENT_TAG = "code-to-asset-percentage-element";
        
        private HourlyGraphElement m_hourlyGraph;
        private Label m_totalTimeLabel;
        private CodeToAssetPercentageElement m_codeToAssetPercentageElement;

        public TodayStatsElement()
        {
            CreateLayout();
        }

        private void CreateLayout()
        {
            var uxmlAsset = Resources.Load<VisualTreeAsset>(UXML_PATH);
            uxmlAsset.CloneTree(this);

            m_hourlyGraph = this.Q<HourlyGraphElement>(HOURLY_GRAPH_TAG);
            m_totalTimeLabel = this.Q<Label>(TOTAL_TIME_LABEL_TAG);
            m_codeToAssetPercentageElement = this.Q<CodeToAssetPercentageElement>(CODE_TO_ASSET_PERCENTAGE_ELEMENT_TAG);
        }

        public void SetData(in TodayStats data)
        {
            m_hourlyGraph.SetData(data.DayTimeSegments);
            m_codeToAssetPercentageElement.SetData(data.CodeTime, data.AssetTime);

            m_totalTimeLabel.text = $"{DevStats.SecondsToFormattedTimePassed(data.TotalTime)}";
        }
    }
}
