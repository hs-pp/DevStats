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
        
        private Button m_getHeartbeatsButton;
        private Label m_heartbeatsInQueueLabel;
        private Label m_untilNextSendLabel;
        
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
        }

        public override void OnShow()
        {
            DevStatsState.Instance.OnQueuedHeartbeatsChanged += UpdateHeartbeatsInQueueLabel;

            UpdateHeartbeatsInQueueLabel();
            m_untilNextSendSchedule = schedule.Execute(UpdateUntilNextSendLabel).Every(500);
        }

        public override void OnHide()
        {
            DevStatsState.Instance.OnQueuedHeartbeatsChanged -= UpdateHeartbeatsInQueueLabel;
            m_untilNextSendSchedule.Pause();
        }

        private void UpdateHeartbeatsInQueueLabel()
        {
            m_heartbeatsInQueueLabel.text = $"In Queue: {DevStatsState.Instance.GetQueuedHeartbeatCount()}";
        }

        private void UpdateUntilNextSendLabel()
        {
            float timeRemaining = DevStats.SEND_INTERVAL - ((float)EditorApplication.timeSinceStartup - DevStatsState.Instance.LastHeartbeatSendTime);
            if (timeRemaining < 0)
            {
                timeRemaining = 0;
            }
            m_untilNextSendLabel.text = $"POST in {(int)timeRemaining}";
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