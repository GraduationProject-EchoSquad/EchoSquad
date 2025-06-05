using UnityEngine;
using UnityEngine.AI;

public class PlayerController : UnitController
{
    public AudioClip itemPickupClip;
    public int lifeRemains = 3;
    private AudioSource playerAudioPlayer;
    private PlayerHealth playerHealth;
    private PlayerMovement playerMovement;
    private PlayerShooter playerShooter;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        playerMovement = GetComponent<PlayerMovement>();
        playerShooter = GetComponent<PlayerShooter>();
        playerAudioPlayer = GetComponent<AudioSource>();
        playerHealth = LivingEntity as PlayerHealth;

        UIManager.Instance.UpdateLifeText(lifeRemains);
        Cursor.visible = false;
        
    }
    
    protected override void HandleDeath()
    {
        playerMovement.enabled = false;
        playerShooter.enabled = false;

        if (lifeRemains > 0)
        {
            lifeRemains--;
            UIManager.Instance.UpdateLifeText(lifeRemains);
            Invoke("Respawn", 3f);
        }
        else
        {
            GameManager.Instance.EndGame();
        }
        
        Cursor.visible = true;
    }

    public void Respawn()
    {
        gameObject.SetActive(false);
        gameObject.SetActive(true);
        playerMovement.enabled = true;
        playerShooter.enabled = true;

        playerShooter.gun.ammoRemain = 120;

        Cursor.visible = false;
    }


    private void OnTriggerEnter(Collider other)
    {
        // 아이템과 충돌한 경우 해당 아이템을 사용하는 처리
        // 사망하지 않은 경우에만 아이템 사용가능
        if (!playerHealth.dead)
        {
            //Todo
        }
    }
}