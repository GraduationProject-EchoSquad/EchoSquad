using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>
/// 게임 시작 전 선택된 VoiceProfile로 미리 TTS 음성 파일을 생성하는 매니저
/// SparkTTSManager의 캐싱 시스템을 활용하여 백그라운드에서 음성 생성
/// </summary>
public class VoicePreGenerator : Singleton<VoicePreGenerator>
{
    [Header("Voice Module System")]
    [Tooltip("모듈식 음성 조각을 미리 생성 (단어/구절 조합 방식)")]
    [SerializeField] private bool useModularVoice = true;

    /// <summary>
    /// 특정 동료의 VoiceProfile로 TTS 음성을 미리 생성
    /// 모듈식: 작은 단위(단어/구절)를 미리 만들어서 조합 사용
    /// </summary>
    public async UniTask GenerateVoiceForTeammate(string teammateName, VoiceProfile profile, System.Action<int, int> onProgress = null)
    {
        if (profile == null)
        {
            Debug.LogError($"[VoicePreGenerator] {teammateName}의 VoiceProfile is null.");
            return;
        }

        Debug.Log($"[VoicePreGenerator] {teammateName} voice generation started (Gender:{profile.gender}, Pitch:{profile.pitch}, Speed:{profile.speed})");

        List<string> phrasesToGenerate;

        if (useModularVoice)
        {
            // 모듈식: 모든 작은 조각들만 생성
            phrasesToGenerate = VoiceModules.GetAllModules();
            Debug.Log($"[VoicePreGenerator] Using modular voice system: {phrasesToGenerate.Count} modules");
        }
        else
        {
            // 전체 문장 방식
            int liveliness = VoicePropertyToInt(profile.speed); // Speed → Liveliness
            int mood = VoicePropertyToInt(profile.pitch);       // Pitch → Mood
            phrasesToGenerate = VoiceLineTemplates.GetAllLinesForProfile(liveliness, mood);
            Debug.Log($"[VoicePreGenerator] Using full phrase system: {phrasesToGenerate.Count} phrases");
        }

        // TTS 생성 및 파일로 저장
        int count = 0;
        int total = phrasesToGenerate.Count;

        foreach (string phrase in phrasesToGenerate)
        {
            // GenerateClipAsync: 캐시 확인 → 없으면 생성 → 디스크 저장
            AudioClip clip = await SparkTTSManager.Instance.GenerateClipAsync(phrase, profile);

            count++;

            // 진행률 콜백 호출
            onProgress?.Invoke(count, total);

            if (count % 5 == 0 || count == total)
            {
                Debug.Log($"[VoicePreGenerator] {teammateName} progress: {count}/{total}");
            }

            // UI 업데이트를 위해 메인 스레드로 돌아감
            await UniTask.Yield();

            // TTS 생성 간격 (너무 빠르면 GPU 과부하)
            await UniTask.Delay(100);
        }

        Debug.Log($"[VoicePreGenerator] {teammateName} voice generation completed! ({count} items saved to disk)");
    }

    /// <summary>
    /// VoiceProperty enum을 0~4 정수로 변환
    /// </summary>
    private int VoicePropertyToInt(VoiceProperty prop)
    {
        switch (prop)
        {
            case VoiceProperty.VeryLow: return 0;
            case VoiceProperty.Low: return 1;
            case VoiceProperty.Moderate: return 2;
            case VoiceProperty.High: return 3;
            case VoiceProperty.VeryHigh: return 4;
            default: return 2;
        }
    }

    /// <summary>
    /// 모든 동료의 음성을 한 번에 생성 (병렬 처리)
    /// </summary>
    public async UniTask GenerateAllTeammateVoices(Dictionary<string, VoiceProfile> teammateProfiles)
    {
        List<UniTask> tasks = new List<UniTask>();

        foreach (var kvp in teammateProfiles)
        {
            string teammateName = kvp.Key;
            VoiceProfile profile = kvp.Value;

            tasks.Add(GenerateVoiceForTeammate(teammateName, profile));
        }

        // 모든 동료의 음성 생성 병렬 처리
        await UniTask.WhenAll(tasks);

        Debug.Log("[VoicePreGenerator] 모든 동료 음성 생성 완료!");
    }

    /// <summary>
    /// 모듈식 음성 사용 여부 설정 (런타임에서 호출 가능)
    /// </summary>
    public void SetUseModularVoice(bool useModular)
    {
        useModularVoice = useModular;
    }

    /// <summary>
    /// 특정 VoiceKey의 음성이 이미 캐싱되어 있는지 확인
    /// (SparkTTSManager의 폴더 존재 여부 체크)
    /// </summary>
    public bool IsVoiceCached(VoiceProfile profile)
    {
        if (profile == null) return false;

        string folderName = $"{profile.gender.ToApiString()}_{profile.pitch.ToApiString()}_{profile.speed.ToApiString()}";
        string folderPath = System.IO.Path.Combine(Application.persistentDataPath, folderName);

        return System.IO.Directory.Exists(folderPath);
    }
}
