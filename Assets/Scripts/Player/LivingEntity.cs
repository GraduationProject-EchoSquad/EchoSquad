using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 생명체로서 동작할 게임 오브젝트들을 위한 뼈대를 제공
// 체력, 데미지 받아들이기, 사망 기능, 사망 이벤트를 제공
public class LivingEntity : MonoBehaviour, IDamageable
{
    [Header("UI")]
    public Slider healthSlider;      // 인스펙터에서 연결
    public TextMeshProUGUI nameText;      // 인스펙터에서 연결
    public UnitController UnitController;      // 인스펙터에서 연결
    
    public float startingHealth = 100f; // 시작 체력
    public float health { get; protected set; } // 현재 체력
    public bool dead { get; protected set; } // 사망 상태
    
    public event Action OnDeath; // 사망시 발동할 이벤트
    
    private const float minTimeBetDamaged = 0.1f;
    private float lastDamagedTime;

    protected bool IsInvulnerable
    {
        get
        {
            if (Time.time >= lastDamagedTime + minTimeBetDamaged) return false;

            return true;
        }
    }

    private void Start()
    {
        UnitController = GetComponent<UnitController>();
        if (UnitController is TeammateController teammateController)
        {
            nameText.text = teammateController.GetTeammateAI().teammateName;
        }
    }

    // 생명체가 활성화될때 상태를 리셋
    protected virtual void OnEnable()
    {
        // 사망하지 않은 상태로 시작
        dead = false;
        // 체력을 시작 체력으로 초기화
        health = startingHealth;
        if (healthSlider != null)
        {
            healthSlider.maxValue = startingHealth; // LivingEntity에서 정의된 최대 체력
            healthSlider.value = health;            // LivingEntity에서 현재 체력(health 필드)
        }
        UpdateUI();
    }

    // 데미지를 입는 기능
    public virtual bool ApplyDamage(DamageMessage damageMessage)
    {
        if (IsInvulnerable || damageMessage.damager == gameObject || dead) return false;

        lastDamagedTime = Time.time;

        // 데미지만큼 체력 감소
        health -= damageMessage.amount;

        // 체력이 0 이하 && 아직 죽지 않았다면 사망 처리 실행
        if (health <= 0) Die();

        return true;
    }
    
    // 체력을 회복하는 기능
    public void RestoreHealth(float newHealth)
    {
        if (dead) return;

        // 체력 추가
        health += newHealth;
        UpdateUI();
    }
    

    // 사망 처리
    public virtual void Die()
    {
        // onDeath 이벤트에 등록된 메서드가 있다면 실행
        if (OnDeath != null) OnDeath();

        // 사망 상태를 참으로 변경
        dead = true;
    }
    
    protected void UpdateUI()
    {
        if (healthSlider != null)
            healthSlider.value = health;  // LivingEntity에서 관리하는 현재 체력
    }
}