using System;
using System.Threading.Tasks;
using DevStatsSystem.Core;
using DevStatsSystem.Core.SerializedData;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Debug = UnityEngine.Debug;

namespace DevStatsSystem.UI
{
    internal class StatsPanel : ADevStatsPanel
    {
        private const string UXML_PATH = "DevStats/UXML/StatsPanel";
        private const string TODAY_STATS_ELEMENT_TAG = "today-stats-element";
        private const string WEEK_STATS_ELEMENT_TAG = "week-stats-element";
        private const string ALL_TIME_STATS_ELEMENT_TAG = "all-time-stats-element";
        private const string LAST_UPDATED_LABEL_TAG = "last-updated-label";
        private const string FORCE_UPDATE_BUTTON_TAG = "force-update-button";
        private const string LOADING_SCREEN_TAG = "loading-screen";
        
        private TodayStatsElement m_todayStatsElement;
        private TimespanStatsElement m_weekStatsElement;
        private AllTimeStatsElement m_allTimeStatsElement;
        private Label m_lastUpdatedLabel;
        private Button m_forceUpdateButton;
        private VisualElement m_loadingScreen;
        
        private CachedStatsPanelData m_data;
        private bool m_isFetchingData = false;

        public StatsPanel()
        {
            CreateLayout();
        }

        private void CreateLayout()
        {
            VisualTreeAsset uxmlAsset = Resources.Load<VisualTreeAsset>(UXML_PATH);
            uxmlAsset.CloneTree(this);
            
            m_todayStatsElement = this.Q<TodayStatsElement>(TODAY_STATS_ELEMENT_TAG);
            m_weekStatsElement = this.Q<TimespanStatsElement>(WEEK_STATS_ELEMENT_TAG);
            m_allTimeStatsElement = this.Q<AllTimeStatsElement>(ALL_TIME_STATS_ELEMENT_TAG);
            m_lastUpdatedLabel = this.Q<Label>(LAST_UPDATED_LABEL_TAG);
            m_forceUpdateButton = this.Q<Button>(FORCE_UPDATE_BUTTON_TAG);
            m_forceUpdateButton.clicked += ManuallyFetchData;
            m_loadingScreen = this.Q<VisualElement>(LOADING_SCREEN_TAG);
            m_loadingScreen.style.display = DisplayStyle.None;
        }
        
        public override void OnShow()
        {
            m_data = CachedStatsPanelData.Instance;
            DevStats.OnInitializedCallback += TryAutoFetchData;
            TryAutoFetchData();
        }

        public override void OnHide()
        {
            DevStats.OnInitializedCallback -= TryAutoFetchData;
        }

        private async void TryAutoFetchData()
        {
            if (ShouldFetchData(DevStatsSettings.Instance.StatsUpdateFrequency, m_data.LastUpdateTime))
            {
                await FetchData();
            }
            LoadDataToUI();
        }
        
        private bool ShouldFetchData(StatsUpdateFrequency frequency, long lastUpdateTime)
        {
            long tickThreshold = 0;
            switch (frequency)
            {
                case StatsUpdateFrequency.OnceADay:
                    tickThreshold = TimeSpan.TicksPerDay;
                    break;
                case StatsUpdateFrequency.TwiceADay:
                    tickThreshold = TimeSpan.TicksPerDay / 2;
                    break;
                case StatsUpdateFrequency.EveryHour:
                    tickThreshold = TimeSpan.TicksPerHour;
                    break;
                case StatsUpdateFrequency.EveryTenMinutes:
                    tickThreshold = TimeSpan.TicksPerMinute * 10;
                    break;
                case StatsUpdateFrequency.AfterEveryCompile:
                    tickThreshold = 0;
                    break;
            }

            return DateTime.Now.Ticks - lastUpdateTime > tickThreshold;
        }

        private async void ManuallyFetchData()
        {
            await FetchData();
            LoadDataToUI();
        }

        private async Task FetchData()
        {
            if (DevStats.Backend == null)
            {
                return;
            }
            
            if (m_isFetchingData)
            {
                return;
            }
            
            OnFetchDataStarted();
            
            StatsData statsData = await DevStats.Backend.GetStats();
            
            if (statsData.Result.Result != CommandResultType.Success)
            {
                Debug.LogError($"Failed to fetch stats! Output: {statsData.Result.Output}");
                OnFetchDataFinished();
                return;
            }
            
            if (EditorApplication.isCompiling)
            {
                Debug.LogWarning("Editor is compiling. Stopping Run Everything!");
                OnFetchDataFinished();
                return;
            }
            
            // Update data.
            m_data.UpdateData(statsData);
            
            OnFetchDataFinished();
        }

        private void LoadDataToUI()
        {
            m_todayStatsElement.SetData(in m_data.TodayStats);
            m_weekStatsElement.SetData(in m_data.WeekStats);
            m_allTimeStatsElement.SetData(in m_data.AllTimeStats);
            m_lastUpdatedLabel.text = $"Last Updated: {new DateTime(m_data.LastUpdateTime).ToLocalTime():hh:mm tt MM/dd/yyy}";
        }

        private void OnFetchDataStarted()
        {
            m_isFetchingData = true;
            m_forceUpdateButton.enabledSelf = false;
            m_loadingScreen.style.display = DisplayStyle.Flex;
        }

        private void OnFetchDataFinished()
        {
            m_isFetchingData = false;
            m_forceUpdateButton.enabledSelf = true;
            m_loadingScreen.style.display = DisplayStyle.None;
        }
    }
}