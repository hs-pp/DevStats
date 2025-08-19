using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace DevStats.Editor
{
    public struct Heartbeat
    {
        public string File;
        public decimal Timestamp;
        public bool IsWrite; // Basically "IsSaved"
        public string Category;
    }
    
    /// <summary>
    /// This provider captures a number of in-editor actions:
    /// Scenes - Open, Close, Save, Change
    /// Prefabs - Save, Change
    /// ScriptableObjects - Create, Destroy, Save, Change (Including UXMLs and UIBuilder)
    /// </summary>
    internal class HeartbeatProvider
    {
        private Action<Heartbeat> TriggerHeartbeat;
        private string m_projectPath = Application.dataPath.Replace("Assets", "");
        private Heartbeat m_previousHeartbeat;

        public void Initialize(Action<Heartbeat> triggerHeartbeatCallback)
        {
            TriggerHeartbeat = triggerHeartbeatCallback;
            EditorSceneManager.sceneOpened += OnSceneOpened;
            EditorSceneManager.sceneClosing += OnSceneClosing;
            EditorApplication.hierarchyChanged += OnHierarchyChanged;
            ObjectChangeEvents.changesPublished += OnChangesPublished;
            AssetSaveDetector.OnAssetSaved += OnAssetSaved;
        }

        public void Deinitialize()
        {
            EditorSceneManager.sceneOpened -= OnSceneOpened;
            EditorSceneManager.sceneClosing -= OnSceneClosing;
            EditorApplication.hierarchyChanged -= OnHierarchyChanged;
            ObjectChangeEvents.changesPublished -= OnChangesPublished;
            AssetSaveDetector.OnAssetSaved -= OnAssetSaved;
        }
        
        private void OnSceneOpened(Scene scene, OpenSceneMode mode)
        {
            SendHeartbeat(SceneToSceneAsset(scene), false);
        }
        
        private void OnSceneClosing(Scene scene, bool removingScene)
        {
            SendHeartbeat(SceneToSceneAsset(scene), false);
        }
        
        private void OnHierarchyChanged()
        {
            if (PrefabStageUtility.GetCurrentPrefabStage() is PrefabStage prefabStage) // Prefab hierarchy has changed.
            {
                SendHeartbeat(AssetDatabase.LoadAssetAtPath<Object>(prefabStage.assetPath), false);
            }
            else if (EditorWindow.focusedWindow.GetType() == InternalBridgeHelper.GetSceneHierarchyWindowType())
            {
                SendHeartbeat(SceneToSceneAsset(EditorSceneManager.GetActiveScene()), false);
            }
            else // Default to scene hierarchy change.
            {
                Debug.LogWarning($"Unknown hierarchy changed ({EditorWindow.focusedWindow.GetType().Name}). Do we want DevStats to track it? ");
            }
        }
        
        private void OnChangesPublished(ref ObjectChangeEventStream stream)
        {
            for (int i = 0; i < stream.length; i++)
            {
                ObjectChangeKind changeKind = stream.GetEventType(i);
                switch (changeKind)
                {
                    case ObjectChangeKind.ChangeAssetObjectProperties:
                        stream.GetChangeAssetObjectPropertiesEvent(i, out ChangeAssetObjectPropertiesEventArgs propChange);
                        Object changeObject = EditorUtility.InstanceIDToObject(propChange.instanceId);
                        if (changeObject is ScriptableObject)
                        {
                            SendHeartbeat(changeObject, false);
                        }
                        break;
                    case ObjectChangeKind.CreateAssetObject:
                        stream.GetCreateAssetObjectEvent(i, out CreateAssetObjectEventArgs createChange);
                        Object createObject = EditorUtility.InstanceIDToObject(createChange.instanceId);
                        if (createObject is ScriptableObject)
                        {
                            SendHeartbeat(createObject, false);
                        }
                        break;
                    case ObjectChangeKind.DestroyAssetObject:
                        stream.GetDestroyAssetObjectEvent(i, out DestroyAssetObjectEventArgs destroyChange);
                        Object destroyObject = EditorUtility.InstanceIDToObject(destroyChange.instanceId);
                        if (destroyObject is ScriptableObject)
                        {
                            SendHeartbeat(destroyObject, false);
                        }
                        break;
                }
            }
        }

        private void OnAssetSaved(Object asset)
        {
            if (asset is ScriptableObject or SceneAsset or GameObject)
            {
                SendHeartbeat(asset, true);
            }
            else
            {
                Debug.LogWarning($"Unknown Object saved ({asset.name}). Do we want DevStats to track it?");
            }
        }

        private SceneAsset SceneToSceneAsset(Scene scene)
        {
            return AssetDatabase.LoadAssetAtPath<SceneAsset>(scene.path);
        }
        
        private void SendHeartbeat(Object asset, bool isSaveAction, string category = null)
        {
            if (asset == null)
            {
                Debug.LogError("Cannot send heartbeat for null asset!");
                return;
            }

            string file = $"{m_projectPath}{AssetDatabase.GetAssetPath(asset)}";
            Heartbeat heartbeat = new Heartbeat()
            {
                File = file,
                Timestamp = (decimal)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() / 1000,
                IsWrite = isSaveAction,
                Category = category,
            };
            
            // Don't add this heartbeat if this one is not a write and is the same file as the last heartbeat and
            // within 1 second of the last one. This will let us skip asset changed triggers that happen right after
            // an asset saved is triggered.
            if (!heartbeat.IsWrite && m_previousHeartbeat.File == heartbeat.File && heartbeat.Timestamp - m_previousHeartbeat.Timestamp < 1)
            {
                return;
            }
            
            if (DevStatsSettings.Get().IsDebugMode)
            {
                Debug.Log($"[HEARTBEAT] {heartbeat.File}, {heartbeat.IsWrite}, {heartbeat.Timestamp}, {heartbeat.Category}");
            }
            
            TriggerHeartbeat?.Invoke(heartbeat);
            m_previousHeartbeat = heartbeat;
        }
    }
    
    /// <summary>
    /// This needs to be a separate class to tap into OnWillSaveAssets callback.
    /// </summary>
    public class AssetSaveDetector : UnityEditor.AssetModificationProcessor
    {
        public static Action<Object> OnAssetSaved;
        public static string[] OnWillSaveAssets(string[] paths)
        {
            foreach (var path in paths)
            {
                Object asset = AssetDatabase.LoadAssetAtPath<Object>(path);
                if (asset != null)
                {
                    OnAssetSaved?.Invoke(asset);
                }
            }

            return paths;
        }
    }
}