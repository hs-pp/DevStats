using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DevStatsSystem.Editor.Core
{
    public static class DevStats
    {
        public const int SEND_INTERVAL = 120; // In seconds
        
        private static WakatimeCli m_wakatimeCli;
        private static HeartbeatProvider m_heartbeatProvider;
        private static DevStatsState m_state;
        private static DevStatsSettings m_settings;
        
        [InitializeOnLoadMethod]
        static void RegisterAssemblyReloadEvents()
        {
            m_settings = DevStatsSettings.Instance;
            m_settings.OnEnabledChanged += OnDevStatsEnabledChanged;
            AssemblyReloadEvents.afterAssemblyReload -= Initialize;
            AssemblyReloadEvents.beforeAssemblyReload -= Deinitialize;
            
            if (m_settings.IsEnabled)
            {
                AssemblyReloadEvents.afterAssemblyReload += Initialize;
                AssemblyReloadEvents.beforeAssemblyReload += Deinitialize;
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
                AssemblyReloadEvents.afterAssemblyReload += Initialize;
                AssemblyReloadEvents.beforeAssemblyReload += Deinitialize;
                Initialize();
            }
            else
            {
                AssemblyReloadEvents.afterAssemblyReload -= Initialize;
                AssemblyReloadEvents.beforeAssemblyReload -= Deinitialize;
                Deinitialize();
            }
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

            m_wakatimeCli = await WakatimeCli.Get();
            if (m_wakatimeCli == null)
            {
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
            
            float timeSinceStartup = (float)EditorApplication.timeSinceStartup;
            if (m_state.GetQueuedHeartbeatCount() > 0 && timeSinceStartup > m_state.LastHeartbeatSendTime + SEND_INTERVAL)
            {
                SendHeartbeat();
                m_state.LastHeartbeatSendTime = timeSinceStartup;
            }
        }

        private static void SendHeartbeat()
        {
            if (m_wakatimeCli == null)
            {
                return;
            }
            
            if (m_state.GetQueuedHeartbeatCount() == 0)
            {
                return;
            }
            
            _ = m_wakatimeCli.SendHeartbeats(new List<Heartbeat>(m_state.GetQueuedHeartbeats()));
            m_state.ClearQueuedHeartbeats();
        }

        public static void Log(string log)
        {
            if (!m_settings.IsDebugMode)
            {
                return;
            }
            
            Debug.Log($"{GetLogHeader()} {log}");
        }

        public static void LogWarning(string warning)
        {
            if (!m_settings.IsDebugMode)
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