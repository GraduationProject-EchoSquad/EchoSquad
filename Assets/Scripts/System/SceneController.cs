using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : Singleton<SceneController>
{
    public enum ESceneData
    {
        Start,
        Title,
        Main,
        UI
    }

    public Dictionary<ESceneData, string> SceneDict = new Dictionary<ESceneData, string>
    {
        { ESceneData.Start , "StartScene"},
        { ESceneData.Title , "TitleScene"},
        { ESceneData.Main , "MainScene"},
        { ESceneData.UI , "UIScene"}
    };

    private async UniTaskVoid Start()
    {
        LoadTitle();
    }

    public async UniTask LoadTitle()
    {
        await LoadSceneAsync(ESceneData.Title);
        await UIManager.Instance.Show<TitleUI>(UIManager.EUIData.Title);
    }

    public async UniTask LoadSceneAsync(ESceneData eSceneData, LoadSceneMode mode = LoadSceneMode.Additive)
    {
        await SceneManager.LoadSceneAsync(SceneDict[eSceneData], mode);
        
        // 방금 로드된 씬 가져오기
        Scene newScene = SceneManager.GetSceneByName(SceneDict[eSceneData]);

        // 새 씬을 Active Scene으로 설정
        SceneManager.SetActiveScene(newScene);

        // Skybox 적용
        DynamicGI.UpdateEnvironment();
    } 
}