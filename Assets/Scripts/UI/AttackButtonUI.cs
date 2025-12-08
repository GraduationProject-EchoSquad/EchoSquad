using Michsky.UI.Shift;
using UnityEngine;
using UnityEngine.UI;

public class AttackButtonUI : MonoBehaviour
{
    [SerializeField]
    private MainButton mainButton;
    public Button button;
    
    public void SetTarget(string targetName)
    {
        mainButton.buttonText = targetName;
    }
}
