using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Cysharp.Threading.Tasks;
using System.Threading;
using LLMUnitySamples;

/// <summary>
/// 개별 동료의 응답 UI를 제어하는 컨트롤러
/// glow 효과, 텍스트 표시, 행동 완료 시 페이드아웃 처리
/// </summary>
public class AllyAnswerUIController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image glowImage;
    [SerializeField] private Image faceIcon;
    [SerializeField] private GameObject shadowObject;
    [SerializeField] private TextMeshProUGUI answerText;

    [Header("Animation Settings")]
    [SerializeField] private float glowPulseSpeed = 2f;
    [SerializeField] private float glowMinAlpha = 0.3f;
    [SerializeField] private float glowMaxAlpha = 1f;
    [SerializeField] private float fadeOutDuration = 0.5f;

    private CancellationTokenSource glowCts;
    private CancellationTokenSource fadeOutCts;
    private bool isAnswering = false;

    private CanvasGroup shadowCanvasGroup;
    private CanvasGroup textCanvasGroup;

    private void Awake()
    {
        // CanvasGroup 컴포넌트 확인 및 추가 (페이드아웃용)
        if (shadowObject != null)
        {
            shadowCanvasGroup = shadowObject.GetComponent<CanvasGroup>();
            if (shadowCanvasGroup == null)
            {
                shadowCanvasGroup = shadowObject.AddComponent<CanvasGroup>();
            }
        }

        // 초기 상태: glow 비활성화, shadow/text 숨김
        SetGlowActive(false);
        SetAnswerVisible(false);
    }

    private void OnDestroy()
    {
        glowCts?.Cancel();
        glowCts?.Dispose();
        fadeOutCts?.Cancel();
        fadeOutCts?.Dispose();
    }

    /// <summary>
    /// 동료 얼굴 아이콘 설정
    /// </summary>
    public void SetFaceIcon(Sprite faceSprite)
    {
        if (faceIcon != null && faceSprite != null)
        {
            faceIcon.sprite = faceSprite;
        }
    }

    /// <summary>
    /// 동료가 응답을 시작할 때 호출
    /// glow 활성화, 텍스트 표시, 행동 상태 태그 추가
    /// </summary>
    /// <param name="message">응답 메시지</param>
    /// <param name="actionType">행동 타입 (Move, Combat, Support)</param>
    public void ShowAnswer(string message, AIActionEnum actionType)
    {
        Debug.Log($"[AllyAnswerUIController] ShowAnswer 호출 - glow:{glowImage != null}, shadow:{shadowObject != null}, text:{answerText != null}");

        // 기존 페이드아웃 취소
        fadeOutCts?.Cancel();
        fadeOutCts?.Dispose();
        fadeOutCts = new CancellationTokenSource();

        isAnswering = true;

        // 행동 상태 태그 추가
        string statusTag = GetStatusTag(actionType);
        string fullMessage = $"{message} - <b>{statusTag}</b>";

        // UI 표시
        SetAnswerVisible(true);
        if (answerText != null)
        {
            answerText.text = fullMessage;
            Debug.Log($"[AllyAnswerUIController] 텍스트 설정 완료: {fullMessage}");
        }

        // Glow 펄스 애니메이션 시작
        StartGlowPulse();
        Debug.Log($"[AllyAnswerUIController] ShowAnswer 완료 - shadowObject.activeSelf: {shadowObject?.activeSelf}");
    }

    /// <summary>
    /// 행동이 완료되었을 때 호출
    /// glow 비활성화, shadow/text 페이드아웃
    /// </summary>
    public void HideAnswer()
    {
        if (!isAnswering) return;

        isAnswering = false;

        // Glow 펄스 중지
        StopGlowPulse();
        SetGlowActive(false);

        // Shadow와 Text 페이드아웃
        FadeOutAnswerAsync(fadeOutCts.Token).Forget();
    }

    /// <summary>
    /// 행동 타입에 따른 상태 태그 반환
    /// </summary>
    private string GetStatusTag(AIActionEnum actionType)
    {
        return actionType switch
        {
            AIActionEnum.Move => "[Moving]",
            AIActionEnum.Combat => "[Combat]",
            AIActionEnum.Support => "[Support]",
            _ => "[Idle]"
        };
    }

    private void SetGlowActive(bool active)
    {
        if (glowImage != null)
        {
            glowImage.gameObject.SetActive(active);
        }
    }

    private void SetAnswerVisible(bool visible)
    {
        if (shadowObject != null)
        {
            shadowObject.SetActive(visible);
            if (shadowCanvasGroup != null)
            {
                shadowCanvasGroup.alpha = visible ? 1f : 0f;
            }
        }
    }

    private void StartGlowPulse()
    {
        glowCts?.Cancel();
        glowCts?.Dispose();
        glowCts = new CancellationTokenSource();

        SetGlowActive(true);
        GlowPulseAsync(glowCts.Token).Forget();
    }

    private void StopGlowPulse()
    {
        glowCts?.Cancel();
    }

    /// <summary>
    /// Glow 펄스 애니메이션 (UniTask)
    /// </summary>
    private async UniTaskVoid GlowPulseAsync(CancellationToken ct)
    {
        if (glowImage == null) return;

        float time = 0f;
        Color originalColor = glowImage.color;

        while (!ct.IsCancellationRequested)
        {
            time += Time.deltaTime * glowPulseSpeed;
            float alpha = Mathf.Lerp(glowMinAlpha, glowMaxAlpha, (Mathf.Sin(time) + 1f) / 2f);

            Color newColor = originalColor;
            newColor.a = alpha;
            glowImage.color = newColor;

            await UniTask.Yield(ct);
        }
    }

    /// <summary>
    /// Shadow와 Text 페이드아웃 애니메이션
    /// </summary>
    private async UniTaskVoid FadeOutAnswerAsync(CancellationToken ct)
    {
        if (shadowCanvasGroup == null) return;

        float elapsed = 0f;
        float startAlpha = shadowCanvasGroup.alpha;

        while (elapsed < fadeOutDuration && !ct.IsCancellationRequested)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeOutDuration;
            shadowCanvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, t);

            await UniTask.Yield(ct);
        }

        if (!ct.IsCancellationRequested)
        {
            SetAnswerVisible(false);
        }
    }
}
