using UnityEngine;

/// <summary>
/// 이 컴포넌트가 부착된 GameObject가 씬(Scene) 전환 시 파괴되지 않도록 합니다.
/// 또한, 싱글톤 패턴을 적용하여 동일한 타입의 객체가 단 하나만 존재하도록 보장합니다.
/// </summary>
public class DontDestroyObject : MonoBehaviour
{
    // 이 클래스 타입의 static 인스턴스를 선언하여 싱글톤으로 작동하게 합니다.
    public static DontDestroyObject Instance { get; private set; }

    private void Awake()
    {
        // --- 싱글톤 패턴 구현 ---
        // 만약 static 인스턴스가 아직 할당되지 않았다면,
        if (Instance == null)
        {
            // 이 인스턴스를 static 인스턴스로 할당합니다.
            Instance = this;
            
            // 이 GameObject를 씬 전환 시 파괴되지 않도록 설정합니다.
            DontDestroyOnLoad(gameObject);
            
            Debug.Log($"[DontDestroyObject] '{gameObject.name}' 인스턴스가 생성되고 파괴 방지 설정되었습니다.");
        }
        // 만약 static 인스턴스가 이미 존재하고, 그것이 이 인스턴스가 아니라면,
        else if (Instance != this)
        {
            // 이 GameObject는 중복된 것이므로 파괴합니다.
            // 이렇게 함으로써 씬을 다시 로드했을 때 객체가 여러 개 생기는 것을 방지합니다.
            Debug.LogWarning($"[DontDestroyObject] '{gameObject.name}'은 이미 존재하는 인스턴스이므로 파괴됩니다.");
            Destroy(gameObject);
        }
    }
}