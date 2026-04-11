using UnityEngine;

[CreateAssetMenu(fileName = "VoiceEffectPreset", menuName = "Vivox/Voice Effect Preset")]
public class VoiceEffectPreset : ScriptableObject
{
    [Header("Identification")]
    public string presetName = "New Voice Preset";

    [Header("Activation")]
    public bool enabledByDefault = true;

    [Header("Voice Degradation")]
    [Range(0f, 1f)] public float obstruction = 0f;
    [Range(0f, 1f)] public float stormIntensity = 0f;
    [Range(0f, 1f)] public float interference = 0f;
    [Range(0f, 1f)] public float distancePenalty = 0f;

    [Header("Volume Override")]
    public bool overrideVolumes = false;

    [Tooltip("-50 à 50")]
    [Range(-50, 50)] public int outputVolume = -15;

    [Tooltip("-50 à 50")]
    [Range(-50, 50)] public int channelVolume = -15;

    [Header("Crackle")]
    public bool simulateCrackles = false;

    [Range(0.01f, 1f)] public float crackleCheckInterval = 0.20f;
    [Range(0.01f, 0.5f)] public float crackleMuteDuration = 0.02f;
}