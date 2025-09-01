using System;
using System.Text;
using DevStatsSystem.Editor.Core;
using DevStatsSystem.Editor.Core.DTOs;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UIElements;

namespace DevStatsSystem.Editor.UI
{
    internal class HeartbeatsPanel : ADevStatsPanel
    {
        private const string URL_PREFIX = "https://api.wakatime.com/api/v1/";
        
        private const string UXML_PATH = "DevStats/UXML/HeartbeatsPanel";
        private const string HEARTBEATS_IN_QUEUE_LABEL_TAG = "heartbeats-in-queue-label";
        private const string UNTIL_NEXT_SEND_LABEL_TAG = "until-next-send-label";
        private const string QUEUED_HEARTBEATS_LISTVIEW_TAG = "queued-heartbeats-listview";
        private const string FAILED_TO_SEND_AREA_TAG = "failed-to-send-area";
        private const string FAILED_TO_SEND_LISTVIEW_TAG = "failed-to-send-listview";
        private const string FAILED_TO_SEND_RETRY_BUTTON_TAG = "failed-to-send-retry-button";
        private const string SENT_HISTORY_LISTVIEW_TAG = "sent-history-listview";
        
        private Label m_heartbeatsInQueueLabel;
        private Label m_untilNextSendLabel;
        private ListView m_queuedHeartbeatsListView;
        private VisualElement m_failedToSendArea;
        private ListView m_failedToSendListView;
        private Button m_failedToSendRetryButton;
        private ListView m_sentHistoryListView;
        
        private IVisualElementScheduledItem m_untilNextSendSchedule;

        public HeartbeatsPanel()
        {
            CreateLayout();
        }

        private void CreateLayout()
        {
            var uxmlAsset = Resources.Load<VisualTreeAsset>(UXML_PATH);
            uxmlAsset.CloneTree(this);
            
            m_heartbeatsInQueueLabel = this.Q<Label>(HEARTBEATS_IN_QUEUE_LABEL_TAG);
            m_untilNextSendLabel = this.Q<Label>(UNTIL_NEXT_SEND_LABEL_TAG);
            m_queuedHeartbeatsListView = this.Q<ListView>(QUEUED_HEARTBEATS_LISTVIEW_TAG);
            m_queuedHeartbeatsListView.makeItem += () => new HeartbeatElement();
            m_queuedHeartbeatsListView.bindItem += (element, i) => { (element as HeartbeatElement).BindHeartbeat((Heartbeat)m_queuedHeartbeatsListView.itemsSource[i]); };
            m_queuedHeartbeatsListView.unbindItem += (element, i) => { (element as HeartbeatElement).UnbindHeartbeat(); };
            
            m_failedToSendArea  = this.Q<VisualElement>(FAILED_TO_SEND_AREA_TAG);
            m_failedToSendListView = this.Q<ListView>(FAILED_TO_SEND_LISTVIEW_TAG);
            m_failedToSendListView.makeItem += () => new FailedToSendInstanceElement();
            m_failedToSendListView.bindItem += (element, i) => { (element as FailedToSendInstanceElement).BindFailedToSendInstance((FailedToSendInstance)m_failedToSendListView.itemsSource[i]); };
            m_failedToSendListView.unbindItem += (element, i) => { (element as FailedToSendInstanceElement).UnbindFailedToSendInstance(); };
            m_failedToSendRetryButton = this.Q<Button>(FAILED_TO_SEND_RETRY_BUTTON_TAG);
            m_failedToSendRetryButton.clicked += OnFailedToSendRetryPressed;

            m_sentHistoryListView = this.Q<ListView>(SENT_HISTORY_LISTVIEW_TAG);
            m_sentHistoryListView.makeItem += () => new SentHeartbeatsInstanceElement();
            m_sentHistoryListView.bindItem += (element, i) => { (element as SentHeartbeatsInstanceElement).BindSentHeartbeatsInstance((SentHeartbeatsInstance)m_sentHistoryListView.itemsSource[i]); };
            m_sentHistoryListView.unbindItem += (element, i) => { (element as SentHeartbeatsInstanceElement).UnbindSentHeartbeatsInstance(); };
        }

        public override void OnShow()
        {
            DevStatsState.Instance.OnQueuedHeartbeatsChanged += OnHeartbeatsInQueueChanged;
            DevStatsData.Instance.OnSentHeartbeatsInstancesChanged += OnSentHeartbeatsInstancesChanged;
            DevStatsData.Instance.OnFailedToSendInstancesChanged += OnFailedToSendInstancesChanged;
            
            m_untilNextSendSchedule = schedule.Execute(UpdateUntilNextSendLabel).Every(500);
            m_queuedHeartbeatsListView.itemsSource = DevStatsState.Instance.GetQueuedHeartbeats();
            m_sentHistoryListView.itemsSource = DevStatsData.Instance.GetSentHeartbeatsInstances();
            m_failedToSendListView.itemsSource = DevStatsData.Instance.GetFailedToSendInstances();
            
            OnHeartbeatsInQueueChanged();
            OnSentHeartbeatsInstancesChanged();
            OnFailedToSendInstancesChanged();
        }

        public override void OnHide()
        {
            DevStatsState.Instance.OnQueuedHeartbeatsChanged -= OnHeartbeatsInQueueChanged;
            DevStatsData.Instance.OnSentHeartbeatsInstancesChanged -= OnSentHeartbeatsInstancesChanged;
            DevStatsData.Instance.OnFailedToSendInstancesChanged -= OnFailedToSendInstancesChanged;

            m_untilNextSendSchedule.Pause();
            m_queuedHeartbeatsListView.itemsSource = null;
            m_sentHistoryListView.itemsSource = null;
            m_failedToSendListView.itemsSource = null;
        }

        private void OnHeartbeatsInQueueChanged()
        {
            m_heartbeatsInQueueLabel.text = $"In Queue: {DevStatsState.Instance.GetQueuedHeartbeatCount()}";
            m_queuedHeartbeatsListView.RefreshItems();
        }
        
        private void OnSentHeartbeatsInstancesChanged()
        {
            m_sentHistoryListView.RefreshItems();
            if (m_sentHistoryListView.itemsSource != null && m_sentHistoryListView.itemsSource.Count > 0)
            {
                m_sentHistoryListView.ScrollToItem(m_sentHistoryListView.itemsSource.Count - 1);
            }
        }

        private void OnFailedToSendInstancesChanged()
        {
            m_failedToSendArea.style.display = DevStatsData.Instance.GetFailedToSendInstances().Count > 0 ? DisplayStyle.Flex : DisplayStyle.None;
            m_failedToSendListView.RefreshItems();
        }

        private void UpdateUntilNextSendLabel()
        {
            TimeSpan timeRemaining = DateTime.Now - new DateTime(DevStatsState.Instance.LastHeartbeatSendTime);
            int secondsRemaining = DevStats.SEND_INTERVAL - (int)timeRemaining.TotalSeconds;
            if (secondsRemaining < 0)
            {
                secondsRemaining = 0;
            }
            m_untilNextSendLabel.text = $"POST in {secondsRemaining}";
        }
        
        private void OnFailedToSendRetryPressed()
        {
            DevStats.RetryFailedHeartbeats();
        }
        
        private void GetHeartbeatsRequest()
        {
            UnityWebRequest request = UnityWebRequest.Get(URL_PREFIX + "users/current/heartbeats?date=" + System.DateTime.Now.ToString("yyyy-MM-dd"));
            string auth = System.Convert.ToBase64String(Encoding.ASCII.GetBytes(DevStatsSettings.Instance.APIKey + ":"));
            request.SetRequestHeader("Authorization", "Basic " + auth);
            
            request.SendWebRequest().completed += (operation) =>
            {
                if (request.result == UnityWebRequest.Result.Success)
                { 
                    Debug.Log(request.downloadHandler.text);
                    HeartbeatsResponseDTO responseDto = JsonUtility.FromJson<HeartbeatsResponseDTO>(request.downloadHandler.text);
                    Debug.Log("Count: " + responseDto.data.Count);
                }
                else
                {
                    Debug.Log(request.result + " " + request.error);
                }
            };
        }
    }
}