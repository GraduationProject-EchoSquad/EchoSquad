using UnityEngine;
using Cysharp.Threading.Tasks;

/// <summary>
/// TeammateController가 사용할 간단한 음성 재생 인터페이스
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class VoiceSpeaker : MonoBehaviour
{
    private AudioSource audioSource;
    private VoiceProfile voiceProfile;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1f; // 3D 사운드
    }

    /// <summary>
    /// VoiceProfile 설정
    /// </summary>
    public void SetVoiceProfile(VoiceProfile profile)
    {
        voiceProfile = profile;
    }

    /// <summary>
    /// 텍스트를 음성으로 재생
    /// </summary>
    public async UniTask SpeakAsync(string text)
    {
        Debug.Log($"[VoiceSpeaker] SpeakAsync called: '{text}' | VoiceProfile null: {voiceProfile == null}");

        if (voiceProfile == null)
        {
            Debug.LogWarning($"[VoiceSpeaker] {gameObject.name} has no VoiceProfile!");
            return;
        }

        Debug.Log($"[VoiceSpeaker] Requesting TTS for: '{text}' with profile Gender={voiceProfile.gender}, Pitch={voiceProfile.pitch}, Speed={voiceProfile.speed}");

        // SparkTTSManager를 통해 AudioClip 가져오기 (캐시 우선)
        AudioClip clip = await SparkTTSManager.Instance.GenerateClipAsync(text, voiceProfile);

        Debug.Log($"[VoiceSpeaker] TTS result: clip null={clip == null}");

        if (clip != null)
        {
            audioSource.clip = clip;
            audioSource.Play();
            Debug.Log($"[VoiceSpeaker] {gameObject.name} playing: \"{text}\" (AudioSource.isPlaying={audioSource.isPlaying})");
        }
        else
        {
            Debug.LogError($"[VoiceSpeaker] Failed to generate clip for: {text}");
        }
    }

    /// <summary>
    /// 모듈식 음성 재생 (여러 조각 연속 재생)
    /// </summary>
    public async UniTask SpeakModulesAsync(params string[] modules)
    {
        foreach (string module in modules)
        {
            await SpeakAsync(module);
            await UniTask.WaitUntil(() => !audioSource.isPlaying); // 재생 완료 대기
        }
    }

    /// <summary>
    /// 재생 중지
    /// </summary>
    public void Stop()
    {
        if (audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }
}
