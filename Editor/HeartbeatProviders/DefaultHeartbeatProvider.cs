using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DevStats.Editor
{
    /// <summary>
    /// This provider captures a number of in-editor actions:
    /// Scenes - Open, Close, Save, Change
    /// Prefabs - Save, Change
    /// ScriptableObjects - Create, Destroy, Save, Change, (Including UXMLs and UIBuilder)
    /// </summary>
    public class DefaultHeartbeatProvider : AHeartbeatProvider
    {
        public override void Initialize()
        {
            EditorSceneManager.sceneOpened += OnSceneOpened;
            EditorSceneManager.sceneSaved += OnSceneSaved;
            EditorSceneManager.sceneClosing += OnSceneClosing;
            EditorApplication.hierarchyChanged += OnHierarchyChanged;
            ObjectChangeEvents.changesPublished += OnChangesPublished;
        }

        public override void Deinitialize()
        {
            EditorSceneManager.sceneOpened -= OnSceneOpened;
            EditorSceneManager.sceneSaved -= OnSceneSaved;
            EditorSceneManager.sceneClosing -= OnSceneClosing;
            EditorApplication.hierarchyChanged -= OnHierarchyChanged;
            ObjectChangeEvents.changesPublished -= OnChangesPublished;
        }
        
        private void OnSceneOpened(Scene scene, OpenSceneMode mode)
        {
            SendHeartbeat(GetSceneFilePath(scene), false);
        }
        
        private void OnSceneSaved(Scene scene)
        {
            SendHeartbeat(GetSceneFilePath(scene), true);
        }
        
        private void OnSceneClosing(Scene scene, bool removingScene)
        {
            SendHeartbeat(GetSceneFilePath(scene), false);
        }
        
        private void OnHierarchyChanged()
        {
            if (PrefabStageUtility.GetCurrentPrefabStage() is PrefabStage prefabStage) // Prefab hierarchy has changed.
            {
                Debug.Log("Prefab hierarchy changed");
                SendHeartbeat(prefabStage.prefabAssetPath, false);
            }
            else // Default to scene hierarchy change.
            {
                Debug.Log("Scene hierarchy changed");
                SendHeartbeat(GetSceneFilePath(), false);
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
                            SendHeartbeat(AssetDatabase.GetAssetPath(changeObject), false);
                            Debug.Log($"ScriptableObject modified: {changeObject.name}");
                        }
                        else
                        {
                            Debug.Log($"HUH Object modified: {changeObject.name} ({changeObject.GetType().Name})");
                        }
                        break;
                    case ObjectChangeKind.CreateAssetObject:
                        stream.GetCreateAssetObjectEvent(i, out CreateAssetObjectEventArgs createChange);
                        Object createObject = EditorUtility.InstanceIDToObject(createChange.instanceId);
                        if (createObject is ScriptableObject)
                        {
                            SendHeartbeat(AssetDatabase.GetAssetPath(createObject), false);
                            Debug.Log($"ScriptableObject created: {createObject.name}");
                        }
                        else
                        {
                            Debug.Log($"HUH Object created: {createObject.name} ({createObject.GetType().Name})");
                        }
                        break;
                    case ObjectChangeKind.DestroyAssetObject:
                        stream.GetDestroyAssetObjectEvent(i, out DestroyAssetObjectEventArgs destroyChange);
                        Object destroyObject = EditorUtility.InstanceIDToObject(destroyChange.instanceId);
                        if (destroyObject is ScriptableObject)
                        {
                            SendHeartbeat(AssetDatabase.GetAssetPath(destroyObject), false);
                            Debug.Log($"ScriptableObject destroyed: {destroyObject.name}");
                        }
                        else
                        {
                            Debug.Log($"HUH Object destroyed: {destroyObject.name} ({destroyObject.GetType().Name})");
                        }
                        break;
                }
            }
        }
        
        private static string GetSceneFilePath(Scene? scene = null)
        {
            if (scene == null)
            {
                scene = EditorSceneManager.GetActiveScene();
            }

            string filePath = "Unsaved Scene";
            if (!string.IsNullOrEmpty(scene?.path))
            {
                filePath = Application.dataPath + "/" + scene?.path.Substring("Assets/".Length);
            }

            return filePath;
        }
    }
    
    /// <summary>
    /// This needs to be a separate class to tap into OnWillSaveAssets callback.
    /// </summary>
    public class SaveDetection : UnityEditor.AssetModificationProcessor
    {
        
        public static void OnWillSaveAssets(string[] paths)
        {
            foreach (var path in paths)
            {
                Debug.Log("Saving asset: " + path);
            }
        }
    }
}