using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ChatManager : MonoBehaviour
{
    public Transform chatPanel;             // ä�� ������ ���� �θ�
    public GameObject TextChat;     // �޽��� ������

    public void AddMessage(string sender, string message)
    {
        GameObject TextClone = Instantiate(TextChat, chatPanel);

        TextClone.GetComponent<TextMeshProUGUI>().text = $"<b>{sender}</b>: {message}";

    }
}
