using System;
using UnityEngine;
using LLMUnity;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Text;

namespace LLMUnitySamples
{
    [System.Serializable]
    public class TestParameters
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
                   $"    \"mode\": \"{mode}\"";
        }
    }

    [System.Serializable]
    public class TestParsedCommand
    {
        public List<string> command_units;
        public AIActionEnum action;
        public TestParameters parameters;
    }

    [System.Serializable]
    public class TestResult
    {
        public string inputCommand;
        public string commandCategory;
        public string commandComplexity;
        public string parsedAction;
        public string parsedCommandRecipients;  // 누구에게 명령을 내리는지
        public string parsedActionTargets;      // 액션의 대상이 무엇인지
        public string parsedParameters;
        public float responseTime;
        public bool success;
        public bool correctAction;
        public bool correctUnits;
        public string errorMessage;
        public string expectedAction;
        public string expectedCommandRecipients;  // 예상되는 명령 수신자
        public string expectedActionTargets;      // 예상되는 액션 대상
        public DateTime timestamp;
    }

    public class LLMTestOnly : MonoBehaviour
    {
        public LLMCharacter llmCharacter;
        public InputField playerText;
        public Text AIText;
        public Button runAllTestsButton;

        private List<TestResult> testResults = new List<TestResult>();
        private bool isRunningTests = false;

        private string[] testCommands = {
            // Move 명령 - 기본
            "Lena go to Kitchen",
            "James follow me", 
            "Sara move left",
            "Lena advance forward",
            "James go to Sara",
            "All units move to Toilet",
            "Lena and James go right",
            
            // Move 명령 - 변형/일반화 테스트 (Valid)
            "send Lena to Kitchen",
            "make James come here",  
            "tell Sara to go left",
            "have Lena move forward",
            "get James to Sara",
            "everyone to Toilet now",
            "both Lena and James right",
            
            // Move 명령 - 복잡한 표현
            "Can you send Lena to the Kitchen please?",
            "I need James to come with me",
            "Sara should move towards the left side",
            "Move everyone to Ammo area",
            
            // Move 명령 - 자연어 변형
            "hey Lena head to Kitchen",
            "James please follow",
            "Sara go leftward",
            "could you move everyone to Ammo?",
            
            // Combat 명령 - 기본
            "James attack zombie",
            "Sara kill alien", 
            "Lena fight lion",
            "James engage zombie",
            "Sara eliminate the alien threat",
            
            // Combat 명령 - 일반화 테스트
            "destroy the zombie James",
            "Sara take out alien",
            "eliminate that lion Lena", 
            "James engage the zombie",
            "get Sara to kill alien",
            
            // Combat 명령 - 복잡한 표현  
            "I want James to take out that zombie",
            "Sara, can you deal with the alien?",
            "Lena needs to handle the lion situation", 
            "Everyone attack the nearest enemy",
            
            // Combat 명령 - 자연어 변형
            "James go fight zombie",
            "have Sara destroy alien",
            "tell Lena to engage lion",
            "make everyone attack enemies",
            
            // Support 명령 - 기본
            "Sara heal Lena",
            "James heal me",
            "Lena shield James", 
            "Sara provide support to Lena",
            
            // Support 명령 - 일반화 테스트
            "give Lena medical aid Sara",
            "James patch me up",
            "protect James with shields Lena",
            "Sara assist Lena",
            "help me James",
            
            // Support 명령 - 복잡한 표현
            "Can Sara give medical assistance to Lena?",
            "I need healing from James",
            "Lena should protect James with shields",
            "Someone heal the wounded",
            
            // Support 명령 - 자연어 변형  
            "Sara go heal Lena",
            "have James give me medical aid",
            "tell Lena to shield James",
            "get someone to heal wounded",
            
            // Scout 명령 - 기본
            "Lena scout from Ammo to Kitchen",
            "James scout Kitchen",
            "Sara scout stealthily from Toilet to Ammo", 
            "Lena reconnaissance Kitchen area",
            
            // Scout 명령 - 일반화 테스트
            "patrol Kitchen area James",
            "Lena check Ammo to Kitchen route",
            "investigate Toilet Sara",
            "survey Kitchen James",
            "watch Kitchen area Lena",
            
            // Scout 명령 - 복잡한 표현
            "Can Lena do surveillance from Ammo to Kitchen?",
            "I need James to check out the Kitchen", 
            "Sara should quietly investigate Toilet to Ammo route",
            
            // Scout 명령 - 자연어 변형
            "have Lena patrol Kitchen",
            "send James to scout Ammo",
            "tell Sara to investigate area",
            "get someone to check Toilet",
            
            // Edge Cases - 일반화 테스트
            "everyone go",           // 불완전한 명령
            "move lena",            // 어순 변경
            "SARA ATTACK ZOMBIE",   // 대문자
            "james, heal sara",     // 쉼표 사용
            "lena please go kitchen", // 소문자 + 예의
            "tell all units to move toilet", // 간접 명령
            
            // 복합/모호한 명령 (Error)
            "Lena go Kitchen and watch for zombies",
            "James follow me but stay ready to fight", 
            "Sara heal anyone who needs it",
            "Everyone move to safety and defend",
            "go there and do that",
            "attack and defend",
            
            // Error 케이스 - 일반화 테스트
            "Do something",
            "Help!",
            "go somewhere", 
            "attack something",
            "heal everyone",
            
            // Invalid Entities (Error)
            "Lena go to Mars",      // 잘못된 위치
            "Attack the purple elephant", // 잘못된 적
            "Heal the building",    // 잘못된 대상  
            "John move left",       // 존재하지 않는 유닛
            "Bob attack zombie",    // 존재하지 않는 유닛
            
            // Invalid Actions (Error)
            "Lena dance",           // 정의되지 않은 액션
            "Sara sing",            // 정의되지 않은 액션
            "James sleep",          // 정의되지 않은 액션
            "Sara attack friendly", // 부적절한 대상
            "Lena shoot teammate"   // 부적절한 대상
        };

        void Start()
        {
            playerText.onSubmit.AddListener(onInputFieldSubmit);
            playerText.Select();
            llmCharacter.grammarString = "";
            
            InitializeCommandMetadata();
            
            if (runAllTestsButton != null)
                runAllTestsButton.onClick.AddListener(RunAllTests);
        }

        string FormatEnumOptions<T>() where T : Enum
        {
            return string.Join(" | ", Enum.GetNames(typeof(T)));
        }

        string ConstructStructuredCommandPrompt(string playerMessage)
        {
            string actions = FormatEnumOptions<AIActionEnum>();
            string unitNames = "Lena | James | Sara";  // AI 동료 이름
            string districtsName = "Ammo | Toilet | Kitchen";  // 테스트용 지역

            return "### Core Rules\n" +
                   "You are a military command parser. Parse natural language into structured commands.\n\n" +
                   
                   "**OUTPUT FORMAT**: Single JSON object only. No explanations.\n\n" +
                   
                   $"**ACTIONS** - Use exactly one: {actions}\n" +
                   "- Move: go, walk, run, advance, retreat, follow, come, travel, proceed\n" +
                   "- Combat: attack, fight, kill, eliminate, engage, destroy, take out, deal with [enemy]\n" +
                   "- Support: heal, shield, protect, assist, help, cover, aid\n" +
                   "- Scout: scout, reconnaissance, patrol, watch, survey, investigate, check out\n" +
                   "- Error: invalid/impossible commands\n\n" +
                   
                   "**ENTITY RECOGNITION**\n" +
                   "- Units: Lena, James, Sara (case-insensitive)\n" +
                   "- 'me/myself/I' → 'Player'\n" +
                   "- 'all/everyone/everybody' → [\"Lena\",\"James\",\"Sara\"]\n" +
                   "- Locations: Kitchen, Toilet, Ammo, Left, Right, Forward, Back\n" +
                   "- Enemies: Zombie, Alien, Lion\n\n" +
                   
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
                   "- `engage_enemy`: The name of the enemy to engage. (e.g., Zombie, Alien, Lion)\n" +
                   "- `support_target`: The target to receive support.\n" +
                   "- `support_type`: The type of support. (e.g., Heal, Shield)\n" +
                   "- `area`: The area to scout.\n" +
                   "- `scout_from` / `scout_to`: The start and end points for scouting.\n" +
                   "- `mode`: Special action mode. (e.g., Stealth, Quick)\n\n" +
                   "### Pattern Examples\n" +
                   "**Movement Patterns:**\n" +
                   "- 'Lena go Kitchen' / 'send Lena to Kitchen' / 'Lena head to Kitchen'\n" +
                   "  → {\"command_units\":[\"Lena\"],\"action\":\"Move\",\"parameters\":{\"destination\":\"Kitchen\"}}\n" +
                   "- 'James follow me' / 'James come with me' / 'James stay with me'\n" +
                   "  → {\"command_units\":[\"James\"],\"action\":\"Move\",\"parameters\":{\"follow_target\":\"Player\"}}\n\n" +
                   
                   "**Combat Patterns:**\n" +
                   "- 'attack the zombie' / 'kill that zombie' / 'take out the zombie'\n" +
                   "  → {\"command_units\":[context],\"action\":\"Combat\",\"parameters\":{\"engage_enemy\":\"Zombie\"}}\n\n" +
                   
                   "**Support Patterns:**\n" +
                   "- 'heal Sara' / 'give Sara medical aid' / 'patch up Sara'\n" +
                   "  → {\"command_units\":[context],\"action\":\"Support\",\"parameters\":{\"support_target\":\"Sara\",\"support_type\":\"Heal\"}}\n" +
                   "- 'help me James' / 'heal me Sara' → James/Sara executes, Player receives\n" +
                   "  → {\"command_units\":[\"James\"],\"action\":\"Support\",\"parameters\":{\"support_target\":\"Player\"}}\n\n" +
                   
                   "**Scout Patterns:**\n" +
                   "- 'scout Kitchen' / 'check out Kitchen' / 'investigate Kitchen'\n" +
                   "  → {\"command_units\":[context],\"action\":\"Scout\",\"parameters\":{\"area\":\"Kitchen\"}}\n\n" +
                   
                   "**Error Patterns:**\n" +
                   "- Multi-step: 'go Kitchen and watch' → {\"command_units\":null,\"action\":\"Error\",\"parameters\":{}}\n" +
                   "- Invalid: 'go to Mars' / 'John move' / 'Lena dance'\n" +
                   "  → {\"command_units\":null,\"action\":\"Error\",\"parameters\":{}}\n\n" +
                   "### Command to Process\n" +
                   $"Command: {playerMessage}";
        }

        private Dictionary<string, (string category, string complexity, string expectedAction, string expectedCommandRecipients, string expectedActionTargets)> commandMetadata = new Dictionary<string, (string, string, string, string, string)>();

        private void InitializeCommandMetadata()
        {
            // Move 명령들
            commandMetadata["Lena go to Kitchen"] = ("Move", "Simple", "Move", "Lena", "Kitchen");
            commandMetadata["James follow me"] = ("Move", "Simple", "Move", "James", "Player");
            commandMetadata["Sara move left"] = ("Move", "Simple", "Move", "Sara", "Left");
            commandMetadata["Lena advance forward"] = ("Move", "Simple", "Move", "Lena", "Forward");
            commandMetadata["James go to Sara"] = ("Move", "Simple", "Move", "James", "Sara");
            commandMetadata["All units move to Toilet"] = ("Move", "Complex", "Move", "Lena,James,Sara", "Toilet");
            commandMetadata["Lena and James go right"] = ("Move", "Complex", "Move", "Lena,James", "Right");
            commandMetadata["Can you send Lena to the Kitchen please?"] = ("Move", "Polite", "Move", "Lena", "Kitchen");
            commandMetadata["I need James to come with me"] = ("Move", "Indirect", "Move", "James", "Player");
            commandMetadata["Sara should move towards the left side"] = ("Move", "Formal", "Move", "Sara", "Left");
            commandMetadata["Move everyone to Ammo area"] = ("Move", "Group", "Move", "Lena,James,Sara", "Ammo");
            
            // Move 명령들 - 일반화 테스트 (Valid Cases)
            commandMetadata["send Lena to Kitchen"] = ("Move", "Imperative", "Move", "Lena", "Kitchen");
            commandMetadata["make James come here"] = ("Move", "Imperative", "Move", "James", "Player");
            commandMetadata["tell Sara to go left"] = ("Move", "Indirect", "Move", "Sara", "Left");
            commandMetadata["have Lena move forward"] = ("Move", "Indirect", "Move", "Lena", "Forward");
            commandMetadata["get James to Sara"] = ("Move", "Indirect", "Move", "James", "Sara");
            commandMetadata["everyone to Toilet now"] = ("Move", "Urgent", "Move", "Lena,James,Sara", "Toilet");
            commandMetadata["both Lena and James right"] = ("Move", "Informal", "Move", "Lena,James", "Right");
            commandMetadata["hey Lena head to Kitchen"] = ("Move", "Casual", "Move", "Lena", "Kitchen");
            commandMetadata["James please follow"] = ("Move", "Polite", "Move", "James", "Player");
            commandMetadata["Sara go leftward"] = ("Move", "Simple", "Move", "Sara", "Left");
            commandMetadata["could you move everyone to Ammo?"] = ("Move", "Polite", "Move", "Lena,James,Sara", "Ammo");
            
            // Combat 명령들
            commandMetadata["James attack zombie"] = ("Combat", "Simple", "Combat", "James", "Zombie");
            commandMetadata["Sara kill alien"] = ("Combat", "Simple", "Combat", "Sara", "Alien");
            commandMetadata["Lena fight lion"] = ("Combat", "Simple", "Combat", "Lena", "Lion");
            commandMetadata["James engage zombie"] = ("Combat", "Military", "Combat", "James", "Zombie");
            commandMetadata["Sara eliminate the alien threat"] = ("Combat", "Formal", "Combat", "Sara", "Alien");
            commandMetadata["I want James to take out that zombie"] = ("Combat", "Indirect", "Combat", "James", "Zombie");
            commandMetadata["Sara, can you deal with the alien?"] = ("Combat", "Polite", "Combat", "Sara", "Alien");
            commandMetadata["Lena needs to handle the lion situation"] = ("Combat", "Indirect", "Combat", "Lena", "Lion");
            commandMetadata["Everyone attack the nearest enemy"] = ("Error", "Vague_Enemy", "Error", "Unknown", "Unknown");
            
            // Support 명령들
            commandMetadata["Sara heal Lena"] = ("Support", "Simple", "Support", "Sara", "Lena");
            commandMetadata["James heal me"] = ("Support", "Simple", "Support", "James", "Player");
            commandMetadata["Lena shield James"] = ("Support", "Simple", "Support", "Lena", "James");
            commandMetadata["Sara provide support to Lena"] = ("Support", "Formal", "Support", "Sara", "Lena");
            commandMetadata["Can Sara give medical assistance to Lena?"] = ("Support", "Polite", "Support", "Sara", "Lena");
            commandMetadata["I need healing from James"] = ("Support", "Indirect", "Support", "James", "Player");
            commandMetadata["Lena should protect James with shields"] = ("Support", "Formal", "Support", "Lena", "James");
            commandMetadata["Someone heal the wounded"] = ("Error", "Vague", "Error", "Unknown", "Unknown");
            
            // Support/Combat/Scout - 주요 Valid 일반화 케이스들
            commandMetadata["destroy the zombie James"] = ("Combat", "Imperative", "Combat", "James", "Zombie");
            commandMetadata["Sara take out alien"] = ("Combat", "Casual", "Combat", "Sara", "Alien");
            commandMetadata["eliminate that lion Lena"] = ("Combat", "Formal", "Combat", "Lena", "Lion");
            commandMetadata["James engage the zombie"] = ("Combat", "Military", "Combat", "James", "Zombie");
            commandMetadata["get Sara to kill alien"] = ("Combat", "Indirect", "Combat", "Sara", "Alien");
            commandMetadata["James patch me up"] = ("Support", "Casual", "Support", "James", "Player");
            commandMetadata["patrol Kitchen area James"] = ("Scout", "Military", "Scout", "James", "Kitchen");
            commandMetadata["SARA ATTACK ZOMBIE"] = ("Combat", "Caps", "Combat", "Sara", "Zombie");
            commandMetadata["james, heal sara"] = ("Support", "Punctuated", "Support", "James", "Sara");
            commandMetadata["lena please go kitchen"] = ("Move", "Lowercase", "Move", "Lena", "Kitchen");
            
            // Additional Valid Combat Cases
            commandMetadata["James go fight zombie"] = ("Combat", "Action", "Combat", "James", "Zombie");
            commandMetadata["have Sara destroy alien"] = ("Combat", "Indirect", "Combat", "Sara", "Alien");
            commandMetadata["tell Lena to engage lion"] = ("Combat", "Indirect", "Combat", "Lena", "Lion");
            commandMetadata["make everyone attack enemies"] = ("Combat", "Indirect", "Combat", "Lena,James,Sara", "Enemy");
            
            // Additional Valid Support Cases  
            commandMetadata["give Lena medical aid Sara"] = ("Support", "Imperative", "Support", "Sara", "Lena");
            commandMetadata["protect James with shields Lena"] = ("Support", "Imperative", "Support", "Lena", "James");
            commandMetadata["Sara assist Lena"] = ("Support", "Simple", "Support", "Sara", "Lena");
            commandMetadata["help me James"] = ("Support", "Casual", "Support", "James", "Player");
            commandMetadata["Sara go heal Lena"] = ("Support", "Action", "Support", "Sara", "Lena");
            commandMetadata["have James give me medical aid"] = ("Support", "Indirect", "Support", "James", "Player");
            commandMetadata["tell Lena to shield James"] = ("Support", "Indirect", "Support", "Lena", "James");
            commandMetadata["get someone to heal wounded"] = ("Error", "Vague", "Error", "Unknown", "Unknown");
            
            // Additional Valid Scout Cases
            commandMetadata["Lena check Ammo to Kitchen route"] = ("Scout", "Casual", "Scout", "Lena", "Ammo→Kitchen");
            commandMetadata["investigate Toilet Sara"] = ("Scout", "Imperative", "Scout", "Sara", "Toilet");
            commandMetadata["survey Kitchen James"] = ("Scout", "Military", "Scout", "James", "Kitchen");
            commandMetadata["watch Kitchen area Lena"] = ("Scout", "Simple", "Scout", "Lena", "Kitchen");
            commandMetadata["have Lena patrol Kitchen"] = ("Scout", "Indirect", "Scout", "Lena", "Kitchen");
            commandMetadata["send James to scout Ammo"] = ("Scout", "Indirect", "Scout", "James", "Ammo");
            commandMetadata["tell Sara to investigate area"] = ("Scout", "Indirect", "Scout", "Sara", "Unknown");
            commandMetadata["get someone to check Toilet"] = ("Error", "Ambiguous", "Error", "Unknown", "Unknown");
            
            // Scout 명령들
            commandMetadata["Lena scout from Ammo to Kitchen"] = ("Scout", "Complex", "Scout", "Lena", "Ammo→Kitchen");
            commandMetadata["James scout Kitchen"] = ("Scout", "Simple", "Scout", "James", "Kitchen");
            commandMetadata["Sara scout stealthily from Toilet to Ammo"] = ("Scout", "Complex", "Scout", "Sara", "Toilet→Ammo");
            commandMetadata["Lena reconnaissance Kitchen area"] = ("Scout", "Military", "Scout", "Lena", "Kitchen");
            commandMetadata["Can Lena do surveillance from Ammo to Kitchen?"] = ("Scout", "Polite", "Scout", "Lena", "Ammo→Kitchen");
            commandMetadata["I need James to check out the Kitchen"] = ("Scout", "Indirect", "Scout", "James", "Kitchen");
            commandMetadata["Sara should quietly investigate Toilet to Ammo route"] = ("Scout", "Formal", "Scout", "Sara", "Toilet→Ammo");
            
            // Multi-step 명령들 - Primary Action으로 처리
            commandMetadata["Lena go Kitchen and watch for zombies"] = ("Error", "Multi-step", "Error", "Unknown", "Unknown"); // 너무 복잡
            commandMetadata["James follow me but stay ready to fight"] = ("Move", "Multi-step", "Move", "James", "Player"); // Primary는 Move
            commandMetadata["Sara heal anyone who needs it"] = ("Support", "Vague", "Support", "Sara", "Unknown");
            commandMetadata["Everyone move to safety and defend"] = ("Error", "Multi-step", "Error", "Unknown", "Unknown"); // 너무 복잡
            
            // Additional Valid Cases from Edge Cases
            commandMetadata["tell all units to move toilet"] = ("Move", "Indirect", "Move", "Lena,James,Sara", "Toilet");
            commandMetadata["everyone go"] = ("Move", "Incomplete", "Move", "Lena,James,Sara", "Unknown");
            commandMetadata["move lena"] = ("Move", "Reversed", "Move", "Lena", "Unknown");
            
            // Error 케이스들
            commandMetadata["Do something"] = ("Error", "Ambiguous", "Error", "Unknown", "Unknown");
            commandMetadata["Help!"] = ("Error", "Ambiguous", "Error", "Unknown", "Unknown");
            commandMetadata["Lena go to Mars"] = ("Error", "Invalid_Location", "Error", "Unknown", "Unknown");
            commandMetadata["Attack the purple elephant"] = ("Error", "Invalid_Enemy", "Error", "Unknown", "Unknown");
            commandMetadata["Heal the building"] = ("Error", "Invalid_Target", "Error", "Unknown", "Unknown");
            commandMetadata["John move left"] = ("Error", "Invalid_Unit", "Error", "Unknown", "Unknown");
            commandMetadata["Lena dance"] = ("Error", "Invalid_Action", "Error", "Unknown", "Unknown");
            commandMetadata["Sara attack friendly"] = ("Error", "Invalid_Target", "Error", "Unknown", "Unknown");
        }

        private TestParsedCommand ParseAndCorrectCommand(string json)
        {
            var actionSynonyms = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "Attack", "Combat" }, { "Engage", "Combat" }, { "Fight", "Combat" }, { "Kill", "Combat" }, { "Eliminate", "Combat" },
                { "Follow", "Move" }, { "Go", "Move" },
                { "Heal", "Support" }, { "Shield", "Support" }, { "Protect", "Support" },
                { "Watch", "Scout" }, { "Reconnaissance", "Scout" }
            };

            if (string.IsNullOrWhiteSpace(json)) return new TestParsedCommand();

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
                var parsedCmd = tempCmd.ToObject<TestParsedCommand>();
                
                // Post-processing validation
                return ValidateAndCorrectCommand(parsedCmd);
            }
            catch (JsonException ex)
            {
                Debug.LogError($"[JSON PARSE ERROR] {ex.Message}\n[Cleaned JSON] {json}");
                return new TestParsedCommand(); // Return empty command on parse failure
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
        
        private TestParsedCommand ValidateAndCorrectCommand(TestParsedCommand cmd)
        {
            if (cmd == null) return new TestParsedCommand();
            
            Debug.Log($"[VALIDATION START] Action: {cmd.action}, Units: {string.Join(",", cmd.command_units ?? new List<string>())}, Enemy: {cmd.parameters?.engage_enemy}");
            
            // Valid entities lists
            var validUnits = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "Lena", "James", "Sara" };
            var validEnemies = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "Zombie", "Alien", "Lion" };
            var validLocations = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "Kitchen", "Toilet", "Ammo", "Left", "Right", "Forward", "Back", "Center" };
            
            // Check for invalid units
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
            
            // Check for invalid enemies in combat actions
            if (cmd.action == AIActionEnum.Combat && cmd.parameters != null && !string.IsNullOrEmpty(cmd.parameters.engage_enemy))
            {
                if (!validEnemies.Contains(cmd.parameters.engage_enemy))
                {
                    Debug.Log($"[VALIDATION] Invalid enemy detected: {cmd.parameters.engage_enemy} -> Setting action to Error");
                    cmd.action = AIActionEnum.Error;
                    return cmd;
                }
            }
            
            // Check for invalid destinations
            if (cmd.parameters != null && !string.IsNullOrEmpty(cmd.parameters.destination))
            {
                if (!validLocations.Contains(cmd.parameters.destination) && !validUnits.Contains(cmd.parameters.destination))
                {
                    Debug.Log($"[VALIDATION] Invalid destination detected: {cmd.parameters.destination} -> Setting action to Error");
                    cmd.action = AIActionEnum.Error;
                    return cmd;
                }
            }
            
            // Check for invalid support targets
            if (cmd.action == AIActionEnum.Support && cmd.parameters != null && !string.IsNullOrEmpty(cmd.parameters.support_target))
            {
                if (!validUnits.Contains(cmd.parameters.support_target) && !cmd.parameters.support_target.Equals("Player", StringComparison.OrdinalIgnoreCase))
                {
                    Debug.Log($"[VALIDATION] Invalid support target detected: {cmd.parameters.support_target} -> Setting action to Error");
                    cmd.action = AIActionEnum.Error;
                    return cmd;
                }
            }
            
            // Check for logical inconsistencies
            if (cmd.command_units != null && cmd.parameters != null)
            {
                // Player can't command themselves
                if (cmd.command_units.Contains("Player") && cmd.parameters.support_target == "Player")
                {
                    Debug.Log($"[VALIDATION] Player can't support themselves -> Setting action to Error");
                    cmd.action = AIActionEnum.Error;
                    return cmd;
                }
                
                // Command unit must be valid
                foreach (var unit in cmd.command_units)
                {
                    if (!string.IsNullOrEmpty(unit) && !validUnits.Contains(unit) && unit != "Player")
                    {
                        Debug.Log($"[VALIDATION] Invalid command unit: {unit} -> Setting action to Error");
                        cmd.action = AIActionEnum.Error;
                        return cmd;
                    }
                }
            }
            
            return cmd;
        }

        private async System.Threading.Tasks.Task<TestResult> ProcessCommand(string message)
        {
            var metadata = commandMetadata.ContainsKey(message) ? 
                commandMetadata[message] : ("Error", "Unknown", "Error", "Unknown", "Unknown");
            
            var result = new TestResult 
            {
                inputCommand = message,
                commandCategory = metadata.Item1,
                commandComplexity = metadata.Item2,
                expectedAction = metadata.Item3,
                expectedCommandRecipients = metadata.Item4,
                expectedActionTargets = metadata.Item5,
                timestamp = DateTime.Now
            };
            
            try
            {
                float t0 = Time.realtimeSinceStartup;
                string json = await llmCharacter.Chat(ConstructStructuredCommandPrompt(message));
                result.responseTime = (Time.realtimeSinceStartup - t0) * 1000f;

                json = json.Trim();
                if (json.StartsWith("```json")) json = json.Substring(7);
                if (json.StartsWith("```")) json = json.Substring(3);
                if (json.EndsWith("```")) json = json.Substring(0, json.Length - 3);
                json = json.Trim();

                TestParsedCommand cmd = ParseAndCorrectCommand(json);

                result.parsedAction = cmd.action.ToString();
                result.parsedCommandRecipients = (cmd.command_units != null) ? string.Join(",", cmd.command_units) : "N/A";
                result.parsedActionTargets = ExtractActionTargets(cmd.action, cmd.parameters);
                result.parsedParameters = cmd.parameters?.ToString() ?? "";
                result.success = true;
                
                result.correctAction = result.parsedAction.Equals(result.expectedAction, StringComparison.OrdinalIgnoreCase);
                
                // Error 케이스는 Units 정확도 체크 불필요 (Error로 판정했으면 성공)
                if (result.expectedAction.Equals("Error", StringComparison.OrdinalIgnoreCase))
                {
                    result.correctUnits = result.correctAction; // Error 판정이 맞으면 Units도 맞음
                }
                else
                {
                    result.correctUnits = CheckUnitsCorrectness(result.parsedCommandRecipients, result.expectedCommandRecipients);
                }
            }
            catch (System.Exception e)
            {
                result.success = false;
                result.errorMessage = e.Message;
            }
            
            return result;
        }

        private bool CheckUnitsCorrectness(string parsed, string expected)
        {
            if (string.IsNullOrEmpty(expected)) return string.IsNullOrEmpty(parsed);
            return parsed.Equals(expected, StringComparison.OrdinalIgnoreCase);
        }

        private string ExtractActionTargets(AIActionEnum action, TestParameters parameters)
        {
            if (parameters == null) return "Unknown";
            switch (action)
            {
                case AIActionEnum.Move:
                    if (!string.IsNullOrEmpty(parameters.follow_target)) return parameters.follow_target;
                    if (!string.IsNullOrEmpty(parameters.destination)) return parameters.destination;
                    return "Unknown";
                case AIActionEnum.Combat:
                    if (!string.IsNullOrEmpty(parameters.engage_enemy)) return parameters.engage_enemy;
                    return "Unknown";
                case AIActionEnum.Support:
                    if (!string.IsNullOrEmpty(parameters.support_target)) return parameters.support_target;
                    return "Unknown";
                case AIActionEnum.Scout:
                    if (!string.IsNullOrEmpty(parameters.scout_from) && !string.IsNullOrEmpty(parameters.scout_to)) return $"{parameters.scout_from}→{parameters.scout_to}";
                    if (!string.IsNullOrEmpty(parameters.area)) return parameters.area;
                     if (!string.IsNullOrEmpty(parameters.engage_enemy)) return parameters.engage_enemy;
                    return "Unknown";
                default: return "Unknown";
            }
        }

        private string GetDetailedActionInfo(AIActionEnum action, TestParameters parameters, string unitsText)
        {
            if (parameters == null) return "";
            switch (action)
            {
                case AIActionEnum.Move:
                    if (!string.IsNullOrEmpty(parameters.follow_target))
                    {
                        if (parameters.follow_target == "Player") return $"Follow you";
                        return $"Follow {parameters.follow_target}";
                    }
                    if (!string.IsNullOrEmpty(parameters.destination))
                    {
                        if (parameters.destination == "Left" || parameters.destination == "Right" || 
                            parameters.destination == "Forward" || parameters.destination == "Back") return $"Move {parameters.destination.ToLower()}";
                        if (parameters.destination == "Player") return $"Come to you";
                        return $"Go to {parameters.destination}";
                    }
                    return $"Move";
                case AIActionEnum.Combat:
                    if (!string.IsNullOrEmpty(parameters.engage_enemy)) return $"Attack {parameters.engage_enemy}";
                    return $"Engage in combat";
                case AIActionEnum.Support:
                    string supportType = !string.IsNullOrEmpty(parameters.support_type) ? parameters.support_type.ToLower() : "support";
                    if (!string.IsNullOrEmpty(parameters.support_target))
                    {
                        if (parameters.support_target == "Player") return $"Provide {supportType} to you";
                        return $"Provide {supportType} to {parameters.support_target}";
                    }
                    return $"Provide {supportType}";
                case AIActionEnum.Scout:
                    string modeText = !string.IsNullOrEmpty(parameters.mode) ? $" {parameters.mode.ToLower()}" : "";
                    if (!string.IsNullOrEmpty(parameters.scout_from) && !string.IsNullOrEmpty(parameters.scout_to)) return $"Scout{modeText} from {parameters.scout_from} to {parameters.scout_to}";
                    if (!string.IsNullOrEmpty(parameters.area)) return $"Scout{modeText} {parameters.area} area";
                    return $"Scout{modeText}";
                case AIActionEnum.Error:
                    return $"Invalid command - cannot execute";
                default:
                    return $"Perform {action}";
            }
        }

        private void DisplayTestResults()
        {
            float totalTime = 0f;
            int successCount = 0;
            int correctActionCount = 0;
            int correctUnitsCount = 0;
            
            // Error 케이스와 Valid 케이스 분리
            var validCases = testResults.Where(r => !r.expectedAction.Equals("Error", StringComparison.OrdinalIgnoreCase)).ToList();
            var errorCases = testResults.Where(r => r.expectedAction.Equals("Error", StringComparison.OrdinalIgnoreCase)).ToList();
            
            string output = "=== 테스트 결과 ===\n";
            
            for (int i = 0; i < testResults.Count; i++)
            {
                var result = testResults[i];
                totalTime += result.responseTime;
                if (result.success) successCount++;
                if (result.correctAction) correctActionCount++;
                if (result.correctUnits) correctUnitsCount++;
                
                string status = result.success ? "✓" : "✗";
                string accuracy = "";
                if (result.success)
                {
                    accuracy = $" Action:{(result.correctAction ? "✓" : "✗")} Units:{(result.correctUnits ? "✓" : "✗")}";
                }
                
                output += $"{i+1:D2}. [{status}] {result.inputCommand} ({result.commandCategory}/{result.commandComplexity}){accuracy}\n";
                output += $"    → {result.parsedAction} | Recipients: {result.parsedCommandRecipients} | Targets: {result.parsedActionTargets} | {result.responseTime:F1}ms\n";
                
                if (!result.success)
                    output += $"    ERROR: {result.errorMessage}\n";
                output += "\n";
            }
            
            float avgTime = totalTime / testResults.Count;
            
            // Valid 케이스들만으로 정확도 계산
            int validCorrectActions = validCases.Count(r => r.correctAction);
            int validCorrectUnits = validCases.Count(r => r.correctUnits);
            int errorCorrectDetections = errorCases.Count(r => r.correctAction); // Error를 Error로 올바르게 판정
            
            output += $"총 {testResults.Count}개 테스트\n";
            output += $"파싱 성공: {successCount}개 ({(float)successCount/testResults.Count*100:F1}%)\n";
            output += $"--- Valid Commands ({validCases.Count}개) ---\n";
            output += $"액션 정확도: {validCorrectActions}개 ({(validCases.Count > 0 ? (float)validCorrectActions/validCases.Count*100 : 0):F1}%)\n";
            output += $"유닛 정확도: {validCorrectUnits}개 ({(validCases.Count > 0 ? (float)validCorrectUnits/validCases.Count*100 : 0):F1}%)\n";
            output += $"--- Error Detection ({errorCases.Count}개) ---\n";
            output += $"에러 감지: {errorCorrectDetections}개 ({(errorCases.Count > 0 ? (float)errorCorrectDetections/errorCases.Count*100 : 0):F1}%)\n";
            output += $"평균 응답시간: {avgTime:F1}ms\n";
            output += $"총 소요시간: {totalTime:F1}ms";
            
            Debug.Log(output);
            AIText.text = $"Valid: {(validCases.Count > 0 ? (float)validCorrectActions/validCases.Count*100 : 0):F1}% | Error Detection: {(errorCases.Count > 0 ? (float)errorCorrectDetections/errorCases.Count*100 : 0):F1}%";
            
            // 엑셀 파일 저장
            SaveResultsToCSV();
        }

        public async void RunAllTests()
        {
            if (isRunningTests) return;
            
            isRunningTests = true;
            testResults.Clear();
            
            AIText.text = "테스트 실행 중...";
            Debug.Log("=== LLM 성능 테스트 시작 ===");
            
            for (int i = 0; i < testCommands.Length; i++)
            {
                string command = testCommands[i];
                AIText.text = $"테스트 {i+1}/{testCommands.Length}: {command}";
                
                var result = await ProcessCommand(command);
                testResults.Add(result);
                
                Debug.Log($"[테스트 {i+1}] {command} → {result.responseTime:F1}ms " +
                         $"(성공: {result.success})");
                
                // 테스트 간 잠시 대기
                await System.Threading.Tasks.Task.Delay(500);
            }
            
            DisplayTestResults();
            isRunningTests = false;
        }

        private void SaveResultsToCSV()
        {
            try
            {
                string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
                string fileName = $"LLM_Test_Results_{timestamp}.csv";
                string filePath = Path.Combine(Application.persistentDataPath, fileName);
                
                StringBuilder csv = new StringBuilder();
                
                // 헤더 추가
                csv.AppendLine("Index,Input Command,Category,Complexity,Expected Action,Expected Command Recipients,Expected Action Targets,Parsed Action,Parsed Command Recipients,Parsed Action Targets,Response Time (ms),Parsing Success,Action Correct,Units Correct,Error Message,Timestamp");
                
                // 데이터 추가
                for (int i = 0; i < testResults.Count; i++)
                {
                    var result = testResults[i];
                    csv.AppendLine($"{i+1},\"{result.inputCommand}\",{result.commandCategory},{result.commandComplexity},{result.expectedAction},\"{result.expectedCommandRecipients}\",\"{result.expectedActionTargets}\",{result.parsedAction},\"{result.parsedCommandRecipients}\",\"{result.parsedActionTargets}\",{result.responseTime:F1},{result.success},{result.correctAction},{result.correctUnits},\"{result.errorMessage}\",{result.timestamp:yyyy-MM-dd HH:mm:ss}");
                }
                
                // 통계 정보 추가
                csv.AppendLine("");
                csv.AppendLine("=== STATISTICS ===");
                float totalTime = testResults.Sum(r => r.responseTime);
                int successCount = testResults.Count(r => r.success);
                int correctActionCount = testResults.Count(r => r.correctAction);
                int correctUnitsCount = testResults.Count(r => r.correctUnits);
                
                csv.AppendLine($"Total Tests,{testResults.Count}");
                csv.AppendLine($"Parsing Success Rate,{(float)successCount/testResults.Count*100:F1}%");
                csv.AppendLine($"Action Accuracy,{(float)correctActionCount/testResults.Count*100:F1}%");
                csv.AppendLine($"Units Accuracy,{(float)correctUnitsCount/testResults.Count*100:F1}%");
                csv.AppendLine($"Average Response Time,{totalTime/testResults.Count:F1}ms");
                csv.AppendLine($"Total Time,{totalTime:F1}ms");
                
                // 카테고리별 통계
                csv.AppendLine("");
                csv.AppendLine("=== CATEGORY STATISTICS ===");
                var categoryStats = testResults
                    .GroupBy(r => r.commandCategory)
                    .Select(g => new {
                        Category = g.Key,
                        Count = g.Count(),
                        SuccessRate = g.Count(r => r.success) * 100.0 / g.Count(),
                        ActionAccuracy = g.Count(r => r.correctAction) * 100.0 / g.Count(),
                        AvgTime = g.Average(r => r.responseTime)
                    });
                
                csv.AppendLine("Category,Count,Success Rate %,Action Accuracy %,Avg Time ms");
                foreach (var stat in categoryStats)
                {
                    csv.AppendLine($"{stat.Category},{stat.Count},{stat.SuccessRate:F1},{stat.ActionAccuracy:F1},{stat.AvgTime:F1}");
                }
                
                File.WriteAllText(filePath, csv.ToString(), Encoding.UTF8);
                Debug.Log($"테스트 결과가 CSV 파일로 저장되었습니다: {filePath}");
                
            }
            catch (System.Exception e)
            {
                Debug.LogError($"CSV 파일 저장 실패: {e.Message}");
            }
        }

        async void onInputFieldSubmit(string message)
        {
            message = message.Trim();
            playerText.interactable = false;
            llmCharacter.grammarString = "";

            float t0 = Time.realtimeSinceStartup;
            string json = await llmCharacter.Chat(ConstructStructuredCommandPrompt(message));
            float elapsedMs = (Time.realtimeSinceStartup - t0) * 1000f;
            
            Debug.Log($"[LLM Raw JSON] {json} ({elapsedMs:F1} ms)");

            json = json.Trim();
            if (json.StartsWith("```json")) json = json.Substring(7);
            if (json.StartsWith("```")) json = json.Substring(3);
            if (json.EndsWith("```")) json = json.Substring(0, json.Length - 3);
            json = json.Trim();

            TestParsedCommand cmd;
            try
            {
                cmd = ParseAndCorrectCommand(json);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[JSON PARSE ERROR] {e.Message}\n[Raw JSON] {json}");
                AIText.text = "⚠️ Command parsing failed! (JSON Error)";
                playerText.interactable = true;
                return;
            }

            if (cmd == null)
            {
                AIText.text = "Could not understand the command.";
                playerText.interactable = true;
                return;
            }

            string unitsText = (cmd.command_units != null) ? string.Join(", ", cmd.command_units) : "N/A";
            string detailedInfo = GetDetailedActionInfo(cmd.action, cmd.parameters, unitsText);
            AIText.text = $"[COMMAND TO: {unitsText}] {detailedInfo}";
            Debug.Log($"[Parsed Command] Units: {unitsText}, Action: {cmd.action}, Params: {cmd.parameters}");

            playerText.interactable = true;
        }

        public void SendCommandFromText(string message)
        {
            onInputFieldSubmit(message);
        }

        public void CancelRequests() => llmCharacter.CancelRequests();

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
