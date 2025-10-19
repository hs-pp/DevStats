using System;
using System.Collections.Generic;
using DevStatsSystem.Core;
using DevStatsSystem.Core.SerializedData;
using UnityEngine;
using UnityEngine.UIElements;

namespace DevStatsSystem.UI
{
    [UxmlElement]
    internal partial class TimespanGraphElement : VisualElement
    {
        private const float MIN_GRAPH_HEIGHT = 2 * 60 * 60; // 2 hours
        private const string UXML_PATH = "DevStats/UXML/TimespanGraphElement";
        private const string GRAPH_CONTAINER_TAG = "graph-container";

        private VisualElement m_graphContainer;
        private List<TimespanGraphDayElement> m_dayElements = new();

        public TimespanGraphElement()
        {
            CreateLayout();
        }

        private void CreateLayout()
        {
            VisualTreeAsset uxmlAsset = Resources.Load<VisualTreeAsset>(UXML_PATH);
            uxmlAsset.CloneTree(this);
            
            m_graphContainer = this.Q<VisualElement>(GRAPH_CONTAINER_TAG);
        }

        public void SetData(List<TimespanDayStat> dayStats)
        {
            foreach (TimespanGraphDayElement dayElement in m_dayElements)
            {
                dayElement.RemoveFromHierarchy();
            }
            m_dayElements.Clear();
            
            if (dayStats == null)
            {
                return;
            }
            
            // Calculate graph height
            float graphHeight = MIN_GRAPH_HEIGHT;
            for (int i = 0; i < dayStats.Count; i++)
            {
                if (dayStats[i].TotalTime > graphHeight)
                {
                    graphHeight = dayStats[i].TotalTime;
                }
            }

            for (int i = 0; i < dayStats.Count; i++)
            {
                TimespanGraphDayElement dayElement = new TimespanGraphDayElement(dayStats[i], (dayStats[i].TotalTime / graphHeight) * 100);
                m_graphContainer.Add(dayElement);
                m_dayElements.Add(dayElement);
            }
        }
    }

    internal class TimespanGraphDayElement : VisualElement
    {
        private const string UXML_PATH = "DevStats/UXML/TimespanGraphDayElement";
        private const string BAR_TAG = "bar";
        private const string HOVER_SHADER_TAG = "hover-shader";

        private VisualElement m_barElement;
        private VisualElement m_hoverShaderElement;
        
        private TimespanDayStat m_dayStat;

        public TimespanGraphDayElement(TimespanDayStat dayStat, float barFillPercentage)
        {
            CreateLayout();
            SetData(dayStat, barFillPercentage);
        }

        private void CreateLayout()
        {
            VisualTreeAsset uxmlAsset = Resources.Load<VisualTreeAsset>(UXML_PATH);
            uxmlAsset.CloneTree(this);
            
            m_barElement = this.Q<VisualElement>(BAR_TAG);
            m_hoverShaderElement = this.Q<VisualElement>(HOVER_SHADER_TAG);
            
            style.flexGrow = 1;
            m_hoverShaderElement.style.display = DisplayStyle.None;

            RegisterCallback<MouseEnterEvent>(_ =>
            {
                m_hoverShaderElement.style.display = DisplayStyle.Flex;
            });

            RegisterCallback<MouseLeaveEvent>(_ =>
            {
                m_hoverShaderElement.style.display = DisplayStyle.None;
            });
        }

        private void SetData(TimespanDayStat dayStat, float barFillPercentage)
        {
            m_dayStat = dayStat;
            m_barElement.style.height = new Length(barFillPercentage, LengthUnit.Percent);
            DateTime day = new DateTime(m_dayStat.Day);
            tooltip = $"{day:ddd, MMM dd}\n{DevStats.SecondsToFormattedTimePassed(m_dayStat.TotalTime)}";
        }
    }
}