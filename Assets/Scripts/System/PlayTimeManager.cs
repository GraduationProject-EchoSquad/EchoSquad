using UnityEngine;

public class PlayTimeManager : Singleton<PlayTimeManager>
{
    private float elapsedTime;      // 게임 시작 후 누적된 시간 (초 단위)
    private bool isTimerRunning;    // 타이머가 현재 작동 중인지 여부

    void Start()
    {
        // 컴포넌트가 활성화되면 타이머를 시작합니다.
        StartTimer();
    }

    void Update()
    {
        // 타이머가 실행 중일 때만 시간을 업데이트합니다.
        if (isTimerRunning)
        {
            // 이전 프레임부터 현재 프레임까지의 시간을 더합니다.
            elapsedTime += Time.deltaTime;
        }
    }

    /// <summary>
    /// 누적된 시간을 '분:초' (MM:SS) 형식으로 변환하여 UI에 표시합니다.
    /// </summary>
    public string GetPlayTimeDisplay()
    {
        // 총 초를 분과 초로 변환합니다.
        // 예: 75초 -> 1분 15초
        int minutes = (int)(elapsedTime / 60);
        int seconds = (int)(elapsedTime % 60);

        // string.Format 또는 C# 6.0 이상의 문자열 보간($)을 사용하여 MM:SS 형식으로 만듭니다.
        // "{minutes:D2}"는 숫자를 항상 두 자리로 표시해줍니다 (예: 7 -> "07").
        return $"{minutes:D2}:{seconds:D2}";
    }

    #region 타이머 제어 메서드 (외부에서 호출 가능)

    /// <summary>
    /// 타이머를 시작하고 시간을 초기화합니다.
    /// </summary>
    public void StartTimer()
    {
        elapsedTime = 0f;
        isTimerRunning = true;
        //UpdatePlayTimeDisplay(); // 타이머를 00:00으로 즉시 표시
    }

    /// <summary>
    /// 타이머를 일시 정지합니다.
    /// </summary>
    public void StopTimer()
    {
        isTimerRunning = false;
    }

    /// <summary>
    /// 타이머를 다시 시작합니다.
    /// </summary>
    public void ResumeTimer()
    {
        isTimerRunning = true;
    }

    #endregion
}
