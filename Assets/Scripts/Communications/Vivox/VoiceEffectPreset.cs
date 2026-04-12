using UnityEngine;

[CreateAssetMenu(fileName = "VoiceEffectPreset", menuName = "Vivox/Voice Effect Preset")]
public class VoiceEffectPreset : ScriptableObject
{
    [Header("Identification")]
    public string presetName = "New Voice Preset";

    [Tooltip("Si plusieurs presets override les volumes, la plus haute priorité gagne.")]
    public int priority = 0;

    [Tooltip("Poids global du preset dans le mix. 1 = normal.")]
    [Range(0f, 1f)] public float weight = 1f;

    [Header("Voice Degradation")]
    [Range(0f, 1f)] public float obstruction = 0f;
    [Range(0f, 1f)] public float stormIntensity = 0f;
    [Range(0f, 1f)] public float interference = 0f;
    [Range(0f, 1f)] public float distancePenalty = 0f;

    [Header("Volume Override")]
    public bool overrideVolumes = false;
    [Range(-50, 50)] public int outputVolume = -15;
    [Range(-50, 50)] public int channelVolume = -15;

    [Header("Crackle")]
    public bool simulateCrackles = false;
    [Range(0.01f, 1f)] public float crackleCheckInterval = 0.20f;
    [Range(0.01f, 0.5f)] public float crackleMuteDuration = 0.02f;

    [Header("Noise Layer")]
    public bool playNoise = false;
    public AudioClip noiseClip;
    [Range(0f, 1f)] public float noiseVolume = 0.5f;
    public bool loopNoise = true;

    [Header("Debug")]
    public bool verbose = false;
}