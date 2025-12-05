using UnityEngine;
using UnityEngine.UI;

public class SettingController : UIBase
{
    [Header("UI")]
    public Slider master_Slider;        // ������ ����
    public Slider effect_Slider;        // ����Ʈ ����
    public Button closeButton;    // X ��ư

    [Header("Prefs Keys")]
    [SerializeField] private string keyA = "Setting.Master_Slider";
    [SerializeField] private string keyB = "Setting.Effect_Slider";

    [Header("Defaults")]
    [Range(0f, 1f)] public float defaultA = 1.0f;
    [Range(0f, 1f)] public float defaultB = 1.0f;

    bool _loaded = false;

    void Awake()
    {
        // X ��ư: �г� �ݱ�
        if (closeButton) closeButton.onClick.AddListener(ClosePanel);

        // �����̴� ������ ���� (�� �ٲ� ������ ����)
        if (master_Slider) master_Slider.onValueChanged.AddListener(OnSliderAChanged);
        if (effect_Slider) effect_Slider.onValueChanged.AddListener(OnSliderBChanged);
    }

    void OnEnable()
    {
        // �г��� ���� ������ �ֽ� ���尪 �ε� �� UI �ݿ�
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
        _loaded = false; // �ʱ�ȭ �߿��� ���� Ʈ���� ����

        float a = PlayerPrefs.HasKey(keyA) ? PlayerPrefs.GetFloat(keyA) : defaultA;
        float b = PlayerPrefs.HasKey(keyB) ? PlayerPrefs.GetFloat(keyB) : defaultB;

        if (master_Slider) master_Slider.value = a;
        if (effect_Slider) effect_Slider.value = b;

        _loaded = true;
    }

    void OnSliderAChanged(float v)
    {
        if (!_loaded) return; // �ʱ� �ε� �� �̺�Ʈ ����
        PlayerPrefs.SetFloat(keyA, v);
        PlayerPrefs.Save();
        // TODO: ���� ���� ����(��: AudioMixer, ��� �Ŵ��� ��)�� v �ݿ�
    }

    void OnSliderBChanged(float v)
    {
        if (!_loaded) return;
        PlayerPrefs.SetFloat(keyB, v);
        PlayerPrefs.Save();
        // TODO: ���� ���� ������ v �ݿ�
    }

    void ClosePanel()
    {
        gameObject.SetActive(false); // X ��ư���� �г� �ݱ�
    }

    // �⺻������ �ǵ�����
    public void ResetToDefaults()
    {
        if (master_Slider) master_Slider.value = defaultA;
        if (effect_Slider) effect_Slider.value = defaultB;
    }

    // �ܺο��� ���� �� �б�
    public float GetValueA() => master_Slider ? master_Slider.value : defaultA;
    public float GetValueB() => effect_Slider ? effect_Slider.value : defaultB;
}

