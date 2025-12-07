using UnityEngine;
using UnityEngine.EventSystems;

public class CursorHoverHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public void OnPointerEnter(PointerEventData eventData)
    {
        CustomCursor.Instance?.SetHover();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        CustomCursor.Instance?.SetDefault();
    }

    private void OnDisable()
    {
        CustomCursor.Instance?.SetDefault();
    }
}
