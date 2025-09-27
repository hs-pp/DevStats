using System.Collections.Generic;
using DevStatsSystem.Core;
using DevStatsSystem.Core.SerializedData;
using UnityEngine;
using UnityEngine.UIElements;

namespace DevStatsSystem.UI
{
    [UxmlElement]
    internal partial class HourlyGraphWidget : VisualElement
    {
        private const string UXML_PATH = "DevStats/UXML/HourlyGraphWidget";
        private const string GRAPH_AREA_TAG = "graph-area";

        private VisualElement m_graphArea;
        
        public HourlyGraphWidget()
        {
            CreateLayout();
            generateVisualContent += OnGenerateVisualContent;
        }

        private void CreateLayout()
        {
            var uxmlAsset = Resources.Load<VisualTreeAsset>(UXML_PATH);
            uxmlAsset.CloneTree(this);
            
            m_graphArea = this.Q<VisualElement>(GRAPH_AREA_TAG);
        }
        
        private void OnGenerateVisualContent(MeshGenerationContext mgc)
        {
            //Debug.Log("GenerateVisualContent ");
        }

        public void SetData(List<TimeSegment> timeSegments)
        {
            m_graphArea.Clear();

            if (timeSegments == null || timeSegments.Count == 0)
            {
                return;
            }
            
            // Add the first time segment.
            m_graphArea.Add(CreateEmptySegment(0, timeSegments[0].StartTime));
            
            for(int i = 1; i < timeSegments.Count; i++)
            {
                // Add the time segment.
                m_graphArea.Add(CreateTimeSegment(timeSegments[i]));
                
                // Add the next empty segment.
                int nextStartTime = i + 1 < timeSegments.Count ? timeSegments[i + 1].StartTime : SECONDS_IN_DAY;
                int endTime = timeSegments[i].StartTime + timeSegments[i].Duration;
                m_graphArea.Add(CreateEmptySegment(endTime, nextStartTime - endTime));
            }
        }

        private const int SECONDS_IN_DAY = 86400;
        private VisualElement CreateEmptySegment(int startTime, int length)
        {
            VisualElement emptySegment = new VisualElement();
            emptySegment.style.flexGrow = length/(float)SECONDS_IN_DAY;
            return emptySegment;
        }
        
        private VisualElement CreateTimeSegment(TimeSegment timeSegment)
        {
            VisualElement timeSegmentElement = new VisualElement();
            timeSegmentElement.style.flexGrow = timeSegment.Duration/(float)SECONDS_IN_DAY;
            timeSegmentElement.tooltip = $"Start: {timeSegment.StartTime} \nLength: {DevStats.SecondsToFormattedTime(timeSegment.Duration)}";
            timeSegmentElement.style.backgroundColor = Color.green;
            return timeSegmentElement;
        }
    }
}