using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Cysharp.Threading.Tasks;

/// <summary>
/// 생성된 TTS AudioClip을 디스크에 캐싱
/// VoiceProfile + Text 조합별로 .wav 파일로 저장
/// </summary>
public class VoiceClipCache : Singleton<VoiceClipCache>
{
    // 메모리 캐시 (빠른 접근)
    private Dictionary<string, AudioClip> memoryCache = new Dictionary<string, AudioClip>();

    // 캐시 루트 폴더
    private string cacheRoot;

    protected override void Awake()
    {
        base.Awake();
        cacheRoot = Path.Combine(Application.persistentDataPath, "VoiceClipCache");
        Directory.CreateDirectory(cacheRoot);
        Debug.Log($"[VoiceClipCache] Cache root: {cacheRoot}");
    }

    /// <summary>
    /// 캐시 키 생성: "male_moderate_moderate/hello" (소문자로 정규화)
    /// </summary>
    private string GetCacheKey(VoiceProfile profile, string text)
    {
        string profileKey = $"{profile.gender.ToApiString()}_{profile.pitch.ToApiString()}_{profile.speed.ToApiString()}";
        // 텍스트를 파일명 안전하게 변환 (대소문자 무시를 위해 소문자로 변환)
        string safeText = text.ToLower().Replace(" ", "_").Replace("!", "").Replace("?", "").Replace(",", "");
        if (safeText.Length > 50) safeText = safeText.Substring(0, 50); // 길이 제한
        return $"{profileKey}/{safeText}";
    }

    /// <summary>
    /// AudioClip 캐시에 저장
    /// </summary>
    public async UniTask SaveClip(VoiceProfile profile, string text, AudioClip clip)
    {
        if (clip == null) return;

        string cacheKey = GetCacheKey(profile, text);
        string filePath = Path.Combine(cacheRoot, cacheKey + ".wav");

        // 디렉토리 생성
        string directory = Path.GetDirectoryName(filePath);
        Directory.CreateDirectory(directory);

        // WAV 파일로 저장
        await SaveWavFile(filePath, clip);

        // 메모리 캐시에도 저장
        memoryCache[cacheKey] = clip;

        Debug.Log($"[VoiceClipCache] Saved: {cacheKey}");
    }

    /// <summary>
    /// 캐시에서 AudioClip 로드 (메모리 → 디스크 순서)
    /// </summary>
    public async UniTask<AudioClip> LoadClip(VoiceProfile profile, string text)
    {
        string cacheKey = GetCacheKey(profile, text);

        // 1. 메모리 캐시 확인
        if (memoryCache.TryGetValue(cacheKey, out AudioClip cachedClip))
        {
            Debug.Log($"[VoiceClipCache] Memory hit: {cacheKey}");
            return cachedClip;
        }

        // 2. 디스크 캐시 확인
        string filePath = Path.Combine(cacheRoot, cacheKey + ".wav");
        if (File.Exists(filePath))
        {
            Debug.Log($"[VoiceClipCache] Disk hit: {cacheKey}");
            AudioClip clip = await LoadWavFile(filePath);
            if (clip != null)
            {
                memoryCache[cacheKey] = clip; // 메모리에도 저장
                return clip;
            }
        }

        Debug.Log($"[VoiceClipCache] Cache miss: {cacheKey}");
        return null;
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

        // 파일 쓰기는 백그라운드 스레드로
        await UniTask.SwitchToThreadPool();

        try
        {

            using (FileStream fs = File.Create(filePath))
            using (BinaryWriter bw = new BinaryWriter(fs))
            {
                // WAV 헤더 작성
                int byteRate = sampleRate * channels * 2; // 16-bit
                int blockAlign = channels * 2;

                bw.Write(new char[4] { 'R', 'I', 'F', 'F' });
                bw.Write(36 + samples.Length * 2);
                bw.Write(new char[4] { 'W', 'A', 'V', 'E' });
                bw.Write(new char[4] { 'f', 'm', 't', ' ' });
                bw.Write(16);
                bw.Write((ushort)1); // PCM
                bw.Write((ushort)channels);
                bw.Write(sampleRate);
                bw.Write(byteRate);
                bw.Write((ushort)blockAlign);
                bw.Write((ushort)16); // Bits per sample

                bw.Write(new char[4] { 'd', 'a', 't', 'a' });
                bw.Write(samples.Length * 2);

                // 샘플 데이터 작성
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
    /// WAV 파일에서 AudioClip 로드
    /// </summary>
    private async UniTask<AudioClip> LoadWavFile(string filePath)
    {
        if (!File.Exists(filePath))
        {
            Debug.LogWarning($"[VoiceClipCache] WAV file not found: {filePath}");
            return null;
        }

        try
        {
            // 파일을 바이트 배열로 읽기 (백그라운드 스레드)
            await UniTask.SwitchToThreadPool();
            byte[] fileData = File.ReadAllBytes(filePath);
            await UniTask.SwitchToMainThread();

            // WAV 헤더 파싱 (44 bytes)
            int channels = fileData[22];
            int sampleRate = System.BitConverter.ToInt32(fileData, 24);
            int byteRate = System.BitConverter.ToInt32(fileData, 28);
            int bitsPerSample = System.BitConverter.ToInt16(fileData, 34);

            // 오디오 데이터 시작 위치 (일반적으로 44번째 바이트부터)
            int headerSize = 44;
            int dataSize = fileData.Length - headerSize;
            int sampleCount = dataSize / (bitsPerSample / 8);

            // float 배열로 변환
            float[] samples = new float[sampleCount];
            for (int i = 0; i < sampleCount; i++)
            {
                int byteIndex = headerSize + i * 2;
                short intSample = System.BitConverter.ToInt16(fileData, byteIndex);
                samples[i] = intSample / 32768f; // 16-bit to float
            }

            // AudioClip 생성 (메인 스레드에서만 가능)
            AudioClip clip = AudioClip.Create(
                Path.GetFileNameWithoutExtension(filePath),
                sampleCount / channels,
                channels,
                sampleRate,
                false
            );

            clip.SetData(samples, 0);

            Debug.Log($"[VoiceClipCache] Loaded WAV: {Path.GetFileName(filePath)} ({sampleRate}Hz, {channels}ch)");
            return clip;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[VoiceClipCache] Failed to load WAV file: {filePath}\n{ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// 캐시 클리어
    /// </summary>
    public void ClearCache()
    {
        memoryCache.Clear();
        if (Directory.Exists(cacheRoot))
        {
            Directory.Delete(cacheRoot, true);
            Directory.CreateDirectory(cacheRoot);
        }
        Debug.Log("[VoiceClipCache] Cache cleared");
    }
}
