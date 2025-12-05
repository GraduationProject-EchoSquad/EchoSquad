using System;
using UnityEngine;
using UnityEngine.Audio;

public enum EAudioMixerType{Master,Music,SFX,Voice}
public class AudioManager : Singleton<AudioManager>
{
    [SerializeField] private AudioMixer audioMixer;
    
    // PlayerPrefs에 저장할 키의 접두사입니다. 충돌을 방지하는 좋은 습관입니다.
    private const string VolumePlayerPrefsKeyPrefix = "AudioVolume_";

    private bool[] isMute = new bool[3];
    private float[] audioVolumes = new float[3];

    private void Start()
    {
        CreateUIAudioIfNeeded();
        
        LoadAllAudioVolumes();
    }

    private void CreateUIAudioIfNeeded()
    {
        // 이미 씬에 존재하는지 확인
        GameObject existingUIAudio = GameObject.Find("UI Audio");

        if (existingUIAudio == null)
        {
            GameObject uiAudio = new GameObject("UI Audio");
            AudioSource audioSource = uiAudio.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f; // 2D 사운드
            audioSource.volume = 1f;
            SetMixerGroup(audioSource, EAudioMixerType.Master);

            // 씬 전환 시에도 유지 (선택사항)
            // DontDestroyOnLoad(uiAudio);

            Debug.Log("[UIManager] Created global UI Audio AudioSource");
        }
    }
    
    private void LoadAllAudioVolumes()
    {
        Debug.Log("저장된 오디오 볼륨 설정을 불러옵니다...");

        // EAudioMixerType 열거형의 모든 멤버를 배열로 가져옵니다.
        foreach (EAudioMixerType mixerType in Enum.GetValues(typeof(EAudioMixerType)))
        {
            // PlayerPrefs에서 값을 불러옵니다. 만약 저장된 값이 없다면 기본값으로 1.0f를 사용합니다.
            float savedVolume = GetAudioVolume(mixerType);

            // 불러온 값으로 실제 오디오 믹서 볼륨을 설정합니다.
            SetAudioVolume(mixerType, savedVolume);

            // 디버깅을 위해 로그를 남기는 것이 좋습니다.
            Debug.Log($"  - {mixerType}: 볼륨 {savedVolume} 적용됨 (Key: {mixerType})");
        }
    }

    public void SetMixerGroup(AudioSource audioSource, EAudioMixerType audioMixerType)
    {
        string audioMixerTypeString = EAudioMixerType.Master.ToString();
        AudioMixerGroup[] foundGroups = audioMixer.FindMatchingGroups(audioMixerTypeString);

        // 2. 그룹을 성공적으로 찾았는지 확인합니다.
        if (foundGroups.Length > 0)
        {
            // 3. 찾은 그룹(보통 배열의 첫 번째 요소)을 AudioSource에 '할당'합니다.
            audioSource.outputAudioMixerGroup = foundGroups[0];
            Debug.Log($"'{audioMixerTypeString}' 그룹을 AudioSource에 성공적으로 할당했습니다.");
        }
        else
        {
            Debug.LogError($"'{audioMixerTypeString}' 그룹을 mainMixer에서 찾을 수 없습니다! 이름을 확인하세요.");
        }
    }
    
    public void SetAudioVolume(EAudioMixerType audioMixerType, float volume)
    {
        // volume이 0이 되는 것을 방지하여 Log10 오류를 막습니다.
        volume = Mathf.Max(volume, 0.0001f);
        
        // 오디오 믹서의 값은 -80 ~ 0까지이기 때문에 0.0001 ~ 1의 Log10 * 20을 한다.
        audioMixer.SetFloat(audioMixerType.ToString(), Mathf.Log10(volume) * 20);
        
        string prefsKey = VolumePlayerPrefsKeyPrefix + audioMixerType.ToString();
        PlayerPrefs.SetFloat(prefsKey, volume);
    }
    
    public float GetAudioVolume(EAudioMixerType audioMixerType)
    {
        string prefsKey = VolumePlayerPrefsKeyPrefix + audioMixerType.ToString();

        // PlayerPrefs에서 값을 불러옵니다. 만약 저장된 값이 없다면 기본값으로 1.0f를 사용합니다.
        float savedVolume = PlayerPrefs.GetFloat(prefsKey, 1.0f);
        return savedVolume;
    }

    public void SetAudioMute(EAudioMixerType audioMixerType)
    {
        int type = (int)audioMixerType;
        if (!isMute[type]) // 뮤트 
        {
            isMute[type] = true;
            audioMixer.GetFloat(audioMixerType.ToString(), out float curVolume);
            audioVolumes[type] = curVolume;
            SetAudioVolume(audioMixerType, 0.001f);
        }
        else
        {
            isMute[type] = false;
            SetAudioVolume(audioMixerType, audioVolumes[type]);
        }
    }
    
}