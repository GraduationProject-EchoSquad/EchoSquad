using UnityEngine;
using System.Collections.Generic;
using LLMUnitySamples;

/// <summary>
/// 3명의 동료 응답 UI를 관리하는 매니저
/// 각 동료별로 AllyAnswerUIController를 매핑하여 개별 응답 표시
/// HUD가 동적으로 로드되므로 싱글톤 대신 static Instance 패턴 사용
/// </summary>
public class AllyAnswerUIManager : MonoBehaviour
{
    public static AllyAnswerUIManager Instance { get; private set; }

    [Header("Ally Answer UI Controllers")]
    [Tooltip("Lena, James, Sara 순서로 할당")]
    [SerializeField] private AllyAnswerUIController lenaAnswerUI;
    [SerializeField] private AllyAnswerUIController jamesAnswerUI;
    [SerializeField] private AllyAnswerUIController saraAnswerUI;

    [Header("Face Icons (Optional)")]
    [SerializeField] private Sprite lenaFaceSprite;
    [SerializeField] private Sprite jamesFaceSprite;
    [SerializeField] private Sprite saraFaceSprite;

    private Dictionary<string, AllyAnswerUIController> allyUIDict;

    private void Awake()
    {
        Instance = this;
        InitializeAllyUIDict();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void InitializeAllyUIDict()
    {
        allyUIDict = new Dictionary<string, AllyAnswerUIController>
        {
            { "Lena", lenaAnswerUI },
            { "James", jamesAnswerUI },
            { "Sara", saraAnswerUI }
        };

        // Face 아이콘 설정
        if (lenaAnswerUI != null && lenaFaceSprite != null)
            lenaAnswerUI.SetFaceIcon(lenaFaceSprite);

        if (jamesAnswerUI != null && jamesFaceSprite != null)
            jamesAnswerUI.SetFaceIcon(jamesFaceSprite);

        if (saraAnswerUI != null && saraFaceSprite != null)
            saraAnswerUI.SetFaceIcon(saraFaceSprite);
    }

    /// <summary>
    /// 특정 동료의 응답 UI 표시
    /// </summary>
    /// <param name="teammateName">동료 이름 (Lena, James, Sara)</param>
    /// <param name="message">응답 메시지</param>
    /// <param name="actionType">행동 타입</param>
    public void ShowAllyAnswer(string teammateName, string message, AIActionEnum actionType)
    {
        if (allyUIDict == null)
        {
            Debug.LogWarning("[AllyAnswerUIManager] allyUIDict is not initialized");
            return;
        }

        if (allyUIDict.TryGetValue(teammateName, out var controller))
        {
            if (controller != null)
            {
                Debug.Log($"[AllyAnswerUIManager] {teammateName} ShowAnswer 호출 전 - controller: {controller.gameObject.name}");
                controller.ShowAnswer(message, actionType);
                Debug.Log($"[AllyAnswerUIManager] {teammateName} 응답 표시: {message}");
            }
            else
            {
                Debug.LogWarning($"[AllyAnswerUIManager] {teammateName}의 UI Controller가 null입니다.");
            }
        }
        else
        {
            Debug.LogWarning($"[AllyAnswerUIManager] 알 수 없는 동료: {teammateName}");
        }
    }

    /// <summary>
    /// 특정 동료의 응답 UI 숨김 (행동 완료 시)
    /// </summary>
    /// <param name="teammateName">동료 이름</param>
    public void HideAllyAnswer(string teammateName)
    {
        if (allyUIDict == null) return;

        if (allyUIDict.TryGetValue(teammateName, out var controller))
        {
            controller?.HideAnswer();
            Debug.Log($"[AllyAnswerUIManager] {teammateName} 응답 숨김");
        }
    }

    /// <summary>
    /// 모든 동료의 응답 UI 숨김
    /// </summary>
    public void HideAllAnswers()
    {
        if (allyUIDict == null) return;

        foreach (var kvp in allyUIDict)
        {
            kvp.Value?.HideAnswer();
        }
    }

    /// <summary>
    /// 특정 동료의 UI Controller 반환
    /// </summary>
    public AllyAnswerUIController GetAllyUI(string teammateName)
    {
        if (allyUIDict != null && allyUIDict.TryGetValue(teammateName, out var controller))
        {
            return controller;
        }
        return null;
    }
}
