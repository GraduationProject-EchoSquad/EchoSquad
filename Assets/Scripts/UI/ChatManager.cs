using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ChatManager : Singleton<ChatManager>
{
    public Transform chatPanel;             // 채팅 내용이 들어가는 부모
    public GameObject TextChat;     // 메시지 프리팹

    public void AddMessage(string sender, string message)
    {
        Debug.Log($"[ChatManager] AddMessage called: {sender}: {message}");

        GameObject TextClone = Instantiate(TextChat, chatPanel);
        TextClone.GetComponent<TextMeshProUGUI>().text = $"<b>{sender}</b>: {message}";

        Debug.Log($"[ChatManager] Message added to UI");
    }
}
