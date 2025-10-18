using System;
using UnityEngine;
using LLMUnity;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
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
        public string area;
        public string scout_from;
        public string scout_to;
        public string mode;
        public string voice;

        public override string ToString()
        {
            return $"    \"destination\": \"{destination}\",\n" +
                   $"    \"follow_target\": \"{follow_target}\",\n" +
                   $"    \"engage_enemy\": \"{engage_enemy}\",\n" +
                   $"    \"support_target\": \"{support_target}\",\n" +
                   $"    \"support_type\": \"{support_type}\",\n" +
                   $"    \"area\": \"{area}\",\n" +
                   $"    \"scout_from\": \"{scout_from}\",\n" +
                   $"    \"scout_to\": \"{scout_to}\",\n" +
                   $"    \"mode\": \"{mode}\",\n" +
                   $"    \"voice\": \"{voice}\"";
        }
    }

    public enum AIActionEnum
    {
        Move,
        Combat,
        Support,
        Scout,
        Error
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

            { "HealNone", new[] { "힐 중이야, 엄호해줘!", "치료 들어간다. 잠깐만!", "회복 중... 부탁해!" } },

            // Error 케이스들
            { "Error", new[] { "명령을 이해할 수 없어!", "뭔 소리야? 다시 말해봐.", "그런 명령은 실행할 수 없어!" } }
        };

        public static string GetVoiceLine(string functionName)
        {
            if (voiceLines.TryGetValue(functionName, out var lines))
                return lines[random.Next(lines.Length)];

            return $"[{functionName}] 명령 실행 중...";
        }
    }

    public class FunctionCalling : Singleton<FunctionCalling>
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

            return "### Core Rules\n" +
                   "You are a military command parser. Parse natural language into structured commands.\n\n" +

                   "**OUTPUT FORMAT**: Single JSON object only. No explanations.\n\n" +

                   $"**ACTIONS** - Use exactly one: {actions}\n" +
                   "- Move: go, walk, run, advance, retreat, follow, come, travel, proceed\n" +
                   "- Combat: attack, fight, kill, eliminate, engage, destroy, take out, deal with [enemy]\n" +
                   "- Support: heal, help, cover, aid\n" +
                   "- Scout: scout, reconnaissance, patrol, watch, survey, investigate, check out\n" +
                   "- Error: invalid/impossible commands\n\n" +

                   "**ENTITY RECOGNITION**\n" +
                   $"- Units: {unitNames} (case-insensitive)\n" +
                   "- 'me/myself/I' → 'Player'\n" +
                   $"- 'all/everyone/everybody' → [{string.Join(",", AIList.Select(ai => $"\"{ai.teammateName}\""))}]\n" +
                   $"- Locations: {districtsName}, Left, Right, Forward, Back\n" +
                   "- Enemies: Zombie, Alien, Lion, Boss\n\n" +

                   "**CRITICAL: 'me' PATTERN HANDLING**\n" +
                   "- 'help me James' → command_units: ['James'], support_target: 'Player'\n" +
                   "- 'heal me Sara' → command_units: ['Sara'], support_target: 'Player'\n" +
                   "- 'I need X from Y' → command_units: ['Y'], target: 'Player'\n" +
                   "- RULE: The unit AFTER 'me' or 'from' executes the command\n\n" +

                   "**ERROR CONDITIONS**\n" +
                   "- Unknown units (John, Bob, etc.)\n" +
                   "- Unknown locations (Mars, outside, etc.)\n" +
                   "- Unknown enemies (elephant, friendly, etc.)\n" +
                   "- Invalid actions (dance, sing, etc.)\n" +
                   "- Multi-step commands (X and Y, X but Y)\n" +
                   "- Vague commands without clear action\n\n" +

                   "### Parameters\n" +
                   "Use each parameter only for its specified role. If not used, set it to `null`.\n" +
                   "- `destination`: Use for moving to a location (Kitchen) or a direction (Left). For following a unit, use `follow_target` instead.\n" +
                   "- `follow_target`: Use ONLY for 'follow' commands. If this value is set, `destination` MUST be `null`.\n" +
                   "- `engage_enemy`: The name of the enemy to engage. (e.g., Zombie, Alien, Lion, Boss)\n" +
                   "- `support_target`: The target to receive support.\n" +
                   "- `support_type`: The type of support. (e.g., Heal, Shield)\n" +
                   "- `area`: The area to scout.\n" +
                   "- `scout_from` / `scout_to`: The start and end points for scouting.\n" +
                   "- `mode`: Special action mode. (e.g., Stealth, Quick)\n" +
                   "- `voice`: **REQUIRED**. You MUST ONLY use pre-generated voice modules listed below. Combine 1-3 modules for natural responses.\n\n" +

                   "### Voice Modules (USE ONLY THESE!)\n" +
                   "**Acknowledgments (choose 0-1 based on personality)**:\n" +
                   "- Taciturn/Serious: \"Okay\", \"Got it\", \"Roger\", \"Understood\", \"Alright\"\n" +
                   "- Moderate: \"Understood!\", \"Got it!\", \"Sure thing!\", \"Okay!\", \"Will do!\"\n" +
                   "- Lively/Cheerful: \"You got it!\", \"Absolutely!\", \"On it!\", \"Sure thing!\", \"Let's do this!\", \"No problem!\"\n\n" +

                   "**Actions (choose 0-1)**:\n" +
                   "\"moving\", \"attacking\", \"following\", \"covering\", \"helping\", \"scouting\", \"defending\", \"falling back\", \"engaging\"\n\n" +

                   "**Directions (choose 0-1)**:\n" +
                   "\"left\", \"right\", \"forward\", \"back\", \"center\"\n\n" +

                   $"**Locations/Districts (if moving to specific place)**:\n" +
                   $"\"{districtsName.Replace(" | ", "\", \"")}\"\n\n" +

                   "**Teammate/Enemy Names (if needed)**:\n" +
                   "\"Lena\", \"James\", \"Sara\", \"Player\", \"zombie\", \"alien\", \"enemy\"\n\n" +

                   "**Extras (Cheerful personality only, optional)**:\n" +
                   "\"Let's go\", \"Here I come\", \"Watch this\", \"No worries\", \"I got this\", \"Easy\"\n\n" +

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

                   "**Scout Patterns:**\n" +
                   "- 'scout Kitchen' (Moderate)\n" +
                   "  → {\"command_units\":[context],\"action\":\"Scout\",\"parameters\":{\"area\":\"Kitchen\",\"voice\":\"Got it!+scouting\"}}\n\n" +

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
            var actionSynonyms = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "Attack", "Combat" }, { "Engage", "Combat" }, { "Fight", "Combat" }, { "Kill", "Combat" }, { "Eliminate", "Combat" },
                { "Follow", "Move" }, { "Go", "Move" },
                { "Heal", "Support" }, { "Shield", "Support" }, { "Protect", "Support" },
                { "Watch", "Scout" }, { "Reconnaissance", "Scout" }
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
            var validEnemies = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "Zombie", "Alien", "Lion", "Boss" };
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

        async void onInputFieldSubmit(string message)
        {
            playerText.interactable = false;
            llmCharacter.grammarString = "";

            // 플레이어가 입력한 명령어를 채팅에 추가
            ChatManager.Instance.AddMessage("Player", message);

            List<TeammateAI> aiList = UnitManager.Instance.teammateUnitDict.Values
                .Select(tc => tc.GetComponent<TeammateAI>())
                .Where(ai => ai != null)
                .ToList();

            MapManager mapManager = MapManager.Instance;
            string districtsName = mapManager.GetDistrictsName();

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
                AIText.text = "⚠️ 명령 해석 실패! (JSON 오류)";
                playerText.interactable = true;
                return;
            }

            DoCommand(cmd, aiList);
        }

        public void DoCommand(ParsedCommand cmd, List<TeammateAI> aiList)
        {
            string unitsText = string.Join(", ", cmd.command_units);
            string functionName = $"{cmd.action}{cmd.Parameters}";
            Debug.Log($"[Parsed] target = {unitsText},\n action = {cmd.action},\n params = {cmd.Parameters}");

            // DEBUG: Check voice field value
            string voiceValue = cmd.Parameters?.voice;
            Debug.Log($"[DEBUG VOICE] Voice field value: '{voiceValue}' | Is null: {voiceValue == null} | Is empty: {string.IsNullOrEmpty(voiceValue)} | Is whitespace: {string.IsNullOrWhiteSpace(voiceValue)}");

            // LLM이 생성한 음성 우선 사용, 없으면 하드코딩된 것 사용
            string result = !string.IsNullOrEmpty(voiceValue) ? voiceValue : Functions.GetVoiceLine(functionName);

            // 명령받은 각 유닛의 이름으로 채팅에 전송
            if (cmd.command_units != null && cmd.command_units.Count > 0)
            {
                foreach (var unitName in cmd.command_units)
                {
                    ChatManager.Instance.AddMessage(unitName, result);
                }
            }

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
