using System;
using LLMUnitySamples;
using UnityEditor.VersionControl;
using UnityEngine;

public class TeammateAI : MonoBehaviour
{
    public string teammateName = "Lena";
    public string teammateNameKorean = "레나";

    public void ExecuteCommand(AIActionEnum action, Parameters param)
    {
        Debug.Log($"[{teammateName}] 명령 수신: {action} - {param}");

        // 예시로 동작 분기 (대소문자 구분 없이)
        if (action == AIActionEnum.Move)
            Move(param);
        else if (action==AIActionEnum.Combat)
            Combat(param);
        else if (action==AIActionEnum.Support)
            Support(param);
        else if (action==AIActionEnum.Scout)
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
        string message = $"{param.destination} 위치 방어 중...";
        Debug.Log($"[{teammateName}] {message}");
        SendChat(message);
    }


    void Combat(Parameters param)
    {
        string message = $"{param.destination} 위치로 공격!";
        Debug.Log($"[{teammateName}] {message}");
        SendChat(message);
        // TODO: 공격 대상 지정, 애니메이션 트리거 등
    }

    void Support(Parameters param)
    {
        string message = $"{param.destination} 정찰 중...";
        Debug.Log($"[{teammateName}] {message}");
        SendChat(message);
        // TODO: 탐색 루트로 이동
    }

    void Scout(Parameters param)
    {
        string message = $"힐 중이야. 엄호해줘!";
        Debug.Log($"[{teammateName}] {message}");
        SendChat(message);
    }

}
