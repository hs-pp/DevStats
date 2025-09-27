using System;
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
        private List<TimeSegmentElement> m_timeSegmentElements = new();
        
        public HourlyGraphWidget()
        {
            CreateLayout();
            
            RegisterCallback<GeometryChangedEvent>(evt =>
            {
                RedrawTimeSegmentElements();
            });
        }

        private void CreateLayout()
        {
            var uxmlAsset = Resources.Load<VisualTreeAsset>(UXML_PATH);
            uxmlAsset.CloneTree(this);
            
            m_graphArea = this.Q<VisualElement>(GRAPH_AREA_TAG);
        }

        public void SetData(List<TimeSegment> timeSegments)
        {
            ClearTimeSegments();

            if (timeSegments == null || timeSegments.Count == 0)
            {
                return;
            }

            foreach (TimeSegment timeSegment in timeSegments)
            {
                TimeSegmentElement timeSegmentElement = new TimeSegmentElement(timeSegment);
                m_graphArea.Add(timeSegmentElement);
                m_timeSegmentElements.Add(timeSegmentElement);
            }
            
            schedule.Execute(RedrawTimeSegmentElements).ExecuteLater(1); // 1 frame later
        }
        
        private VisualElement CreateTimeSegmentElement(TimeSegment timeSegment)
        {
            VisualElement timeSegmentElement = new VisualElement();


            m_graphArea.Add(timeSegmentElement);
            return timeSegmentElement;
        }

        private void RedrawTimeSegmentElements()
        {
            float width = resolvedStyle.width;
            if (float.IsNaN(width) || width == 0)
            {
                return;
            }

            foreach (TimeSegmentElement timeSegmentElement in m_timeSegmentElements)
            {
                timeSegmentElement.Redraw(m_graphArea.resolvedStyle.width);
            }
        }
        
        private void ClearTimeSegments()
        {
            foreach (TimeSegmentElement timeSegmentElement in m_timeSegmentElements)
            {
                timeSegmentElement.RemoveFromHierarchy();
            }
            m_timeSegmentElements.Clear();
        }
    }

    internal class TimeSegmentElement : VisualElement
    {
        private const float SECONDS_IN_DAY = 86400;
        private static Color NORMAL_COLOR = Color.white;
        private static Color HOVER_COLOR = Color.yellow;

        private TimeSegment m_timeSegment;
        
        public TimeSegmentElement(TimeSegment timeSegment)
        {
            m_timeSegment = timeSegment;
            style.position = Position.Absolute;
            style.backgroundColor = NORMAL_COLOR;
            tooltip = $"Start: {SecondsToFormattedStartTime(timeSegment.StartTime)} \nLength: {DevStats.SecondsToFormattedTime(timeSegment.Duration)}";
            
            RegisterCallback<MouseEnterEvent>(_ =>
            {
                style.backgroundColor = HOVER_COLOR;
            });

            RegisterCallback<MouseLeaveEvent>(_ =>
            {
                style.backgroundColor = NORMAL_COLOR;
            });
        }

        public void Redraw(float width)
        {
            float xStart = (m_timeSegment.StartTime / SECONDS_IN_DAY) * width;
            float xStop = ((m_timeSegment.Duration) / SECONDS_IN_DAY) * width;
                
            style.left = xStart;
            style.width = xStop;
            style.top = 0;
            style.bottom = 0;
        }
        
        private string SecondsToFormattedStartTime(float startTime)
        {
            return DateTime.Today.AddSeconds(startTime).ToString("h:mmtt");
        }
    }
}