using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AI;

public class PlayerController : UnitController
{
    public AudioClip itemPickupClip;
    public int lifeRemains = 3;
    private AudioSource playerAudioPlayer;
    private PlayerHealth playerHealth;
    private PlayerMovement playerMovement;
    private PlayerShooter playerShooter;
    
    [SerializeField]
    private Camera miniMapCamera;  // 씬에 배치된 CommandMapCamera

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        playerMovement = GetComponent<PlayerMovement>();
        playerShooter = GetComponent<PlayerShooter>();
        playerAudioPlayer = GetComponent<AudioSource>();
        playerHealth = LivingEntity as PlayerHealth;

        SetMiniMapTexture().Forget();

        SetLifeRemains(3);
    }

    private async UniTaskVoid SetMiniMapTexture()
    {
        miniMapCamera.targetTexture = await Addressables.LoadAssetAsync<RenderTexture>("RenderTextures/MiniMapRenderTexture.renderTexture").ToUniTask();
    }

    public override async UniTaskVoid HandleDeath()
    {
        base.HandleDeath();
        playerMovement.enabled = false;
        playerShooter.enabled = false;

        /*if (lifeRemains > 0)
        {
            SetLifeRemains(lifeRemains - 1);


            //UIManager.Instance.UpdateLifeText(lifeRemains);
            await UniTask.WaitForSeconds(3f);
            Respawn();
        }
        else
        {
            PubSubManager.Instance.Publish<OnPlayerDeathData>(PubSubEvent.OnPlayerDeath, data => {});
            //GameManager.Instance.EndGame(false);
        }*/
    }

    public void SetLifeRemains(int NewLifeRemains)
    {
        lifeRemains = NewLifeRemains;
        PubSubManager.Instance.Publish<OnLifeChangedData>(PubSubEvent.OnLifeChanged,
            data => data.LiveCount = lifeRemains);
    }

    public void Respawn()
    {
        ChangeUnitState(EUnitState.Idle, true);

        gameObject.SetActive(false);
        gameObject.SetActive(true);
        playerMovement.enabled = true;
        playerShooter.enabled = true;

        playerShooter.gun.SetAmmoRemain(120);
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

#if UNITY_EDITOR


    public void ForceDeadImmediately()
    {
        SetLifeRemains(0);
        LivingEntity.Die();
    }
#endif
}