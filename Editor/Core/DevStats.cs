using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DevStatsSystem.Core.SerializedData;
using DevStatsSystem.Wakatime;
using UnityEditor;

namespace DevStatsSystem.Core
{
    /// <summary>
    /// The core of the DevStats system. This is the main class that determines whether the system is running and when to
    /// send out heartbeats back to the backend.
    /// </summary>
    public static class DevStats
    {
        private static IDevStatsBackend Backend;
        private static HeartbeatProvider m_heartbeatProvider;
        private static DevStatsState m_state;
        private static DevStatsSettings m_settings;

        public static bool IsRunning { get; private set; }
        private static bool CanRun() => m_settings.IsEnabled && Backend.CanRun;
        internal static Action<bool> OnIsRunningChanged;
        
        [InitializeOnLoadMethod]
        static void RegisterAssemblyReloadEvents()
        {
            Backend = new WakatimeBackend();
            Backend.OnCanRunChanged += TriggerIsRunningChanged;
            
            m_settings = DevStatsSettings.Instance;
            m_settings.OnIsEnabledChanged += TriggerIsRunningChanged;
            OnIsRunningChanged += OnDevStatsIsRunningChanged;
            
            IsRunning = CanRun();
            
            m_state = DevStatsState.Instance;
            
            m_heartbeatProvider = new(TriggerHeartbeat);

            AssemblyReloadEvents.afterAssemblyReload -= OnAfterAssemblyReload;
            AssemblyReloadEvents.afterAssemblyReload += OnAfterAssemblyReload;
            AssemblyReloadEvents.beforeAssemblyReload -= OnBeforeAssemblyReload;
            AssemblyReloadEvents.beforeAssemblyReload += OnBeforeAssemblyReload;
        }

        private static void OnAfterAssemblyReload()
        {
            if (IsRunning)
            {
                Initialize();
            }
        }

        private static void OnBeforeAssemblyReload()
        {
            // This is right before the project compiles. Make sure to save first.
            m_settings.Save();
            
            if (IsRunning)
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
            
            m_heartbeatProvider.Initialize();
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
            if (!IsRunning)
            {
                return;
            }
            
            m_state.AddHeartbeatToQueue(heartbeat);
        }

        private static void OnEditorUpdate()
        {
            if (!IsRunning)
            {
                return;
            }

            long nowTime = DateTime.UtcNow.Ticks;
            int sendInterval = (int)m_settings.PostFrequency * 10000000;
            if (m_state.GetQueuedHeartbeatCount() > 0 && nowTime > m_state.LastHeartbeatSendTime + sendInterval)
            {
                SendHeartbeatsToBackend(m_state.GetQueuedHeartbeats());
                m_state.ClearQueuedHeartbeats();
                m_state.LastHeartbeatSendTime = nowTime;
            }
        }

        private static async void SendHeartbeatsToBackend(List<Heartbeat> heartbeats)
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

        internal static void RetryFailedHeartbeats()
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
            SendHeartbeatsToBackend(allFailedHeartbeats);
            m_state.ClearFailedToSendInstances();
        }
        
        private static void TriggerIsRunningChanged(bool isRunning)
        {
            if (IsRunning != CanRun())
            {
                IsRunning = CanRun();
                OnIsRunningChanged?.Invoke(IsRunning);
            }
        }
        
        // Exposing Backend functions for panels.
        internal static ABackendSettingsWidget CreateSettingsWidgetInstance()
        {
            return Backend.CreateSettingsWidgetInstance();
        }

        internal static Task<StatsData> GetStats()
        {
            return Backend.GetStats();
        }
        
        // Nicely formatted time strings.
        internal static string SecondsToFormattedTimePassed(float seconds)
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
        
        internal static string SecondsToFormattedTimeSinceMidnight(float startTime)
        {
            return DateTime.Today.AddSeconds(startTime).ToString("h:mmtt");
        }
    }
}