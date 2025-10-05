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
        private static int SEND_INTERVAL_NANOSECONDS = 0;

        internal static IDevStatsBackend Backend;
        private static HeartbeatProvider m_heartbeatProvider;
        private static DevStatsState m_state;
        private static DevStatsSettings m_settings;

        public static Action OnInitializedCallback;
        
        [InitializeOnLoadMethod]
        static void RegisterAssemblyReloadEvents()
        {
            m_settings = DevStatsSettings.Instance;
            m_settings.OnEnabledChanged += OnDevStatsEnabledChanged;
            m_settings.OnHeartbeatSendIntervalChanged += OnHeartbeatSendIntervalChanged;
            OnHeartbeatSendIntervalChanged();

            AssemblyReloadEvents.afterAssemblyReload -= OnAfterAssemblyReload;
            AssemblyReloadEvents.afterAssemblyReload += OnAfterAssemblyReload;
            AssemblyReloadEvents.beforeAssemblyReload -= OnBeforeAssemblyReload;
            AssemblyReloadEvents.beforeAssemblyReload += OnBeforeAssemblyReload;
        }

        private static void OnAfterAssemblyReload()
        {
            if (m_settings.IsEnabled)
            {
                Initialize();
            }
        }

        private static void OnBeforeAssemblyReload()
        {
            m_settings.Save();
            
            if (m_settings.IsEnabled)
            {
                Deinitialize();
            }
        }

        private static void OnDevStatsEnabledChanged(bool newValue, bool prevValue)
        {
            if (newValue == prevValue)
            {
                return;
            }
            
            if (newValue)
            {
                Initialize();
            }
            else
            {
                Deinitialize();
            }
        }
        
        private static void OnHeartbeatSendIntervalChanged()
        {
            SEND_INTERVAL_NANOSECONDS = m_settings.HeartbeatSendInterval * 10000000;
        }
        
        private static async void Initialize()
        {
            if (m_settings.IsEnabled && string.IsNullOrEmpty(m_settings.APIKey))
            {
                LogError("DevStats is enabled but API key is missing. Open the DevStats window from \"Window/DevStats\" and set the API key!");
            }
            else
            {
                Log("Initialized DevStats.");
            }

            Backend = new WakatimeBackend(); // Make this not hardcoded if we ever want a different backend.
            CommandResult result = await Backend.Load();
            if (result.Result == CommandResultType.Failure)
            {
                LogError("DevStats Backend failed to load.");
                Backend = null;
                return;
            }
            m_heartbeatProvider = new(TriggerHeartbeat);
            m_heartbeatProvider.Initialize();

            m_state = DevStatsState.Instance;
            EditorApplication.update += OnEditorUpdate;
            
            OnInitializedCallback?.Invoke();
        }
        
        private static void Deinitialize()
        {
            m_state.Save();
            
            m_heartbeatProvider.Deinitialize();
            EditorApplication.update -= OnEditorUpdate;
            
            Log("Deinitialized DevStats");
        }
        
        /// <summary>
        /// Anyone can call this.
        /// </summary>
        public static void TriggerHeartbeat(Heartbeat heartbeat)
        {
            if (!m_settings.IsRunning())
            {
                return;
            }
            
            m_state.AddHeartbeatToQueue(heartbeat);
        }

        private static void OnEditorUpdate()
        {
            if (!m_settings.IsRunning())
            {
                return;
            }

            long timeSinceStartup = DateTime.Now.Ticks;
            if (m_state.GetQueuedHeartbeatCount() > 0 && timeSinceStartup > m_state.LastHeartbeatSendTime + SEND_INTERVAL_NANOSECONDS)
            {
                SendHeartbeatsToCli(m_state.GetQueuedHeartbeats());
                m_state.ClearQueuedHeartbeats();
                m_state.LastHeartbeatSendTime = timeSinceStartup;
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

        public static void Log(string log)
        {
            if (!m_settings.PrintDebugLogs)
            {
                return;
            }
            
            Debug.Log($"{GetLogHeader()} {log}");
        }

        public static void LogWarning(string warning)
        {
            if (!m_settings.PrintDebugLogs)
            {
                return;
            }
            
            Debug.LogWarning($"{GetLogHeader()} {warning}");
        }

        public static void LogError(string error)
        {
            Debug.LogError($"{GetLogHeader()} {error}");
        }

        private static string GetLogHeader()
        {
            return "<b><color=#F37828>[DevStats]</color></b>";
        }
    }
}