using System;
using UnityEngine;
using LLMUnity;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
        public string mode;
        public string voice;

        public override string ToString()
        {
            return $"    \"destination\": \"{destination}\",\n" +
                   $"    \"follow_target\": \"{follow_target}\",\n" +
                   $"    \"engage_enemy\": \"{engage_enemy}\",\n" +
                   $"    \"support_target\": \"{support_target}\",\n" +
                   $"    \"support_type\": \"{support_type}\",\n" +
                   $"    \"mode\": \"{mode}\",\n" +
                   $"    \"voice\": \"{voice}\"";
        }
    }

    public enum AIActionEnum
    {
        Move,
        Combat,
        Support,
        Error
    }

    public class FunctionCalling : Singleton<FunctionCalling>
    {
        public LLMCharacter llmCharacter;
        public InputField playerText;
        public Text AIText;

        void Start()
        {
            if (playerText)
            {
                playerText.onSubmit.AddListener((message) => onInputFieldSubmit(message).Forget());
                playerText.Select();
            }

            if (llmCharacter)
            {
                llmCharacter.grammarString = ""; // 자유 입력 모드
            }
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
        
        string GetEnemyNames()
        {
            var validEnemies = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (EnemyType enemyType in Enum.GetValues(typeof(EnemyType)))
            {
                validEnemies.Add(enemyType.ToString());
            }

            string enemyNames = string.Join(" | ", validEnemies);
            return enemyNames;
        }
        private string PreprocessCommand(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return input;

            // 1. STT 오류 수정 (음성 인식 오인식)
            var sttCorrections = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                // 적 이름 오인식
                { "pose", "Boss" },
                { "post", "Boss" },
                { "boss", "Boss" },
                { "both", "Boss" },
                { "force", "Boss" },
                { "pause", "Boss" },

                // 동료 이름 오인식
                { "lina", "Lena" },
                { "lena", "Lena" },
                { "Elena", "Lena" },
                { "Rena", "Lena" },
                { "Sarah", "Sara" },

                // 장소 이름 오인식
                { "amo", "Ammo" },
                { "ammo", "Ammo" },
                { "toilet", "Toilet" },
                { "kitchen", "Kitchen" },
                { "a", "A" },
                { "b", "B" },
                { "c", "C" },
                { "be", "B" },

                // 기타 자주 틀리는 단어
                { "a tack", "attack" },
                { "fallow", "follow" },
            };

            // 2. 액션 동의어 통일 (LLM 전에 미리 처리)
            var actionCorrections = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "attack", "combat" },
                { "engage", "combat" },
                { "fight", "combat" },
                { "kill", "combat" },
                { "eliminate", "combat" },

                { "follow", "move" },
                { "go", "move" },

                { "heal", "support" },
                { "shield", "support" },
                { "protect", "support" },
            };

            // 모든 수정 사항 병합
            var allCorrections = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var pair in sttCorrections)
                allCorrections[pair.Key] = pair.Value;
            foreach (var pair in actionCorrections)
                allCorrections[pair.Key] = pair.Value;

            // 단어 경계를 고려한 치환
            foreach (var pair in allCorrections)
            {
                string pattern = $@"\b{System.Text.RegularExpressions.Regex.Escape(pair.Key)}\b";
                input = System.Text.RegularExpressions.Regex.Replace(
                    input,
                    pattern,
                    pair.Value,
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase
                );
            }

            return input;
        }

        string ConstructStructuredCommandPrompt(string playerMessage, List<TeammateAI> AIList, string districtsName)
        {
            string actions = FormatEnumOptions<AIActionEnum>();
            string unitNames = GetAINames(AIList);
            string enemyNames = GetEnemyNames();

            return "### Core Rules\n" +
                   "You are a military command parser. Parse natural language into structured commands.\n\n" +

                   "**OUTPUT FORMAT**: Single JSON object only. No explanations. MUST include ALL fields:\n" +
                   "```json\n" +
                   "{\n" +
                   "  \"command_units\": [\"UnitName\"],  // REQUIRED: Array of unit names \n" +
                   "  \"action\": \"Move\",               // REQUIRED: Action type\n" +
                   "  \"parameters\": {                  // REQUIRED: Object with parameters\n" +
                   "    \"destination\": \"value\",      // Optional parameter\n" +
                   "    \"voice\": \"Roger+following\"   // REQUIRED: Voice response\n" +
                   "  }\n" +
                   "}\n" +
                   "```\n\n" +

                   $"**ACTIONS** - Use exactly one: {actions}\n" +
                   "- Move: go, walk, run, advance, retreat, follow, come, travel, proceed\n" +
                   "- Combat: attack, fight, kill, eliminate, engage, destroy, take out, deal with [enemy]\n" +
                   "- Support: heal, help, cover, aid\n" +
                   "- Error: invalid/impossible commands\n\n" +

                   "**ENTITY RECOGNITION**\n" +
                   $"- Units: {unitNames} (case-insensitive)\n" +
                   "- 'me/myself/I' → 'Player'\n" +
                   $"- 'all/everyone/everybody' → [{string.Join(",", AIList.Select(ai => $"\"{ai.teammateName}\""))}]\n" +
                   $"- Locations: {districtsName}, Left, Right, Forward, Back\n" +
                   $"- Enemies: {enemyNames}\n\n" +

                   "**PARAMETERS RULE (CRITICAL!)**\n" +
                   $"- If target is a PERSON ({unitNames}, Player) → use `follow_target` or `support_target`\n" +
                   $"- If target is a LOCATION ({districtsName}, Left, Right, Forward, Back) → use `destination`\n" +
                   "- NEVER use both `destination` and `follow_target` at the same time!\n" +
                   "- Examples:\n" +
                   "  - 'Lena move me' → follow_target: 'Player' (NOT destination!)\n" +
                   "  - 'James follow Player' → follow_target: 'Player'\n" +
                   "  - 'Sara go Kitchen' → destination: 'Kitchen'\n\n" +

                   "**CRITICAL: 'me' PATTERN HANDLING**\n" +
                   "- When command contains 'me/myself/I', it means Player is the TARGET, not the executor\n" +
                   "- The UNIT NAME in the command is ALWAYS the executor\n" +
                   "- Examples (all mean the same - unit executes, Player is target):\n" +
                   "  - 'Lena follow me' → command_units: ['Lena'], follow_target: 'Player'\n" +
                   "  - 'follow me Lena' → command_units: ['Lena'], follow_target: 'Player'\n" +
                   "  - 'James help me' → command_units: ['James'], support_target: 'Player'\n" +
                   "  - 'help me James' → command_units: ['James'], support_target: 'Player'\n" +
                   "- RULE: Find the unit name, make it executor. 'me' = Player = target.\n\n" +

                   "**ERROR CONDITIONS**\n" +
                   "- Unknown units (John, Bob, etc.)\n" +
                   "- Unknown locations (Mars, outside, etc.)\n" +
                   "- Unknown enemies (elephant, friendly, etc.)\n" +
                   "- Invalid actions (dance, sing, etc.)\n" +
                   "- Multi-step commands (X and Y, X but Y)\n" +
                   "- Vague commands without clear action\n\n" +

                   VoiceModules.GetVoiceModulesPrompt() + "\n" +

                   $"**Locations/Districts (if moving to specific place)**:\n" +
                   $"\"{districtsName.Replace(" | ", "\", \"")}\"\n\n" +

                   "**CRITICAL RULES**:\n" +
                   "1. ONLY use exact phrases from the lists above\n" +
                   "2. Combine modules with '+' symbol: \"Got it!+moving+left\"\n" +
                   "3. Match personality: Taciturn=short (1 module), Cheerful=longer (2-3 modules)\n" +
                   "4. District/Location names can be used in combinations: \"Sure thing!+moving+Kitchen\"\n" +
                   "5. Examples:\n" +
                   "   - Taciturn: \"Roger\" OR \"Got it+moving+left\"\n" +
                   "   - Cheerful: \"Absolutely!+moving+left\" OR \"Let's do this!+engaging+enemy\"\n" +
                   "   - To district: \"Sure thing!+moving\" OR \"Got it!+moving+Kitchen\"\n\n" +

                   "### Pattern Examples (WITH VOICE MODULES)\n" +
                   "**Movement Patterns:**\n" +
                   $"- '{(AIList.Count > 0 ? AIList[0].teammateName : "Lena")} go Kitchen' (Cheerful personality)\n" +
                   $"  → {{\"command_units\":[\"{(AIList.Count > 0 ? AIList[0].teammateName : "Lena")}\"],\"action\":\"Move\",\"parameters\":{{\"destination\":\"Kitchen\",\"voice\":\"Sure thing!+moving\"}}}}\n" +
                   $"- 'James follow me' (Taciturn personality)\n" +
                   "  → {\"command_units\":[\"James\"],\"action\":\"Move\",\"parameters\":{\"follow_target\":\"Player\",\"voice\":\"Roger+following\"}}\n" +
                   $"- 'Sara go left' (Moderate personality)\n" +
                   "  → {\"command_units\":[\"Sara\"],\"action\":\"Move\",\"parameters\":{\"destination\":\"Left\",\"voice\":\"Got it!+moving+left\"}}\n\n" +

                   "**Combat Patterns:**\n" +
                   "- 'attack the zombie' (Cheerful)\n" +
                   "  → {\"command_units\":[context],\"action\":\"Combat\",\"parameters\":{\"engage_enemy\":\"Zombie\",\"voice\":\"Let's do this!+engaging+zombie\"}}\n" +
                   "- 'kill that alien' (Taciturn)\n" +
                   "  → {\"command_units\":[context],\"action\":\"Combat\",\"parameters\":{\"engage_enemy\":\"Alien\",\"voice\":\"Roger+attacking+alien\"}}\n\n" +

                   "**Support Patterns:**\n" +
                   "- 'heal Sara' (Moderate)\n" +
                   "  → {\"command_units\":[context],\"action\":\"Support\",\"parameters\":{\"support_target\":\"Sara\",\"support_type\":\"Heal\",\"voice\":\"Understood!+helping+Sara\"}}\n" +
                   "- 'help me James' (Cheerful) → James executes, Player receives\n" +
                   "  → {\"command_units\":[\"James\"],\"action\":\"Support\",\"parameters\":{\"support_target\":\"Player\",\"voice\":\"On it!+Here I come\"}}\n\n" +

                   "**Error Patterns:**\n" +
                   "- Multi-step: 'go Kitchen and watch'\n" +
                   "  → {\"command_units\":null,\"action\":\"Error\",\"parameters\":{\"voice\":\"I got this\"}}\n" +
                   "- Invalid: 'go to Mars' / 'John move'\n" +
                   "  → {\"command_units\":null,\"action\":\"Error\",\"parameters\":{\"voice\":\"Understood\"}}\n\n" +

                   "### Command to Process\n" +
                   $"Command: {playerMessage}";
        }

        private ParsedCommand ParseAndCorrectCommand(string json)
        {
            // LLM이 잘못된 Action으로 해석했을 때 매칭이 되는 Action으로 바꿔주는 작업
            var actionSynonyms = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                  // Combat 관련
                  { "Attack", "Combat" },     // Attack → Combat
                  { "Engage", "Combat" },     // Engage → Combat
                  { "Fight", "Combat" },      // Fight → Combat
                  { "Kill", "Combat" },       // Kill → Combat
                  { "Eliminate", "Combat" },  // Eliminate → Combat

                  // Move 관련
                  { "Follow", "Move" },       // Follow → Move
                  { "Go", "Move" },           // Go → Move

                  // Support 관련
                  { "Heal", "Support" },      // Heal → Support
                  { "Shield", "Support" },    // Shield → Support
                  { "Protect", "Support" },   // Protect → Support
            };

            if (string.IsNullOrWhiteSpace(json)) return new ParsedCommand();

            // Debug: Log raw JSON before cleanup
            Debug.Log($"[RAW JSON BEFORE CLEANUP] {json}");

            // Enhanced JSON cleanup
            json = CleanupJsonString(json);

            // Debug: Log cleaned JSON
            Debug.Log($"[CLEANED JSON] {json}");

            try
            {
                JObject tempCmd = JObject.Parse(json);

                if (tempCmd["action"] != null)
                {
                    string actionValue = tempCmd["action"].ToString();
                    if (actionSynonyms.TryGetValue(actionValue, out string correctAction))
                    {
                        tempCmd["action"] = correctAction;
                    }
                }
                var parsedCmd = tempCmd.ToObject<ParsedCommand>();

                // Post-processing validation
                return ValidateAndCorrectCommand(parsedCmd);
            }
            catch (JsonException ex)
            {
                Debug.LogError($"[JSON PARSE ERROR] {ex.Message}\n[Cleaned JSON] {json}");
                return new ParsedCommand(); // Return empty command on parse failure
            }
        }

        private string CleanupJsonString(string json)
        {
            if (string.IsNullOrWhiteSpace(json)) return "{}";

            json = json.Trim();

            // Remove code blocks
            if (json.StartsWith("```json"))
            {
                json = json.Substring(7);
            }
            else if (json.StartsWith("```"))
            {
                json = json.Substring(3);
            }

            if (json.EndsWith("```"))
            {
                json = json.Substring(0, json.Length - 3);
            }

            json = json.Trim();

            // Find the first { and last } to extract only the JSON object
            int firstBrace = json.IndexOf('{');
            int lastBrace = json.LastIndexOf('}');

            if (firstBrace >= 0 && lastBrace >= 0 && lastBrace > firstBrace)
            {
                json = json.Substring(firstBrace, lastBrace - firstBrace + 1);
            }

            // Remove any extra text after the JSON object
            // Find the complete JSON object by counting braces
            int braceCount = 0;
            int endIndex = -1;

            for (int i = 0; i < json.Length; i++)
            {
                if (json[i] == '{')
                {
                    braceCount++;
                }
                else if (json[i] == '}')
                {
                    braceCount--;
                    if (braceCount == 0)
                    {
                        endIndex = i;
                        break;
                    }
                }
            }

            if (endIndex >= 0)
            {
                json = json.Substring(0, endIndex + 1);
            }

            // Fix unquoted values first
            json = FixUnquotedValues(json);

            // Simple brace balance fix
            while (true)
            {
                int openBraces = json.Count(c => c == '{');
                int closeBraces = json.Count(c => c == '}');

                if (openBraces == closeBraces) break;

                if (openBraces > closeBraces)
                {
                    json += "}";
                }
                else
                {
                    // Too many closing braces - remove from end
                    int lastBraceIndex = json.LastIndexOf('}');
                    if (lastBraceIndex >= 0)
                        json = json.Remove(lastBraceIndex, 1);
                    else
                        break;
                }

                // Safety check to prevent infinite loop
                if (json.Length > 1000) break;
            }

            return json.Trim();
        }

        private string FixUnquotedValues(string json)
        {
            // Fix broken parameters object - look for unterminated string at position 60
            // Pattern: "parameters":{"} where there's a quote but no proper key:value
            json = System.Text.RegularExpressions.Regex.Replace(json, @"""parameters"":\{""(?=[}\]])", "\"parameters\":{}");

            // Fix parameters with incomplete content
            json = System.Text.RegularExpressions.Regex.Replace(json, @"""parameters"":\{""[^""]*""?\}?(?=\})", "\"parameters\":{}");
            json = System.Text.RegularExpressions.Regex.Replace(json, @"""parameters"":\{""""?\}", "\"parameters\":{}");

            // Fix unterminated strings - add missing quotes before delimiters
            json = System.Text.RegularExpressions.Regex.Replace(json, @":""([^"",}\]]*?)(?=[,}\]])", ":\"$1\"");
            json = System.Text.RegularExpressions.Regex.Replace(json, @":""([^""]*?)$", ":\"$1\"");

            // Fix common unquoted values
            json = System.Text.RegularExpressions.Regex.Replace(json, @"\[context\]", "[\"context\"]");
            json = System.Text.RegularExpressions.Regex.Replace(json, @":context([,\]}])", ":\"context\"$1");
            json = System.Text.RegularExpressions.Regex.Replace(json, @":null([,\]}])", ":null$1"); // keep null as is
            json = System.Text.RegularExpressions.Regex.Replace(json, @":([a-zA-Z_][a-zA-Z0-9_]*)([,\]}])", ":\"$1\"$2");

            // Fix malformed parameters object
            json = System.Text.RegularExpressions.Regex.Replace(json, @"""parameters"":\{""""\}", "\"parameters\":{}");
            json = System.Text.RegularExpressions.Regex.Replace(json, @"""parameters"":\{\s*\}", "\"parameters\":{}");

            // Convert arrays to strings for parameters that should be strings
            // support_target array -> string conversion
            json = System.Text.RegularExpressions.Regex.Replace(json,
                @"""support_target"":\s*\[\s*""([^""]+)""\s*(?:,\s*""[^""]+"")*\s*\]",
                "\"support_target\":\"$1\"");

            // engage_enemy array -> string conversion
            json = System.Text.RegularExpressions.Regex.Replace(json,
                @"""engage_enemy"":\s*\[\s*""([^""]+)""\s*(?:,\s*""[^""]+"")*\s*\]",
                "\"engage_enemy\":\"$1\"");

            // destination array -> string conversion
            json = System.Text.RegularExpressions.Regex.Replace(json,
                @"""destination"":\s*\[\s*""([^""]+)""\s*(?:,\s*""[^""]+"")*\s*\]",
                "\"destination\":\"$1\"");

            // follow_target array -> string conversion
            json = System.Text.RegularExpressions.Regex.Replace(json,
                @"""follow_target"":\s*\[\s*""([^""]+)""\s*(?:,\s*""[^""]+"")*\s*\]",
                "\"follow_target\":\"$1\"");

            // Fix "null" as string instead of null value
            json = System.Text.RegularExpressions.Regex.Replace(json, @"""command_units"":""null""", "\"command_units\":null");

            return json;
        }

        private ParsedCommand ValidateAndCorrectCommand(ParsedCommand cmd)
        {
            if (cmd == null) return new ParsedCommand();

            Debug.Log($"[VALIDATION START] Action: {cmd.action}, Units: {string.Join(",", cmd.command_units ?? new List<string>())}, Enemy: {cmd.Parameters?.engage_enemy}");

            var aiList = UnitManager.Instance.teammateUnitDict.Values
                .Select(tc => tc.GetComponent<TeammateAI>())
                .Where(ai => ai != null)
                .ToList();

            var validUnits = new HashSet<string>(aiList.Select(ai => ai.teammateName), StringComparer.OrdinalIgnoreCase);

            // Dynamically populate the validEnemies set from the EnemyType enum
            var validEnemies = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (EnemyType enemyType in Enum.GetValues(typeof(EnemyType)))
            {
                validEnemies.Add(enemyType.ToString());
            }

            var validLocations = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "Left", "Right", "Forward", "Back", "Center" };

            MapManager mapManager = MapManager.Instance;
            if (mapManager != null)
            {
                foreach (var districtName in mapManager.districtDict.Keys)
                {
                    validLocations.Add(districtName);
                }
            }

            if (cmd.command_units != null)
            {
                foreach (var unit in cmd.command_units)
                {
                    if (!string.IsNullOrEmpty(unit) && !validUnits.Contains(unit))
                    {
                        Debug.Log($"[VALIDATION] Invalid unit detected: {unit} -> Setting action to Error");
                        cmd.action = AIActionEnum.Error;
                        return cmd;
                    }
                }
            }

            if (cmd.action == AIActionEnum.Combat && cmd.Parameters != null && !string.IsNullOrEmpty(cmd.Parameters.engage_enemy))
            {
                if (!validEnemies.Contains(cmd.Parameters.engage_enemy))
                {
                    Debug.Log($"[VALIDATION] Invalid enemy detected: {cmd.Parameters.engage_enemy} -> Setting action to Error");
                    cmd.action = AIActionEnum.Error;
                    return cmd;
                }
            }

            if (cmd.Parameters != null && !string.IsNullOrEmpty(cmd.Parameters.destination))
            {
                if (!validLocations.Contains(cmd.Parameters.destination) && !validUnits.Contains(cmd.Parameters.destination))
                {
                    Debug.Log($"[VALIDATION] Invalid destination detected: {cmd.Parameters.destination} -> Setting action to Error");
                    cmd.action = AIActionEnum.Error;
                    return cmd;
                }
            }

            if (cmd.action == AIActionEnum.Support && cmd.Parameters != null && !string.IsNullOrEmpty(cmd.Parameters.support_target))
            {
                if (!validUnits.Contains(cmd.Parameters.support_target) && !cmd.Parameters.support_target.Equals("Player", StringComparison.OrdinalIgnoreCase))
                {
                    Debug.Log($"[VALIDATION] Invalid support target detected: {cmd.Parameters.support_target} -> Setting action to Error");
                    cmd.action = AIActionEnum.Error;
                    return cmd;
                }
            }

            return cmd;
        }
        
        /// <summary>
        /// 플레이어의 입력 필드 제출 시 호출되는 비동기 메서드입니다.
        /// LLM에 명령을 보내고, 응답을 파싱하여 게임 내 AI 유닛에 명령을 실행합니다.
        /// </summary>
        /// <param name="message">플레이어가 입력한 명령어 텍스트입니다.</param>
        async UniTaskVoid onInputFieldSubmit(string message)
        {
            if (playerText)
            {
                playerText.interactable = false;
            }

            if (llmCharacter)
            {
                llmCharacter.grammarString = "";
            }

            // 원본 메시지 저장 (Whisper 원문 - 채팅 표시용)
            string originalMessage = message;

            // STT 오류 및 동의어 전처리 (LLM 전송용)
            message = PreprocessCommand(message);
            Debug.Log($"[ORIGINAL] {originalMessage}");
            Debug.Log($"[PREPROCESSED] {message}");

            // 플레이어가 입력한 명령어를 채팅에 추가 (Whisper 원문 사용!)
            ChatManager.Instance.AddMessage("Player", originalMessage);

            List<TeammateAI> aiList = UnitManager.Instance.teammateUnitDict.Values
                .Select(tc => tc.GetComponent<TeammateAI>())
                .Where(ai => ai != null)
                .ToList();

            MapManager mapManager = MapManager.Instance;
            string districtsName = mapManager.GetDistrictsName();

            // Clear chat history to prevent context accumulation across commands
            llmCharacter.ClearChat();

            float t0 = Time.realtimeSinceStartup;

            string json = await llmCharacter.Chat(ConstructStructuredCommandPrompt(message, aiList, districtsName));

            float elapsedMs = (Time.realtimeSinceStartup - t0) * 1000f;
            Debug.Log($"[LLM Response Time: {elapsedMs:F1} ms]");

            ParsedCommand cmd;
            try
            {
                cmd = ParseAndCorrectCommand(json);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[JSON PARSE ERROR] {e.Message}\n[Raw JSON] {json}");
                if(AIText)
                    AIText.text = "⚠️ 명령 해석 실패! (JSON 오류)";
                if(playerText)
                    playerText.interactable = true;
                return;
            }

            DoCommand(cmd, aiList);
        }
        
        /// <summary>
        /// 파싱된 명령(ParsedCommand)을 기반으로 게임 내 AI 유닛에 실제 명령을 실행하는 메서드입니다.
        /// </summary>
        /// <param name="cmd">LLM으로부터 파싱된 명령 객체입니다.</param>
        /// <param name="aiList">현재 활성화된 모든 팀원 AI 유닛 목록입니다.</param>
        public void DoCommand(ParsedCommand cmd, List<TeammateAI> aiList)
        {
            // Null-safety check for command_units (Error actions may have null units)
            if (cmd == null || cmd.command_units == null)
            {
                Debug.LogWarning($"[DoCommand] Invalid command or null command_units. Action: {cmd?.action}");
                if(playerText)
                    playerText.interactable = true;
                return;
            }

            string unitsText = string.Join(", ", cmd.command_units);
            string functionName = $"{cmd.action}{cmd.Parameters}";
            Debug.Log($"[Parsed] target = {unitsText},\n action = {cmd.action},\n params = {cmd.Parameters}");

            // DEBUG: Check voice field value
            string voiceValue = cmd.Parameters?.voice;
            Debug.Log($"[DEBUG VOICE] Voice field value: '{voiceValue}' | Is null: {voiceValue == null} | Is empty: {string.IsNullOrEmpty(voiceValue)} | Is whitespace: {string.IsNullOrWhiteSpace(voiceValue)}");

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
            if(playerText)
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
