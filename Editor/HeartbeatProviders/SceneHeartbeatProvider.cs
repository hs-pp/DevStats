using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DevStats.Editor
{
    public class SceneHeartbeatProvider : AHeartbeatProvider
    {
        public override void Initialize()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            EditorApplication.contextualPropertyMenu += OnContextualPropertyMenu;
            EditorApplication.hierarchyChanged += OnHierarchyChanged;
            EditorSceneManager.newSceneCreated += OnSceneCreated;
            EditorSceneManager.sceneOpened += OnSceneOpened;
            EditorSceneManager.sceneSaved += OnSceneSaved;
            EditorSceneManager.sceneClosing += OnSceneClosing;
        }

        public override void Deinitialize()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.contextualPropertyMenu -= OnContextualPropertyMenu;
            EditorApplication.hierarchyChanged -= OnHierarchyChanged;
            EditorSceneManager.newSceneCreated -= OnSceneCreated;
            EditorSceneManager.sceneOpened -= OnSceneOpened;
            EditorSceneManager.sceneSaved -= OnSceneSaved;
            EditorSceneManager.sceneClosing -= OnSceneClosing;
        }
        
        private void OnPlayModeStateChanged(PlayModeStateChange obj)
        {
            SendHeartbeat(GetSceneFilePath(), false, "PlayMode");
        }
        
        private void OnContextualPropertyMenu(GenericMenu menu, SerializedProperty property)
        {
            SendHeartbeat(GetSceneFilePath(), false);
        }
        
        private void OnHierarchyChanged()
        {
            SendHeartbeat(GetSceneFilePath(), false);
        }
        
        private void OnSceneCreated(Scene scene, NewSceneSetup setup, NewSceneMode mode)
        {
            SendHeartbeat(GetSceneFilePath(scene), false);
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

        private string GetSceneFilePath(Scene? scene = null)
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
}