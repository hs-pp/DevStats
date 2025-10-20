using DevStatsSystem.Core;
using DevStatsSystem.Core.SerializedData;
using UnityEngine;
using UnityEngine.UIElements;

namespace DevStatsSystem.Wakatime
{
    public class WakatimeSettingsElement : ABackendSettingsWidget
    {
        private const string UXML_PATH = "DevStats/Wakatime/WakatimeSettingsElement";
        private const string APIKEY_FIELD_TAG = "api-key-field";
        private const string USERNAME_DISPLAY_LABEL_TAG = "username-display-label";
        private const string API_LINK_FULL_ELEMENT_TAG = "api-link-full-element";
        private const string API_WEBLINK_LABEL_TAG = "api-weblink-label";
        private const string KEYSTROKE_TIMEOUT_ENUM_TAG = "keystroke-timeout-enum";
        
        private TextField m_apiKeyField;
        private Label m_usernameDisplayLabel;
        private VisualElement m_apiLinkFullElement;
        private Label m_apiWeblinkLabel;
        private EnumField m_keystrokeTimeoutEnum;
        
        public WakatimeSettingsElement()
        {
            CreateLayout();
        }

        private void CreateLayout()
        {
            VisualTreeAsset uxmlAsset = Resources.Load<VisualTreeAsset>(UXML_PATH);
            uxmlAsset.CloneTree(this);
            
            m_apiKeyField = this.Q<TextField>(APIKEY_FIELD_TAG);
            m_usernameDisplayLabel = this.Q<Label>(USERNAME_DISPLAY_LABEL_TAG);
            m_apiLinkFullElement = this.Q<VisualElement>(API_LINK_FULL_ELEMENT_TAG);
            m_apiWeblinkLabel = this.Q<Label>(API_WEBLINK_LABEL_TAG);
            m_keystrokeTimeoutEnum = this.Q<EnumField>(KEYSTROKE_TIMEOUT_ENUM_TAG);

            m_apiKeyField.value = WakatimeSettings.Instance.APIKey;
            WakatimeSettings.Instance.OnCanRunReevaluated += OnCanRunReevaluated;
            m_apiKeyField.RegisterValueChangedCallback(evt => { WakatimeSettings.Instance.APIKey = evt.newValue; });
            m_apiWeblinkLabel.AddManipulator(new Clickable(() => { Application.OpenURL("https://wakatime.com/settings/account"); }));
            m_keystrokeTimeoutEnum.value = WakatimeSettings.Instance.KeystrokeTimeout;
            m_keystrokeTimeoutEnum.RegisterValueChangedCallback(evt => WakatimeSettings.Instance.KeystrokeTimeout = (KeystrokeTimeout)evt.newValue);
            
            OnCanRunReevaluated();
        }

        private void OnCanRunReevaluated()
        {
            m_apiLinkFullElement.style.display = !WakatimeSettings.Instance.CanRun ? DisplayStyle.Flex : DisplayStyle.None;
            m_usernameDisplayLabel.text = !string.IsNullOrEmpty(WakatimeSettings.Instance.UserData.id) ? "User <b><color=green>Found</color><b>" : "User <b><color=red>Not Found</color><b>";
            m_usernameDisplayLabel.style.display = !string.IsNullOrEmpty(WakatimeSettings.Instance.APIKey) ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }
}