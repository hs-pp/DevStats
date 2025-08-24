using System.Text;
using DevStatsSystem.Editor.Core;
using DevStatsSystem.Editor.Core.DTOs;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UIElements;

namespace DevStatsSystem.Editor.UI
{
    internal class HeartbeatsPanel : ADevStatsPanel
    {
        private const string URL_PREFIX = "https://api.wakatime.com/api/v1/";
        
        private const string UXML_PATH = "DevStats/UXML/HeartbeatsPanel";
        private const string GET_HEARTBEATS_BUTTON = "get-heartbeats-button";
        
        private Button m_getHeartbeatsButton;

        public HeartbeatsPanel()
        {
            CreateLayout();
        }

        private void CreateLayout()
        {
            var uxmlAsset = Resources.Load<VisualTreeAsset>(UXML_PATH);
            uxmlAsset.CloneTree(this);
            
            m_getHeartbeatsButton = this.Q<Button>(GET_HEARTBEATS_BUTTON);
            m_getHeartbeatsButton.clicked += GetHeartbeatsRequest;
        }
        
        public override void OnShow()
        {
        }

        public override void OnHide()
        {
            
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