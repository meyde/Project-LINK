using UnityEngine;

public class VoiceEffectEventSource : MonoBehaviour
{
    [SerializeField] private VivoxVoiceEffectsController voiceEffects;
    [SerializeField] private VoiceEffectPreset preset;

    public void SetEventActive(bool active)
    {
        if (voiceEffects == null)
            return;

        if (active)
            voiceEffects.ApplyPreset(preset);
        else
            voiceEffects.ClearPreset();
    }
    public void ActivateTornado()
    {
        voiceEffects.ApplyPreset(preset);
    }

    public void DeactivateTornado()
    {
        voiceEffects.ClearPreset();
    }
}