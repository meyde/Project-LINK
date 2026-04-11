using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Vivox;

[DisallowMultipleComponent]
public class VivoxVoiceEffectsController : MonoBehaviour
{
    [System.Serializable]
    public class MixedVoiceState
    {
        [Range(0f, 1f)] public float obstruction;
        [Range(0f, 1f)] public float stormIntensity;
        [Range(0f, 1f)] public float interference;
        [Range(0f, 1f)] public float distancePenalty;

        public bool overrideVolumes;
        public int outputVolume;
        public int channelVolume;

        public bool simulateCrackles;
        public float crackleCheckInterval;
        public float crackleMuteDuration;

        public void Clear()
        {
            obstruction = 0f;
            stormIntensity = 0f;
            interference = 0f;
            distancePenalty = 0f;

            overrideVolumes = false;
            outputVolume = 0;
            channelVolume = 0;

            simulateCrackles = false;
            crackleCheckInterval = 0f;
            crackleMuteDuration = 0f;
        }
    }

    private class ActivePresetEntry
    {
        public Object source;
        public VoiceEffectPreset preset;
    }

    [Header("Controller")]
    [SerializeField] private bool autoApply = true;
    [SerializeField] private bool verboseLogs = false;

    [Header("Base Voice Volumes")]
    [Range(-50, 50)]
    [SerializeField] private int baseOutputVolume = 0;

    [Range(-50, 50)]
    [SerializeField] private int baseChannelVolume = 0;

    [Header("Noise")]
    [SerializeField] private VoiceEffectNoisePlayer noisePlayer;

    [Header("Noise Intensity Curves")]
    [SerializeField]
    private AnimationCurve stormNoiseCurve =
        new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 1f));

    [SerializeField]
    private AnimationCurve interferenceNoiseCurve =
        new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 1f));

    [SerializeField]
    private AnimationCurve obstructionNoiseCurve =
        new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 0.35f));

    [SerializeField]
    private AnimationCurve distanceNoiseCurve =
        new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 0.15f));

    [Header("Storm Flutter")]
    [SerializeField] private bool stormAddsStaticFlutter = true;
    [SerializeField] private float stormFlutterSpeed = 22f;
    [SerializeField] private float stormFlutterVolumeAmount = 0.35f;
    [SerializeField] private float stormFlutterPitchAmount = 0.20f;

    [Header("Interference Flutter")]
    [SerializeField] private bool interferenceAddsRadioFlutter = true;
    [SerializeField] private float interferenceFlutterSpeed = 38f;
    [SerializeField] private float interferenceFlutterVolumeAmount = 0.55f;
    [SerializeField] private float interferenceFlutterPitchAmount = 0.30f;
    [SerializeField] private float interferenceHardGateAmount = 0.35f;

    [Header("Crackles")]
    [SerializeField] private bool allowCrackles = true;
    [SerializeField] private bool cracklesAlsoDipVoice = true;
    [SerializeField] private int crackleDipChannelVolume = -8;
    [SerializeField] private float defaultCrackleCheckInterval = 0.20f;
    [SerializeField] private float defaultCrackleMuteDuration = 0.02f;

    [Header("Runtime Debug")]
    [SerializeField] private MixedVoiceState currentMixedState = new MixedVoiceState();
    [SerializeField] private int activePresetCount = 0;
    [SerializeField, Range(0f, 1f)] private float currentNoiseIntensity = 0f;

    private readonly List<ActivePresetEntry> _activePresets = new List<ActivePresetEntry>();

    private float _crackleTimer;
    private float _temporaryMuteTimer;

    private int _lastAppliedOutputVolume = int.MinValue;
    private int _lastAppliedChannelVolume = int.MinValue;
    private string _lastAppliedChannelName;

    private void Update()
    {
        if (!autoApply || VivoxManager.Instance == null)
            return;

        RebuildMixedState();
        ApplyComputedVoiceState();
        UpdateCrackleSimulation();
    }

    public void AddPreset(Object source, VoiceEffectPreset preset)
    {
        if (source == null || preset == null)
            return;

        int existingIndex = FindEntryIndexBySource(source);
        if (existingIndex >= 0)
        {
            _activePresets[existingIndex].preset = preset;
        }
        else
        {
            _activePresets.Add(new ActivePresetEntry
            {
                source = source,
                preset = preset
            });
        }

        activePresetCount = _activePresets.Count;

        if (verboseLogs || preset.verbose)
            Debug.Log($"[VoiceEffects] Preset ajouté : {preset.presetName}");

        RebuildMixedState();
        ApplyComputedVoiceState();
    }

    public void RemovePreset(Object source)
    {
        if (source == null)
            return;

        int index = FindEntryIndexBySource(source);
        if (index < 0)
            return;

        _activePresets.RemoveAt(index);
        activePresetCount = _activePresets.Count;

        RebuildMixedState();
        ApplyComputedVoiceState();
    }

    public void ClearAllPresets()
    {
        _activePresets.Clear();
        activePresetCount = 0;
        currentMixedState.Clear();
        currentNoiseIntensity = 0f;

        ResetToBaseValues();

        if (noisePlayer != null)
            noisePlayer.StopNoise();
    }

    public void RebuildMixedState()
    {
        currentMixedState.Clear();
        activePresetCount = _activePresets.Count;

        if (_activePresets.Count == 0)
            return;

        float totalWeight = 0f;

        VoiceEffectPreset highestPriorityVolumePreset = null;
        float shortestCrackleInterval = float.MaxValue;
        float longestCrackleDuration = 0f;
        bool anyCrackles = false;

        for (int i = 0; i < _activePresets.Count; i++)
        {
            VoiceEffectPreset preset = _activePresets[i].preset;
            if (preset == null)
                continue;

            float w = Mathf.Clamp01(preset.weight);
            totalWeight += w;

            currentMixedState.obstruction += preset.obstruction * w;
            currentMixedState.stormIntensity += preset.stormIntensity * w;
            currentMixedState.interference += preset.interference * w;
            currentMixedState.distancePenalty += preset.distancePenalty * w;

            if (preset.overrideVolumes)
            {
                if (highestPriorityVolumePreset == null || preset.priority > highestPriorityVolumePreset.priority)
                    highestPriorityVolumePreset = preset;
            }

            if (preset.simulateCrackles)
            {
                anyCrackles = true;
                shortestCrackleInterval = Mathf.Min(shortestCrackleInterval, preset.crackleCheckInterval);
                longestCrackleDuration = Mathf.Max(longestCrackleDuration, preset.crackleMuteDuration);
            }
        }

        if (totalWeight > 0f)
        {
            currentMixedState.obstruction = Mathf.Clamp01(currentMixedState.obstruction / totalWeight);
            currentMixedState.stormIntensity = Mathf.Clamp01(currentMixedState.stormIntensity / totalWeight);
            currentMixedState.interference = Mathf.Clamp01(currentMixedState.interference / totalWeight);
            currentMixedState.distancePenalty = Mathf.Clamp01(currentMixedState.distancePenalty / totalWeight);
        }

        if (highestPriorityVolumePreset != null)
        {
            currentMixedState.overrideVolumes = true;
            currentMixedState.outputVolume = highestPriorityVolumePreset.outputVolume;
            currentMixedState.channelVolume = highestPriorityVolumePreset.channelVolume;
        }

        if (anyCrackles)
        {
            currentMixedState.simulateCrackles = true;
            currentMixedState.crackleCheckInterval = shortestCrackleInterval == float.MaxValue
                ? defaultCrackleCheckInterval
                : shortestCrackleInterval;
            currentMixedState.crackleMuteDuration = Mathf.Max(longestCrackleDuration, defaultCrackleMuteDuration);
        }

        if (verboseLogs)
        {
            Debug.Log(
                $"[VoiceEffects] Mixed -> count={activePresetCount} " +
                $"storm={currentMixedState.stormIntensity:F2} interf={currentMixedState.interference:F2}");
        }
    }

    public void ApplyComputedVoiceState()
    {
        if (VivoxManager.Instance == null)
            return;

        string channelName = VivoxManager.Instance.CurrentChannelName;
        if (string.IsNullOrWhiteSpace(channelName))
            return;

        int outputVolume = currentMixedState.overrideVolumes
            ? currentMixedState.outputVolume
            : baseOutputVolume;

        int channelVolume = currentMixedState.overrideVolumes
            ? currentMixedState.channelVolume
            : baseChannelVolume;

        outputVolume = Mathf.Clamp(outputVolume, -50, 50);
        channelVolume = Mathf.Clamp(channelVolume, -50, 50);

        if (_lastAppliedOutputVolume != outputVolume)
        {
            VivoxService.Instance.SetOutputDeviceVolume(outputVolume);
            _lastAppliedOutputVolume = outputVolume;
        }

        if (_lastAppliedChannelVolume != channelVolume || _lastAppliedChannelName != channelName)
        {
            VivoxService.Instance.SetChannelVolumeAsync(channelName, channelVolume);
            _lastAppliedChannelVolume = channelVolume;
            _lastAppliedChannelName = channelName;
        }

        ApplyNoiseLayer();

        if (verboseLogs)
        {
            Debug.Log($"[VoiceEffects] Applied -> output={outputVolume} channel={channelVolume} noise={currentNoiseIntensity:F2}");
        }
    }

    private void ApplyNoiseLayer()
    {
        if (noisePlayer == null)
            return;

        VoiceEffectPreset topNoisePreset = GetHighestPriorityNoisePreset();

        if (topNoisePreset == null || !topNoisePreset.playNoise || topNoisePreset.noiseClip == null)
        {
            currentNoiseIntensity = 0f;
            noisePlayer.StopNoise();
            return;
        }

        float stormNoise = stormNoiseCurve.Evaluate(currentMixedState.stormIntensity);
        float interferenceNoise = interferenceNoiseCurve.Evaluate(currentMixedState.interference);
        float obstructionNoise = obstructionNoiseCurve.Evaluate(currentMixedState.obstruction);
        float distanceNoise = distanceNoiseCurve.Evaluate(currentMixedState.distancePenalty);

        float combinedNoise =
            stormNoise * 0.45f +
            interferenceNoise * 0.40f +
            obstructionNoise * 0.10f +
            distanceNoise * 0.05f;

        currentNoiseIntensity = Mathf.Clamp01(combinedNoise);

        float finalNoiseVolume = Mathf.Clamp01(topNoisePreset.noiseVolume * currentNoiseIntensity);
        float noisePitch = 1f;

        if (stormAddsStaticFlutter)
        {
            float stormStrength = currentMixedState.stormIntensity;
            float t = Time.time * stormFlutterSpeed;

            float stormA = Mathf.PerlinNoise(t, 0f);
            float stormB = Mathf.PerlinNoise(0f, t * 0.73f);
            float stormFlutter = ((stormA + stormB) * 0.5f - 0.5f) * 2f;

            float volumeFlutter = 1f + (stormFlutter * stormFlutterVolumeAmount * stormStrength);
            finalNoiseVolume *= Mathf.Clamp(volumeFlutter, 0.65f, 1.35f);

            noisePitch += stormFlutter * stormFlutterPitchAmount * stormStrength;
        }

        if (interferenceAddsRadioFlutter)
        {
            float interferenceStrength = currentMixedState.interference;
            float t = Time.time * interferenceFlutterSpeed;

            float radioA = Mathf.PerlinNoise(t * 1.37f, 3.17f);
            float radioB = Mathf.PerlinNoise(5.91f, t * 0.83f);
            float radioFlutter = ((radioA + radioB) * 0.5f - 0.5f) * 2f;

            float hardGate = Mathf.PerlinNoise(t * 2.7f, 9.1f);
            float gateFactor = Mathf.Lerp(1f, Mathf.Lerp(1f - interferenceHardGateAmount, 1f, hardGate), interferenceStrength);

            finalNoiseVolume *= Mathf.Clamp01(1f + (radioFlutter * interferenceFlutterVolumeAmount * interferenceStrength));
            finalNoiseVolume *= gateFactor;

            noisePitch += radioFlutter * interferenceFlutterPitchAmount * interferenceStrength;
        }

        finalNoiseVolume = Mathf.Clamp01(finalNoiseVolume);

        if (finalNoiseVolume <= 0.001f)
        {
            noisePlayer.StopNoise();
            return;
        }

        noisePlayer.PlayNoise(
            topNoisePreset.noiseClip,
            finalNoiseVolume,
            topNoisePreset.loopNoise,
            noisePitch
        );
    }

    private VoiceEffectPreset GetHighestPriorityNoisePreset()
    {
        VoiceEffectPreset best = null;

        for (int i = 0; i < _activePresets.Count; i++)
        {
            VoiceEffectPreset preset = _activePresets[i].preset;
            if (preset == null || !preset.playNoise || preset.noiseClip == null)
                continue;

            if (best == null || preset.priority > best.priority)
                best = preset;
        }

        return best;
    }

    public void ResetToBaseValues()
    {
        if (VivoxManager.Instance == null)
            return;

        string channelName = VivoxManager.Instance.CurrentChannelName;
        if (string.IsNullOrWhiteSpace(channelName))
            return;

        int output = Mathf.Clamp(baseOutputVolume, -50, 50);
        int channel = Mathf.Clamp(baseChannelVolume, -50, 50);

        VivoxService.Instance.SetOutputDeviceVolume(output);
        VivoxService.Instance.SetChannelVolumeAsync(channelName, channel);

        _lastAppliedOutputVolume = output;
        _lastAppliedChannelVolume = channel;
        _lastAppliedChannelName = channelName;

        if (noisePlayer != null)
            noisePlayer.StopNoise();

        currentNoiseIntensity = 0f;
    }

    private void UpdateCrackleSimulation()
    {
        if (!allowCrackles || !currentMixedState.simulateCrackles)
            return;

        _crackleTimer += Time.deltaTime;

        if (_temporaryMuteTimer > 0f)
        {
            _temporaryMuteTimer -= Time.deltaTime;

            if (_temporaryMuteTimer <= 0f)
                ApplyComputedVoiceState();
        }

        if (_crackleTimer < currentMixedState.crackleCheckInterval)
            return;

        _crackleTimer = 0f;

        float crackleChance =
            currentMixedState.stormIntensity * 0.65f +
            currentMixedState.interference * 0.35f;

        crackleChance = Mathf.Clamp01(crackleChance);

        if (Random.value <= crackleChance)
            TriggerShortCrackle(currentMixedState.crackleMuteDuration);
    }

    private void TriggerShortCrackle(float duration)
    {
        _temporaryMuteTimer = duration;

        if (!cracklesAlsoDipVoice || VivoxManager.Instance == null)
            return;

        string channelName = VivoxManager.Instance.CurrentChannelName;
        if (string.IsNullOrWhiteSpace(channelName))
            return;

        VivoxService.Instance.SetChannelVolumeAsync(channelName, crackleDipChannelVolume);

        if (verboseLogs)
            Debug.Log("[VoiceEffects] Crackle simulé.");
    }

    private int FindEntryIndexBySource(Object source)
    {
        for (int i = 0; i < _activePresets.Count; i++)
        {
            if (_activePresets[i].source == source)
                return i;
        }

        return -1;
    }
}