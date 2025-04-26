using System;
using UnityEditor.VersionControl;
using UnityEngine;

public class TeammateAI : MonoBehaviour
{
    public string teammateName = "Lena";
    public string teammateNameKorean = "����";

    public void ExecuteCommand(string action, string location)
    {
        Debug.Log($"[{teammateName}] ��� ����: {action} - {location}");

        // ���÷� ���� �б� (��ҹ��� ���� ����)
        if (string.Equals(action, "Defend", StringComparison.OrdinalIgnoreCase))
            Defend(location);
        else if (string.Equals(action, "Attack", StringComparison.OrdinalIgnoreCase))
            Attack(location);
        else if (string.Equals(action, "Scout", StringComparison.OrdinalIgnoreCase))
            Scout(location);
        else if (string.Equals(action, "Heal", StringComparison.OrdinalIgnoreCase))
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
