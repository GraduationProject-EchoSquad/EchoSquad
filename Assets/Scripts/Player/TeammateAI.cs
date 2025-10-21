using System;
using LLMUnitySamples;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class TeammateAI : MonoBehaviour
{
    public string teammateName = "Lena";
    public string teammateNameKorean = "레나";
    [SerializeField] private TeammateController unitController;

    public void ExecuteCommand(AIActionEnum action, Parameters param)
    {
        Debug.Log($"[{teammateName}] 명령 수신: {action} - {param}");

        // 예시로 동작 분기 (대소문자 구분 없이)
        if (action == AIActionEnum.Move)
            Move(param);
        else if (action == AIActionEnum.Combat)
            Combat(param);
        else if (action == AIActionEnum.Support)
            Support(param);

        // Voice를 자연스럽게 조합
        string naturalVoice = BuildNaturalVoice(action, param);
        DoVoice(naturalVoice);
    }

    void Move(Parameters param)
    {
        //TODO 처리 필요
        UnitManager unitManager = UnitManager.Instance;
        MapManager mapManager = MapManager.Instance;

        string message = "";

        //follow target
        if (!string.IsNullOrEmpty(param.follow_target) && param.follow_target != "null")
        {
            if (string.Equals(param.follow_target,"Player",  StringComparison.OrdinalIgnoreCase))
            {
                unitController.SetFollowUnit(unitManager.GetPlayerUnit());
            }
            else if (unitManager.teammateUnitDict.TryGetValue(param.follow_target, out var followUnit))
            {
                unitController.SetFollowUnit(followUnit);

                Debug.Log($"Move To AI Unit {param.follow_target}");

                message = $"[{teammateName}] : {param.follow_target}";
            }
        }
        else
        {
            //Left | Right | Center | Back | Forward
            //플레이어 기준 방향 이동
            if (string.Equals(param.destination, "Left", StringComparison.OrdinalIgnoreCase))
            {
                unitController.MoveDirection(unitManager.GetPlayerUnit().transform.position,
                    -unitManager.GetPlayerUnit().transform.right);
            }
            else if (string.Equals(param.destination, "Right", StringComparison.OrdinalIgnoreCase))
            {
                unitController.MoveDirection(unitManager.GetPlayerUnit().transform.position,
                    unitManager.GetPlayerUnit().transform.right);
            }
            else if (string.Equals(param.destination, "Back", StringComparison.OrdinalIgnoreCase))
            {
                unitController.MoveDirection(unitManager.GetPlayerUnit().transform.position,
                    -unitManager.GetPlayerUnit().transform.forward);
            }
            else if (string.Equals(param.destination, "Forward", StringComparison.OrdinalIgnoreCase))
            {
                unitController.MoveDirection(unitManager.GetPlayerUnit().transform.position,
                    unitManager.GetPlayerUnit().transform.forward);
            }
            else if (unitManager.teammateUnitDict.TryGetValue(param.destination, out var followUnit))
            {
                unitController.MoveToUnit(followUnit);

                Debug.Log($"Move To AI Unit {param.destination}");
            }
            else if (mapManager.districtDict.TryGetValue(param.destination, out var district))
            {
                unitController.MoveToObject(district);
                Debug.Log($"Move To District {param.destination}");
            }

            message = $"[{teammateName}] : {param.destination}";
        }

        Debug.Log(message);
    }


    void Combat(Parameters param)
    {
        string message = $"{param.engage_enemy} 공격!";
        Debug.Log($"[{teammateName}] {message}");
        UnitManager unitManager = UnitManager.Instance;
        // TODO: 공격 대상 지정, 애니메이션 트리거 등
        if (!string.IsNullOrEmpty(param.engage_enemy) && param.engage_enemy != "null")
        {
            if (string.Equals(param.engage_enemy,"Boss", StringComparison.OrdinalIgnoreCase))
            {
                BossController bossController = unitManager.GetBossController();
                if (bossController == null)
                {
                    Debug.LogWarning("No Boss!");
                    return;
                }
                unitController.GetUnitShooter().SetAimTargetUnit(bossController);
            }
        }
    }

    void Support(Parameters param)
    {
        string message = $"힐 중이야. 엄호해줘!";
        Debug.Log($"[{teammateName}] {message}");
    }

    void Scout(Parameters param)
    {
        string message = $"{param.destination} 정찰 중...";
        Debug.Log($"[{teammateName}] {message}");
        // TODO: 탐색 루트로 이동
    }

    /// <summary>
    /// LLM의 voice 모듈과 parameters를 조합해서 자연스러운 음성 생성
    /// </summary>
    string BuildNaturalVoice(AIActionEnum action, Parameters param)
    {
        if (string.IsNullOrEmpty(param.voice))
        {
            return "Okay"; // 기본값
        }

        string voice = param.voice;

        // Action별로 자연스러운 문장 추가
        switch (action)
        {
            case AIActionEnum.Move:
                // follow_target이 있으면 "following [target]" 추가
                if (!string.IsNullOrEmpty(param.follow_target) && param.follow_target != "null")
                {
                    string target = param.follow_target.Equals("Player", StringComparison.OrdinalIgnoreCase)
                        ? "you"
                        : param.follow_target;

                    // "Roger+following" → "Roger+following+you"
                    if (!voice.Contains(target, StringComparison.OrdinalIgnoreCase))
                    {
                        voice += $"+{target}";
                    }
                }
                // destination이 있으면 "to [destination]" 추가
                else if (!string.IsNullOrEmpty(param.destination) && param.destination != "null")
                {
                    string dest = param.destination;

                    // 방향인 경우 (Left, Right, Forward, Back) - 이미 voice에 포함되어 있을 수 있음
                    if (dest.Equals("Left", StringComparison.OrdinalIgnoreCase) ||
                        dest.Equals("Right", StringComparison.OrdinalIgnoreCase) ||
                        dest.Equals("Forward", StringComparison.OrdinalIgnoreCase) ||
                        dest.Equals("Back", StringComparison.OrdinalIgnoreCase))
                    {
                        // 방향은 이미 voice에 있으면 추가 안 함
                        if (!voice.Contains(dest, StringComparison.OrdinalIgnoreCase))
                        {
                            voice += $"+{dest.ToLower()}";
                        }
                    }
                    else
                    {
                        // District나 다른 장소면 "to [place]" 추가
                        voice += $"+to+{dest}";
                    }
                }
                break;

            case AIActionEnum.Combat:
                // engage_enemy가 있으면 enemy 이름 추가
                if (!string.IsNullOrEmpty(param.engage_enemy) && param.engage_enemy != "null")
                {
                    string enemy = param.engage_enemy.ToLower();

                    // "Let's do this!+engaging" → "Let's do this!+engaging+zombie"
                    if (!voice.Contains(enemy, StringComparison.OrdinalIgnoreCase))
                    {
                        voice += $"+{enemy}";
                    }
                }
                break;

            case AIActionEnum.Support:
                // support_target이 있으면 대상 추가
                if (!string.IsNullOrEmpty(param.support_target) && param.support_target != "null")
                {
                    string target = param.support_target.Equals("Player", StringComparison.OrdinalIgnoreCase)
                        ? "you"
                        : param.support_target;

                    if (!voice.Contains(target, StringComparison.OrdinalIgnoreCase))
                    {
                        voice += $"+{target}";
                    }
                }
                break;
        }

        return voice;
    }

    async void DoVoice(string voiceText)
    {
        // voice 필드가 '+' 구분자로 모듈이 조합되어 있는지 확인
        if (string.IsNullOrEmpty(voiceText))
        {
            Debug.LogWarning($"[{teammateName}] Voice text is null or empty");
            return;
        }

        // TeammateController의 DoVoice() 사용 (VoiceProfile이 설정되어 있음)
        unitController.DoVoice(voiceText);
    }
}