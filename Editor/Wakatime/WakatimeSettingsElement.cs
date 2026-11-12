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
        private const string APIKEY_GREEN_DOT_IMAGE_TAG = "api-key-green-dot-image";
        private const string APIKEY_YELLOW_DOT_IMAGE_TAG = "api-key-yellow-dot-image";
        private const string APIKEY_RED_DOT_IMAGE_TAG = "api-key-red-dot-image";
        private const string API_LINK_FULL_ELEMENT_TAG = "api-link-full-element";
        private const string API_WEBLINK_LABEL_TAG = "api-weblink-label";
        private const string OAUTH_TOKEN_FIELD_TAG = "oauth-token-field";
        private const string OAUTH_TOKEN_GREEN_DOT_IMAGE_TAG = "oauth-token-green-dot-image";
        private const string OAUTH_TOKEN_YELLOW_DOT_IMAGE_TAG = "oauth-token-yellow-dot-image";
        private const string OAUTH_TOKEN_RED_DOT_IMAGE_TAG = "oauth-token-red-dot-image";
        private const string OAUTH_LINK_FULL_ELEMENT_TAG = "oauth-link-full-element";
        private const string OAUTH_WEBLINK_LABEL_TAG = "oauth-weblink-label";
        private const string KEYSTROKE_TIMEOUT_ENUM_TAG = "keystroke-timeout-enum";

        private const string API_KEY_URL = "https://wakatime.com/api-key";
        private const string OAUTH_TOKEN_URL = "https://wakatime.com/oauth";

        private TextField m_apiKeyField;
        private VisualElement m_apiKeyGreenDotImageElement;
        private VisualElement m_apiKeyYellowDotImageElement;
        private VisualElement m_apiKeyRedDotImageElement;
        private VisualElement m_apiLinkFullElement;
        private Label m_apiWeblinkLabel;
        private TextField m_oauthTokenField;
        private VisualElement m_oauthTokenGreenDotImageElement;
        private VisualElement m_oauthTokenYellowDotImageElement;
        private VisualElement m_oauthTokenRedDotImageElement;
        private VisualElement m_oauthLinkFullElement;
        private Label m_oauthWeblinkLabel;
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
            m_apiKeyGreenDotImageElement = this.Q<VisualElement>(APIKEY_GREEN_DOT_IMAGE_TAG);
            m_apiKeyYellowDotImageElement = this.Q<VisualElement>(APIKEY_YELLOW_DOT_IMAGE_TAG);
            m_apiKeyRedDotImageElement = this.Q<VisualElement>(APIKEY_RED_DOT_IMAGE_TAG);
            m_apiLinkFullElement = this.Q<VisualElement>(API_LINK_FULL_ELEMENT_TAG);
            m_apiWeblinkLabel = this.Q<Label>(API_WEBLINK_LABEL_TAG);
            m_oauthTokenField = this.Q<TextField>(OAUTH_TOKEN_FIELD_TAG);
            m_oauthTokenGreenDotImageElement = this.Q<VisualElement>(OAUTH_TOKEN_GREEN_DOT_IMAGE_TAG);
            m_oauthTokenYellowDotImageElement = this.Q<VisualElement>(OAUTH_TOKEN_YELLOW_DOT_IMAGE_TAG);
            m_oauthTokenRedDotImageElement = this.Q<VisualElement>(OAUTH_TOKEN_RED_DOT_IMAGE_TAG);
            m_oauthLinkFullElement = this.Q<VisualElement>(OAUTH_LINK_FULL_ELEMENT_TAG);
            m_oauthWeblinkLabel = this.Q<Label>(OAUTH_WEBLINK_LABEL_TAG);
            m_keystrokeTimeoutEnum = this.Q<EnumField>(KEYSTROKE_TIMEOUT_ENUM_TAG);

            m_apiKeyField.value = WakatimeSettings.Instance.APIKey;
            m_oauthTokenField.value = WakatimeSettings.Instance.OAuthToken;
            WakatimeSettings.Instance.OnSettingsReevaluated += OnSettingsReevaluated;
            m_apiKeyField.RegisterValueChangedCallback(evt => { WakatimeSettings.Instance.APIKey = evt.newValue; });
            m_oauthTokenField.RegisterValueChangedCallback(evt => { WakatimeSettings.Instance.OAuthToken = evt.newValue; });
            m_apiWeblinkLabel.AddManipulator(new Clickable(() => { Application.OpenURL(API_KEY_URL); }));
            m_oauthWeblinkLabel.AddManipulator(new Clickable(() => { Application.OpenURL(OAUTH_TOKEN_URL); }));
            m_keystrokeTimeoutEnum.value = WakatimeSettings.Instance.KeystrokeTimeout;
            m_keystrokeTimeoutEnum.RegisterValueChangedCallback(evt => WakatimeSettings.Instance.KeystrokeTimeout = (KeystrokeTimeout)evt.newValue);

            OnSettingsReevaluated();
        }

        private void OnSettingsReevaluated()
        {
            WakatimeSettings settings = WakatimeSettings.Instance;

            m_apiLinkFullElement.style.display = !settings.CanRun ? DisplayStyle.Flex : DisplayStyle.None;
            ShowStatusDot(m_apiKeyGreenDotImageElement, m_apiKeyYellowDotImageElement, m_apiKeyRedDotImageElement,
                string.IsNullOrEmpty(settings.APIKey), settings.CanRun);

            m_oauthLinkFullElement.style.display = !settings.CanFetchStats ? DisplayStyle.Flex : DisplayStyle.None;
            ShowStatusDot(m_oauthTokenGreenDotImageElement, m_oauthTokenYellowDotImageElement, m_oauthTokenRedDotImageElement,
                string.IsNullOrEmpty(settings.OAuthToken), settings.CanFetchStats);
        }

        /// <summary>
        /// Yellow when the value is empty, green when it's valid, red otherwise.
        /// </summary>
        private static void ShowStatusDot(VisualElement greenDot, VisualElement yellowDot, VisualElement redDot, bool isEmpty, bool isValid)
        {
            yellowDot.style.display = isEmpty ? DisplayStyle.Flex : DisplayStyle.None;
            greenDot.style.display = !isEmpty && isValid ? DisplayStyle.Flex : DisplayStyle.None;
            redDot.style.display = !isEmpty && !isValid ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }
}
