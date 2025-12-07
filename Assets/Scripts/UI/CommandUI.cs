using System.Collections.Generic;
using LLMUnitySamples;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CommandUI : UIBase
{
    [Header("Command Buttons")]
    [SerializeField] private Button attackButton;
    [SerializeField] private Button moveButton;
    [SerializeField] private Button followButton;
    [SerializeField] private Button supportButton;

    [Header("Teammate Selection (Left Panel - Always Visible)")]
    [SerializeField] private Button lenaButton;
    [SerializeField] private Button jamesButton;
    [SerializeField] private Button saraButton;

    [Header("Right Panel")]
    [SerializeField] private GameObject defaultPanel;   // 기본 안내 텍스트 패널
    [SerializeField] private GameObject movePanel;      // 미니맵 패널
    [SerializeField] private GameObject followPanel;    // Follow 대상 선택 패널
    [SerializeField] private GameObject attackPanel;    // Attack 패널 (있으면)
    [SerializeField] private GameObject supportPanel;   // Support 패널 (있으면)

    [Header("Follow Target Cards (Inside Follow Panel)")]
    [SerializeField] private GameObject followLenaCard;
    [SerializeField] private GameObject followJamesCard;
    [SerializeField] private GameObject followSaraCard;
    [SerializeField] private GameObject followPlayerCard;

    [Header("Follow Target Buttons")]
    [SerializeField] private Button followLenaButton;
    [SerializeField] private Button followJamesButton;
    [SerializeField] private Button followSaraButton;
    [SerializeField] private Button followPlayerButton;

    [Header("Action")]
    [SerializeField] private Button goAheadButton;
    [SerializeField] private Button closeButton;

    [Header("Move Panel - Map View")]
    [SerializeField] private TextMeshProUGUI moveSelectedText;  // "A Selected" 등 표시
    [SerializeField] private Button districtButtonA;
    [SerializeField] private Button districtButtonB;
    [SerializeField] private Button districtButtonC;
    private Camera commandMapCamera;                         // CameraManager에서 가져옴
    private string selectedDistrict = null;                  // 선택된 구역 이름

    // 현재 선택 상태
    private enum CommandType { None, Attack, Move, Follow, Support }
    private CommandType selectedCommand = CommandType.None;
    private string selectedTeammate = null;
    private string followTargetName = null;
    private bool hasMoveDestination = false;

    private Dictionary<string, Button> teammateButtons = new Dictionary<string, Button>();
    private Dictionary<string, Button> followTargetButtons = new Dictionary<string, Button>();
    private Dictionary<string, GameObject> followTargetCards = new Dictionary<string, GameObject>();
    private Dictionary<string, Button> districtButtons = new Dictionary<string, Button>();

    // 하이라이트 색상
    private Color normalColor = Color.white;
    private Color selectedColor = new Color(1f, 0.8f, 0.2f); // 노란색 계열

    private void Start()
    {
        // CameraManager에서 CommandMapCamera 가져오기
        if (CameraManager.Instance != null)
        {
            commandMapCamera = CameraManager.Instance.GetCommandMapCamera();
        }

        SetupButtons();

        // CommandMapCamera는 기본적으로 비활성화
        if (commandMapCamera != null)
        {
            commandMapCamera.gameObject.SetActive(false);
        }

        ResetSelection();
    }

    private void SetupButtons()
    {
        // Command buttons
        if (attackButton) attackButton.onClick.AddListener(() => SelectCommand(CommandType.Attack));
        if (moveButton) moveButton.onClick.AddListener(() => SelectCommand(CommandType.Move));
        if (followButton) followButton.onClick.AddListener(() => SelectCommand(CommandType.Follow));
        if (supportButton) supportButton.onClick.AddListener(() => SelectCommand(CommandType.Support));

        // Teammate selection buttons
        if (lenaButton)
        {
            lenaButton.onClick.AddListener(() => SelectTeammate("Lena"));
            teammateButtons["Lena"] = lenaButton;
        }
        if (jamesButton)
        {
            jamesButton.onClick.AddListener(() => SelectTeammate("James"));
            teammateButtons["James"] = jamesButton;
        }
        if (saraButton)
        {
            saraButton.onClick.AddListener(() => SelectTeammate("Sara"));
            teammateButtons["Sara"] = saraButton;
        }

        // Follow target buttons & cards
        if (followLenaButton)
        {
            followLenaButton.onClick.AddListener(() => SelectFollowTarget("Lena"));
            followTargetButtons["Lena"] = followLenaButton;
        }
        if (followLenaCard) followTargetCards["Lena"] = followLenaCard;

        if (followJamesButton)
        {
            followJamesButton.onClick.AddListener(() => SelectFollowTarget("James"));
            followTargetButtons["James"] = followJamesButton;
        }
        if (followJamesCard) followTargetCards["James"] = followJamesCard;

        if (followSaraButton)
        {
            followSaraButton.onClick.AddListener(() => SelectFollowTarget("Sara"));
            followTargetButtons["Sara"] = followSaraButton;
        }
        if (followSaraCard) followTargetCards["Sara"] = followSaraCard;

        if (followPlayerButton)
        {
            followPlayerButton.onClick.AddListener(() => SelectFollowTarget("Player"));
            followTargetButtons["Player"] = followPlayerButton;
        }
        if (followPlayerCard) followTargetCards["Player"] = followPlayerCard;

        // Go Ahead button
        if (goAheadButton) goAheadButton.onClick.AddListener(ExecuteCommand);

        // Close button
        if (closeButton) closeButton.onClick.AddListener(ClosePanel);

        // District buttons
        if (districtButtonA)
        {
            districtButtonA.onClick.AddListener(() => SelectDistrict("A"));
            districtButtons["A"] = districtButtonA;
        }
        if (districtButtonB)
        {
            districtButtonB.onClick.AddListener(() => SelectDistrict("B"));
            districtButtons["B"] = districtButtonB;
        }
        if (districtButtonC)
        {
            districtButtonC.onClick.AddListener(() => SelectDistrict("C"));
            districtButtons["C"] = districtButtonC;
        }
    }

    private void OnEnable()
    {
        // CameraManager에서 CommandMapCamera 가져오기 (매번 확인)
        if (commandMapCamera == null && CameraManager.Instance != null)
        {
            commandMapCamera = CameraManager.Instance.GetCommandMapCamera();
        }

        // 선택 상태 초기화
        selectedCommand = CommandType.None;
        selectedTeammate = null;
        followTargetName = null;
        hasMoveDestination = false;
        selectedDistrict = null;

        if (moveSelectedText != null)
        {
            moveSelectedText.text = "";
        }

        // 한 프레임 뒤에 UI 업데이트 (Animator 준비 대기)
        StartCoroutine(DelayedUpdateUI());

        Cursor.lockState = CursorLockMode.None;
    }

    private System.Collections.IEnumerator DelayedUpdateUI()
    {
        yield return null;
        UpdateUI();
    }

    private void OnDisable()
    {
        ExitMapViewMode();
        Cursor.lockState = CursorLockMode.Confined;
    }

    private void OnDestroy()
    {
        // Cleanup listeners
        if (attackButton) attackButton.onClick.RemoveAllListeners();
        if (moveButton) moveButton.onClick.RemoveAllListeners();
        if (followButton) followButton.onClick.RemoveAllListeners();
        if (supportButton) supportButton.onClick.RemoveAllListeners();
        if (lenaButton) lenaButton.onClick.RemoveAllListeners();
        if (jamesButton) jamesButton.onClick.RemoveAllListeners();
        if (saraButton) saraButton.onClick.RemoveAllListeners();
        if (followLenaButton) followLenaButton.onClick.RemoveAllListeners();
        if (followJamesButton) followJamesButton.onClick.RemoveAllListeners();
        if (followSaraButton) followSaraButton.onClick.RemoveAllListeners();
        if (followPlayerButton) followPlayerButton.onClick.RemoveAllListeners();
        if (goAheadButton) goAheadButton.onClick.RemoveAllListeners();
        if (closeButton) closeButton.onClick.RemoveAllListeners();
        if (districtButtonA) districtButtonA.onClick.RemoveAllListeners();
        if (districtButtonB) districtButtonB.onClick.RemoveAllListeners();
        if (districtButtonC) districtButtonC.onClick.RemoveAllListeners();
    }

    private void ResetSelection()
    {
        selectedCommand = CommandType.None;
        selectedTeammate = null;
        followTargetName = null;
        hasMoveDestination = false;
        selectedDistrict = null;

        if (moveSelectedText != null)
        {
            moveSelectedText.text = "";
        }

        UpdateUI();
    }

    private void SelectCommand(CommandType command)
    {
        selectedCommand = command;
        followTargetName = null;
        hasMoveDestination = false;
        UpdateUI();
    }

    private void SelectTeammate(string teammateName)
    {
        selectedTeammate = teammateName;
        UpdateUI();
    }

    private void SelectFollowTarget(string targetName)
    {
        followTargetName = targetName;
        UpdateUI();
    }

    private void SelectDistrict(string district)
    {
        selectedDistrict = district;
        hasMoveDestination = true;

        if (moveSelectedText != null)
        {
            moveSelectedText.text = $"{district} Selected";
        }

        Debug.Log($"[CommandUI] District selected: {district}");
        UpdateUI();
    }

    private void HighlightButton(Button button, bool selected)
    {
        if (button == null) return;

        button.interactable = !selected;

        var animator = button.GetComponent<Animator>();
        if (animator != null)
        {
            animator.SetTrigger(selected ? "Pressed" : "Normal");
        }
    }

    private void UpdateUI()
    {
        // Command button states + highlight
        HighlightButton(attackButton, selectedCommand == CommandType.Attack);
        HighlightButton(moveButton, selectedCommand == CommandType.Move);
        HighlightButton(followButton, selectedCommand == CommandType.Follow);
        HighlightButton(supportButton, selectedCommand == CommandType.Support);

        // Teammate button states + highlight
        foreach (var kvp in teammateButtons)
        {
            HighlightButton(kvp.Value, kvp.Key == selectedTeammate);
        }

        // District button states (A, B, C) - 노란색으로 변경
        foreach (var kvp in districtButtons)
        {
            var image = kvp.Value.GetComponent<Image>();
            if (image != null)
            {
                image.color = (kvp.Key == selectedDistrict) ? selectedColor : normalColor;
            }
        }

        // Right Panel visibility - Teammate + Command 둘 다 선택해야 표시
        // Support는 오른쪽 패널 필요 없음
        bool showRightPanel = (selectedTeammate != null && selectedCommand != CommandType.None && selectedCommand != CommandType.Support);

        if (defaultPanel) defaultPanel.SetActive(!showRightPanel && selectedCommand != CommandType.Support);

        bool showMovePanel = showRightPanel && selectedCommand == CommandType.Move;
        if (movePanel) movePanel.SetActive(showMovePanel);

        // Move 패널 활성화 시 MiniMapCamera 조작
        if (showMovePanel)
        {
            EnterMapViewMode();
        }
        else
        {
            ExitMapViewMode();
        }

        if (followPanel) followPanel.SetActive(showRightPanel && selectedCommand == CommandType.Follow);
        if (attackPanel) attackPanel.SetActive(showRightPanel && selectedCommand == CommandType.Attack);
        if (supportPanel) supportPanel.SetActive(false); // Support는 오른쪽 패널 안 씀

        // Follow panel: 선택한 동료는 숨기기
        if (selectedCommand == CommandType.Follow)
        {
            UpdateFollowTargetButtons();
        }

        // Go Ahead button state
        UpdateGoAheadButton();
    }

    private void UpdateFollowTargetButtons()
    {
        // 카드 표시/숨김
        foreach (var kvp in followTargetCards)
        {
            if (kvp.Key == "Player")
            {
                // Player는 항상 표시
                kvp.Value.SetActive(true);
            }
            else
            {
                // 선택한 동료 카드는 숨기기
                bool isSelected = (kvp.Key == selectedTeammate);
                kvp.Value.SetActive(!isSelected);
            }
        }

        // 버튼 상태 + highlight
        foreach (var kvp in followTargetButtons)
        {
            HighlightButton(kvp.Value, kvp.Key == followTargetName);
        }
    }

    private void UpdateGoAheadButton()
    {
        if (goAheadButton == null) return;

        bool canExecute = false;

        // Teammate + Command 둘 다 선택해야 함
        if (selectedTeammate != null && selectedCommand != CommandType.None)
        {
            switch (selectedCommand)
            {
                case CommandType.Move:
                    canExecute = hasMoveDestination;
                    break;
                case CommandType.Follow:
                    canExecute = !string.IsNullOrEmpty(followTargetName);
                    break;
                case CommandType.Attack:
                case CommandType.Support:
                    canExecute = true;
                    break;
            }
        }

        goAheadButton.interactable = canExecute;
    }

    private void ExecuteCommand()
    {
        if (string.IsNullOrEmpty(selectedTeammate))
        {
            Debug.LogWarning("[CommandUI] No teammate selected");
            return;
        }

        // TeammateAI 가져오기
        if (!UnitManager.Instance.teammateUnitDict.TryGetValue(selectedTeammate, out var teammateController))
        {
            Debug.LogWarning($"[CommandUI] Teammate not found: {selectedTeammate}");
            return;
        }

        var teammateAI = teammateController.GetComponent<TeammateAI>();
        if (teammateAI == null)
        {
            Debug.LogWarning($"[CommandUI] TeammateAI not found on {selectedTeammate}");
            return;
        }

        // AIActionEnum 및 Parameters 생성
        AIActionEnum action = AIActionEnum.Move;
        Parameters param = new Parameters();

        switch (selectedCommand)
        {
            case CommandType.Move:
                action = AIActionEnum.Move;
                if (hasMoveDestination && !string.IsNullOrEmpty(selectedDistrict))
                {
                    param.destination = selectedDistrict;
                    param.voice = $"Moving+to+{selectedDistrict}";
                }
                break;

            case CommandType.Follow:
                action = AIActionEnum.Move;
                param.follow_target = followTargetName;
                param.voice = "Roger+following";
                break;

            case CommandType.Attack:
                action = AIActionEnum.Combat;
                param.engage_enemy = "nearestTarget";
                param.voice = "Engaging+enemy";
                break;

            case CommandType.Support:
                action = AIActionEnum.Support;
                param.support_target = "Player";
                param.voice = "Supporting";
                break;
        }

        // 명령 실행
        teammateAI.ExecuteCommand(action, param);
        Debug.Log($"[CommandUI] Command executed: {selectedCommand} -> {selectedTeammate}");

        // UI 닫기
        ClosePanel();
    }

    private void ClosePanel()
    {
        UIManager.Instance.Hide(UIManager.EUIData.Command);
    }

    private void EnterMapViewMode()
    {
        if (commandMapCamera != null)
        {
            commandMapCamera.gameObject.SetActive(true);
        }
    }

    private void ExitMapViewMode()
    {
        if (commandMapCamera != null)
        {
            commandMapCamera.gameObject.SetActive(false);
        }
    }
}

