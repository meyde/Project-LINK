using Unity.Netcode;
using UnityEngine;

public class NetworkVoiceEffectEventSource : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private VivoxVoiceEffectsController voiceEffects;
    [SerializeField] private VoiceEffectPreset preset;

    [Header("Debug")]
    [SerializeField] private bool verboseLogs = true;

    private readonly NetworkVariable<bool> isEffectActive = new NetworkVariable<bool>(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    public override void OnNetworkSpawn()
    {
        isEffectActive.OnValueChanged += OnEffectStateChanged;

        // Applique l'état actuel quand l'objet spawn localement
        ApplyEffectState(isEffectActive.Value);
    }

    public override void OnNetworkDespawn()
    {
        isEffectActive.OnValueChanged -= OnEffectStateChanged;

        // Nettoyage local quand l'objet disparaît
        if (voiceEffects != null)
            voiceEffects.RemovePreset(this);
    }

    private void OnEffectStateChanged(bool oldValue, bool newValue)
    {
        ApplyEffectState(newValue);
    }

    private void ApplyEffectState(bool active)
    {
        if (voiceEffects == null)
        {
            Debug.LogError("[NetworkVoiceEffectEventSource] Aucun VivoxVoiceEffectsController assigné.");
            return;
        }

        if (preset == null)
        {
            Debug.LogError("[NetworkVoiceEffectEventSource] Aucun preset assigné.");
            return;
        }

        if (active)
        {
            voiceEffects.AddPreset(this, preset);

            if (verboseLogs)
                Debug.Log($"[NetworkVoiceEffectEventSource] Preset activé localement : {preset.presetName}");
        }
        else
        {
            voiceEffects.RemovePreset(this);

            if (verboseLogs)
                Debug.Log($"[NetworkVoiceEffectEventSource] Preset désactivé localement : {preset.presetName}");
        }
    }

    public void SetEventActive(bool active)
    {
        if (IsServer)
        {
            SetEffectStateServer(active);
            return;
        }

        SetEventActiveServerRpc(active);
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void SetEventActiveServerRpc(bool active)
    {
        SetEffectStateServer(active);
    }

    private void SetEffectStateServer(bool active)
    {
        if (isEffectActive.Value == active)
            return;

        isEffectActive.Value = active;

        if (verboseLogs)
            Debug.Log($"[NetworkVoiceEffectEventSource] Etat réseau changé -> {active}");
    }

    public void ActivateEffect()
    {
        SetEventActive(true);
    }

    public void DeactivateEffect()
    {
        SetEventActive(false);
    }
}