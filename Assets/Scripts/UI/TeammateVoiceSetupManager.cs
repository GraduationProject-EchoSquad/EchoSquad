using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// 게임 시작 전 동료 음성 설정 UI 관리
// UIManager의 Canvas 아래에 AllyStatChoiceUI 프리팹을 생성하고 제어
public class TeammateVoiceSetupManager : Singleton<TeammateVoiceSetupManager>
{
    // 런타임에 생성된 UI Controller
    private AllyStatChoiceUI ui;

    // 동료별 선택된 VoiceProfile 저장
    private Dictionary<string, VoiceProfile> selectedVoiceProfiles = new Dictionary<string, VoiceProfile>();

    // UI 완료 대기용 TaskCompletionSource
    private UniTaskCompletionSource setupCompletionSource;

    protected override void Awake()
    {
        base.Awake();
    }

    /// <summary>
    /// UI가 생성된 후 동료 이름 기반으로 VoiceProfile 초기화 (기본값)
    /// 실제 슬라이더 값은 OnConfirmButtonClicked()에서 적용됨
    /// </summary>
    private void InitializeVoiceProfiles()
    {
        if (ui == null) return;

        var settings = ui.GetAllSettings();
        foreach (var kvp in settings)
        {
            string teammateName = kvp.Key;

            if (!selectedVoiceProfiles.ContainsKey(teammateName))
            {
                // 기본값으로 VoiceProfile 생성 (나중에 슬라이더 값으로 덮어씀)
                var profile = ScriptableObject.CreateInstance<VoiceProfile>();
                profile.gender = VoiceGender.Male;
                profile.pitch = VoiceProperty.Moderate;
                profile.speed = VoiceProperty.Moderate;
                selectedVoiceProfiles[teammateName] = profile;
            }
        }

        Debug.Log($"[TeammateVoiceSetupManager] Initialized {selectedVoiceProfiles.Count} VoiceProfiles with default values");
    }

    public async UniTask ShowAndWaitForCompletion()
    {
        setupCompletionSource = new UniTaskCompletionSource();

        // MapManager의 district 이름들을 VoiceModules에 등록
        RegisterDistrictNamesForVoice();

        // UI 생성 및 표시 (CreateUI 완료 대기)
        await CreateUI();
        ShowUI();

        // 사용자가 확인 버튼을 누를 때까지 대기
        await setupCompletionSource.Task;

        // UI 숨김 및 파괴
        HideUI();
    }

    /// <summary>
    /// MapManager의 district 이름들을 VoiceModules에 등록
    /// </summary>
    private void RegisterDistrictNamesForVoice()
    {
        MapManager mapManager = MapManager.Instance;
        if (mapManager == null || mapManager.districtDict == null)
        {
            Debug.LogWarning("[TeammateVoiceSetupManager] MapManager or districtDict is null!");
            return;
        }

        List<string> districtNames = new List<string>(mapManager.districtDict.Keys);
        VoiceModules.RegisterDistrictNames(districtNames);
    }

    // UI 프리팹을 UIManager의 Canvas 아래에 Instantiate
    private async UniTask CreateUI()
    {
        if (ui != null) return; // 이미 생성됨

        // UIManager로부터 메인 Canvas 가져오기
        Canvas mainCanvas = UIManager.Instance.GetMainCanvas();
        if (mainCanvas == null)
        {
            Debug.LogError("[TeammateVoiceSetupManager] Cannot find Main Canvas from UIManager!");
            return;
        }


        // Instantiate 직후 비활성화 (Shift UI 컴포넌트 초기화 문제 방지)
        ui = await UIManager.Instance.GetUI<AllyStatChoiceUI>(UIManager.EUIData
            .TeamVoiceSetUp); //Instantiate(uiPrefab, mainCanvas.transform);
        ui.gameObject.SetActive(false);

        if (ui != null)
        {
            // 확인 버튼 이벤트 연결
            ui.OnConfirmClicked = OnConfirmButtonClicked;
            Debug.Log("[TeammateVoiceSetupManager] UI prefab created under UIManager's Canvas");
        }
        else
        {
            Debug.LogError("[TeammateVoiceSetupManager] Cannot find AllyStatChoiceUIController component!");
        }

        // RectTransform 풀스크린으로 설정
        RectTransform rectTransform = ui.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            Debug.Log("[TeammateVoiceSetupManager] RectTransform set to fullscreen");
        }
    }

    private async UniTaskVoid ShowUI()
    {
        if (ui != null)
        {
            Debug.Log("[TeammateVoiceSetupManager] Activating AllyStatChoiceUI");
            ui.gameObject.SetActive(true);
            ui.SetProgressText("Select your teammates' personalities");

            // UI가 생성되었으므로 동료 이름 기반으로 VoiceProfile 초기화
            InitializeVoiceProfiles();
        }
        else
        {
            Debug.LogError("[TeammateVoiceSetupManager] uiController is null!");
        }

        // 커서 표시 (음성 설정을 위해)
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Debug.Log("[TeammateVoiceSetupManager] AllyStatChoiceUI shown");
    }

    private async UniTaskVoid HideUI()
    {
        if (ui != null)
        {
            Destroy(ui.gameObject);
            ui = null;
            Debug.Log("[TeammateVoiceSetupManager] AllyStatChoiceUI destroyed");
        }

        // 커서 다시 숨김 (게임플레이를 위해)
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = false;
    }

    private async void OnConfirmButtonClicked()
    {
        // 버튼 비활성화 (중복 클릭 방지)
        ui?.SetConfirmButtonInteractable(false);

        // UI Controller에서 모든 설정값 가져오기
        var settings = ui?.GetAllSettings();
        if (settings != null)
        {
            foreach (var kvp in settings)
            {
                string teammateName = kvp.Key;
                var (gender, liveliness, mood) = kvp.Value;

                if (selectedVoiceProfiles.ContainsKey(teammateName))
                {
                    var profile = selectedVoiceProfiles[teammateName];
                    profile.gender = gender;
                    profile.speed = ConvertToVoiceProperty(liveliness); // 활발함 → Speed
                    profile.pitch = ConvertToVoiceProperty(mood); // 분위기 → Pitch

                    Debug.Log(
                        $"{teammateName} voice settings: {profile.gender}, Pitch:{profile.pitch}, Speed:{profile.speed}");
                }
            }
        }

        // 백그라운드에서 TTS 파일 생성 시작
        await PreGenerateVoiceFiles();

        // OnPreparationComplete 이벤트 발행
        PubSubManager.Instance.Publish(PubSubEvent.OnPreparationComplete);

        // UniTask 완료 처리
        setupCompletionSource?.TrySetResult();

        Debug.Log("Teammate voice setup completed!");
    }

    /// <summary>
    /// 슬라이더 값(0~4)을 VoiceProperty enum으로 변환
    /// </summary>
    private VoiceProperty ConvertToVoiceProperty(int sliderValue)
    {
        switch (sliderValue)
        {
            case 0: return VoiceProperty.VeryLow;
            case 1: return VoiceProperty.Low;
            case 2: return VoiceProperty.Moderate;
            case 3: return VoiceProperty.High;
            case 4: return VoiceProperty.VeryHigh;
            default: return VoiceProperty.Moderate;
        }
    }

    /// <summary>
    /// 설정된 VoiceProfile을 동료들에게 적용
    /// </summary>
    public void ApplyVoiceProfilesToTeammates()
    {
        Debug.Log($"[TeammateVoiceSetupManager] ApplyVoiceProfilesToTeammates called. Profiles count: {selectedVoiceProfiles.Count}");

        foreach (var kvp in selectedVoiceProfiles)
        {
            string teammateName = kvp.Key;
            VoiceProfile profile = kvp.Value;

            Debug.Log($"[TeammateVoiceSetupManager] Applying profile to {teammateName}: Gender={profile.gender}, Pitch={profile.pitch}, Speed={profile.speed}");
            UnitManager.Instance.ApplyTeammateVoiceProfile(teammateName, profile);
        }
    }

    /// <summary>
    /// 선택된 VoiceProfile로 미리 TTS 파일 생성
    /// </summary>
    private async UniTask PreGenerateVoiceFiles()
    {
        // Preview 버튼 비활성화 (음성 생성 중 충돌 방지)
        ui?.SetPreviewButtonsInteractable(false);

        ui?.SetProgressText("Preparing voice generation...");

        int totalTeammates = selectedVoiceProfiles.Count;
        int completed = 0;

        foreach (var kvp in selectedVoiceProfiles)
        {
            string teammateName = kvp.Key;
            VoiceProfile profile = kvp.Value;

            ui?.SetProgressText($"Generating {teammateName}'s voice... ({completed + 1}/{totalTeammates})");

            // 진행률 콜백 추가
            var progressCallback = new System.Action<int, int>((current, total) =>
            {
                int percentage = (int)((float)current / total * 100);
                ui?.SetProgressText($"Generating {teammateName}'s voice... {current}/{total} ({percentage}%)");
            });

            // VoicePreGenerator를 통해 백그라운드 생성
            await VoicePreGenerator.Instance.GenerateVoiceForTeammate(teammateName, profile, progressCallback);

            completed++;
            ui?.SetProgressText($"{teammateName} completed! ({completed}/{totalTeammates})");
            await UniTask.Delay(300); // 완료 메시지 잠깐 보여주기

            Debug.Log($"{teammateName} voice file generation completed ({completed}/{totalTeammates})");
        }

        ui?.SetProgressText("All voice files generated!");

        // Preview 버튼 다시 활성화 (생성 완료)
        ui?.SetPreviewButtonsInteractable(true);

        // 잠깐 대기 (사용자에게 완료 메시지 보여주기)
        await UniTask.Delay(1000);
    }

    public VoiceProfile GetTeammateVoiceProfile(string teammateName)
    {
        return selectedVoiceProfiles.TryGetValue(teammateName, out var profile) ? profile : null;
    }
}

// 개별 동료의 음성 설정 UI 아이템 (Inspector에서 각 동료별로 할당)
[System.Serializable]
public class TeammateVoiceItem
{
    [Header("Teammate Info")] public string teammateName; // 동료 이름 (Lena, James, Sara)
    public VoiceGender gender; // 고정 Gender (Inspector에서 설정)
    public TextMeshProUGUI nameText; // 이름 표시

    [Header("Personality Sliders")] [Tooltip("활발함: 0(과묵함) ~ 4(수다스러움) → Speed")]
    public Slider livelinessSlider; // 활발함 슬라이더 (0~4)

    public TextMeshProUGUI livelinessValueText; // 활발함 수치 표시

    [Tooltip("분위기: 0(진지함) ~ 4(명랑함) → Pitch")]
    public Slider moodSlider; // 분위기 슬라이더 (0~4)

    public TextMeshProUGUI moodValueText; // 분위기 수치 표시

    [Header("Preview Button (Optional)")] public Button previewButton; // 미리듣기 버튼

    public void Initialize()
    {
        // 슬라이더 값 변경 시 텍스트 업데이트
        if (livelinessSlider != null)
        {
            livelinessSlider.onValueChanged.AddListener((value) =>
            {
                UpdateLivelinessText(value);
                PreventInvalidCombination();
            });
            livelinessSlider.minValue = 0;
            livelinessSlider.maxValue = 4;
            livelinessSlider.wholeNumbers = true;
            livelinessSlider.value = 2; // 기본값
            UpdateLivelinessText(2);
        }

        if (moodSlider != null)
        {
            moodSlider.onValueChanged.AddListener((value) =>
            {
                UpdateMoodText(value);
                PreventInvalidCombination();
            });
            moodSlider.minValue = 0;
            moodSlider.maxValue = 4;
            moodSlider.wholeNumbers = true;
            moodSlider.value = 2; // 기본값
            UpdateMoodText(2);
        }

        // 미리듣기 버튼 이벤트 연결
        if (previewButton != null)
        {
            previewButton.onClick.AddListener(OnPreviewButtonClicked);
        }
    }

    /// <summary>
    /// Female + VeryHigh(4) + VeryHigh(4) 조합 방지
    /// </summary>
    private void PreventInvalidCombination()
    {
        if (gender == VoiceGender.Female &&
            livelinessSlider != null && livelinessSlider.value == 4 &&
            moodSlider != null && moodSlider.value == 4)
        {
            // VeryHigh + VeryHigh 조합 시 Liveliness를 3(High)로 자동 조정
            livelinessSlider.value = 3;
            Debug.LogWarning($"[TeammateVoiceItem] Female + VeryHigh/VeryHigh combination is not supported. Liveliness adjusted to High.");
        }
    }

    // 미리듣기 버튼 클릭 시 미리 생성된 "Hello" 음성 재생
    private void OnPreviewButtonClicked()
    {
        VoiceProperty pitch = ConvertSliderToVoiceProperty(GetMoodValue());
        VoiceProperty speed = ConvertSliderToVoiceProperty(GetLivelinessValue());

        Debug.Log(
            $"[TeammateVoiceItem] Preview voice for {teammateName}: Gender={gender}, Pitch={pitch}, Speed={speed}");

        // Resources 폴더에서 미리 생성된 음성 로드 및 재생
        PlayPreviewVoiceFromResources(gender, pitch, speed);
    }

    /// <summary>
    /// Resources/PreviewVoices 폴더에서 미리 생성된 음성을 로드하여 재생
    /// </summary>
    private void PlayPreviewVoiceFromResources(VoiceGender gender, VoiceProperty pitch, VoiceProperty speed)
    {
        // 파일명 생성 (PreviewVoicePreGeneratorEditor와 동일한 형식)
        string genderStr = gender.ToApiString();
        string pitchStr = pitch.ToApiString();
        string speedStr = speed.ToApiString();
        string textSafe = "hello"; // "Hello".ToLower()

        string resourcePath = $"PreviewVoices/{genderStr}_{pitchStr}_{speedStr}_{textSafe}";

        // Resources 폴더에서 로드
        AudioClip clip = Resources.Load<AudioClip>(resourcePath);

        if (clip != null)
        {
            Debug.Log($"[TeammateVoiceItem] ✓ Loaded preview voice from Resources: {resourcePath}");

            // AudioSource를 통해 재생
            if (SparkTTSManager.Instance != null && SparkTTSManager.Instance.audioSource != null)
            {
                AudioSource audioSource = SparkTTSManager.Instance.audioSource;
                audioSource.Stop();
                audioSource.clip = clip;
                audioSource.Play();
            }
            else
            {
                Debug.LogWarning("[TeammateVoiceItem] SparkTTSManager or AudioSource is null!");
            }
        }
        else
        {
            Debug.LogWarning($"[TeammateVoiceItem] ✗ Preview voice not found in Resources: {resourcePath}");
            Debug.LogWarning($"[TeammateVoiceItem] Please generate preview voices using Tools > Voice > Preview Voice Pre-Generator");

            // Fallback: 실시간 생성 (미리 생성된 음성이 없을 경우)
            Debug.Log($"[TeammateVoiceItem] Fallback to real-time TTS generation...");
            SparkTTSManager.Instance.CreateStyleVoice("Hello", gender, pitch, speed);
        }
    }

    // 슬라이더 값을 VoiceProperty로 변환
    private VoiceProperty ConvertSliderToVoiceProperty(int value)
    {
        switch (value)
        {
            case 0: return VoiceProperty.VeryLow;
            case 1: return VoiceProperty.Low;
            case 2: return VoiceProperty.Moderate;
            case 3: return VoiceProperty.High;
            case 4: return VoiceProperty.VeryHigh;
            default: return VoiceProperty.Moderate;
        }
    }

    private void UpdateLivelinessText(float value)
    {
        if (livelinessValueText == null) return;

        string[] labels = { "Taciturn", "Quiet", "Moderate", "Lively", "Talkative" };
        int index = Mathf.Clamp((int)value, 0, 4);
        livelinessValueText.text = labels[index];
    }

    private void UpdateMoodText(float value)
    {
        if (moodValueText == null) return;

        string[] labels = { "Serious", "Calm", "Moderate", "Bright", "Cheerful" };
        int index = Mathf.Clamp((int)value, 0, 4);
        moodValueText.text = labels[index];
    }

    public VoiceGender GetSelectedGender()
    {
        return gender; // Inspector에서 설정한 고정값 반환
    }

    public int GetLivelinessValue()
    {
        if (livelinessSlider == null) return 2;
        return Mathf.Clamp((int)livelinessSlider.value, 0, 4);
    }

    public int GetMoodValue()
    {
        if (moodSlider == null) return 2;
        return Mathf.Clamp((int)moodSlider.value, 0, 4);
    }
}