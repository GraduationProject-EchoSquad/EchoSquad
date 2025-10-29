using System.Collections.Generic;
using System.IO;
using Cysharp.Threading.Tasks;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Preview Voice용 모든 음성 조합(50개)을 미리 생성하여 Resources 폴더에 저장
/// GameObject에 붙여서 PlayMode에서 실행
/// Inspector에서 우클릭 > Generate All Preview Voices 선택
/// </summary>
public class PreviewVoiceGenerator : MonoBehaviour
{
    [Header("Preview Settings")]
    [Tooltip("Preview에서 사용할 텍스트 (기본값: Hello)")]
    [SerializeField] private string previewText = "Hello";

    [Header("Generation Progress")]
    [Tooltip("현재 생성 진행률")]
    [SerializeField] private int currentProgress = 0;

    [Tooltip("전체 생성 개수")]
    [SerializeField] private int totalCount = 50;

    [Tooltip("생성 중 여부")]
    [SerializeField] private bool isGenerating = false;

    private const string RESOURCES_FOLDER = "Assets/Resources/PreviewVoices";
    private const int DELAY_BETWEEN_GENERATION_MS = 5000; // 각 음성 생성 후 대기 시간 (5초)

    /// <summary>
    /// Inspector에서 우클릭 > Generate All Preview Voices 선택하여 실행
    /// </summary>
    [ContextMenu("Generate All Preview Voices (50 combinations)")]
    public void GenerateAllPreviewVoices()
    {
        if (isGenerating)
        {
            Debug.LogWarning("[PreviewVoiceGenerator] Already generating voices. Please wait...");
            return;
        }

        if (!Application.isPlaying)
        {
            Debug.LogError("[PreviewVoiceGenerator] Must be in PlayMode! Press Play button first.");
            return;
        }

        if (SparkTTSManager.Instance == null)
        {
            Debug.LogError("[PreviewVoiceGenerator] SparkTTSManager not found! Make sure it exists in the scene.");
            return;
        }

        Debug.Log("[PreviewVoiceGenerator] ⚠️ First voice will take 1-5 minutes. Please be patient...");
        GenerateAllAsync().Forget();
    }

    /// <summary>
    /// 테스트: 1개만 생성
    /// </summary>
    [ContextMenu("TEST: Generate Only 1 Voice (Female/VeryHigh/VeryHigh)")]
    public void GenerateOneVoiceTest()
    {
        if (!Application.isPlaying)
        {
            Debug.LogError("[PreviewVoiceGenerator] Must be in PlayMode!");
            return;
        }

        if (SparkTTSManager.Instance == null)
        {
            Debug.LogError("[PreviewVoiceGenerator] SparkTTSManager not found!");
            return;
        }

        Debug.Log("[PreviewVoiceGenerator] TEST: Generating 1 voice (Female/VeryHigh/VeryHigh)...");
        GenerateOneTestAsync().Forget();
    }

    private async UniTaskVoid GenerateOneTestAsync()
    {
        isGenerating = true;

#if UNITY_EDITOR
        if (!Directory.Exists(RESOURCES_FOLDER))
        {
            Directory.CreateDirectory(RESOURCES_FOLDER);
            AssetDatabase.Refresh();
        }
#endif

        Debug.Log("[PreviewVoiceGenerator] TEST: Starting...");

        await GenerateSingleVoice(VoiceGender.Female, VoiceProperty.VeryHigh, VoiceProperty.VeryHigh);

        Debug.Log("[PreviewVoiceGenerator] TEST: Complete!");

#if UNITY_EDITOR
        AssetDatabase.Refresh();
#endif

        isGenerating = false;
    }

    private async UniTaskVoid GenerateAllAsync()
    {
        isGenerating = true;
        currentProgress = 0;
        totalCount = 2 * 5 * 5; // 50개

        // Resources 폴더 생성
#if UNITY_EDITOR
        if (!Directory.Exists(RESOURCES_FOLDER))
        {
            Directory.CreateDirectory(RESOURCES_FOLDER);
            AssetDatabase.Refresh();
        }
#endif

        Debug.Log($"[PreviewVoiceGenerator] ===== Starting Generation =====");
        Debug.Log($"[PreviewVoiceGenerator] Text: '{previewText}'");
        Debug.Log($"[PreviewVoiceGenerator] Total: {totalCount} voices");
        Debug.Log($"[PreviewVoiceGenerator] Save to: {RESOURCES_FOLDER}");

        var startTime = System.DateTime.Now;

        // 모든 조합 생성
        foreach (VoiceGender gender in System.Enum.GetValues(typeof(VoiceGender)))
        {
            foreach (VoiceProperty pitch in System.Enum.GetValues(typeof(VoiceProperty)))
            {
                foreach (VoiceProperty speed in System.Enum.GetValues(typeof(VoiceProperty)))
                {
                    await GenerateSingleVoice(gender, pitch, speed);

                    currentProgress++;

                    // 진행률 로그
                    if (currentProgress % 5 == 0 || currentProgress == totalCount)
                    {
                        float percentage = (float)currentProgress / totalCount * 100f;
                        Debug.Log($"[PreviewVoiceGenerator] Progress: {currentProgress}/{totalCount} ({percentage:F1}%)");
                    }

                    // 딜레이
                    await UniTask.Delay(DELAY_BETWEEN_GENERATION_MS);
                }
            }
        }

#if UNITY_EDITOR
        // AssetDatabase Refresh
        Debug.Log("[PreviewVoiceGenerator] Refreshing AssetDatabase...");
        AssetDatabase.Refresh();
#endif

        var endTime = System.DateTime.Now;
        var duration = endTime - startTime;

        Debug.Log($"[PreviewVoiceGenerator] ===== Complete! =====");
        Debug.Log($"[PreviewVoiceGenerator] Generated: {currentProgress}/{totalCount}");
        Debug.Log($"[PreviewVoiceGenerator] Time: {duration.TotalMinutes:F1} minutes ({duration.TotalSeconds:F0} seconds)");
        Debug.Log($"[PreviewVoiceGenerator] Location: {RESOURCES_FOLDER}");

        isGenerating = false;
    }

    private async UniTask GenerateSingleVoice(VoiceGender gender, VoiceProperty pitch, VoiceProperty speed)
    {
        string genderStr = gender.ToApiString();
        string pitchStr = pitch.ToApiString();
        string speedStr = speed.ToApiString();
        string comboName = $"{genderStr}_{pitchStr}_{speedStr}";

        try
        {
            Debug.Log($"[PreviewVoiceGenerator] [{currentProgress + 1}/{totalCount}] Starting: {comboName}");

            // AudioSource의 이전 clip 지우기
            if (SparkTTSManager.Instance.audioSource != null)
            {
                SparkTTSManager.Instance.audioSource.clip = null;
            }

            // 원래 Preview 버튼처럼 CreateStyleVoice 호출
            SparkTTSManager.Instance.CreateStyleVoice(previewText, gender, pitch, speed);

            // TTS 생성 완료 대기 (최대 120초 = 2분)
            int waitTime = 0;
            int maxWaitTime = 120000; // 2분
            int checkInterval = 1000; // 1초마다 체크

            while (waitTime < maxWaitTime)
            {
                await UniTask.Delay(checkInterval);
                waitTime += checkInterval;

                // AudioSource에 clip이 설정되었는지 확인
                if (SparkTTSManager.Instance.audioSource != null &&
                    SparkTTSManager.Instance.audioSource.clip != null)
                {
                    Debug.Log($"[PreviewVoiceGenerator] AudioClip ready after {waitTime / 1000}s");
                    break;
                }

                // 30초마다 로그
                if (waitTime % 30000 == 0)
                {
                    Debug.Log($"[PreviewVoiceGenerator] Still waiting... {waitTime / 1000}s elapsed");
                }
            }

            // AudioClip 가져오기
            AudioClip clip = SparkTTSManager.Instance.audioSource?.clip;

            if (clip != null)
            {
                // 오디오 재생이 끝날 때까지 대기
                AudioSource audioSource = SparkTTSManager.Instance.audioSource;
                if (audioSource != null && audioSource.isPlaying)
                {
                    Debug.Log($"[PreviewVoiceGenerator] Waiting for audio playback to finish...");
                    while (audioSource.isPlaying)
                    {
                        await UniTask.Delay(100);
                    }
                    Debug.Log($"[PreviewVoiceGenerator] Audio playback finished");
                }

                // 추가 대기 (SparkTTS 내부 정리)
                await UniTask.Delay(2000);

                string textSafe = previewText.ToLower().Replace(" ", "_");
                string fileName = $"{comboName}_{textSafe}.wav";
                string filePath = Path.Combine(RESOURCES_FOLDER, fileName);

                // Resources 폴더에 WAV 저장
                await SaveWavFile(filePath, clip);

                Debug.Log($"[PreviewVoiceGenerator] ✓ [{currentProgress + 1}/{totalCount}] SUCCESS: {fileName}");
            }
            else
            {
                Debug.LogWarning($"[PreviewVoiceGenerator] ✗ [{currentProgress + 1}/{totalCount}] SKIP: {comboName} - No AudioClip after {waitTime / 1000}s (continuing...)");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[PreviewVoiceGenerator] ✗ [{currentProgress + 1}/{totalCount}] ERROR: {comboName} - {e.Message} (continuing...)");
            // 에러가 나도 계속 진행
        }
    }

    /// <summary>
    /// AudioClip을 WAV 파일로 저장
    /// </summary>
    private async UniTask SaveWavFile(string filePath, AudioClip clip)
    {
        // 메인 스레드에서 AudioClip 데이터 추출
        float[] samples = new float[clip.samples * clip.channels];
        clip.GetData(samples, 0);
        int sampleRate = clip.frequency;
        int channels = clip.channels;

        // 백그라운드 스레드에서 파일 쓰기
        await UniTask.SwitchToThreadPool();

        try
        {
            using (FileStream fs = File.Create(filePath))
            using (BinaryWriter bw = new BinaryWriter(fs))
            {
                // WAV 헤더
                int byteRate = sampleRate * channels * 2;
                int blockAlign = channels * 2;

                bw.Write(new char[4] { 'R', 'I', 'F', 'F' });
                bw.Write(36 + samples.Length * 2);
                bw.Write(new char[4] { 'W', 'A', 'V', 'E' });
                bw.Write(new char[4] { 'f', 'm', 't', ' ' });
                bw.Write(16);
                bw.Write((ushort)1);
                bw.Write((ushort)channels);
                bw.Write(sampleRate);
                bw.Write(byteRate);
                bw.Write((ushort)blockAlign);
                bw.Write((ushort)16);

                bw.Write(new char[4] { 'd', 'a', 't', 'a' });
                bw.Write(samples.Length * 2);

                foreach (float sample in samples)
                {
                    short intSample = (short)(sample * short.MaxValue);
                    bw.Write(intSample);
                }
            }
        }
        finally
        {
            await UniTask.SwitchToMainThread();
        }
    }


    /// <summary>
    /// 현재 캐시된 음성 개수 확인
    /// </summary>
    [ContextMenu("Check Existing Voices")]
    public void CheckExistingVoices()
    {
        if (!Directory.Exists(RESOURCES_FOLDER))
        {
            Debug.Log("[PreviewVoiceGenerator] Resources/PreviewVoices folder not found. Generate voices first.");
            return;
        }

        string[] wavFiles = Directory.GetFiles(RESOURCES_FOLDER, "*.wav", SearchOption.TopDirectoryOnly);

        Debug.Log($"[PreviewVoiceGenerator] Existing voices: {wavFiles.Length}/50");
        Debug.Log($"[PreviewVoiceGenerator] Location: {RESOURCES_FOLDER}");
    }

    /// <summary>
    /// Resources 폴더 열기
    /// </summary>
    [ContextMenu("Open Resources Folder")]
    public void OpenResourcesFolder()
    {
#if UNITY_EDITOR
        if (!Directory.Exists(RESOURCES_FOLDER))
        {
            Directory.CreateDirectory(RESOURCES_FOLDER);
            AssetDatabase.Refresh();
        }

        // Project 창에서 폴더 선택
        Object obj = AssetDatabase.LoadAssetAtPath<Object>(RESOURCES_FOLDER);
        if (obj != null)
        {
            Selection.activeObject = obj;
            EditorGUIUtility.PingObject(obj);
        }
#endif
    }
}
