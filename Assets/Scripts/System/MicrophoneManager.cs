using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Whisper.Utils;

public class MicrophoneManager : Singleton<MicrophoneManager>
{
    [SerializeField] public MicrophoneRecord microphoneRecord;
    private const string MicPlayerPrefsKey = "SelectedMicrophoneDevice";

    private void Start()
    {
        // 3. PlayerPrefs에서 이전에 저장된 마이크 이름을 불러옵니다.
        string savedMic = PlayerPrefs.GetString(MicPlayerPrefsKey, null);

        // 4. 저장된 마이크가 있고, 현재도 사용 가능한 장치 목록에 포함되어 있는지 확인합니다.
        if (!string.IsNullOrEmpty(savedMic) && microphoneRecord.AvailableMicDevices.Contains(savedMic))
        {
            // 유효하다면, 현재 마이크로 설정합니다.
            SetMicrophoneRecordIndex(savedMic);
            Debug.Log($"저장된 마이크 '{savedMic}'을(를) 불러왔습니다.");
        }
    }

    public void SetMicrophoneRecordIndex(int idx)
    {
        if (microphoneRecord.microphoneDropdown == null) return;
        var opt = microphoneRecord.microphoneDropdown.options[idx];
       SetMicrophoneRecordIndex(opt.text == microphoneRecord.microphoneDefaultLabel ? null : opt.text);
    }
    
    public void SetMicrophoneRecordIndex(string micName)
    {
        microphoneRecord.SelectedMicDevice = micName;
        PlayerPrefs.SetString(MicPlayerPrefsKey, micName);
    }


    public void SetDropDown(Dropdown micDropdown)
    {
        microphoneRecord.microphoneDropdown = micDropdown;
        if(microphoneRecord.microphoneDropdown != null)
        {
            microphoneRecord.microphoneDropdown.options = microphoneRecord.AvailableMicDevices
                .Prepend(microphoneRecord.microphoneDefaultLabel)
                .Select(text => new Dropdown.OptionData(text))
                .ToList();
            microphoneRecord. microphoneDropdown.value = microphoneRecord.microphoneDropdown.options
                .FindIndex(op => op.text == microphoneRecord.microphoneDefaultLabel);
            //microphoneRecord.microphoneDropdown.onValueChanged.AddListener(microphoneRecord.OnMicrophoneChanged);
        }
    }
}
