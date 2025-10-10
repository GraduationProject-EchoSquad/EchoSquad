using System;
using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using SparkTTS;
using SparkTTS.Models;
using TMPro;

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

public class SparkTTSManager : Singleton<SparkTTSManager>
{
    [Header("References")] public AudioSource audioSource;
    public AudioClip referenceAudioClip;

    // Character voice components
    private CharacterVoiceFactory _voiceFactory;

    //private CharacterVoice _currentVoice;
    // 현재 활성화된 목소리
    private CharacterVoice _currentVoice;
    private Dictionary<VoiceKey, CharacterVoice> _voiceCache = new Dictionary<VoiceKey, CharacterVoice>();

    // 동시 요청 처리를 위한 큐와 락 객체
    private readonly Queue<Tuple<string, VoiceKey>> _requestQueue = new Queue<Tuple<string, VoiceKey>>();
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
        var voiceKey = new VoiceKey(gender, pitch, speed);
        _requestQueue.Enqueue(new Tuple<string, VoiceKey>(text, voiceKey));

        // 이미 처리 중인 작업이 없다면 새로운 처리 시작
        if (!_isGenerating)
        {
            ProcessQueueAsync().Forget();
        }
    }

    private async UniTaskVoid ProcessQueueAsync()
    {
        if (_isGenerating)
        {
            return;
        }

        _isGenerating = true;

        while (_requestQueue.Count > 0)
        {
            var request = _requestQueue.Dequeue();
            string text = request.Item1;
            VoiceKey voiceKey = request.Item2;

            if (_isGenerating)
            {
                Debug.Log(
                    $"Processing voice for key: text='{text}', Gender={voiceKey.Gender}, Pitch={voiceKey.Pitch}, Speed={voiceKey.Speed}");

                // 캐시에서 목소리를 찾아보고, 없으면 새로 생성합니다.
                if (!_voiceCache.TryGetValue(voiceKey, out _currentVoice))
                {
                    Debug.Log("Voice not in cache. Creating a new one...");
                    _currentVoice = await _voiceFactory.CreateFromStyleAsync(
                        voiceKey.Gender,
                        voiceKey.Pitch,
                        voiceKey.Speed,
                        text); // 첫 생성 시 텍스트로 미리 생성

                    if (_currentVoice != null)
                    {
                        _voiceCache[voiceKey] = _currentVoice; // 새 목소리를 캐시에 추가
                        Debug.Log("Voice created and cached successfully.");
                    }
                    else
                    {
                        Debug.LogError("Failed to create voice.");
                        continue; // 다음 요청 처리
                    }
                }
                else
                {
                    Debug.Log("Voice found in cache.");
                }

                // 음성 생성 및 재생
                await GenerateAndPlaySpeechAsync(text);
            }

            _isGenerating = false;
        }
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
        _currentVoice?.Dispose();
        foreach (var voice in _voiceCache.Values)
        {
            voice.Dispose();
        }

        _voiceCache.Clear();
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