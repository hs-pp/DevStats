using System;
using System.Collections.Generic;
using System.Text;
using DevStatsSystem.Core.SerializedData;
using DevStatsSystem.Wakatime;
using UnityEditor;
using UnityEngine;

namespace DevStatsSystem.Core
{
    public static class DevStats
    {
        internal static IDevStatsBackend Backend;
        private static HeartbeatProvider m_heartbeatProvider;
        private static DevStatsState m_state;
        private static DevStatsSettings m_settings;
        
        internal static bool IsRunning()
        {
            return m_settings.IsEnabled && Backend.CanRun;
        }
        internal static Action<bool> OnIsRunningChanged;

        private static void TriggerIsRunningChanged(bool isRunning)
        {
            Debug.Log("IsRunningChanged " + IsRunning());
            OnIsRunningChanged?.Invoke(IsRunning());
        }
        
        [InitializeOnLoadMethod]
        static void RegisterAssemblyReloadEvents()
        {
            Backend = new WakatimeBackend(); // Make this not hardcoded if we ever want a different backend. Or just swap it.
            Backend.OnCanRunChanged += TriggerIsRunningChanged;
            
            m_settings = DevStatsSettings.Instance;
            m_settings.OnIsEnabledChanged += TriggerIsRunningChanged;
            OnIsRunningChanged += OnDevStatsIsRunningChanged;

            AssemblyReloadEvents.afterAssemblyReload -= OnAfterAssemblyReload;
            AssemblyReloadEvents.afterAssemblyReload += OnAfterAssemblyReload;
            AssemblyReloadEvents.beforeAssemblyReload -= OnBeforeAssemblyReload;
            AssemblyReloadEvents.beforeAssemblyReload += OnBeforeAssemblyReload;
        }

        private static void OnAfterAssemblyReload()
        {
            if (m_settings.IsEnabled && !Backend.CanRun)
            {
                // This if check makes assumptions for the log but it's helpful so leaving this in here.
                Debug.LogWarning("DevStats is enabled but API key is missing. Open the DevStats window from \"Window/DevStats\" and set the API key!");
            }
            
            if (IsRunning())
            {
                Initialize();
            }
        }

        private static void OnBeforeAssemblyReload()
        {
            m_settings.Save();
            
            if (IsRunning())
            {
                Deinitialize();
            }
        }

        private static void OnDevStatsIsRunningChanged(bool newValue)
        {
            if (newValue)
            {
                Initialize();
            }
            else
            {
                Deinitialize();
            }
        }
        
        private static async void Initialize()
        {
            CommandResult result = await Backend.Load();
            if (result.Result == CommandResultType.Failure)
            {
                Backend = null;
                return;
            }
            m_heartbeatProvider = new(TriggerHeartbeat);
            m_heartbeatProvider.Initialize();

            m_state = DevStatsState.Instance;
            EditorApplication.update += OnEditorUpdate;
        }
        
        private static void Deinitialize()
        {
            m_state.Save();
            Backend.Unload();
            
            m_heartbeatProvider.Deinitialize();
            EditorApplication.update -= OnEditorUpdate;
        }
        
        /// <summary>
        /// Anyone can call this.
        /// </summary>
        public static void TriggerHeartbeat(Heartbeat heartbeat)
        {
            if (!IsRunning())
            {
                return;
            }
            
            m_state.AddHeartbeatToQueue(heartbeat);
        }

        private static void OnEditorUpdate()
        {
            if (!IsRunning())
            {
                return;
            }

            long nowTime = DateTime.UtcNow.Ticks;
            int sendInterval = (int)m_settings.PostFrequency * 10000000;
            if (m_state.GetQueuedHeartbeatCount() > 0 && nowTime > m_state.LastHeartbeatSendTime + sendInterval)
            {
                SendHeartbeatsToCli(m_state.GetQueuedHeartbeats());
                m_state.ClearQueuedHeartbeats();
                m_state.LastHeartbeatSendTime = nowTime;
            }
        }

        private static async void SendHeartbeatsToCli(List<Heartbeat> heartbeats)
        {
            if (Backend == null)
            {
                return;
            }

            if (heartbeats == null || heartbeats.Count == 0)
            {
                return;
            }
            
            List<Heartbeat> localHeartbeats = new List<Heartbeat>(heartbeats);
            CommandResult result = await Backend.SendHeartbeats(localHeartbeats);
            if (result.Result == CommandResultType.Success)
            {
                m_state.AddSentHeartbeatInstance(localHeartbeats);
            }
            else
            {
                m_state.AddFailedToSendInstance(localHeartbeats);
            }
        }

        /// <summary>
        /// Mostly for testing.
        /// </summary>
        private static void SendToFailed(List<Heartbeat> heartbeats)
        {
            if (heartbeats == null || heartbeats.Count == 0)
            {
                return;
            }
            
            m_state.AddFailedToSendInstance(new(heartbeats));
        }

        public static void RetryFailedHeartbeats()
        {
            if (m_state.GetFailedToSendInstances().Count == 0)
            {
                return;
            }

            List<Heartbeat> allFailedHeartbeats = new();
            foreach (FailedToSendInstance instance in m_state.GetFailedToSendInstances())
            {
                allFailedHeartbeats.AddRange(instance.Heartbeats);
            }
            SendHeartbeatsToCli(allFailedHeartbeats);
            m_state.ClearFailedToSendInstances();
        }
        
        public static string GetProjectName()
        {
            return Application.productName;
        }
        
        public static string GetLanguage()
        {
            return "Unity3D Asset";
        }
        
        public static string SecondsToFormattedTimePassed(float seconds)
        {
            if (seconds == 0)
            {
                return "<b><size=16>0</size></b>sec";
            }
            
            bool hasHours = false;
            bool hasMinutes = false;
            TimeSpan time = TimeSpan.FromSeconds(seconds);
            StringBuilder strBuilder = new StringBuilder();
            if (time.Hours > 0 || time.Days > 0)
            {
                strBuilder.AppendFormat($"<b><size=16>{time.Days * 24 + time.Hours}</size></b>hr ");
                hasHours = true;
            }
            if (time.Minutes > 0 || hasHours)
            {
                strBuilder.AppendFormat($"<b><size=16>{time.Minutes}</size></b>min ");
                hasMinutes = true;
            }
            if (time.Seconds > 0 || hasHours || hasMinutes)
            {
                strBuilder.AppendFormat($"<b><size=16>{time.Seconds}</size></b>sec");
            }
            
            return strBuilder.ToString();
        }
        
        public static string SecondsToFormattedTimeSinceMidnight(float startTime)
        {
            return DateTime.Today.AddSeconds(startTime).ToString("h:mmtt");
        }
    }
}