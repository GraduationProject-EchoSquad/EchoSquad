using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// AllyStatChoiceUI 프리팹에 부착되는 UI 컨트롤러
// TeammateVoiceSetupManager가 이 컨트롤러를 통해 UI 제어
public class AllyStatChoiceUIController : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI progressText;
    public Button confirmButton;

    [Header("Teammate Panels")]
    public List<TeammateVoiceItem> teammateItems = new List<TeammateVoiceItem>();

    // 확인 버튼 클릭 이벤트
    public System.Action OnConfirmClicked;

    private void Awake()
    {
        // 확인 버튼 이벤트 연결
        if (confirmButton != null)
        {
            confirmButton.onClick.AddListener(() => OnConfirmClicked?.Invoke());
        }

        // 각 슬라이더 초기화
        foreach (var item in teammateItems)
        {
            item.Initialize();
        }
    }

    public void SetProgressText(string text)
    {
        if (progressText != null)
        {
            progressText.text = text;
        }
    }

    public void SetConfirmButtonInteractable(bool interactable)
    {
        if (confirmButton != null)
        {
            confirmButton.interactable = interactable;
        }
    }

    /// <summary>
    /// 모든 Preview 버튼 활성화/비활성화
    /// </summary>
    public void SetPreviewButtonsInteractable(bool interactable)
    {
        foreach (var item in teammateItems)
        {
            if (item.previewButton != null)
            {
                item.previewButton.interactable = interactable;
            }
        }
    }

    // 모든 동료의 설정값 가져오기
    public Dictionary<string, (VoiceGender gender, int liveliness, int mood)> GetAllSettings()
    {
        var settings = new Dictionary<string, (VoiceGender, int, int)>();

        foreach (var item in teammateItems)
        {
            if (!string.IsNullOrEmpty(item.teammateName))
            {
                settings[item.teammateName] = (
                    item.GetSelectedGender(),
                    item.GetLivelinessValue(),
                    item.GetMoodValue()
                );
            }
        }

        return settings;
    }
}
