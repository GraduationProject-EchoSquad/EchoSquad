using GLTFast.Schema;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.AI;
using Cysharp.Threading.Tasks;
using UnityEngine.Audio;

public class TeammateController : UnitController
{
    private const float NavSearchRadius = 100.0f;

    [SerializeField] private VoiceProfile voiceProfile;
    [SerializeField] private NavMeshAgent navMeshAgent;
    [SerializeField] private TeammateAI teammateAI;
    [SerializeField] private CinemachineFreeLook camera;

    private UnitShooter unitShooter;
    [SerializeField]
    private VoiceSpeaker voiceSpeaker; // 3D 음성 재생 컴포넌트

    private float waitBeforeRelease = 3f; // 멈춘 뒤 몇 초 후 target 제거
    private float stopTimer = 0f;
    private UnitController followTarget = null;

    public float speed = 6f;
    [SerializeField] float turnSpeed = 120f;
    [SerializeField] float accel = 8f;

    [SerializeField] float patrolRadius = 3f; // 순찰 반경
    [SerializeField] float patrolInterval = 5f; // 다음 순찰 지점 선택까지 대기 시간
    private Vector3 homePosition; // 시작 위치 저장
    float patrolTimer;

    [Header("Combat Retreat Settings")]
    [SerializeField] float minSafeDistance = 3f; // 최소 안전거리
    [SerializeField] float retreatDistance = 5f; // 후퇴 목표 거리

    protected override void Start()
    {
        base.Start();
        navMeshAgent = GetComponent<NavMeshAgent>();
        unitShooter = GetComponent<UnitShooter>();
        navMeshAgent.speed = speed;
        navMeshAgent.angularSpeed = turnSpeed;
        navMeshAgent.acceleration = accel;
        patrolTimer = patrolInterval;
        homePosition = transform.position;
    }


    public void Init(EUnitTeamType newUnitTeamType, string engName, string korName, VoiceProfile profile)
    {
        base.Init(newUnitTeamType);
        teammateAI.teammateName = engName;
        teammateAI.teammateNameKorean = korName;
        this.voiceProfile = profile;

        // VoiceSpeaker에 VoiceProfile 설정
        if (voiceSpeaker != null)
        {
            voiceSpeaker.SetVoiceProfile(profile);
        }
    }

    void Update()
    {
        if (IsDead())
        {
            return;
        }

        HandleMovement();

        if (unitState == EUnitState.Scout || unitState == EUnitState.Idle)
        {
            // Idle 또는 Scout 중에 타겟을 발견하면 즉시 전투 상태로 전환
            if (unitShooter.GetAimTargetUnit() != null)
            {
                ChangeUnitState(EUnitState.Combat);
                return;
            }

            if (unitState == EUnitState.Scout)
            {
                //CameraManager.Instance.SetCinemachineTarget(null);
                //SetCamera(false);
                if (unitShooter.GetAimTargetUnit() != null) // 타겟 발견시 순찰 중단
                {
                    if (navMeshAgent.hasPath)
                    {
                        navMeshAgent.ResetPath(); // 목적지 초기화
                        ChangeUnitState(EUnitState.Idle);
                    }

                    return;
                }

                if (IsNavArrivedTargetPosition())
                {
                    ChangeUnitState(EUnitState.Idle);
                }
            }

            else if (unitState == EUnitState.Idle)
            {
                //CameraManager.Instance.SetCinemachineTarget(null);
                //SetCamera(false);
                patrolTimer -= Time.deltaTime;


                if (patrolTimer <= 0f)
                {
                    PickPatrolPoint();
                    patrolTimer = patrolInterval;
                }
            }
        }
        //정찰상태
        /*if (unitState == EUnitState.Scout)
        {
            SetCamera(false);
            if (unitShooter.GetAimTargetUnit() != null) // 타겟 발견시 순찰 중단
            {
                if (navMeshAgent.hasPath)
                {
                    navMeshAgent.ResetPath(); // 목적지 초기화
                    ChangeUnitState(EUnitState.Idle);
                }

                return;
            }

            if (IsNavArrivedTargetPosition())
            {
                ChangeUnitState(EUnitState.Idle);
            }
        }
        //목적 없는 상태
        else if (unitState == EUnitState.Idle)
        {
            SetCamera(false);
            patrolTimer -= Time.deltaTime;


            if (patrolTimer <= 0f)
            {
                PickPatrolPoint();
                patrolTimer = patrolInterval;
            }
        }*/
        //LLM 이동 명령 수행하는 상태
        else if (unitState == EUnitState.Move)
        {
            var target = unitShooter.GetAimTargetUnit();
            if (target != null && target.IsDead() == false)
            {
                // 타겟이 시야에 있는지 확인
                if (unitShooter.IsVisibleTarget(target))
                {
                    transform.LookAt(target.transform.position);
                }
                else
                {
                    unitShooter.SetAimTargetUnit(null);
                }
            }
            
            if (followTarget != null)
            {
                FollowTarget();
            }
            else if (IsNavArrivedTargetPosition())
            {
                homePosition = transform.position;
                ChangeUnitState(EUnitState.Idle);
                CameraManager.Instance.SetCinemachineTarget(null);
            }
        } //전투 상태
        else if (unitState == EUnitState.Combat)
        {
            var target = unitShooter.GetAimTargetUnit();
            if (target == null || target.IsDead())
            {
                // 타겟을 잃으면 Idle 상태로 복귀
                ChangeUnitState(EUnitState.Idle);
                return;
            }

            // 후퇴가 필요한지 체크
            bool isRetreating = HandleCombatRetreat();

            // 후퇴 중이 아닐 때만 기존 전투 로직 실행
            if (!isRetreating)
            {
                // 타겟이 시야에 있는지 확인
                if (unitShooter.IsVisibleTarget(target))
                {
                    // 시야에 있으면 이동을 멈추고 타겟을 바라봄
                    navMeshAgent.ResetPath();
                    transform.LookAt(target.transform.position);
                }
                else
                {
                    // 시야에 없으면 타겟의 위치로 이동 (추적)
                    navMeshAgent.SetDestination(target.transform.position);
                }
            }
        }
        else if (unitState == EUnitState.Supprot)
        {
            if (followTarget == null)
            {
                ChangeUnitState(EUnitState.Idle);
                return;
            }
            
            FollowTarget();

        }
        /*if (IsNavArrivedTargetPosition())
        {
            if (!navMeshAgent.hasPath || navMeshAgent.velocity.sqrMagnitude == 0f)
            {
                navMeshAgent.ResetPath(); // 목적지 초기화
                Debug.Log("도달 완료. 경로 초기화됨.");
            }
        }*/
    }

    void PickPatrolPoint()
    {
        // homePosition을 중심으로 랜덤 위치 생성
        Vector3 randomDir = Random.insideUnitSphere * patrolRadius + homePosition;

        // NavMesh 위 유효 위치로 보정
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDir, out hit, patrolRadius, NavMesh.AllAreas))
        {
            navMeshAgent.SetDestination(hit.position);
            Debug.Log($"새 순찰 지점: {hit.position}");
            ChangeUnitState(EUnitState.Scout);
        }
    }

    void HandleMovement()
    {
        /*if (IsGrounded())
        {
            animator.SetBool("IsFalling", false);
        }
        else
        {
            animator.SetBool("IsFalling", true);
        }*/

        float speed = navMeshAgent.velocity.magnitude;
        Vector3 worldVel = navMeshAgent.velocity;
        Vector3 localVel = transform.InverseTransformDirection(worldVel);
        Vector2 moveInput = new Vector2(localVel.x, localVel.z).normalized;
        animator.SetFloat("Horizontal Move", moveInput.x * speed, 0.05f, Time.deltaTime);
        animator.SetFloat("Vertical Move", moveInput.y * speed, 0.05f, Time.deltaTime);
    }

    bool HandleCombatRetreat()
    {
        // 주변의 모든 적들을 체크
        var enemies = UnitManager.Instance.GetUnitTeamTypeList(GetOppositeTeamType());

        UnitController nearestEnemy = null;
        float nearestDistance = Mathf.Infinity;

        foreach (var enemy in enemies)
        {
            if (enemy.IsDead()) continue;

            float dist = Vector3.Distance(transform.position, enemy.transform.position);
            if (dist < nearestDistance)
            {
                nearestDistance = dist;
                nearestEnemy = enemy;
            }
        }

        // 가장 가까운 적이 최소 안전거리보다 가까우면 후퇴
        if (nearestEnemy != null && nearestDistance < minSafeDistance)
        {
            // 적의 반대방향으로 후퇴
            Vector3 directionAwayFromEnemy = (transform.position - nearestEnemy.transform.position).normalized;
            Vector3 retreatPosition = transform.position + directionAwayFromEnemy * retreatDistance;

            // NavMesh 위의 유효한 위치로 보정
            NavMeshHit hit;
            if (NavMesh.SamplePosition(retreatPosition, out hit, retreatDistance, NavMesh.AllAreas))
            {
                navMeshAgent.SetDestination(hit.position);
            }

            return true; // 후퇴 중
        }

        return false; // 후퇴 불필요
    }

    bool IsGrounded()
    {
        float rayDistance = 1f;
        Vector3 origin = transform.position + Vector3.up * 0.1f;
        return Physics.Raycast(origin, Vector3.down, rayDistance);
    }


    public void MoveToObject(GameObject gameObject)
    {
        NavMeshHit hit;
        if (NavMesh.SamplePosition(gameObject.transform.position, out hit, NavSearchRadius, NavMesh.AllAreas))
        {
            followTarget = null;
            navMeshAgent.SetDestination(hit.position); // 보정된 위치로 이동
            ChangeUnitState(EUnitState.Move);
            CameraManager.Instance.SetCinemachineTarget(this.gameObject);
            //SetCamera(true);
        }
    }

    public void MoveToUnit(UnitController unitController)
    {
        NavMeshHit hit;
        if (NavMesh.SamplePosition(unitController.transform.position, out hit, NavSearchRadius, NavMesh.AllAreas))
        {
            followTarget = null;
            navMeshAgent.SetDestination(hit.position); // 보정된 위치로 이동
            ChangeUnitState(EUnitState.Move);
            CameraManager.Instance.SetCinemachineTarget(this.gameObject);
            //SetCamera(true);
        }
    }

    public float maxDistance = 100f; // 최대 이동 거리

    public void MoveDirection(Vector3 origin, Vector3 direction)
    {
        Vector3 target = origin + direction * maxDistance;

        NavMeshHit hit;

        // NavMeshRaycast로 장애물이나 NavMesh의 경계를 확인
        if (NavMesh.Raycast(origin, target, out hit, NavMesh.AllAreas))
        {
            // 벽이나 끊긴 경로까지의 지점을 목적지로 설정
            navMeshAgent.SetDestination(hit.position);
        }
        else
        {
            // 장애물이 없으면 최대 거리까지 이동
            navMeshAgent.SetDestination(target);
        }

        ChangeUnitState(EUnitState.Move);
        CameraManager.Instance.SetCinemachineTarget(this.gameObject);
        //SetCamera(true);
    }

    public void SetFollowUnit(UnitController followUnitController)
    {
        followTarget = followUnitController;
        ChangeUnitState(EUnitState.Move);
        CameraManager.Instance.SetCinemachineTarget(this.gameObject);
        //SetCamera(true);
    }
    
    public void SetFollowHealUnit(UnitController followUnitController)
    {
        followTarget = followUnitController;
        ChangeUnitState(EUnitState.Supprot);
        CameraManager.Instance.SetCinemachineTarget(this.gameObject);
        //SetCamera(true);
    }

    private void FollowTarget()
    {
        if (followTarget == null)
        {
            return;
        }

        var targetPosition = followTarget.transform.position;
        navMeshAgent.SetDestination(targetPosition);

        float distanceToTarget = Vector3.Distance(transform.position, targetPosition);

        if (distanceToTarget <= navMeshAgent.stoppingDistance)
        {
            // agent가 실제로 멈췄는지 (목표 도착 + 속도 거의 없음)
            if (IsNavArrivedTargetPosition())
            {
                CameraManager.Instance.SetCinemachineTarget(null);
                //SetCamera(false);
                stopTimer += Time.deltaTime;

                if (stopTimer >= waitBeforeRelease)
                {
                    homePosition = transform.position;
                    Debug.Log("타겟 도착 후 3초 경과 → 타겟 해제");
                    if (unitState == EUnitState.Supprot)
                    {
                        DoHeal(followTarget);
                    }
                    followTarget = null;
                    stopTimer = 0f;
                    ChangeUnitState(EUnitState.Idle);
                }
            }
            else
            {
                // 아직 멈춘 상태가 아니므로 타이머 초기화
                stopTimer = 0f;
            }
        }
        else
        {
            // 아직 도착하지 않음
            stopTimer = 0f;
        }
    }

    public bool IsNavArrivedTargetPosition()
    {
        return !navMeshAgent.pathPending && navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance;
    }

    void DoHeal(UnitController targetUnit)
    {
        if (targetUnit.IsDead() == false)
        {
            return;
        }

        if (targetUnit is PlayerController playerController)
        {
            playerController.Respawn();
        }
        /*var health = targetUnit.GetComponent<LivingEntity>();
        health.RestoreHealth(100);*/
    }

    public TeammateAI GetTeammateAI()
    {
        return teammateAI;
    }

    public UnitShooter GetUnitShooter()
    {
        return unitShooter;
    }


    void SetCamera(bool setActive)
    {
        if (GameManager.Instance.isTest == false) return;
        camera.gameObject.SetActive(setActive);
    }

    /// <summary>
    /// 음성 재생 (VoiceSpeaker 사용 - 3D 공간 오디오)
    /// </summary>
    public async void DoVoice(string voiceText)
    {
        if (voiceSpeaker == null)
        {
            Debug.LogError($"[TeammateController] {teammateAI.teammateName}의 VoiceSpeaker가 없습니다!");
            return;
        }

        if (voiceProfile == null)
        {
            Debug.LogWarning($"[TeammateController] {teammateAI.teammateName}의 VoiceProfile이 설정되지 않았습니다.");
            return;
        }

        // '+' 기호로 모듈이 조합되어 있는지 확인
        if (voiceText.Contains("+"))
        {
            // 모듈 조합 재생 (예: "Sure thing!+moving")
            string[] modules = voiceText.Split('+');
            for (int i = 0; i < modules.Length; i++)
            {
                modules[i] = modules[i].Trim();
            }

            // VoiceSpeaker의 모듈 조합 재생 사용
            await voiceSpeaker.SpeakModulesAsync(modules);
        }
        else
        {
            // 단일 텍스트 재생
            await voiceSpeaker.SpeakAsync(voiceText);
        }
    }

    /// <summary>
    /// VoiceProfile 업데이트 (TeammateVoiceSetupManager에서 호출)
    /// </summary>
    public void UpdateVoiceProfile(VoiceProfile newProfile)
    {
        voiceProfile = newProfile;

        // VoiceSpeaker가 아직 생성 안 되었으면 생성 (Start() 이전에 호출될 수 있음)
        if (voiceSpeaker == null)
        {
            voiceSpeaker = GetComponent<VoiceSpeaker>();
            if (voiceSpeaker == null)
            {
                voiceSpeaker = gameObject.AddComponent<VoiceSpeaker>();
            }
        }

        // VoiceSpeaker에 프로필 적용
        voiceSpeaker.SetVoiceProfile(newProfile);

        Debug.Log($"[TeammateController] {teammateAI.teammateName}의 VoiceProfile 업데이트: Gender={newProfile.gender}, Pitch={newProfile.pitch}, Speed={newProfile.speed}");
    }

    public override async UniTaskVoid HandleDeath()
    {
        // 1. 부모 클래스의 HandleDeath 실행 (IsDead()가 true가 됨)
        base.HandleDeath();

        // 2. 이동 및 AI 정지
        if (navMeshAgent.isOnNavMesh)
        {
            navMeshAgent.isStopped = true;
            navMeshAgent.enabled = false;
        }

        // 3. 애니메이션 재생
        animator.SetTrigger("Die");

        // Todo: 사망 사운드 재생
        //if (audioSource != null && deathClip != null)
        //{
        //    audioSource.PlayOneShot(deathClip);
        //}
    }
}