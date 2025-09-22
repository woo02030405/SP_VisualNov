using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VN.Dialogue;

public class SettingsUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Dropdown languageDropdown;
    [SerializeField] private TMP_Dropdown skipDropdown;
    [SerializeField] private Toggle autoToggle;
    [SerializeField] private Slider autoDelaySlider;
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Button applyButton;
    [SerializeField] private Button closeButton;

    [Header("Other References")]
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private DialogueManager dialogueManager;

    private const string PREF_LANG = "lang";        // "ko" / "en" / "jp"
    private const string PREF_BGM = "bgm";
    private const string PREF_SFX = "sfx";
    private const string PREF_SKIPMODE = "skipmode";
    private const string PREF_AUTOMODE = "automode";
    private const string PREF_AUTODELAY = "autodelay";

    private void Awake()
    {
        if (!panelRoot) panelRoot = gameObject;
    }

    private void Start()
    {
        if (languageDropdown == null || skipDropdown == null || autoToggle == null || autoDelaySlider == null)
        {
            Debug.LogError("[SettingsUI] UI 요소가 연결되지 않았습니다!");
            return;
        }

        // 언어
        string langCode = PlayerPrefs.GetString(PREF_LANG, "ko"); // "ko"/"en"
        languageDropdown.value = (langCode == "en") ? 1 : 0;
        languageDropdown.RefreshShownValue();

        // 볼륨
        bgmSlider.value = PlayerPrefs.GetFloat(PREF_BGM, 1f);
        sfxSlider.value = PlayerPrefs.GetFloat(PREF_SFX, 1f);

        // Skip 모드
        int skipMode = PlayerPrefs.GetInt(PREF_SKIPMODE, (int)SkipMode.ReadOnly);
        skipDropdown.value = skipMode;
        skipDropdown.RefreshShownValue();

        // Auto 모드
        bool auto = PlayerPrefs.GetInt(PREF_AUTOMODE, 0) == 1;
        float autoDelay = PlayerPrefs.GetFloat(PREF_AUTODELAY, 2f);
        autoToggle.isOn = auto;
        autoDelaySlider.value = autoDelay;

        if (applyButton) applyButton.onClick.AddListener(ApplySettings);
        if (closeButton) closeButton.onClick.AddListener(BackAction);
    }

    public void OpenPanel() => panelRoot.SetActive(true);

    public void BackAction()
    {
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "SettingsScene")
        {
            SceneNavigator.Load("MainMenuScene");
        }
        else
        {
            ClosePanel();
        }
    }

    public void ClosePanel() => panelRoot.SetActive(false);

    public void ApplySettings()
    {
        // 언어 드롭다운 변경 시 → "ko"/"en"로 저장
        string langCode = (languageDropdown.value == 1) ? "en" : "ko";
        PlayerPrefs.SetString(PREF_LANG, langCode);

        LocalizationManager.Language lang = (langCode == "en")
            ? LocalizationManager.Language.English
            : LocalizationManager.Language.Korean;
        LocalizationManager.SetLanguage(lang);

        // (참고) 실시간으로 텍스트 교체가 필요하다면, CSV 재로드 후 현재 노드 갱신 로직을 추가하는 편이 좋음
        dialogueManager?.RefreshCurrentNode();

        // 볼륨
        PlayerPrefs.SetFloat(PREF_BGM, bgmSlider.value);
        PlayerPrefs.SetFloat(PREF_SFX, sfxSlider.value);

        // Skip 모드
        int skipModeIndex = skipDropdown.value;
        PlayerPrefs.SetInt(PREF_SKIPMODE, skipModeIndex);
        if (dialogueManager != null)
            dialogueManager.skipMode = (SkipMode)skipModeIndex;

        // Auto 모드
        bool auto = autoToggle.isOn;
        float autoDelay = autoDelaySlider.value;
        PlayerPrefs.SetInt(PREF_AUTOMODE, auto ? 1 : 0);
        PlayerPrefs.SetFloat(PREF_AUTODELAY, autoDelay);
        if (dialogueManager != null)
        {
            dialogueManager.autoMode = auto;
            dialogueManager.autoDelay = autoDelay;
        }

        PlayerPrefs.Save();
        BackAction();

    }


}
