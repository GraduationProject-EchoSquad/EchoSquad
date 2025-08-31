using UnityEngine;
using UnityEngine.UI;

public class SettingController : MonoBehaviour
{
    [Header("UI")]
    public Slider master_Slider;        // 마스터 음량
    public Slider effect_Slider;        // 이펙트 음량
    public Button closeButton;    // X 버튼

    [Header("Prefs Keys")]
    [SerializeField] private string keyA = "Setting.Master_Slider";
    [SerializeField] private string keyB = "Setting.Effect_Slider";

    [Header("Defaults")]
    [Range(0f, 1f)] public float defaultA = 1.0f;
    [Range(0f, 1f)] public float defaultB = 1.0f;

    bool _loaded = false;

    void Awake()
    {
        // X 버튼: 패널 닫기
        if (closeButton) closeButton.onClick.AddListener(ClosePanel);

        // 슬라이더 리스너 연결 (값 바꿀 때마다 저장)
        if (master_Slider) master_Slider.onValueChanged.AddListener(OnSliderAChanged);
        if (effect_Slider) effect_Slider.onValueChanged.AddListener(OnSliderBChanged);
    }

    void OnEnable()
    {
        // 패널이 열릴 때마다 최신 저장값 로드 후 UI 반영
        LoadAndApply();
    }

    void OnDestroy()
    {
        if (master_Slider) master_Slider.onValueChanged.RemoveListener(OnSliderAChanged);
        if (effect_Slider) effect_Slider.onValueChanged.RemoveListener(OnSliderBChanged);
        if (closeButton) closeButton.onClick.RemoveListener(ClosePanel);
    }

    void LoadAndApply()
    {
        _loaded = false; // 초기화 중에는 저장 트리거 방지

        float a = PlayerPrefs.HasKey(keyA) ? PlayerPrefs.GetFloat(keyA) : defaultA;
        float b = PlayerPrefs.HasKey(keyB) ? PlayerPrefs.GetFloat(keyB) : defaultB;

        if (master_Slider) master_Slider.value = a;
        if (effect_Slider) effect_Slider.value = b;

        _loaded = true;
    }

    void OnSliderAChanged(float v)
    {
        if (!_loaded) return; // 초기 로딩 중 이벤트 무시
        PlayerPrefs.SetFloat(keyA, v);
        PlayerPrefs.Save();
        // TODO: 실제 적용 로직(예: AudioMixer, 밝기 매니저 등)에 v 반영
    }

    void OnSliderBChanged(float v)
    {
        if (!_loaded) return;
        PlayerPrefs.SetFloat(keyB, v);
        PlayerPrefs.Save();
        // TODO: 실제 적용 로직에 v 반영
    }

    void ClosePanel()
    {
        gameObject.SetActive(false); // X 버튼으로 패널 닫기
    }

    // 기본값으로 되돌리기
    public void ResetToDefaults()
    {
        if (master_Slider) master_Slider.value = defaultA;
        if (effect_Slider) effect_Slider.value = defaultB;
    }

    // 외부에서 현재 값 읽기
    public float GetValueA() => master_Slider ? master_Slider.value : defaultA;
    public float GetValueB() => effect_Slider ? effect_Slider.value : defaultB;
}

