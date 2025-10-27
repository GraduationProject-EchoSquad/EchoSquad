using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 생명체로서 동작할 게임 오브젝트들을 위한 뼈대를 제공
// 체력, 데미지 받아들이기, 사망 기능, 사망 이벤트를 제공
public class LivingEntity : MonoBehaviour, IDamageable
{
    [Header("UI")] public Slider healthSlider; // 인스펙터에서 연결
    public TextMeshProUGUI nameText; // 인스펙터에서 연결
    public UnitController UnitController; // 인스펙터에서 연결

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
        else if (UnitController is EnemyController enemyController)
        {
            nameText.text = enemyController.enemyType.ToString();
        }
    }

    // 생명체가 활성화될때 상태를 리셋
    protected virtual void OnEnable()
    {
        // 사망하지 않은 상태로 시작
        dead = false;
        // 체력을 시작 체력으로 초기화
        health = startingHealth;

        UpdateUI();
    }

    // 데미지를 입는 기능
    public virtual bool ApplyDamage(DamageMessage damageMessage)
    {
        if (IsInvulnerable || damageMessage.damager == gameObject || dead) return false;

        lastDamagedTime = Time.time;

        // 데미지만큼 체력 감소
        health -= damageMessage.amount;

        UpdateUI();

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

        // 체력이 최대 체력을 넘지 않도록 제한
        health = Mathf.Min(health, startingHealth);

        UpdateUI();
    }

    // 최대 체력을 증가시키는 별도 메서드 추가
    public void IncreaseMaxHealth(float amount)
    {
        if (dead) return;

        // 1. 최대 체력 증가
        startingHealth += amount;

        // 2. UI 업데이트 (RestoreHealth에서 이미 호출되지만, maxValue 변경을 위해 한번 더 호출)
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
            healthSlider.maxValue = startingHealth;
        healthSlider.value = health; // LivingEntity에서 관리하는 현재 체력
    }
}