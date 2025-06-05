using System;
using UnityEngine;
using LLMUnity;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine.Serialization;

namespace LLMUnitySamples
{
    [System.Serializable]
    public class ParsedCommand
    {
        public List<string> command_units;
        public AIActionEnum action;
        public Parameters Parameters;
    }

    [System.Serializable]
    public class Parameters
    {
        public string destination;
        public string follow_target;
        public string engage_enemy;
        public string support_target;
        public string support_type;
        public string area;
        public string mode;

        public override string ToString()
        {
            return $"    \"destination\": \"{destination}\",\n" +
                   $"    \"follow_target\": \"{follow_target}\",\n\n" +
                   $"    \"engage_enemy\": \"{engage_enemy}\",\n\n" +
                   $"    \"support_target\": \"{support_target}\",\n" +
                   $"    \"support_type\": \"{support_type}\",\n\n" +
                   $"    \"area\": \"{area}\",\n" +
                   $"    \"mode\": \"{mode}\"\n";
        }
    }

    public enum AIActionEnum
    {
        Move,
        Combat,
        Support,
        Scout
    }

    public static class Functions
    {
        static System.Random random = new System.Random();

        static readonly Dictionary<string, string[]> voiceLines = new()
        {
            { "AttackLeft", new[] { "좌측 공격 돌입!", "좌측 적 제압 간다!", "공격 시작, 좌측으로 간다!" } },
            { "AttackRight", new[] { "우측 전진!", "우측 밀어붙인다!", "공격 개시, 우측으로!" } },
            { "AttackCenter", new[] { "중앙 돌격!", "중앙 적 진입, 바로 가!", "중앙 공격 개시!" } },
            { "AttackForward", new[] { "전방으로 돌격!", "앞으로 밀어붙여!", "전면 공격 간다!" } },
            { "AttackBack", new[] { "후방 정리 간다!", "뒤쪽 적 처리하러 가!", "뒤에서 온다, 내가 간다!" } },

            { "DefendLeft", new[] { "좌측 방어 맡을게!", "적들 좌측이야, 내가 막아!", "좌측 고정!" } },
            { "DefendRight", new[] { "우측 방어 간다!", "우측 적 많아, 내가 처리할게!", "우측은 내가 맡는다!" } },
            { "DefendCenter", new[] { "중앙 수비 들어간다.", "중앙 위험해, 내가 막는다!", "중앙 방어 중!" } },
            { "DefendForward", new[] { "전방 지킨다!", "앞쪽 방어는 나한테 맡겨!", "전면 방어 진입!" } },
            { "DefendBack", new[] { "뒤는 내가 맡는다!", "후방 지켜야 돼, 내가 갈게!", "후방 지원 들어간다!" } },

            { "ScoutLeft", new[] { "좌측 정찰 중...", "조용히 좌측 살펴볼게.", "정찰 개시, 좌측!" } },
            { "ScoutRight", new[] { "우측 상황 파악 중!", "우측 확인 들어간다.", "정찰 시작, 우측!" } },
            { "ScoutCenter", new[] { "중앙 시야 확보 중.", "중앙 감시 간다.", "중앙 정찰 진행 중." } },
            { "ScoutForward", new[] { "전방 확인 중...", "앞쪽 정찰 중이야.", "앞에 뭐 있나 보고 올게!" } },
            { "ScoutBack", new[] { "후방 정찰 중...", "뒤쪽 확인하고 올게.", "뒤에 뭐 있나 본다!" } },

            { "HealNone", new[] { "힐 중이야, 엄호해줘!", "치료 들어간다. 잠깐만!", "회복 중... 부탁해!" } }
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

        //TODO 다른곳에 정리
        string FormatEnumOptions<T>() where T : Enum
        {
            return string.Join(" | ", Enum.GetNames(typeof(T)));
        }

        string GetAINames(List<TeammateAI> aiList)
        {
            HashSet<string> uniqueNames = new HashSet<string>();

            foreach (var ai in aiList)
            {
                uniqueNames.Add(ai.teammateName);
                //uniqueNames.Add(ai.teammateNameKorean);
            }

            string AINames = string.Join(" | ", uniqueNames);
            return AINames;
        }

        string ConstructStructuredCommandPrompt(string playerMessage, List<TeammateAI> AIList, string districtsName)
        {
            string actions = FormatEnumOptions<AIActionEnum>();
            string unitNames = GetAINames(AIList);

            return $"Command: \"{playerMessage}\"\n\n" +
                   "Analyze the command and convert it into the following JSON structure.\n\n" +
                   "You MUST strictly follow these rules:\n\n" +
                   "1. Output ONLY valid JSON. Do NOT include explanations or any other text.\n\n" +
                   "2. For the \"action\" field, use ONLY ONE enum value from:\n" +
                   $"<One of: {actions}>\n\n" +
                   "3. For the \"command_units\" field, output a JSON array of units involved in the command. Use ONLY values from:\n" +
                   $"<One or more of: {unitNames}>\n\n" +
                   "4. If the command includes 'follow me', 'come with me', or similar, map it to \"follow_target\": \"Player\"\n\n" +
                   "5. If the command involves multiple actions or units, merge them if compatible, or ignore secondary commands.\n\n" +
                   "6. For each parameter, use only the allowed values. If not used, set to null.\n\n" +
                   $"   - destination      → Left | Right | Center | Back | Forward | {unitNames} | {districtsName}\n" +
                   $"   - follow_target    → {unitNames} | Player | null\n" +
                   "   - engage_enemy     → Nearest | Sniper | Tank | null\n" +
                   "   - support_target   → Alpha | WoundedUnit | null\n" +
                   "   - support_type     → Heal | Shield | null\n" +
                   "   - area             → Left | Right | EnemyBase | null\n" +
                   "   - mode             → Stealth | Quick | null\n\n" +
                   "7. Example output:\n" +
                   "{\n" +
                   "  \"command_units\": [\"James\"],\n" +
                   "  \"action\": \"Move\",\n" +
                   "  \"parameters\": {\n" +
                   "    \"destination\": \"Left\",\n" +
                   "    \"follow_target\": null,\n" +
                   "    \"engage_enemy\": null,\n" +
                   "    \"support_target\": null,\n" +
                   "    \"support_type\": null,\n" +
                   "    \"area\": null,\n" +
                   "    \"mode\": null\n" +
                   "  }\n" +
                   "}\n";
        }

        async void onInputFieldSubmit(string message)
        {
            playerText.interactable = false;
            llmCharacter.grammarString = "";
            
            List<TeammateAI> aiList = UnitManager.Instance.allayUnitDict.Values
                .Select(tc => tc.GetComponent<TeammateAI>())
                .Where(ai => ai != null)
                .ToList();
            
            //TODO, 할당한 곳에서 가져와야함. MapManager?
            MapManager mapManager = MapManager.Instance;
            string districtsName = mapManager.GetDistrictsName();

            // 시간 측정 시작
            float t0 = Time.realtimeSinceStartup;

            string json = await llmCharacter.Chat(ConstructStructuredCommandPrompt(message, aiList, districtsName));
            Debug.Log($"[LLM Raw JSON] {json}");

            // 측정 종료
            float elapsedMs = (Time.realtimeSinceStartup - t0) * 1000f;
            Debug.Log($"LLM 응답 시간: {elapsedMs:F1} ms");

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

            // 2) 중괄호 짝 맞추기
            int openCount = json.Count(c => c == '{');
            int closeCount = json.Count(c => c == '}');
            while (closeCount < openCount)
            {
                json += "}";
                closeCount++;
            }

            ParsedCommand cmd;
            try
            {
                cmd = JsonConvert.DeserializeObject<ParsedCommand>(json);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[JSON PARSE ERROR] {e.Message}\n[Raw JSON] {json}");
                AIText.text = "⚠️ 명령 해석 실패! (JSON 오류)";
                playerText.interactable = true;
                return;
            }

            string unitsText = string.Join(", ", cmd.command_units);
            string functionName = $"{cmd.action}{cmd.Parameters}";
            Debug.Log($"[Parsed] target = {unitsText},\n action = {cmd.action},\n params = {cmd.Parameters}");

            // 대사 출력
            string result = Functions.GetVoiceLine(functionName);
            AIText.text = $"[To {unitsText}] {result}";

            //행동 주체들
            // AI 실행
            foreach (var ai in aiList)
            {
                if (cmd.command_units.Contains(ai.teammateName))
                {
                    ai.ExecuteCommand(cmd.action, cmd.Parameters);
                }
                else if (cmd.command_units.Contains(ai.teammateNameKorean))
                {
                    ai.ExecuteCommand(cmd.action, cmd.Parameters);
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