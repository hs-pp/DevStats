using System;
using System.Diagnostics;
using System.Threading.Tasks;
using DevStatsSystem.Core.SerializedData;
using DevStatsSystem.Core.Wakatime;
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
        private const string ALL_TIME_STATS_ELEMENT_TAG = "all-time-stats-element";
        private const string LAST_UPDATED_LABEL_TAG = "last-updated-label";
        private const string FORCE_UPDATE_BUTTON_TAG = "force-update-button";
        
        private TodayStatsElement m_todayStatsElement;
        private AllTimeStatsElement m_allTimeStatsElement;
        private Label m_lastUpdatedLabel;
        private Button m_forceUpdateButton;
        
        private CachedStatsPanelData m_data;
        private bool m_isFetchingData = false;

        public StatsPanel()
        {
            CreateLayout();
        }

        private void CreateLayout()
        {
            var uxmlAsset = Resources.Load<VisualTreeAsset>(UXML_PATH);
            uxmlAsset.CloneTree(this);
            
            m_todayStatsElement = this.Q<TodayStatsElement>(TODAY_STATS_ELEMENT_TAG);
            m_allTimeStatsElement = this.Q<AllTimeStatsElement>(ALL_TIME_STATS_ELEMENT_TAG);
            m_lastUpdatedLabel = this.Q<Label>(LAST_UPDATED_LABEL_TAG);
            m_forceUpdateButton = this.Q<Button>(FORCE_UPDATE_BUTTON_TAG);
            m_forceUpdateButton.clicked += ManuallyFetchData;
        }
        
        public override void OnShow()
        {
            m_data = CachedStatsPanelData.Instance;
            TryAutoFetchData();
        }

        public override void OnHide()
        {
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
            if (m_isFetchingData)
            {
                return;
            }
            
            Debug.Log("Started loading everything");
            
            m_isFetchingData = true;
            OnFetchDataStarted();
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                m_isFetchingData = false;
                OnFetchDataFinished();
                return;
            }
            
            var durationsPayload = await WakatimeWebRequests.GetDayDurationRequest();
            Debug.Log($"Durations:\n{durationsPayload.payload}");
            
            await Task.Delay(TimeSpan.FromSeconds(1));
            
            var heartbeatsPayload = await WakatimeWebRequests.GetHeartbeatsRequest();
            Debug.Log($"Heartbeats:\n{heartbeatsPayload.payload}");
            
            await Task.Delay(TimeSpan.FromSeconds(1));

            var statsPayload = await WakatimeWebRequests.GetStatsRequest();
            Debug.Log($"Stats:\n{statsPayload.payload}");

            await Task.Delay(TimeSpan.FromSeconds(1));

            var summariesPayload = await WakatimeWebRequests.GetSummariesRequest(7);
            Debug.Log($"Summaries:\n{summariesPayload.payload}");
            
            if (EditorApplication.isCompiling)
            {
                m_isFetchingData = false;
                Debug.LogWarning("Editor is compiling. Stopping Run Everything");
                return;
            }
            
            // Update data.
            m_data.UpdateData(in durationsPayload.payload, in heartbeatsPayload.payload, in statsPayload.payload, in summariesPayload.payload);
            
            stopwatch.Stop();
            Debug.Log($"Finished loading everything T:{stopwatch.ElapsedMilliseconds/1000f}s");
            
            m_isFetchingData = false;
            OnFetchDataFinished();
        }

        private void LoadDataToUI()
        {
            m_todayStatsElement.SetData(in m_data.TodayStats);
            m_allTimeStatsElement.SetData(in m_data.AllTimeStats);
            m_lastUpdatedLabel.text = $"Last Updated: {new DateTime(m_data.LastUpdateTime).ToLocalTime():hh:mm tt MM/dd/yyy}";
        }

        private void OnFetchDataStarted()
        {
            m_forceUpdateButton.enabledSelf = false;
        }

        private void OnFetchDataFinished()
        {
            m_forceUpdateButton.enabledSelf = true;
        }
    }
}