using DevStatsSystem.Editor.Core;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace DevStatsSystem.Editor.UI
{
    public class DevStatsWindow : EditorWindow
    {
        private static string UXML_PATH = "DevStats/DevStatsWindow";

        private static string TEST_BUTTON_TAG = "test-button";
        private static string APIKEY_FIELD_TAG = "api-key-field";
        private static string ISENABLED_TOGGLE_TAG = "isenabled-toggle";
        private static string DEBUGMODE_TOGGLE_TAG = "debugmode-toggle";

        private Button m_testButton;
        private TextField m_apiKeyField;
        private Toggle m_isEnabledToggle;
        private Toggle m_debugModeToggle;

        [MenuItem("Window/DevStats")]
        public static void OpenWindow()
        {
            GetWindow<DevStatsWindow>().Show();
        }

        public void OnEnable()
        {
            titleContent = new GUIContent("DevStats");

            CreateLayout();
        }

        private void CreateLayout()
        {
            var uxmlAsset = Resources.Load<VisualTreeAsset>(UXML_PATH);
            uxmlAsset.CloneTree(rootVisualElement);

            m_testButton = rootVisualElement.Q<Button>(TEST_BUTTON_TAG);
            m_apiKeyField = rootVisualElement.Q<TextField>(APIKEY_FIELD_TAG);
            m_isEnabledToggle = rootVisualElement.Q<Toggle>(ISENABLED_TOGGLE_TAG);
            m_debugModeToggle = rootVisualElement.Q<Toggle>(DEBUGMODE_TOGGLE_TAG);
            
            m_testButton.clicked += TestButtonClicked;
            m_apiKeyField.value = DevStatsSettings.Get().APIKey;
            m_apiKeyField.RegisterValueChangedCallback(evt => DevStatsSettings.Get().SetAPIKey(evt.newValue));
            m_isEnabledToggle.value = DevStatsSettings.Get().IsEnabled;
            m_isEnabledToggle.RegisterValueChangedCallback(evt => DevStatsSettings.Get().SetIsEnabled(evt.newValue));
            m_debugModeToggle.value = DevStatsSettings.Get().IsDebugMode;
            m_debugModeToggle.RegisterValueChangedCallback(evt => DevStatsSettings.Get().SetDebugMode(evt.newValue));
        }

        private void TestButtonClicked()
        {
            Debug.Log(DevStats.GetTimeRemainingDebug());
        }
    }
}