using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 성격에 따른 대사 변형 시스템
/// Liveliness (0~4): Taciturn → Quiet → Moderate → Lively → Talkative
/// Mood (0~4): Serious → Calm → Moderate → Bright → Cheerful
/// </summary>
[CreateAssetMenu(fileName = "VoiceLineDatabase", menuName = "EchoSquad/Voice Line Database")]
public class VoiceLineDatabase : ScriptableObject
{
    [System.Serializable]
    public class PersonalityVariant
    {
        [Header("Personality Range")]
        [Range(0, 4)] public int minLiveliness = 0;
        [Range(0, 4)] public int maxLiveliness = 4;
        [Range(0, 4)] public int minMood = 0;
        [Range(0, 4)] public int maxMood = 4;

        [Header("Voice Lines")]
        public List<string> lines = new List<string>();

        public bool Matches(int liveliness, int mood)
        {
            return liveliness >= minLiveliness && liveliness <= maxLiveliness &&
                   mood >= minMood && mood <= maxMood;
        }
    }

    [System.Serializable]
    public class VoiceLineEntry
    {
        public string commandKey; // 예: "MoveLeft", "AttackZombie", "FollowPlayer"
        public List<PersonalityVariant> variants = new List<PersonalityVariant>();

        public string GetLine(int liveliness, int mood)
        {
            foreach (var variant in variants)
            {
                if (variant.Matches(liveliness, mood) && variant.lines.Count > 0)
                {
                    return variant.lines[Random.Range(0, variant.lines.Count)];
                }
            }
            return "Command received."; // Fallback
        }
    }

    [Header("Voice Line Entries")]
    public List<VoiceLineEntry> entries = new List<VoiceLineEntry>();

    public string GetVoiceLine(string commandKey, int liveliness, int mood)
    {
        var entry = entries.Find(e => e.commandKey == commandKey);
        if (entry != null)
        {
            return entry.GetLine(liveliness, mood);
        }
        return $"[{commandKey}]"; // Fallback
    }
}
