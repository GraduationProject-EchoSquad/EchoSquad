// C:/UnityStudy/EchoSquad/Assets/Scripts/VoiceProfile.cs

using UnityEngine;
public enum VoiceGender
{
    Male,
    Female
}
 
public enum VoiceProperty
{
    VeryLow,
    Low,
    Moderate,
    High,
    VeryHigh
}
 
public static class VoiceEnumExtensions
{
    /// <summary>
    /// Converts the enum to the string format required by the SparkTTS API.
    /// </summary>
    public static string ToApiString(this VoiceGender gender)
    {
        return gender.ToString().ToLower(); // "male", "female"
    }
 
    /// <summary>
    /// Converts the enum to the string format required by the SparkTTS API.
    /// </summary>
    public static string ToApiString(this VoiceProperty property)
    {
        switch (property)
        {
            case VoiceProperty.VeryLow:
                return "very_low";
            case VoiceProperty.Low:
                return "low";
            case VoiceProperty.Moderate:
                return "moderate";
            case VoiceProperty.High:
                return "high";
            case VoiceProperty.VeryHigh:
                return "very_high";
            default:
                return "moderate";
        }
    }
}

[CreateAssetMenu(fileName = "NewVoiceProfile", menuName = "EchoSquad/Voice Profile")]
public class VoiceProfile : ScriptableObject
{
    [Header("TTS Voice Settings")]
    [Tooltip("e.g., male, female")]
    public VoiceGender gender = VoiceGender.Male;

    [Tooltip("e.g., low, moderate, high")]
    public VoiceProperty pitch = VoiceProperty.Moderate;

    [Tooltip("e.g., slow, moderate, fast")]
    public VoiceProperty speed = VoiceProperty.Moderate;
}