using DevStatsSystem.Core.SerializedData;
using UnityEngine;
using UnityEngine.UIElements;

namespace DevStatsSystem.UI
{
    internal class SettingsPanel : ADevStatsPanel
    {
        private const string UXML_PATH = "DevStats/UXML/SettingsPanel";
        private const string ISENABLED_TOGGLE_TAG = "isenabled-toggle";
        private const string DEBUGMODE_TOGGLE_TAG = "debugmode-toggle";
        private const string STATS_REFRESH_RATE_ENUM_TAG = "stats-refresh-rate-enum";
        private const string POST_FREQUENCY_ENUM_TAG = "post-frequency-enum";
        private const string SAME_FILE_COOLDOWN_ENUM_TAG = "same-file-cooldown-enum";
        private const string APIKEY_FIELD_TAG = "api-key-field";
        private const string KEYSTROKE_TIMEOUT_ENUM_TAG = "keystroke-timeout-enum";
        
        private Toggle m_isEnabledToggle;
        private Toggle m_debugModeToggle;
        private EnumField m_statsRefreshRateEnum;
        private EnumField m_postFrequencyEnum;
        private EnumField m_sameFileCooldownEnum;
        
        // If we ever decide to support other backends, we should move these settings out to its own "wakatime" settings.
        private TextField m_apiKeyField;
        private EnumField m_keystrokeTimeoutEnum;

        public SettingsPanel()
        {
            CreateLayout();
        }

        private void CreateLayout()
        {
            VisualTreeAsset uxmlAsset = Resources.Load<VisualTreeAsset>(UXML_PATH);
            uxmlAsset.CloneTree(this);
            
            m_isEnabledToggle = this.Q<Toggle>(ISENABLED_TOGGLE_TAG);
            m_debugModeToggle = this.Q<Toggle>(DEBUGMODE_TOGGLE_TAG);
            m_statsRefreshRateEnum = this.Q<EnumField>(STATS_REFRESH_RATE_ENUM_TAG);
            m_postFrequencyEnum = this.Q<EnumField>(POST_FREQUENCY_ENUM_TAG);
            m_sameFileCooldownEnum = this.Q<EnumField>(SAME_FILE_COOLDOWN_ENUM_TAG);
            m_apiKeyField = this.Q<TextField>(APIKEY_FIELD_TAG);
            m_keystrokeTimeoutEnum = this.Q<EnumField>(KEYSTROKE_TIMEOUT_ENUM_TAG);
            
            m_isEnabledToggle.value = DevStatsSettings.Instance.IsEnabled;
            m_isEnabledToggle.RegisterValueChangedCallback(evt => DevStatsSettings.Instance.IsEnabled = evt.newValue);
            m_debugModeToggle.value = DevStatsSettings.Instance.PrintDebugLogs;
            m_debugModeToggle.RegisterValueChangedCallback(evt => DevStatsSettings.Instance.PrintDebugLogs = evt.newValue);
            m_statsRefreshRateEnum.value = DevStatsSettings.Instance.StatsRefreshRate;
            m_statsRefreshRateEnum.RegisterValueChangedCallback(evt => DevStatsSettings.Instance.StatsRefreshRate = (StatsRefreshRate)evt.newValue);
            m_postFrequencyEnum.value = DevStatsSettings.Instance.PostFrequency;
            m_postFrequencyEnum.RegisterValueChangedCallback(evt => DevStatsSettings.Instance.PostFrequency = (PostFrequency)evt.newValue);
            m_sameFileCooldownEnum.value = DevStatsSettings.Instance.SameFileCooldown;
            m_sameFileCooldownEnum.RegisterValueChangedCallback(evt => DevStatsSettings.Instance.SameFileCooldown = (SameFileCooldown)evt.newValue);
            m_apiKeyField.value = DevStatsSettings.Instance.APIKey;
            m_apiKeyField.RegisterValueChangedCallback(evt => DevStatsSettings.Instance.APIKey = evt.newValue);
            m_keystrokeTimeoutEnum.value = DevStatsSettings.Instance.KeystrokeTimeout;
            m_keystrokeTimeoutEnum.RegisterValueChangedCallback(evt => DevStatsSettings.Instance.KeystrokeTimeout = (KeystrokeTimeout)evt.newValue);
        }
    }
}