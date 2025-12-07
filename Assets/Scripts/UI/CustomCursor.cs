using UnityEngine;

/// <summary>
/// 하드웨어 커서 기반 커스텀 커서
/// </summary>
public class CustomCursor : Singleton<CustomCursor>
{
    [Header("Cursor Textures")]
    [SerializeField] private Texture2D defaultCursor;
    [SerializeField] private Texture2D hoverCursor;

    [Header("Hotspot (클릭 포인트)")]
    [SerializeField] private Vector2 hotspot = Vector2.zero;

    protected override void Awake()
    {
        base.Awake();
        Show();
        SetDefault();
    }

    public void Show()
    {
        Cursor.visible = true;
    }

    public void Hide()
    {
        Cursor.visible = false;
    }

    public void SetDefault()
    {
        Cursor.SetCursor(defaultCursor, hotspot, CursorMode.Auto);
    }

    public void SetHover()
    {
        Cursor.SetCursor(hoverCursor, hotspot, CursorMode.Auto);
    }

    public void SetCursor(Texture2D texture)
    {
        Cursor.SetCursor(texture, hotspot, CursorMode.Auto);
    }
}
