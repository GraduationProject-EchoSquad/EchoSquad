using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DebugManager))]
public class DebugManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // 기본 인스펙터 UI를 그립니다.
        DrawDefaultInspector();

        // 타겟 스크립트(DebugManager)의 인스턴스를 가져옵니다.
        DebugManager debugManager = (DebugManager)target;

        // 버튼들을 그리기 위한 공간을 만듭니다.
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Debug Actions", EditorStyles.boldLabel);

        // "플레이어 즉사" 버튼
        if (GUILayout.Button("Kill Player"))
        {
            debugManager.KillPlayer();
        }
        
        if (GUILayout.Button("Kill Player Immediately"))
        {
            debugManager.KillPlayerImmediately();
        }

        // "다음 웨이브로" 버튼
        if (GUILayout.Button("Skip to Next Wave"))
        {
            debugManager.SkipToNextWave();
        }
    }
}