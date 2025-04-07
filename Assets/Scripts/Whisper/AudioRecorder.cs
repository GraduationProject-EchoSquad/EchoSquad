using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Build;
using UnityEngine;

public class AudioRecorder : MonoBehaviour
{
    public AudioClip recordedClip;  // 저장된 오디오 클립
    public int recordingLength = 3; // 녹음 시간 (초)
    public int sampleRate = 44100;   // 샘플레이트 (Hz)
    
    private string microphoneDevice;
    public RunWhisper whisper;

    private void Start()
    {
        // 마이크 장치가 있는지 확인
        if (Microphone.devices.Length > 0)
        {
            microphoneDevice = Microphone.devices[0];
            //StartRecording();
            Debug.Log(microphoneDevice);
        }
        else
        {
            Debug.LogError("마이크 장치를 찾을 수 없습니다.");
        }
    }

    public void StartRecording()
    {
        if (Microphone.IsRecording(microphoneDevice))
        {
            Debug.LogWarning("이미 녹음 중입니다.");
            return;
        }

        recordedClip = Microphone.Start(microphoneDevice, false, recordingLength, sampleRate);
    }
    
    public async void StopRecording()
    {
        if (!Microphone.IsRecording(microphoneDevice))
        {
            Debug.LogWarning("녹음 중이 아닙니다.");
            return;
        }

        Microphone.End(null);

        whisper.audioClip = recordedClip;
        whisper.DoTest();
        Debug.Log("녹음 종료됨");
    }
    
    // recordedClip을 재생
    public void PlayRecordedClip()
    {
        if (recordedClip == null)
        {
            Debug.Log("녹음 없음");
        }
        
        if (recordedClip != null)
        {
            AudioSource audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
                audioSource = gameObject.AddComponent<AudioSource>();

            audioSource.clip = recordedClip;
            audioSource.Play();
        }
    }
}
