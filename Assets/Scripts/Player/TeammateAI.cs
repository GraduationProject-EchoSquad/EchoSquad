using UnityEditor.VersionControl;
using UnityEngine;

public class TeammateAI : MonoBehaviour
{
    public string teammateName = "Lena";
    public string teammateNameKorean = "레나";

    public void ExecuteCommand(string action, string location)
    {
        Debug.Log($"[{teammateName}] 명령 수신: {action} - {location}");

        // 예시로 동작 분기
        if (action == "Defend")
            Defend(location);
        else if (action == "Attack")
            Attack(location);
        else if (action == "Scout")
            Scout(location);
        else if (action == "Heal")
            Heal();
    }
    void SendChat(string message)
    {
        ChatManager chat = FindObjectOfType<ChatManager>();
        if (chat != null)
        {
            chat.AddMessage(teammateName, message);
        }
    }

    void Defend(string location)
    {
        string message = $"{location} 위치 방어 중...";
        Debug.Log($"[{teammateName}] {message}");
        SendChat(message);
    }


    void Attack(string location)
    {
        string message = $"{location} 위치로 공격!";
        Debug.Log($"[{teammateName}] {message}");
        SendChat(message);
        // TODO: 공격 대상 지정, 애니메이션 트리거 등
    }

    void Scout(string location)
    {
        string message = $"{location} 정찰 중...";
        Debug.Log($"[{teammateName}] {message}");
        SendChat(message);
        // TODO: 탐색 루트로 이동
    }

    void Heal()
    {
        string message = $"힐 중이야. 엄호해줘!";
        Debug.Log($"[{teammateName}] {message}");
        SendChat(message);
    }

}
