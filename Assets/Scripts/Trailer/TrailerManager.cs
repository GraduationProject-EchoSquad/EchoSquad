using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class TrailerManager : Singleton<TrailerManager>
{
    // 인트로가 끝났을 때 호출할 이벤트
    public event Action OnIntroFinished;

    [SerializeField] private bool doIntro = false;

    // ▼▼▼▼▼ 여기만 남겼습니다 ▼▼▼▼▼
    [SerializeField] private GameObject vcamUnder;   // RoomUnder 가상 카메라
    [SerializeField] private float durUnder = 5f;   // 마지막 카메라 재생 시간
    [SerializeField] private PortalGate_Controller portalGateScript;

    [Header("Character Sequence")]
    [SerializeField] private GameObject characterObject; // 연출을 할 캐릭터 오브젝트
    [SerializeField] private Animator characterAnimator; // 캐릭터의 애니메이터

    //[SerializeField] private GameObject player;       // 데모 끝나고 켤 플레이어

    private void Start()
    {

        if (doIntro == false)
        {
            OnIntroFinished?.Invoke();

            // 마지막 포탈만 켭니다.
            if (portalGateScript != null)
                portalGateScript.F_TogglePortalGate(true);

            return;
        }

        // vcamUnder만 비활성화
        if (vcamUnder != null)
            vcamUnder.SetActive(false);
        //player.SetActive(false);

        // portalGateScript만 초기화
        if (portalGateScript != null)
            portalGateScript.F_TogglePortalGate(false);

        DemoRoutine().Forget();
    }

    // ▼▼▼▼▼ DemoRoutine 수정 ▼▼▼▼▼
    private async UniTaskVoid DemoRoutine()
    {
        // 3) 포탈 게이트 켜기 & RoomUnder 카메라로 전환 (이것만 재생)
        if (portalGateScript != null)
            portalGateScript.F_TogglePortalGate(true);

        if (vcamUnder != null)
            vcamUnder.SetActive(true);

        await UniTask.WaitForSeconds(durUnder);

        // 5) 캐릭터 애니메이션 시퀀스 시작
        if (characterObject == null || characterAnimator == null)
        {
            Debug.LogError("TrailerManager에 Character Object 또는 Animator가 할당되지 않았습니다!");
            // OnIntroFinished?.Invoke(); // (필요시 인트로 종료 호출)
            return; // 캐릭터가 없으면 종료
        }

        // 0. 캐릭터 활성화 (스폰)
        characterObject.SetActive(true);
        // (만약 'Spawn' 트리거가 있다면) characterAnimator.SetTrigger("Spawn");
        await UniTask.WaitForSeconds(0.5f); // 스폰 후 잠시 대기

        // 1. 'Hit' (피격) 모션
        // ?? "Hit" 트리거 이름은 실제 애니메이터 파라미터 이름으로 바꿔주세요.
        characterAnimator.SetTrigger("Hit");
        await UniTask.WaitForSeconds(0.5f); // 피격 애니메이션 길이 (초) - ?? 실제 길이에 맞게 조절

        // 2. 'Walk' (걷기) 모션 및 실제 이동
        // ?? "Walk"가 Bool 파라미터가 맞는지, 속도(walkSpeed)가 적절한지 확인하세요.
        characterAnimator.SetBool("Walk", true); // "Walk" 파라미터 켜기

        await UniTask.WaitForSeconds(0.3f);

        // 걷기 + 멈추는 애니메이션을 포함한 총 시간
        float totalWalkSequenceTime = 0.7f;

        // ?? [중요] 애니메이터에서 Walk -> Idle로 돌아오는 'Transition Duration' 값
        // (이 시간만큼 미리 멈추기 시작해야 함)
        float animBlendOutTime = 0.15f;

        float walkSpeed = 3.6f;
        float timer = 0f;
        bool isStopping = false;

        while (timer < totalWalkSequenceTime)
        {
            // [실제 이동] totalWalkSequenceTime(0.7초) 동안 계속 이동합니다.
            characterObject.transform.Translate(Vector3.forward * walkSpeed * Time.deltaTime, Space.Self);

            // [애니메이션 제어] 
            // 멈추기 0.2초 전에 (즉, 0.5초가 되는 시점에)
            // "Walk"를 끄라는 신호를 미리 보냅니다.
            if (!isStopping && timer >= totalWalkSequenceTime - animBlendOutTime)
            {
                isStopping = true;
                characterAnimator.SetBool("Walk", false);
            }

            timer += Time.deltaTime;
            await UniTask.Yield(); // 다음 프레임까지 대기
        }

        // 3. 'Attack' (공격) 모션
        await UniTask.WaitForSeconds(0.2f); // 공격 전 잠시 멈춤
        // ?? "Attack" 트리거 이름은 실제 애니메이터 파라미터 이름으로 바꿔주세요.
        characterAnimator.SetTrigger("Cast Spell");
        await UniTask.WaitForSeconds(1.5f); // 공격 애니메이션 길이 (초) - ?? 실제 길이에 맞게 조절

        // 6) 모든 시퀀스 종료
        Debug.Log("트레일러 시퀀스 종료");

        // 4) 카메라 끄고 인트로 종료
        if (vcamUnder != null)
            vcamUnder.SetActive(false);
        //player.SetActive(true);
    }
    // ▲▲▲▲▲ DemoRoutine 수정 ▲▲▲▲▲
}