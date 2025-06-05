using System;
using System.Collections;
using UnityEngine;

public class Intro_Controller : Singleton<Intro_Controller>
{
    // 인트로가 끝났을 때 호출할 이벤트
    public event Action OnIntroFinished;

    [SerializeField] private GameObject vcamLobby;   // 로비 가상 카메라
    [SerializeField] private GameObject vcamRoom1;   // Room1 가상 카메라
    [SerializeField] private GameObject vcamRoom2;   // Room2 가상 카메라
    [SerializeField] private GameObject vcamUnder;   // RoomUnder 가상 카메라
    //[SerializeField] private GameObject player;      // 데모 끝나고 켤 플레이어

    [SerializeField] private float durLobby = 5f;    // 로비 카메라 재생 시간
    [SerializeField] private float durRoom1 = 5f;
    [SerializeField] private float durRoom2 = 5f;
    [SerializeField] private float durUnder = 5f;

    // 이펙트용 컴포넌트들…
    [SerializeField] private Runestone_Controller runestoneScript;
    [SerializeField] private Portal_Controller portalSimpleScripts;
    [SerializeField] private PortalRound_Controller portalRoundScripts;
    [SerializeField] private PortalGate_Controller portalGateScript;

    private void Start()
    {
        // 처음엔 모든 VCam과 Player를 비활성화
        vcamLobby.SetActive(false);
        vcamRoom1.SetActive(false);
        vcamRoom2.SetActive(false);
        vcamUnder.SetActive(false);
        //player.SetActive(false);

        // 다른 이펙트들도 초기화…
        runestoneScript.ToggleRuneStone(false);
        portalRoundScripts.F_TogglePortalRound(false);
        portalGateScript.F_TogglePortalGate(false);
        portalSimpleScripts.TogglePortal(false);

        StartCoroutine(DemoRoutine());
    }

    private IEnumerator DemoRoutine()
    {
        // 0) 룬스톤 켜기 & 로비 카메라 재생
        runestoneScript.ToggleRuneStone(true);

        vcamLobby.SetActive(true);
        yield return new WaitForSeconds(durLobby);
        vcamLobby.SetActive(false);

        // 1) 포탈 라운드 켜기 & Room1 카메라 재생
        portalRoundScripts.F_TogglePortalRound(true);

        vcamRoom1.SetActive(true);
        yield return new WaitForSeconds(durRoom1);

        // 2) 포탈 심플 켜기 & Room2 카메라로 전환
        portalSimpleScripts.TogglePortal(true);

        vcamRoom1.SetActive(false);
        vcamRoom2.SetActive(true);
        yield return new WaitForSeconds(durRoom2);

        // 3) 포탈 게이트 켜기 & RoomUnder 카메라로 전환
        portalGateScript.F_TogglePortalGate(true);

        vcamRoom2.SetActive(false);
        vcamUnder.SetActive(true);
        yield return new WaitForSeconds(durUnder);

        // 4) 모든 카메라 끄고 플레이어 활성화
        vcamUnder.SetActive(false);
        //player.SetActive(true);

        OnIntroFinished?.Invoke();
    }
}