using UnityEditor.VersionControl;
using UnityEngine;

public class TeammateAI : MonoBehaviour
{
    public string teammateName = "Lena";
    public string teammateNameKorean = "����";

    public void ExecuteCommand(string action, string location)
    {
        Debug.Log($"[{teammateName}] ��� ����: {action} - {location}");

        // ���÷� ���� �б�
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
        string message = $"{location} ��ġ ��� ��...";
        Debug.Log($"[{teammateName}] {message}");
        SendChat(message);
    }


    void Attack(string location)
    {
        string message = $"{location} ��ġ�� ����!";
        Debug.Log($"[{teammateName}] {message}");
        SendChat(message);
        // TODO: ���� ��� ����, �ִϸ��̼� Ʈ���� ��
    }

    void Scout(string location)
    {
        string message = $"{location} ���� ��...";
        Debug.Log($"[{teammateName}] {message}");
        SendChat(message);
        // TODO: Ž�� ��Ʈ�� �̵�
    }

    void Heal()
    {
        string message = $"�� ���̾�. ��ȣ����!";
        Debug.Log($"[{teammateName}] {message}");
        SendChat(message);
    }

}
