using DevStatsSystem.Editor.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace DevStatsSystem.Editor.UI
{
    internal class SettingsPanel : ADevStatsPanel
    {
        private const string UXML_PATH = "DevStats/UXML/SettingsPanel";
        private const string APIKEY_FIELD_TAG = "api-key-field";
        private const string ISENABLED_TOGGLE_TAG = "isenabled-toggle";
        private const string DEBUGMODE_TOGGLE_TAG = "debugmode-toggle";

        private TextField m_apiKeyField;
        private Toggle m_isEnabledToggle;
        private Toggle m_debugModeToggle;

        public SettingsPanel()
        {
            CreateLayout();
        }

        private void CreateLayout()
        {
            var uxmlAsset = Resources.Load<VisualTreeAsset>(UXML_PATH);
            uxmlAsset.CloneTree(this);
            
            m_apiKeyField = this.Q<TextField>(APIKEY_FIELD_TAG);
            m_isEnabledToggle = this.Q<Toggle>(ISENABLED_TOGGLE_TAG);
            m_debugModeToggle = this.Q<Toggle>(DEBUGMODE_TOGGLE_TAG);

            m_apiKeyField.value = DevStatsSettings.Instance.APIKey;
            m_apiKeyField.RegisterValueChangedCallback(evt => DevStatsSettings.Instance.APIKey = evt.newValue);
            m_isEnabledToggle.value = DevStatsSettings.Instance.IsEnabled;
            m_isEnabledToggle.RegisterValueChangedCallback(evt => DevStatsSettings.Instance.SetIsEnabled(evt.newValue));
            m_debugModeToggle.value = DevStatsSettings.Instance.PrintDebugLogs;
            m_debugModeToggle.RegisterValueChangedCallback(
                evt => DevStatsSettings.Instance.PrintDebugLogs = evt.newValue);
        }
        
        public override void OnShow()
        {
            
        }

        public override void OnHide()
        {
            
        }
    }
}