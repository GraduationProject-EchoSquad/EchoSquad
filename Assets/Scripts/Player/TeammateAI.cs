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
        else if (action == AIActionEnum.Scout)
            Scout(param);

        DoVoice(param.voice);
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

    async void DoVoice(string voiceText)
    {
        // voice 필드가 '+' 구분자로 모듈이 조합되어 있는지 확인
        if (string.IsNullOrEmpty(voiceText))
        {
            Debug.LogWarning($"[{teammateName}] Voice text is null or empty");
            return;
        }

        // '+' 기호로 분리 (예: "Roger+moving+left")
        if (voiceText.Contains("+"))
        {
            string[] modules = voiceText.Split('+');

            // 공백 제거
            for (int i = 0; i < modules.Length; i++)
            {
                modules[i] = modules[i].Trim();
            }

            // VoiceSpeaker의 모듈 조합 재생 사용
            VoiceSpeaker speaker = unitController.GetComponent<VoiceSpeaker>();
            if (speaker != null)
            {
                await speaker.SpeakModulesAsync(modules);
                Debug.Log($"[{teammateName}] Voice modules played: {string.Join(" + ", modules)}");
            }
            else
            {
                Debug.LogWarning($"[{teammateName}] VoiceSpeaker component not found!");
            }
        }
        else
        {
            // 단일 모듈 또는 일반 텍스트 (하위 호환성)
            unitController.DoVoice(voiceText);
        }
    }
}