using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace DevStatsSystem.Editor.UI
{
    internal class DevStatsWindow : EditorWindow
    {
        private enum TabType
        {
            Stats,
            Heartbeats,
            Settings,
            About,
        }

        private class Tab
        {
            public TabType Type;
            public VisualElement TabButton;
            public Type PanelType;
            public ADevStatsPanel PanelInstance;
        }
        
        private const string UXML_PATH = "DevStats/UXML/DevStatsWindow";
        private const string STATS_TAB_TAG = "stats-tab";
        private const string HEARTBEATS_TAB_TAG = "heartbeats-tab";
        private const string SETTINGS_TAB_TAG = "settings-tab";
        private const string ABOUT_TAB_TAG = "about-tab";
        private const string CONTENT_AREA_TAG = "content-area";
        
        private static Color SELECTED_TAB_COLOR = new Color(0.234f, 0.234f, 0.234f);
        private static Color UNSELECTED_TAB_COLOR = new Color(0.164f, 0.164f, 0.164f);

        private VisualElement m_statsTab;
        private VisualElement m_heartbeatsTab;
        private VisualElement m_settingsTab;
        private VisualElement m_aboutTab;
        private VisualElement m_contentArea;
        
        private List<ADevStatsPanel> m_panels = new();
        private ADevStatsPanel m_openPanel;

        [MenuItem("Window/DevStats")]
        public static void OpenWindow()
        {
            GetWindow<DevStatsWindow>().Show();
        }

        public void OnEnable()
        {
            titleContent = new GUIContent("DevStats");

            CreateLayout();

            OpenSettingsTab();
        }

        private void CreateLayout()
        {
            var uxmlAsset = Resources.Load<VisualTreeAsset>(UXML_PATH);
            uxmlAsset.CloneTree(rootVisualElement);
            
            m_statsTab = rootVisualElement.Q<VisualElement>(STATS_TAB_TAG);
            m_heartbeatsTab = rootVisualElement.Q<VisualElement>(HEARTBEATS_TAB_TAG);
            m_settingsTab = rootVisualElement.Q<VisualElement>(SETTINGS_TAB_TAG);
            m_aboutTab = rootVisualElement.Q<VisualElement>(ABOUT_TAB_TAG);
            m_contentArea = rootVisualElement.Q<VisualElement>(CONTENT_AREA_TAG);
            
            m_statsTab.AddManipulator(new Clickable(OpenStatsTab));
            m_heartbeatsTab.AddManipulator(new Clickable(OpenHeartbeatsTab));
            m_settingsTab.AddManipulator(new Clickable(OpenSettingsTab));
            m_aboutTab.AddManipulator(new Clickable(OpenAboutTab));
        }

        private void OpenStatsTab()
        {
            SetSelectedButton(m_statsTab);
            OpenPanelOfType(typeof(StatsPanel));
        }

        private void OpenHeartbeatsTab()
        {
            SetSelectedButton(m_heartbeatsTab);
            OpenPanelOfType(typeof(HeartbeatPanel));
        }
        
        private void OpenSettingsTab()
        {
            SetSelectedButton(m_settingsTab);
            OpenPanelOfType(typeof(SettingsPanel));
        }

        private void OpenAboutTab()
        {
            SetSelectedButton(m_aboutTab);
            OpenPanelOfType(typeof(AboutPanel));
        }
        
        private void SetSelectedButton(VisualElement button)
        {
            m_statsTab.style.backgroundColor = UNSELECTED_TAB_COLOR;
            m_heartbeatsTab.style.backgroundColor = UNSELECTED_TAB_COLOR;
            m_settingsTab.style.backgroundColor = UNSELECTED_TAB_COLOR;
            m_aboutTab.style.backgroundColor = UNSELECTED_TAB_COLOR;
            
            button.style.backgroundColor = SELECTED_TAB_COLOR;
        }
        
        private void OpenPanelOfType(Type panelType)
        {
            ADevStatsPanel panel = m_panels.Find(x => x.GetType() == panelType);
            if (panel == null)
            {
                panel = Activator.CreateInstance(panelType) as ADevStatsPanel;
                m_panels.Add(panel);
            }

            if (m_openPanel != null)
            {
                m_openPanel.OnHide();
                m_contentArea.Clear();
            }
            
            m_openPanel = panel;
            m_openPanel.OnShow();
            m_contentArea.Add(panel);
        }
    }
}