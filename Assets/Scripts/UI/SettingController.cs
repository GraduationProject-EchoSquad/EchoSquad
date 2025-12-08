using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SettingController : UIBase
{
    [Header("UI")] public Slider master_Slider;
    public Slider effect_Slider;
    public Slider voice_Slider;
    public Dropdown micDropdown;
    public Button closeButton;
    [Header("Defaults")] [Range(0f, 1f)] public float defaultMaster = 1.0f;
    [Range(0f, 1f)] public float defaultEffect = 1.0f;
    [Range(0f, 1f)] public float defaultVoice = 1.0f;

    private UnityAction<float> _masterSliderAction;
    private UnityAction<float> _effectSliderAction;
    private UnityAction<float> _voiceSliderAction;

    void Start()
    {
        // X ��ư: �г� �ݱ�
        if (closeButton) closeButton.onClick.AddListener(ClosePanel);


        _masterSliderAction = (value) => OnSliderChanged(EAudioMixerType.Master, value);
        _effectSliderAction = (value) => OnSliderChanged(EAudioMixerType.SFX, value);
        _voiceSliderAction = (value) => OnSliderChanged(EAudioMixerType.Voice, value);

        // 3. 변수에 저장된 액션을 리스너로 등록합니다.
        if (master_Slider) master_Slider.onValueChanged.AddListener(_masterSliderAction);
        if (effect_Slider) effect_Slider.onValueChanged.AddListener(_effectSliderAction);
        if (voice_Slider) voice_Slider.onValueChanged.AddListener(_voiceSliderAction);

        if (micDropdown)
        {
            MicrophoneManager.Instance.SetDropDown(micDropdown);
            LoadAndApply();
            micDropdown.onValueChanged.AddListener((idx) => MicrophoneManager.Instance.SetMicrophoneRecordIndex(idx));
        }
    }

    void OnEnable()
    {
        LoadAndApply();
    }

    void OnDestroy()
    {
        // 4. 등록할 때 사용했던 '바로 그 변수'를 사용하여 리스너를 정확히 제거합니다.
        if (master_Slider) master_Slider.onValueChanged.RemoveListener(_masterSliderAction);
        if (effect_Slider) effect_Slider.onValueChanged.RemoveListener(_effectSliderAction);
        if (voice_Slider) voice_Slider.onValueChanged.RemoveListener(_voiceSliderAction);

        // 버튼 리스너도 제거합니다.
        if (closeButton) closeButton.onClick.RemoveListener(ClosePanel);
    }

    void LoadAndApply()
    {
        if (master_Slider) master_Slider.value = AudioManager.Instance.GetAudioVolume(EAudioMixerType.Master);
        if (effect_Slider) effect_Slider.value = AudioManager.Instance.GetAudioVolume(EAudioMixerType.SFX);
        if (voice_Slider) voice_Slider.value = AudioManager.Instance.GetAudioVolume(EAudioMixerType.Voice);
        
        List<string> options = MicrophoneManager.Instance.microphoneRecord.AvailableMicDevices.ToList();
        string currentMic = MicrophoneManager.Instance.microphoneRecord.SelectedMicDevice;
        int currentIndex = options.IndexOf(currentMic);
        if (currentIndex != -1)
        {
            //기본값이 dropDown에 추가되므로 + 1
            micDropdown.value = currentIndex + 1;
        }
    }

    void OnSliderChanged(EAudioMixerType audioMixerType, float v)
    {
        AudioManager.Instance.SetAudioVolume(audioMixerType, v);
        // TODO: ���� ���� ����(��: AudioMixer, ��� �Ŵ��� ��)�� v �ݿ�
    }

    void ClosePanel()
    {
        gameObject.SetActive(false); // X ��ư���� �г� �ݱ�
    }

    // �⺻������ �ǵ�����
    public void ResetToDefaults()
    {
        if (master_Slider) master_Slider.value = defaultMaster;
        if (effect_Slider) effect_Slider.value = defaultEffect;
        if (voice_Slider) voice_Slider.value = defaultVoice;
    }

    // �ܺο��� ���� �� �б�
    public float GetValueA() => master_Slider ? master_Slider.value : defaultMaster;
    public float GetValueB() => effect_Slider ? effect_Slider.value : defaultEffect;
}