using System;
using LLMUnitySamples;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.AI;

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
    }

    void SendChat(string message)
    {
        ChatManager chat = FindObjectOfType<ChatManager>();
        if (chat != null)
        {
            chat.AddMessage(teammateName, message);
        }
    }

    void Move(Parameters param)
    {
        //TODO 처리 필요
        UnitManager unitManager = FindObjectOfType<UnitManager>();
        MapManager mapManager = FindObjectOfType<MapManager>();

        string message = "";
        
        //follow target
        if (!string.IsNullOrEmpty(param.follow_target) && param.follow_target != "null")
        {
            if (param.follow_target.Equals("Player"))
            {
                unitController.followTarget = unitManager.PlayerUnit.gameObject;
            }
            else if (unitManager.allayUnitDict.TryGetValue(param.follow_target, out var followUnit))
            {
                unitController.followTarget = followUnit.gameObject;

                Debug.Log($"Move To AI Unit {param.follow_target}");

                message = $"[{teammateName}] : {param.follow_target}";
            }
        }
        else
        {
            //Left | Right | Center | Back | Forward
            if (string.Equals(param.destination, "Left", StringComparison.OrdinalIgnoreCase))
            {
            }
            else if (string.Equals(param.destination, "Right", StringComparison.OrdinalIgnoreCase))
            {
            }
            else if (string.Equals(param.destination, "Back", StringComparison.OrdinalIgnoreCase))
            {
            }
            else if (string.Equals(param.destination, "Forward", StringComparison.OrdinalIgnoreCase))
            {
            }
            else if (unitManager.allayUnitDict.TryGetValue(param.destination, out var followUnit))
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
        SendChat(message);
    }


    void Combat(Parameters param)
    {
        string message = $"{param.engage_enemy} 공격!";
        Debug.Log($"[{teammateName}] {message}");
        SendChat(message);
        // TODO: 공격 대상 지정, 애니메이션 트리거 등
    }

    void Support(Parameters param)
    {
        string message = $"힐 중이야. 엄호해줘!";
        Debug.Log($"[{teammateName}] {message}");
        SendChat(message);
    }

    void Scout(Parameters param)
    {
        string message = $"{param.destination} 정찰 중...";
        Debug.Log($"[{teammateName}] {message}");
        SendChat(message);
        // TODO: 탐색 루트로 이동
    }
}