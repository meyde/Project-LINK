using UnityEngine;
using Unity.Services.Vivox;

[DisallowMultipleComponent]
public class VivoxVoiceEffectsController : MonoBehaviour
{
    [System.Serializable]
    public class VoiceDegradationState
    {
        [Range(0f, 1f)] public float obstruction = 0f;
        [Range(0f, 1f)] public float stormIntensity = 0f;
        [Range(0f, 1f)] public float interference = 0f;
        [Range(0f, 1f)] public float distancePenalty = 0f;
    }

    [Header("Activation")]
    [SerializeField] private bool autoApply = true;
    [SerializeField] private bool verboseLogs = false;

    [Header("Fallback Base Values")]
    [Range(-50, 50)]
    [SerializeField] private int baseOutputVolume = -15;

    [Range(-50, 50)]
    [SerializeField] private int baseChannelVolume = -15;

    [Header("Current State")]
    [SerializeField] private VoiceDegradationState state = new VoiceDegradationState();

    [Header("Current Preset")]
    [SerializeField] private VoiceEffectPreset activePreset;

    [Header("Curves")]
    [SerializeField]
    private AnimationCurve obstructionCurve =
        new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 0.4f));

    [SerializeField]
    private AnimationCurve stormCurve =
        new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 1f));

    [SerializeField]
    private AnimationCurve interferenceCurve =
        new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 1f));

    [SerializeField]
    private AnimationCurve distanceCurve =
        new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 0.7f));

    [Header("Crackle Simulation")]
    [SerializeField] private bool allowCrackles = true;
    [SerializeField] private float baseCrackleCheckInterval = 0.20f;
    [SerializeField] private float baseCrackleMuteDuration = 0.02f;

    private float _crackleTimer;
    private float _temporaryMuteTimer;

    private int _lastAppliedOutputVolume = int.MinValue;
    private int _lastAppliedChannelVolume = int.MinValue;
    private string _lastAppliedChannelName;

    public VoiceEffectPreset ActivePreset => activePreset;

    private void Update()
    {
        if (!autoApply || VivoxManager.Instance == null)
            return;

        ApplyComputedVoiceState();
        UpdateCrackleSimulation();
    }

    [ContextMenu("Apply Computed Voice State")]
    public void ApplyComputedVoiceState()
    {
        if (VivoxManager.Instance == null)
            return;

        string channelName = VivoxManager.Instance.CurrentChannelName;
        if (string.IsNullOrWhiteSpace(channelName))
            return;

        int outputVolume = baseOutputVolume;
        int channelVolume = baseChannelVolume;

        if (activePreset != null && activePreset.overrideVolumes)
        {
            outputVolume = activePreset.outputVolume;
            channelVolume = activePreset.channelVolume;
        }

        // Chaque paramètre agit différemment
        float obstructionLoss = Mathf.Clamp01(obstructionCurve.Evaluate(state.obstruction));
        float stormLoss = Mathf.Clamp01(stormCurve.Evaluate(state.stormIntensity));
        float interferenceLoss = Mathf.Clamp01(interferenceCurve.Evaluate(state.interference));
        float distanceLoss = Mathf.Clamp01(distanceCurve.Evaluate(state.distancePenalty));

        // Obstruction : baisse douce du channel
        int obstructionChannelPenalty = Mathf.RoundToInt(Mathf.Lerp(0f, -10f, obstructionLoss));

        // Storm : baisse forte du channel et un peu de sortie
        int stormChannelPenalty = Mathf.RoundToInt(Mathf.Lerp(0f, -18f, stormLoss));
        int stormOutputPenalty = Mathf.RoundToInt(Mathf.Lerp(0f, -8f, stormLoss));

        // Interference : légère baisse générale
        int interferenceChannelPenalty = Mathf.RoundToInt(Mathf.Lerp(0f, -7f, interferenceLoss));
        int interferenceOutputPenalty = Mathf.RoundToInt(Mathf.Lerp(0f, -5f, interferenceLoss));

        // Distance : agit surtout sur la sortie
        int distanceOutputPenalty = Mathf.RoundToInt(Mathf.Lerp(0f, -12f, distanceLoss));

        outputVolume += stormOutputPenalty + interferenceOutputPenalty + distanceOutputPenalty;
        channelVolume += obstructionChannelPenalty + stormChannelPenalty + interferenceChannelPenalty;

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

        if (verboseLogs)
        {
            Debug.Log(
                $"[VoiceEffects] preset={(activePreset ? activePreset.presetName : "None")} " +
                $"obs={state.obstruction:F2} storm={state.stormIntensity:F2} " +
                $"interf={state.interference:F2} dist={state.distancePenalty:F2} " +
                $"output={outputVolume} channel={channelVolume}");
        }
    }

    [ContextMenu("Reset To Base Values")]
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

        if (verboseLogs)
            Debug.Log($"[VoiceEffects] Reset base values -> output={output}, channel={channel}");
    }

    public void ApplyPreset(VoiceEffectPreset preset)
    {
        activePreset = preset;

        if (preset == null)
        {
            ClearAllEffects();
            return;
        }

        state.obstruction = Mathf.Clamp01(preset.obstruction);
        state.stormIntensity = Mathf.Clamp01(preset.stormIntensity);
        state.interference = Mathf.Clamp01(preset.interference);
        state.distancePenalty = Mathf.Clamp01(preset.distancePenalty);

        if (verboseLogs)
            Debug.Log("[VoiceEffects] Preset appliqué : " + preset.presetName);

        ApplyComputedVoiceState();
    }

    public void ClearPreset()
    {
        activePreset = null;
        ClearAllEffects();
    }

    public void ClearAllEffects()
    {
        state.obstruction = 0f;
        state.stormIntensity = 0f;
        state.interference = 0f;
        state.distancePenalty = 0f;

        ResetToBaseValues();
    }

    private void UpdateCrackleSimulation()
    {
        if (!allowCrackles || activePreset == null || !activePreset.simulateCrackles)
            return;

        _crackleTimer += Time.deltaTime;

        if (_temporaryMuteTimer > 0f)
        {
            _temporaryMuteTimer -= Time.deltaTime;

            if (_temporaryMuteTimer <= 0f)
                ApplyComputedVoiceState();
        }

        float interval = activePreset.crackleCheckInterval > 0f
            ? activePreset.crackleCheckInterval
            : baseCrackleCheckInterval;

        if (_crackleTimer < interval)
            return;

        _crackleTimer = 0f;

        // Ici l’interference et la storm influencent davantage les coupures
        float crackleChance =
            state.interference * 0.65f +
            state.stormIntensity * 0.35f;

        crackleChance = Mathf.Clamp01(crackleChance);

        if (Random.value <= crackleChance)
        {
            float duration = activePreset.crackleMuteDuration > 0f
                ? activePreset.crackleMuteDuration
                : baseCrackleMuteDuration;

            TriggerShortCrackle(duration);
        }
    }

    private void TriggerShortCrackle(float duration)
    {
        if (VivoxManager.Instance == null)
            return;

        string channelName = VivoxManager.Instance.CurrentChannelName;
        if (string.IsNullOrWhiteSpace(channelName))
            return;

        VivoxService.Instance.SetChannelVolumeAsync(channelName, -50);
        _temporaryMuteTimer = duration;

        if (verboseLogs)
            Debug.Log("[VoiceEffects] Crackle simulé.");
    }
}