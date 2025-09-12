using System;
using System.Diagnostics;
using System.Threading.Tasks;
using DevStatsSystem.Editor.Core;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Debug = UnityEngine.Debug;

namespace DevStatsSystem.Editor.UI
{
    internal class StatsPanel : ADevStatsPanel
    {
        private const string UXML_PATH = "DevStats/UXML/StatsPanel";
        private const string TEST_BUTTON_TAG = "test-button";
        
        private Button m_testButton;

        public StatsPanel()
        {
            CreateLayout();
        }

        private void CreateLayout()
        {
            var uxmlAsset = Resources.Load<VisualTreeAsset>(UXML_PATH);
            uxmlAsset.CloneTree(this);
            
            m_testButton = this.Q<Button>(TEST_BUTTON_TAG);
            m_testButton.clicked += () =>
            {
                _ = LoadData();
            };
        }
        
        public override void OnShow()
        {
            
        }

        public override void OnHide()
        {
        }

        private async Task LoadData()
        {
            Debug.Log("Started loading everything");
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                return;
            }
            
            var durationsPayload = await WakatimeWebRequests.GetDayDurationRequest();
            Debug.Log($"Durations:\n{durationsPayload}");
            
            await Task.Delay(TimeSpan.FromSeconds(1));
            
            var heartbeatsPayload = await WakatimeWebRequests.GetHeartbeatsRequest();
            Debug.Log($"Heartbeats:\n{heartbeatsPayload}");
            
            await Task.Delay(TimeSpan.FromSeconds(1));

            var statsPayload = await WakatimeWebRequests.GetStatsRequest();
            Debug.Log($"Stats:\n{statsPayload}");

            await Task.Delay(TimeSpan.FromSeconds(1));

            var summariesPayload = await WakatimeWebRequests.GetSummariesRequest(7);
            Debug.Log($"Summaries:\n{summariesPayload}");
            
            if (EditorApplication.isCompiling)
            {
                Debug.LogWarning("Editor is compiling. Stopping Run Everything");
                return;
            }
            
            stopwatch.Stop();
            Debug.Log($"Finished loading everything T:{stopwatch.ElapsedMilliseconds/1000f}s");
        }
    }
}