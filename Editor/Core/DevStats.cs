using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DevStatsSystem.Editor.Core
{
    public static class DevStats
    {
        private const int SEND_INTERVAL = 120; // Should be every 2 minutes.
        
        private static WakatimeCliController m_wakatimeCli;
        private static HeartbeatProvider m_heartbeatProvider;
        private static List<Heartbeat> m_queuedHeartbeats = new();
        private static float m_lastHeartbeatSendTime = 0;
        
        [InitializeOnLoadMethod]
        static void RegisterAssemblyReloadEvents()
        {
            AssemblyReloadEvents.beforeAssemblyReload -= OnBeforeAssemblyReload;
            AssemblyReloadEvents.beforeAssemblyReload += OnBeforeAssemblyReload;

            AssemblyReloadEvents.afterAssemblyReload -= OnAfterAssemblyReload;
            AssemblyReloadEvents.afterAssemblyReload += OnAfterAssemblyReload;
        }

        public static float GetTimeRemainingDebug()
        {
            return SEND_INTERVAL - ((float)EditorApplication.timeSinceStartup - m_lastHeartbeatSendTime);
        }
        
        private static void OnBeforeAssemblyReload()
        {
            // Send any unsent heartbeats first.
            SendHeartbeat();
            
            m_heartbeatProvider.Deinitialize();
            EditorApplication.update -= OnEditorUpdate;
        }
        
        private static async void OnAfterAssemblyReload()
        {
            if (DevStatsSettings.Get().IsEnabled && string.IsNullOrEmpty(DevStatsSettings.Get().APIKey))
            {
                LogError("DevStats is enabled but API key is missing. Open the DevStats window from \"Window/DevStats\" and set the API key!");
            }
            else
            {
                Log("Started DevStats.");
            }

            m_wakatimeCli = await WakatimeCliController.Get();
            m_heartbeatProvider = new();
            m_heartbeatProvider.Initialize(TriggerHeartbeat);
            EditorApplication.update += OnEditorUpdate;
        }
        
        /// <summary>
        /// Anyone can call this.
        /// </summary>
        public static void TriggerHeartbeat(Heartbeat heartbeat)
        {
            if (!DevStatsSettings.Get().IsRunning())
            {
                return;
            }
            
            m_queuedHeartbeats.Add(heartbeat);
        }

        private static void OnEditorUpdate()
        {
            if (!DevStatsSettings.Get().IsRunning())
            {
                return;
            }
            
            float timeSinceStartup = (float)EditorApplication.timeSinceStartup;
            if (m_wakatimeCli != null && m_queuedHeartbeats.Count > 0 && timeSinceStartup > m_lastHeartbeatSendTime + SEND_INTERVAL)
            {
                SendHeartbeat();
                m_lastHeartbeatSendTime = timeSinceStartup;
            }
        }

        private static void SendHeartbeat()
        {
            if (m_queuedHeartbeats.Count == 0)
            {
                return;
            }
            
            m_wakatimeCli.SendHeartbeats(m_queuedHeartbeats);
            m_queuedHeartbeats.Clear();
        }

        public static void Log(string log)
        {
            if (!DevStatsSettings.Get().IsDebugMode)
            {
                return;
            }
            
            Debug.Log($"{GetLogHeader()} {log}");
        }

        public static void LogWarning(string warning)
        {
            if (!DevStatsSettings.Get().IsDebugMode)
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