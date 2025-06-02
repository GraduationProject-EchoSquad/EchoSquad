using System;
using System.Collections;
using UnityEngine;

public class Intro_Controller : MonoBehaviour
{
    // ��Ʈ�ΰ� ������ �� ȣ���� �̺�Ʈ
    public event Action OnIntroFinished;

    [SerializeField] private GameObject vcamLobby;   // �κ� ���� ī�޶�
    [SerializeField] private GameObject vcamRoom1;   // Room1 ���� ī�޶�
    [SerializeField] private GameObject vcamRoom2;   // Room2 ���� ī�޶�
    [SerializeField] private GameObject vcamUnder;   // RoomUnder ���� ī�޶�
    [SerializeField] private GameObject player;      // ���� ������ �� �÷��̾�

    [SerializeField] private float durLobby = 5f;    // �κ� ī�޶� ��� �ð�
    [SerializeField] private float durRoom1 = 5f;
    [SerializeField] private float durRoom2 = 5f;
    [SerializeField] private float durUnder = 5f;

    // ����Ʈ�� ������Ʈ�顦
    [SerializeField] private Runestone_Controller runestoneScript;
    [SerializeField] private Portal_Controller portalSimpleScripts;
    [SerializeField] private PortalRound_Controller portalRoundScripts;
    [SerializeField] private PortalGate_Controller portalGateScript;

    private void Start()
    {
        // ó���� ��� VCam�� Player�� ��Ȱ��ȭ
        vcamLobby.SetActive(false);
        vcamRoom1.SetActive(false);
        vcamRoom2.SetActive(false);
        vcamUnder.SetActive(false);
        player.SetActive(false);

        // �ٸ� ����Ʈ�鵵 �ʱ�ȭ��
        runestoneScript.ToggleRuneStone(false);
        portalRoundScripts.F_TogglePortalRound(false);
        portalGateScript.F_TogglePortalGate(false);
        portalSimpleScripts.TogglePortal(false);

        StartCoroutine(DemoRoutine());
    }

    private IEnumerator DemoRoutine()
    {
        // 0) �齺�� �ѱ� & �κ� ī�޶� ���
        runestoneScript.ToggleRuneStone(true);

        vcamLobby.SetActive(true);
        yield return new WaitForSeconds(durLobby);
        vcamLobby.SetActive(false);

        // 1) ��Ż ���� �ѱ� & Room1 ī�޶� ���
        portalRoundScripts.F_TogglePortalRound(true);

        vcamRoom1.SetActive(true);
        yield return new WaitForSeconds(durRoom1);

        // 2) ��Ż ���� �ѱ� & Room2 ī�޶�� ��ȯ
        portalSimpleScripts.TogglePortal(true);

        vcamRoom1.SetActive(false);
        vcamRoom2.SetActive(true);
        yield return new WaitForSeconds(durRoom2);

        // 3) ��Ż ����Ʈ �ѱ� & RoomUnder ī�޶�� ��ȯ
        portalGateScript.F_TogglePortalGate(true);

        vcamRoom2.SetActive(false);
        vcamUnder.SetActive(true);
        yield return new WaitForSeconds(durUnder);

        // 4) ��� ī�޶� ���� �÷��̾� Ȱ��ȭ
        vcamUnder.SetActive(false);
        player.SetActive(true);

        OnIntroFinished?.Invoke();
    }
}