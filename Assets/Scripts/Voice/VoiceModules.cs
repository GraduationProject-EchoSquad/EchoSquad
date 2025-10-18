using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 모듈식 음성 시스템 - 작은 단위를 조합해서 문장 생성
/// 예: "Okay" + "moving" + "left" = "Okay, moving left!"
/// </summary>
public static class VoiceModules
{
    // === 성격별 기본 응답 (가장 자주 쓰임) ===
    public static readonly Dictionary<string, string[]> Acknowledgments = new()
    {
        // Taciturn (0-1) + Serious (0-1)
        { "T_S", new[] { "Okay", "Got it", "Roger", "Understood", "Alright" } },

        // Taciturn/Quiet (0-1) + Moderate/Bright (2-4)
        { "T_M", new[] { "Okay!", "Got it!", "Sure!", "Alright!", "On it!" } },

        // Moderate (2) 전체
        { "M", new[] { "Understood!", "Got it!", "Sure thing!", "Okay!", "Will do!" } },

        // Lively/Talkative (3-4) + Serious/Calm (0-2)
        { "L_S", new[] { "Alright, got it!", "Understood!", "Okay, on it!", "Sure thing!", "Copy that!" } },

        // Lively/Talkative (3-4) + Bright/Cheerful (3-4)
        { "L_C", new[] { "You got it!", "Absolutely!", "On it!", "Sure thing!", "Let's do this!", "No problem!" } }
    };

    // === 행동 동사 ===
    public static readonly Dictionary<string, string> Actions = new()
    {
        { "Moving", "moving" },
        { "Attacking", "attacking" },
        { "Following", "following" },
        { "Covering", "covering" },
        { "Helping", "helping" },
        { "Scouting", "scouting" },
        { "Defending", "defending" },
        { "Retreating", "falling back" },
        { "Engaging", "engaging" }
    };

    // === 방향/위치 ===
    public static readonly Dictionary<string, string> Directions = new()
    {
        { "Left", "left" },
        { "Right", "right" },
        { "Forward", "forward" },
        { "Back", "back" },
        { "Center", "center" }
    };

    // === 대상 (동료 이름들) ===
    public static readonly string[] TeammateNames = new[] { "Lena", "James", "Sara", "Player" };

    // === 적 이름 ===
    public static readonly string[] EnemyNames = new[] { "zombie", "zombies", "alien", "aliens", "enemy", "enemies" };

    // === 추가 수식어 (Talkative+Cheerful 전용) ===
    public static readonly string[] Extras = new[]
    {
        "Let's go",
        "Here I come",
        "Watch this",
        "No worries",
        "I got this",
        "Easy"
    };

    // === District 이름들 (MapManager에서 동적으로 추가) ===
    private static List<string> districtNames = new List<string>();

    /// <summary>
    /// MapManager의 district 이름들을 VoiceModules에 등록
    /// (Preparation Phase에서 호출)
    /// </summary>
    public static void RegisterDistrictNames(List<string> districts)
    {
        districtNames.Clear();
        districtNames.AddRange(districts);
        Debug.Log($"[VoiceModules] Registered {districts.Count} district names: {string.Join(", ", districts)}");
    }

    /// <summary>
    /// 등록된 district 이름들 가져오기
    /// </summary>
    public static List<string> GetDistrictNames()
    {
        return new List<string>(districtNames);
    }

    /// <summary>
    /// Liveliness + Mood로 적절한 기본 응답 카테고리 선택
    /// </summary>
    public static string GetAcknowledgmentKey(int liveliness, int mood)
    {
        if (liveliness <= 1 && mood <= 1) return "T_S";
        if (liveliness <= 1 && mood >= 2) return "T_M";
        if (liveliness == 2) return "M";
        if (liveliness >= 3 && mood <= 2) return "L_S";
        if (liveliness >= 3 && mood >= 3) return "L_C";
        return "M"; // Fallback
    }

    /// <summary>
    /// 성격에 맞는 랜덤 기본 응답
    /// </summary>
    public static string GetRandomAcknowledgment(int liveliness, int mood)
    {
        string key = GetAcknowledgmentKey(liveliness, mood);
        if (Acknowledgments.TryGetValue(key, out var acks))
        {
            return acks[Random.Range(0, acks.Length)];
        }
        return "Okay";
    }

    /// <summary>
    /// 모든 필요한 TTS 모듈 조각들 추출 (사전 생성용)
    /// </summary>
    public static List<string> GetAllModules()
    {
        List<string> modules = new List<string>();

        // 모든 기본 응답
        foreach (var ackList in Acknowledgments.Values)
        {
            modules.AddRange(ackList);
        }

        // 모든 행동 동사
        modules.AddRange(Actions.Values);

        // 모든 방향
        modules.AddRange(Directions.Values);

        // 모든 동료 이름
        modules.AddRange(TeammateNames);

        // 모든 적 이름
        modules.AddRange(EnemyNames);

        // 추가 수식어
        modules.AddRange(Extras);

        // District 이름들 (MapManager에서 등록된 것)
        modules.AddRange(districtNames);

        // 중복 제거
        return new List<string>(new HashSet<string>(modules));
    }
}
