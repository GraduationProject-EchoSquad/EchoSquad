using UnityEngine;
using LLMUnity;
using UnityEngine.UI;
using System.Collections.Generic;

namespace LLMUnitySamples
{
    [System.Serializable]
    public class ParsedCommand
    {
        public string target;
        public string action;
        public string location;
    }

    public static class Functions
    {
        static System.Random random = new System.Random();

        static readonly Dictionary<string, string[]> voiceLines = new()
        {
            { "AttackLeft",     new[] { "좌측 공격 돌입!", "좌측 적 제압 간다!", "공격 시작, 좌측으로 간다!" } },
            { "AttackRight",    new[] { "우측 전진!", "우측 밀어붙인다!", "공격 개시, 우측으로!" } },
            { "AttackCenter",   new[] { "중앙 돌격!", "중앙 적 진입, 바로 가!", "중앙 공격 개시!" } },
            { "AttackForward",  new[] { "전방으로 돌격!", "앞으로 밀어붙여!", "전면 공격 간다!" } },
            { "AttackBack",     new[] { "후방 정리 간다!", "뒤쪽 적 처리하러 가!", "뒤에서 온다, 내가 간다!" } },

            { "DefendLeft",     new[] { "좌측 방어 맡을게!", "적들 좌측이야, 내가 막아!", "좌측 고정!" } },
            { "DefendRight",    new[] { "우측 방어 간다!", "우측 적 많아, 내가 처리할게!", "우측은 내가 맡는다!" } },
            { "DefendCenter",   new[] { "중앙 수비 들어간다.", "중앙 위험해, 내가 막는다!", "중앙 방어 중!" } },
            { "DefendForward",  new[] { "전방 지킨다!", "앞쪽 방어는 나한테 맡겨!", "전면 방어 진입!" } },
            { "DefendBack",     new[] { "뒤는 내가 맡는다!", "후방 지켜야 돼, 내가 갈게!", "후방 지원 들어간다!" } },

            { "ScoutLeft",      new[] { "좌측 정찰 중...", "조용히 좌측 살펴볼게.", "정찰 개시, 좌측!" } },
            { "ScoutRight",     new[] { "우측 상황 파악 중!", "우측 확인 들어간다.", "정찰 시작, 우측!" } },
            { "ScoutCenter",    new[] { "중앙 시야 확보 중.", "중앙 감시 간다.", "중앙 정찰 진행 중." } },
            { "ScoutForward",   new[] { "전방 확인 중...", "앞쪽 정찰 중이야.", "앞에 뭐 있나 보고 올게!" } },
            { "ScoutBack",      new[] { "후방 정찰 중...", "뒤쪽 확인하고 올게.", "뒤에 뭐 있나 본다!" } },

            { "HealNone",       new[] { "힐 중이야, 엄호해줘!", "치료 들어간다. 잠깐만!", "회복 중... 부탁해!" } }
        };

        public static string GetVoiceLine(string functionName)
        {
            if (voiceLines.TryGetValue(functionName, out var lines))
                return lines[random.Next(lines.Length)];

            return $"[{functionName}] 명령 실행 중...";
        }
    }

    public class FunctionCalling : MonoBehaviour
    {
        public LLMCharacter llmCharacter;
        public InputField playerText;
        public Text AIText;

        void Start()
        {
            playerText.onSubmit.AddListener(onInputFieldSubmit);
            playerText.Select();
            llmCharacter.grammarString = ""; // 자유 입력 모드
        }

        string ConstructStructuredCommandPrompt(string playerMessage)
        {
            return $"명령: \"{playerMessage}\"\n\n" +
                   "아래 JSON 형식에 맞춰 분석하라. **다른 텍스트 없이 JSON만 출력하라.**\n\n" +
                   "{\n" +
                   "  \"target\": \"<AI 이름>\",\n" +
                   "  \"action\": \"Attack | Defend | Scout | Heal\",\n" +
                   "  \"location\": \"Left | Right | Center | Back | Forward | None\"\n" +
                   "}\n";
        }

        async void onInputFieldSubmit(string message)
        {
            playerText.interactable = false;
            llmCharacter.grammarString = "";

            string json = await llmCharacter.Chat(ConstructStructuredCommandPrompt(message));
            Debug.Log($"[LLM Raw JSON] {json}");

            // 코드블럭 제거
            json = json.Trim();
            if (json.StartsWith("```"))
            {
                int firstBrace = json.IndexOf('{');
                int lastBrace = json.LastIndexOf('}');
                if (firstBrace >= 0 && lastBrace >= 0)
                {
                    json = json.Substring(firstBrace, lastBrace - firstBrace + 1);
                }
            }

            ParsedCommand cmd;
            try
            {
                cmd = JsonUtility.FromJson<ParsedCommand>(json);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[JSON PARSE ERROR] {e.Message}\n[Raw JSON] {json}");
                AIText.text = "⚠️ 명령 해석 실패! (JSON 오류)";
                playerText.interactable = true;
                return;
            }

            string functionName = $"{cmd.action}{cmd.location}";
            Debug.Log($"[Parsed] target = {cmd.target}, action = {cmd.action}, location = {cmd.location}");

            // 대사 출력
            string result = Functions.GetVoiceLine(functionName);
            AIText.text = $"[To {cmd.target}] {result}";

            // AI 실행
            foreach (var ai in FindObjectsOfType<TeammateAI>())
            {
                if (ai.teammateName.ToLower() == cmd.target.ToLower())
                {
                    ai.ExecuteCommand(cmd.action, cmd.location);
                    break;
                }
            }

            playerText.interactable = true;
        }

        /// 외부에서 텍스트 명령 전달 가능 (Whisper에서 호출)
        public void SendCommandFromText(string message)
        {
            onInputFieldSubmit(message);
        }

        public void CancelRequests() => llmCharacter.CancelRequests();
        public void ExitGame() => Application.Quit();

        bool onValidateWarning = true;
        void OnValidate()
        {
            if (onValidateWarning && !llmCharacter.remote && llmCharacter.llm != null && llmCharacter.llm.model == "")
            {
                Debug.LogWarning($"Please select a model in the {llmCharacter.llm.gameObject.name} GameObject!");
                onValidateWarning = false;
            }
        }
    }
}
