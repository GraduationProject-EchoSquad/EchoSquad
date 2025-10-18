using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 하드코딩된 대사 템플릿 (나중에 ScriptableObject로 이동 가능)
/// </summary>
public static class VoiceLineTemplates
{
    // Liveliness levels: 0=Taciturn, 1=Quiet, 2=Moderate, 3=Lively, 4=Talkative
    // Mood levels: 0=Serious, 1=Calm, 2=Moderate, 3=Bright, 4=Cheerful

    private static Dictionary<string, List<(int minLive, int maxLive, int minMood, int maxMood, string[] lines)>> templates = new()
    {
        // === MOVE COMMANDS ===
        {
            "MoveLeft", new List<(int, int, int, int, string[])>
            {
                (0, 1, 0, 1, new[] { "Left.", "Going left.", "Left side." }), // Taciturn/Quiet + Serious/Calm
                (0, 1, 2, 4, new[] { "Moving left!", "Left, got it!", "On my way left!" }), // Taciturn/Quiet + Moderate/Bright/Cheerful
                (2, 2, 0, 4, new[] { "Moving to the left!", "Heading left now!", "Going left!" }), // Moderate
                (3, 4, 0, 2, new[] { "Alright, moving left!", "Got it, heading left!", "Left side, on my way!" }), // Lively/Talkative + Serious/Calm/Moderate
                (3, 4, 3, 4, new[] { "Sure thing! Going left!", "Let's do this, moving left!", "Left side, here I come!" }) // Lively/Talkative + Bright/Cheerful
            }
        },
        {
            "MoveRight", new List<(int, int, int, int, string[])>
            {
                (0, 1, 0, 1, new[] { "Right.", "Going right.", "Right side." }),
                (0, 1, 2, 4, new[] { "Moving right!", "Right, got it!", "On my way right!" }),
                (2, 2, 0, 4, new[] { "Moving to the right!", "Heading right now!", "Going right!" }),
                (3, 4, 0, 2, new[] { "Alright, moving right!", "Got it, heading right!", "Right side, on my way!" }),
                (3, 4, 3, 4, new[] { "Sure thing! Going right!", "Let's do this, moving right!", "Right side, here I come!" })
            }
        },
        {
            "MoveForward", new List<(int, int, int, int, string[])>
            {
                (0, 1, 0, 1, new[] { "Forward.", "Advancing.", "Moving up." }),
                (0, 1, 2, 4, new[] { "Moving forward!", "Advancing now!", "Going forward!" }),
                (2, 2, 0, 4, new[] { "Advancing forward!", "Moving up ahead!", "Heading forward!" }),
                (3, 4, 0, 2, new[] { "Alright, pushing forward!", "Got it, moving up!", "Advancing now!" }),
                (3, 4, 3, 4, new[] { "Let's push forward!", "Moving up, let's go!", "Forward we go!" })
            }
        },
        {
            "MoveBack", new List<(int, int, int, int, string[])>
            {
                (0, 1, 0, 1, new[] { "Back.", "Retreating.", "Falling back." }),
                (0, 1, 2, 4, new[] { "Moving back!", "Falling back now!", "Retreating!" }),
                (2, 2, 0, 4, new[] { "Retreating now!", "Falling back!", "Moving to the rear!" }),
                (3, 4, 0, 2, new[] { "Alright, falling back!", "Got it, moving back!", "Retreating now!" }),
                (3, 4, 3, 4, new[] { "Okay, pulling back!", "Let's regroup, moving back!", "Falling back!" })
            }
        },
        {
            "FollowPlayer", new List<(int, int, int, int, string[])>
            {
                (0, 1, 0, 1, new[] { "Following.", "On you.", "Right behind." }),
                (0, 1, 2, 4, new[] { "Following you!", "I'm with you!", "Right behind you!" }),
                (2, 2, 0, 4, new[] { "Following you now!", "I'm right behind!", "Staying with you!" }),
                (3, 4, 0, 2, new[] { "Got it, following you!", "Right behind you!", "I'm with you!" }),
                (3, 4, 3, 4, new[] { "On it! Right behind you!", "Following you, let's go!", "I've got your back!" })
            }
        },

        // === COMBAT COMMANDS ===
        {
            "AttackLeft", new List<(int, int, int, int, string[])>
            {
                (0, 1, 0, 1, new[] { "Engaging left.", "Attacking left.", "Left target." }),
                (0, 1, 2, 4, new[] { "Attacking left side!", "Engaging left!", "Taking left!" }),
                (2, 2, 0, 4, new[] { "Engaging enemies left!", "Attacking left flank!", "Left side attack!" }),
                (3, 4, 0, 2, new[] { "Got it, engaging left!", "Attacking left side!", "Taking them out on the left!" }),
                (3, 4, 3, 4, new[] { "Let's take the left! Attacking now!", "Left side, here we go!", "Engaging left, let's do this!" })
            }
        },
        {
            "AttackZombie", new List<(int, int, int, int, string[])>
            {
                (0, 1, 0, 1, new[] { "Engaging zombie.", "Zombie target.", "Attacking." }),
                (0, 1, 2, 4, new[] { "Attacking zombie!", "Taking it down!", "Engaging zombie!" }),
                (2, 2, 0, 4, new[] { "Engaging the zombie!", "Taking down the zombie!", "Attacking zombie now!" }),
                (3, 4, 0, 2, new[] { "Got it, taking down that zombie!", "Engaging zombie!", "Attacking the zombie!" }),
                (3, 4, 3, 4, new[] { "Let's get that zombie!", "Taking it down!", "Zombie's going down!" })
            }
        },

        // === SUPPORT COMMANDS ===
        {
            "HelpPlayer", new List<(int, int, int, int, string[])>
            {
                (0, 1, 0, 1, new[] { "Coming.", "On my way.", "Helping." }),
                (0, 1, 2, 4, new[] { "On my way!", "Coming to help!", "I'm coming!" }),
                (2, 2, 0, 4, new[] { "On my way to help!", "Coming to assist!", "Help is coming!" }),
                (3, 4, 0, 2, new[] { "Got it, coming to help!", "On my way!", "I'll help you!" }),
                (3, 4, 3, 4, new[] { "On my way! Hang in there!", "Coming to help, hold on!", "I've got you, coming!" })
            }
        },
        {
            "SupportPlayer", new List<(int, int, int, int, string[])>
            {
                (0, 1, 0, 1, new[] { "Supporting.", "Covering.", "Got you." }),
                (0, 1, 2, 4, new[] { "Covering you!", "I've got your back!", "Supporting!" }),
                (2, 2, 0, 4, new[] { "Providing support!", "Covering you now!", "I've got you covered!" }),
                (3, 4, 0, 2, new[] { "Got it, covering you!", "I'm on support!", "You're covered!" }),
                (3, 4, 3, 4, new[] { "You got it! I'll cover you!", "I've got your back, let's go!", "Covering you, no worries!" })
            }
        },

        // === SCOUT COMMANDS ===
        {
            "ScoutLeft", new List<(int, int, int, int, string[])>
            {
                (0, 1, 0, 1, new[] { "Scouting left.", "Checking left.", "Left recon." }),
                (0, 1, 2, 4, new[] { "Scouting left!", "Checking left side!", "Recon left!" }),
                (2, 2, 0, 4, new[] { "Scouting left area!", "Checking left flank!", "Reconnaissance left!" }),
                (3, 4, 0, 2, new[] { "Got it, scouting left!", "Checking left side!", "Recon on the left!" }),
                (3, 4, 3, 4, new[] { "On it! Scouting left!", "Let me check the left!", "Scouting left, here we go!" })
            }
        },

        // === ERROR / GENERIC ===
        {
            "Error", new List<(int, int, int, int, string[])>
            {
                (0, 1, 0, 1, new[] { "Unknown command.", "Cannot do that.", "Invalid." }),
                (0, 1, 2, 4, new[] { "Can't do that!", "Invalid command!", "I don't understand!" }),
                (2, 2, 0, 4, new[] { "I can't do that!", "Invalid command!", "Can't execute that!" }),
                (3, 4, 0, 2, new[] { "Sorry, can't do that!", "That doesn't work!", "Invalid command!" }),
                (3, 4, 3, 4, new[] { "Whoa, can't do that!", "That's not possible!", "I don't understand that command!" })
            }
        }
    };

    public static string GetLine(string commandKey, int liveliness, int mood)
    {
        liveliness = Mathf.Clamp(liveliness, 0, 4);
        mood = Mathf.Clamp(mood, 0, 4);

        if (templates.TryGetValue(commandKey, out var variants))
        {
            foreach (var (minLive, maxLive, minMood, maxMood, lines) in variants)
            {
                if (liveliness >= minLive && liveliness <= maxLive &&
                    mood >= minMood && mood <= maxMood)
                {
                    return lines[Random.Range(0, lines.Length)];
                }
            }
        }

        return $"Command {commandKey} acknowledged."; // Fallback
    }

    /// <summary>
    /// 모든 commandKey 목록 반환 (TTS 사전 생성용)
    /// </summary>
    public static List<string> GetAllCommandKeys()
    {
        return new List<string>(templates.Keys);
    }

    /// <summary>
    /// 특정 VoiceProfile(liveliness, mood)에 맞는 모든 대사 추출
    /// </summary>
    public static List<string> GetAllLinesForProfile(int liveliness, int mood)
    {
        List<string> allLines = new List<string>();

        foreach (var commandKey in templates.Keys)
        {
            string line = GetLine(commandKey, liveliness, mood);
            if (!allLines.Contains(line))
            {
                allLines.Add(line);
            }
        }

        return allLines;
    }
}
