using DevStatsSystem.Core;
using DevStatsSystem.Core.SerializedData;
using UnityEngine;
using UnityEngine.UIElements;

namespace DevStatsSystem.UI
{
    internal class SettingsPanel : ADevStatsPanel
    {
        private const string UXML_PATH = "DevStats/UXML/SettingsPanel";
        private const string ISENABLED_TOGGLE_TAG = "isenabled-toggle";
        private const string STATS_REFRESH_RATE_ENUM_TAG = "stats-refresh-rate-enum";
        private const string POST_FREQUENCY_ENUM_TAG = "post-frequency-enum";
        private const string SAME_FILE_COOLDOWN_ENUM_TAG = "same-file-cooldown-enum";
        private const string BACKEND_WIDGET_AREA_TAG = "backend-widget-area";

        private Toggle m_isEnabledToggle;
        private EnumField m_statsRefreshRateEnum;
        private EnumField m_postFrequencyEnum;
        private EnumField m_sameFileCooldownEnum;
        private VisualElement m_backendWidgetArea;

        public SettingsPanel()
        {
            CreateLayout();
        }

        private void CreateLayout()
        {
            VisualTreeAsset uxmlAsset = Resources.Load<VisualTreeAsset>(UXML_PATH);
            uxmlAsset.CloneTree(this);
            
            m_isEnabledToggle = this.Q<Toggle>(ISENABLED_TOGGLE_TAG);
            m_statsRefreshRateEnum = this.Q<EnumField>(STATS_REFRESH_RATE_ENUM_TAG);
            m_postFrequencyEnum = this.Q<EnumField>(POST_FREQUENCY_ENUM_TAG);
            m_sameFileCooldownEnum = this.Q<EnumField>(SAME_FILE_COOLDOWN_ENUM_TAG);
            m_backendWidgetArea = this.Q<VisualElement>(BACKEND_WIDGET_AREA_TAG);

            m_isEnabledToggle.value = DevStatsSettings.Instance.IsEnabled;
            m_isEnabledToggle.RegisterValueChangedCallback(evt => DevStatsSettings.Instance.IsEnabled = evt.newValue);
            m_statsRefreshRateEnum.value = DevStatsSettings.Instance.StatsRefreshRate;
            m_statsRefreshRateEnum.RegisterValueChangedCallback(evt => DevStatsSettings.Instance.StatsRefreshRate = (StatsRefreshRate)evt.newValue);
            m_postFrequencyEnum.value = DevStatsSettings.Instance.PostFrequency;
            m_postFrequencyEnum.RegisterValueChangedCallback(evt => DevStatsSettings.Instance.PostFrequency = (PostFrequency)evt.newValue);
            m_sameFileCooldownEnum.value = DevStatsSettings.Instance.SameFileCooldown;
            m_sameFileCooldownEnum.RegisterValueChangedCallback(evt => DevStatsSettings.Instance.SameFileCooldown = (SameFileCooldown)evt.newValue);

            AddBackendSettingsWidget(DevStats.Backend.CreateSettingsWidgetInstance());
        }

        private void AddBackendSettingsWidget(ABackendSettingsWidget backendSettingsWidget)
        {
            m_backendWidgetArea.Clear();
            if (backendSettingsWidget != null)
            {
                m_backendWidgetArea.Add(backendSettingsWidget);
            }
        }
    }
}