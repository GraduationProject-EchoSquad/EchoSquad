using UnityEngine;
using UnityEngine.UI;

// LivingEntity: 기본 체력/데미지 로직을 제공한다고 가정
public class PlayerHealth : LivingEntity
{
    [Header("UI")]
    public Slider healthSlider;      // 인스펙터에서 연결

    private Animator animator;
    private AudioSource playerAudioPlayer;
    public AudioClip deathClip;
    public AudioClip hitClip;

    private void Awake()
    {
        playerAudioPlayer = GetComponent<AudioSource>();
        animator = GetComponent<Animator>();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        // 체력 최대값과 현재값으로 슬라이더 초기화
        if (healthSlider != null)
        {
            healthSlider.maxValue = startingHealth; // LivingEntity에서 정의된 최대 체력
            healthSlider.value = health;            // LivingEntity에서 현재 체력(health 필드)
        }
        UpdateUI();
    }

    public override void RestoreHealth(float newHealth)
    {
        base.RestoreHealth(newHealth);
        UpdateUI();
    }

    public override bool ApplyDamage(DamageMessage damageMessage)
    {
        if (!base.ApplyDamage(damageMessage))
            return false;

        // 데미지 이펙트나 사운드
        EffectManager.Instance.PlayHitEffect(
            damageMessage.hitPoint,
            damageMessage.hitNormal,
            transform,
            EffectManager.EffectType.Flesh
        );
        playerAudioPlayer.PlayOneShot(hitClip);

        UpdateUI();
        return true;
    }

    public override void Die()
    {
        base.Die();

        UpdateUI();  // 체력 0으로 반영
        playerAudioPlayer.PlayOneShot(deathClip);
        animator.SetTrigger("Die");
    }

    private void UpdateUI()
    {
        if (healthSlider != null)
            healthSlider.value = health;  // LivingEntity에서 관리하는 현재 체력
    }
}
