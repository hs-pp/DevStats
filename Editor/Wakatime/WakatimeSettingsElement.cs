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
        private const string GREEN_DOT_IMAGE_TAG = "green-dot-image";
        private const string YELLOW_DOT_IMAGE_TAG = "yellow-dot-image";
        private const string RED_DOT_IMAGE_TAG = "red-dot-image";
        private const string API_LINK_FULL_ELEMENT_TAG = "api-link-full-element";
        private const string API_WEBLINK_LABEL_TAG = "api-weblink-label";
        private const string KEYSTROKE_TIMEOUT_ENUM_TAG = "keystroke-timeout-enum";
        
        private TextField m_apiKeyField;
        private VisualElement m_greenDotImageElement;
        private VisualElement m_yellowDotImageElement;
        private VisualElement m_redDotImageElement;
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
            m_greenDotImageElement = this.Q<VisualElement>(GREEN_DOT_IMAGE_TAG);
            m_yellowDotImageElement = this.Q<VisualElement>(YELLOW_DOT_IMAGE_TAG);
            m_redDotImageElement = this.Q<VisualElement>(RED_DOT_IMAGE_TAG);
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
            m_greenDotImageElement.style.display = DisplayStyle.None;
            m_yellowDotImageElement.style.display = DisplayStyle.None;
            m_redDotImageElement.style.display = DisplayStyle.None;
            if (string.IsNullOrEmpty(WakatimeSettings.Instance.APIKey))
            {
                m_yellowDotImageElement.style.display = DisplayStyle.Flex;
            }
            else if(string.IsNullOrEmpty(WakatimeSettings.Instance.UserData.id))
            {
                m_redDotImageElement.style.display = DisplayStyle.Flex;
            }
            else
            {
                m_greenDotImageElement.style.display = DisplayStyle.Flex;
            }
        }
    }
}