using System.Collections.Generic;
using DevStatsSystem.Core;
using DevStatsSystem.Core.SerializedData;
using UnityEngine;
using UnityEngine.UIElements;

namespace DevStatsSystem.UI
{
    [UxmlElement]
    internal partial class HourlyGraphElement : VisualElement
    {
        private const string UXML_PATH = "DevStats/UXML/HourlyGraphElement";
        private const string GRAPH_AREA_TAG = "graph-area";
        
        private VisualElement m_graphArea;
        private List<TimeSegmentElement> m_timeSegmentElements = new();
        
        public HourlyGraphElement()
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
            
            schedule.Execute(RedrawTimeSegmentElements).ExecuteLater(1); // 1 frame later so style resolves.
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
        private static Color NORMAL_COLOR = Color.deepSkyBlue;
        private static Color HOVER_COLOR = Color.white;

        private TimeSegment m_timeSegment;
        
        public TimeSegmentElement(TimeSegment timeSegment)
        {
            m_timeSegment = timeSegment;
            style.position = Position.Absolute;
            style.backgroundColor = NORMAL_COLOR;
            tooltip = @$"{DevStats.SecondsToFormattedTimeSinceMidnight(timeSegment.StartTime)} - {DevStats.SecondsToFormattedTimeSinceMidnight(timeSegment.StartTime + timeSegment.Duration)}
({DevStats.SecondsToFormattedTimePassed(timeSegment.Duration)})";
            
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
    }
}