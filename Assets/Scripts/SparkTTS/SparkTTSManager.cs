using System;
using UnityEngine;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using SparkTTS;
using System.IO;
using SparkTTS.Models;

// 음성 특성을 나타내는 키 구조체
public struct VoiceKey : IEquatable<VoiceKey>
{
    public readonly string Gender;
    public readonly string Pitch;
    public readonly string Speed;

    public VoiceKey(string gender, string pitch, string speed)
    {
        Gender = gender;
        Pitch = pitch;
        Speed = speed;
    }

    public bool Equals(VoiceKey other) => Gender == other.Gender && Pitch == other.Pitch && Speed == other.Speed;
    public override bool Equals(object obj) => obj is VoiceKey other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(Gender, Pitch, Speed);
}

/// <summary>
/// TTS 요청의 기본 클래스
/// </summary>
public abstract class VoiceRequest
{
    public string Text { get; }
    protected VoiceRequest(string text) { Text = text; }
}

/// <summary>
/// 스타일 기반 TTS 요청
/// </summary>
public class StyleVoiceRequest : VoiceRequest
{
    public VoiceKey Key { get; }
    public StyleVoiceRequest(string text, VoiceKey key) : base(text) { Key = key; }
}

/// <summary>
/// 오디오 파일 클립 기반 TTS 요청
/// </summary>
public class FileVoiceRequest : VoiceRequest
{
    public AudioClip Clip { get; }
    public FileVoiceRequest(string text, AudioClip clip) : base(text) { Clip = clip; }
}

public class SparkTTSManager : Singleton<SparkTTSManager>
{
    [Header("References")] public AudioSource audioSource;

    // Character voice components
    private CharacterVoiceFactory _voiceFactory;

    // 현재 활성화된 목소리
    private CharacterVoice _currentVoice;
    private readonly Dictionary<VoiceKey, CharacterVoice> _styleVoiceCache = new Dictionary<VoiceKey, CharacterVoice>();
    private readonly Dictionary<string, CharacterVoice> _fileVoiceCache = new Dictionary<string, CharacterVoice>();

    // 동시 요청 처리를 위한 큐와 락 객체
    private readonly Queue<VoiceRequest> _requestQueue = new Queue<VoiceRequest>();
    private bool _isGenerating = false;

    void Start()
    {
        // Initialize factory
        _voiceFactory = new CharacterVoiceFactory(ExecutionProvider.CUDA);

        Debug.Log("CharacterVoiceDemo initialized. Ready to create voices and generate speech.");
    }

    /// <summary>
    /// Creates a style-based voice with the specified gender and current dropdown settings.
    /// </summary>
    public void CreateStyleVoice(string text, string gender = "male", string pitch = "moderate",
        string speed = "moderate")
    {
        var request = new StyleVoiceRequest(text, new VoiceKey(gender, pitch, speed));
        _requestQueue.Enqueue(request);

        // 이미 처리 중인 작업이 없다면 새로운 처리 시작
        if (!_isGenerating)
        {
            ProcessQueueAsync().Forget();
        }
    }

    public void CreateVoiceFromClip(string text, AudioClip clip)
    {
        if (clip == null) return;

        var request = new FileVoiceRequest(text, clip);
        _requestQueue.Enqueue(request);

        // 이미 처리 중인 작업이 없다면 새로운 처리 시작
        if (!_isGenerating)
        {
            ProcessQueueAsync().Forget();
        }
    }

    private async UniTask ProcessQueueAsync()
    {
        if (_isGenerating)
        {
            return;
        }

        _isGenerating = true;

        while (_requestQueue.Count > 0)
        {
            var request = _requestQueue.Dequeue();
            string text = request.Text;
            _currentVoice = null; // 다음 요청 처리를 위해 초기화

            switch (request)
            {
                case StyleVoiceRequest styleRequest:
                {
                    VoiceKey voiceKey = styleRequest.Key;
                    Debug.Log($"Processing Style Request: text='{text}', Gender={voiceKey.Gender}");

                    if (!_styleVoiceCache.TryGetValue(voiceKey, out _currentVoice))
                    {
                        string folderName = $"{voiceKey.Gender}_{voiceKey.Pitch}_{voiceKey.Speed}";
                        string folderPath = Path.Combine(Application.persistentDataPath, folderName);

                        if (Directory.Exists(folderPath))
                        {
                            Debug.Log($"Loading style voice from folder: {folderPath}");
                            _currentVoice = await _voiceFactory.CreateFromFolderAsync(folderPath);
                        }
                        else
                        {
                            Debug.Log("Style voice not in cache or file. Creating...");
                            _currentVoice = await _voiceFactory.CreateFromStyleAsync(voiceKey.Gender, voiceKey.Pitch, voiceKey.Speed, text);
                            if (_currentVoice != null)
                            {
                                Directory.CreateDirectory(folderPath); // 폴더가 없으면 생성
                                Debug.Log($"Saving style voice to folder: {folderPath}");
                                await _currentVoice.SaveVoiceAsync(folderPath);
                            }
                        }

                        if (_currentVoice != null) _styleVoiceCache[voiceKey] = _currentVoice;
                    }
                    else
                    {
                        Debug.Log("Style voice found in memory cache.");
                    }
                    break;
                }
                case FileVoiceRequest fileRequest:
                {
                    AudioClip clip = fileRequest.Clip;
                    Debug.Log($"Processing File Request: text='{text}', Clip='{clip.name}'");

                    if (!_fileVoiceCache.TryGetValue(clip.name, out _currentVoice))
                    {
                        string folderName = clip.name;
                        string folderPath = Path.Combine(Application.persistentDataPath, folderName);

                        if (Directory.Exists(folderPath))
                        {
                            Debug.Log($"Loading file voice from folder: {folderPath}");
                            _currentVoice = await _voiceFactory.CreateFromFolderAsync(folderPath);
                        }
                        else
                        {
                            Debug.Log("File voice not in cache or file. Creating from AudioClip...");
                            _currentVoice = _voiceFactory.CreateFromReference(clip);
                            if (_currentVoice != null)
                            {
                                Directory.CreateDirectory(folderPath); // 폴더가 없으면 생성
                                Debug.Log($"Saving file voice to folder: {folderPath}");
                                await _currentVoice.SaveVoiceAsync(folderPath);
                            }
                        }
                        
                        if (_currentVoice != null) _fileVoiceCache[clip.name] = _currentVoice;
                    }
                    else
                    {
                        Debug.Log("File voice found in memory cache.");
                    }
                    break;
                }
            }

            if (_currentVoice != null)
            {
                await GenerateAndPlaySpeechAsync(text);
            }
            else
            {
                Debug.LogError("Failed to create or retrieve voice.");
            }
        }

        _isGenerating = false;
    }

    /// <summary>
    /// Plays the last generated speech.
    /// </summary>
    private async UniTask GenerateAndPlaySpeechAsync(string text)
    {
        if (_currentVoice == null)
        {
            Debug.LogError("No character voice available.");
            return;
        }

        AudioClip generatedClip = await _currentVoice.GenerateSpeechAsync(text);
        if (generatedClip != null)
        {
            Debug.Log("Speech generated successfully. Playing audio...");
            PlayAudioClip(generatedClip);
        }
        else
        {
            Debug.LogError("Failed to generate speech.");
        }
    }

    /// <summary>
    /// Plays an audio clip using the audio source.
    /// </summary>
    private void PlayAudioClip(AudioClip clip)
    {
        if (audioSource == null || clip == null)
        {
            Debug.LogError("AudioSource or AudioClip is null.");
            return;
        }

        audioSource.Stop();
        audioSource.clip = clip;
        audioSource.Play();
    }

    void OnDestroy()
    {
        // Clean up resources
        // _currentVoice는 캐시된 인스턴스 중 하나이므로 별도 Dispose 불필요
        foreach (var voice in _styleVoiceCache.Values)
        {
            voice.Dispose();
        }
        _styleVoiceCache.Clear();

        foreach (var voice in _fileVoiceCache.Values)
        {
            voice.Dispose();
        }
        _fileVoiceCache.Clear();

        _voiceFactory?.Dispose();
    }
}

/// <summary>
/// Helper class for executing actions on the main Unity thread.
/// This is needed because async operations complete on background threads.
/// </summary>
public class UnityMainThreadDispatcher : MonoBehaviour
{
    private static UnityMainThreadDispatcher _instance;
    private readonly Queue<System.Action> _executionQueue = new Queue<System.Action>();
    private readonly object _lock = new object();

    public static UnityMainThreadDispatcher Instance()
    {
        if (_instance == null)
        {
            GameObject go = new GameObject("UnityMainThreadDispatcher");
            _instance = go.AddComponent<UnityMainThreadDispatcher>();
            DontDestroyOnLoad(go);
        }

        return _instance;
    }

    public void Enqueue(System.Action action)
    {
        lock (_lock)
        {
            _executionQueue.Enqueue(action);
        }
    }

    void Update()
    {
        lock (_lock)
        {
            while (_executionQueue.Count > 0)
            {
                _executionQueue.Dequeue().Invoke();
            }
        }
    }
}