using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace DevStatsSystem.Editor.UI
{
    internal class DevStatsWindow : EditorWindow
    {
        [Serializable]
        private enum TabType
        {
            Stats,
            Heartbeats,
            Settings,
            About,
        }

        private class TabInstance
        {
            public TabType Type;
            public VisualElement Button;
            public ADevStatsPanel Panel;
        }
        
        private const string UXML_PATH = "DevStats/UXML/DevStatsWindow";
        private const string STATS_TAB_TAG = "stats-tab";
        private const string HEARTBEATS_TAB_TAG = "heartbeats-tab";
        private const string SETTINGS_TAB_TAG = "settings-tab";
        private const string ABOUT_TAB_TAG = "about-tab";
        private const string CONTENT_AREA_TAG = "content-area";
        
        private static Color SELECTED_TAB_COLOR = new Color(0.21875f, 0.21875f, 0.21875f);
        private static Color UNSELECTED_TAB_COLOR = new Color(0.164f, 0.164f, 0.164f);

        private VisualElement m_statsTab;
        private VisualElement m_heartbeatsTab;
        private VisualElement m_settingsTab;
        private VisualElement m_aboutTab;
        private VisualElement m_contentArea;
        
        private List<TabInstance> m_tabInstances = new();

        [SerializeField]
        private TabType m_openTabType = TabType.About;

        [MenuItem("Window/DevStats")]
        public static void OpenWindow()
        {
            GetWindow<DevStatsWindow>().Show();
        }

        public void OnEnable()
        {
            titleContent = new GUIContent("DevStats");

            CreateLayout();
            OpenTab(m_openTabType);
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
            
            m_statsTab.AddManipulator(new Clickable(() => { OpenTab(TabType.Stats); }));
            m_heartbeatsTab.AddManipulator(new Clickable(() => { OpenTab(TabType.Heartbeats); }));
            m_settingsTab.AddManipulator(new Clickable(() => { OpenTab(TabType.Settings); }));
            m_aboutTab.AddManipulator(new Clickable(() => { OpenTab(TabType.About); }));
            
            m_tabInstances.Add(new TabInstance()
            {
                Type = TabType.Stats,
                Button = m_statsTab,
                Panel = new StatsPanel(),
            });
            m_tabInstances.Add(new TabInstance()
            {
                Type = TabType.Heartbeats,
                Button = m_heartbeatsTab,
                Panel = new HeartbeatsPanel(),
            });
            m_tabInstances.Add(new TabInstance()
            {
                Type = TabType.Settings,
                Button = m_settingsTab,
                Panel = new SettingsPanel(),
            });
            m_tabInstances.Add(new TabInstance()
            {
                Type = TabType.About,
                Button = m_aboutTab,
                Panel = new AboutPanel(),
            });
        }

        private void OpenTab(TabType tabType)
        {
            TabInstance instance = m_tabInstances.Find(x => x.Type == tabType);
            
            SetSelectedButton(instance);
            OpenPanel(instance);
            m_openTabType = tabType;
        }
        
        private void SetSelectedButton(TabInstance tabInstance)
        {
            m_statsTab.style.backgroundColor = UNSELECTED_TAB_COLOR;
            m_heartbeatsTab.style.backgroundColor = UNSELECTED_TAB_COLOR;
            m_settingsTab.style.backgroundColor = UNSELECTED_TAB_COLOR;
            m_aboutTab.style.backgroundColor = UNSELECTED_TAB_COLOR;
            
            tabInstance.Button.style.backgroundColor = SELECTED_TAB_COLOR;
        }

        private void OpenPanel(TabInstance tabInstance)
        {
            foreach (VisualElement child in m_contentArea.Children())
            {
                if (child is ADevStatsPanel childPanel)
                {
                    childPanel.OnHide();
                }
            }
            m_contentArea.Clear();
            
            tabInstance.Panel.OnShow();
            m_contentArea.Add(tabInstance.Panel);
        }
    }
}