using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace DevStats.Editor
{
    public class DevStatsWindow : EditorWindow
    {
        private static string UXML_PATH = "DevStats/IRLStatsWindow";

        private static string TEST_BUTTON_TAG = "test-button";

        private Button m_testButton;

        [MenuItem("Window/IRLStats")]
        public static void OpenWindow()
        {
            GetWindow<DevStatsWindow>().Show();
        }

        public void OnEnable()
        {
            titleContent = new GUIContent("IRL Stats");

            CreateLayout();
        }

        private void CreateLayout()
        {
            var uxmlAsset = Resources.Load<VisualTreeAsset>(UXML_PATH);
            uxmlAsset.CloneTree(rootVisualElement);

            m_testButton = rootVisualElement.Q<Button>(TEST_BUTTON_TAG);
            m_testButton.clicked += TestButtonClicked;
        }

        private void TestButtonClicked()
        {
            //IrlStatsController.SendHeartbeat();
        }
    }
}