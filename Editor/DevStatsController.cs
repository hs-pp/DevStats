using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace DevStats.Editor
{
    public static class DevStatsController
    {
        private const int SEND_INTERVAL = 120; // Should be every 2 minutes.
        
        private static WakatimeCliInterface m_wakatimeCli;
        private static List<AHeartbeatProvider> m_heartbeatProviders = new();
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
            CleanupHeartbeatProviders();
            EditorApplication.update -= OnEditorUpdate;
        }
        
        private static async void OnAfterAssemblyReload()
        {
            m_wakatimeCli = await WakatimeCliInterface.Get();
            CreateHeartbeatProviders();
            EditorApplication.update += OnEditorUpdate;
        }
        
        private static void CreateHeartbeatProviders()
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (Type type in assembly.GetTypes())
                {
                    if (!type.IsClass || type.IsAbstract || type.IsGenericType)
                    {
                        continue;
                    }

                    if (typeof(AHeartbeatProvider).IsAssignableFrom(type))
                    {
                        AHeartbeatProvider provider = Activator.CreateInstance(type) as AHeartbeatProvider;
                        if (provider != null)
                        {
                            provider.Initialize();
                            provider.TriggerHeartbeat += OnHeartbeatTriggered;
                            m_heartbeatProviders.Add(provider);
                        }
                    }
                }
            }
        }

        private static void CleanupHeartbeatProviders()
        {
            foreach (AHeartbeatProvider provider in m_heartbeatProviders)
            {
                provider.Deinitialize();
                provider.TriggerHeartbeat -= OnHeartbeatTriggered;
            }

            m_heartbeatProviders.Clear();
        }

        private static void OnHeartbeatTriggered(Heartbeat heartbeat)
        {
            Debug.Log($"Queued heartbeat \n{heartbeat.ToString()}");
            m_queuedHeartbeats.Add(heartbeat);
        }

        private static void OnEditorUpdate()
        {
            float timeSinceStartup = (float)EditorApplication.timeSinceStartup;
            if (m_wakatimeCli != null && m_queuedHeartbeats.Count > 0 && timeSinceStartup > m_lastHeartbeatSendTime + SEND_INTERVAL)
            {
                SendHeartbeat();
                m_lastHeartbeatSendTime = timeSinceStartup;
            }
        }

        private static void SendHeartbeat()
        {
            m_wakatimeCli.SendHeartbeats(m_queuedHeartbeats);
            m_queuedHeartbeats.Clear();
        }
    }
}