using Cysharp.Threading.Tasks;
using EasyTextEffects;
using TMPro;
using UnityEngine;

public class CountdownUI : UIBase
{
    [Header("Countdown Text"), SerializeField]
    private TextMeshProUGUI countdownText; // drag in inspector

    [SerializeField] private TextEffect countdownEffect; // EasyTextEffects component

    [Header("Countdown Start"), SerializeField]
    public int countdownStart = 5;

    [SerializeField] public int countdownInterval = 1;

    public async UniTask PlayCountdownText(int currentWaveIndex)
    {
        if (countdownText == null)
        {
            return;
        }

        // (1) 5,4,3,2,1 카운트
        for (int i = countdownStart; i > 0; i--)
        {
            // 예: "<link=wave+fadein+movein>5</link>"
            countdownText.text = $"<link=wave+fadein+movein>{i}</link>";
            // 태그 해석 갱신
            if (countdownEffect != null)
            {
                countdownEffect.UpdateStyleInfos();
                countdownEffect.Refresh(); // 필요한 경우 Refresh() 호출
            }

            await UniTask.WaitForSeconds(countdownInterval);
        }

        // (2) "Wave {번호}" 표시 (currentWaveIndex가 0부터 시작하므로 +1)
        int waveNumber = currentWaveIndex;
        countdownText.text = $"<link=wave+fadein+movein>Wave {waveNumber}</link>";
        if (countdownEffect != null)
        {
            countdownEffect.UpdateStyleInfos();
            countdownEffect.Refresh();
        }

        await UniTask.WaitForSeconds(countdownInterval);

        // (3) "Fight!" 표시
        countdownText.text = $"<b><size=250px><link=gradient+wave+rotate+scale>Start!</link></size></b>";
        if (countdownEffect != null)
        {
            countdownEffect.UpdateStyleInfos();
            countdownEffect.Refresh();
        }
        
        await UniTask.WaitForSeconds(countdownInterval);

        // (4) 텍스트 숨김
        countdownText.text = string.Empty;
        if (countdownText.gameObject.activeSelf)
            countdownText.gameObject.SetActive(false);
    }
}