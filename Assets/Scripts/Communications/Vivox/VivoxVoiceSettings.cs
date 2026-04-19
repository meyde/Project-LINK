using System.Threading.Tasks;
using UnityEngine;
using Unity.Services.Vivox;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
public class VivoxVoiceSettings : MonoBehaviour
{
    [Header("Auto Apply")]
    [SerializeField] private bool applyOnStart = true;

    [Header("Input Device Only")]
    [Range(-50, 50)]
    [SerializeField] private int inputDeviceVolume = 0;

    [Header("Micro")]
    [SerializeField] private bool muteMicrophone = false;

    [Header("Debug")]
    [SerializeField] private bool verboseLogs = true;

    public bool IsMicrophoneMuted => muteMicrophone;

    private async void Start()
    {
        Debug.Log("[VoiceSettings] Start OK");

        if (applyOnStart)
            await ApplyAllAsync();
    }

    public void OnToggleMute()
    {
        ToggleMuteImmediate();
    }

    [ContextMenu("Apply Voice Settings")]
    public async void ApplyAllFromContextMenu()
    {
        await ApplyAllAsync();
    }

    public async Task ApplyAllAsync()
    {
        if (VivoxManager.Instance == null)
        {
            Log("[VoiceSettings] Aucun VivoxManager dans la scène.");
            return;
        }

        VivoxService.Instance.SetInputDeviceVolume(inputDeviceVolume);
        Log("[VoiceSettings] Volume entrée appliqué : " + inputDeviceVolume);

        // Si on veut unmute, on demande d'abord l'autorisation au limiter
        if (!muteMicrophone && VoiceTimeLimiter.LocalInstance != null)
        {
            bool allowed = VoiceTimeLimiter.LocalInstance.TrySetMuteState(false);
            muteMicrophone = !allowed;

            if (!allowed)
                Log("[VoiceSettings] Unmute refusé par VoiceTimeLimiter.");
        }
        else
        {
            if (muteMicrophone)
            {
                VivoxService.Instance.MuteInputDevice();
                Log("[VoiceSettings] Micro coupé.");
            }
            else
            {
                VivoxService.Instance.UnmuteInputDevice();
                Log("[VoiceSettings] Micro activé.");
            }
        }

        await Task.CompletedTask;
    }

    public void ToggleMuteImmediate()
    {
        bool targetMutedState = !muteMicrophone;

        if (VoiceTimeLimiter.LocalInstance != null)
        {
            bool success = VoiceTimeLimiter.LocalInstance.TrySetMuteState(targetMutedState);

            if (!success && !targetMutedState)
            {
                muteMicrophone = true;
                Debug.Log("[VoiceSettings] ToggleMuteImmediate -> UNMUTE REFUSÉ");
                return;
            }
        }
        else
        {
            if (targetMutedState)
                VivoxService.Instance.MuteInputDevice();
            else
                VivoxService.Instance.UnmuteInputDevice();
        }

        muteMicrophone = targetMutedState;

        if (muteMicrophone)
            Debug.Log("[VoiceSettings] ToggleMuteImmediate -> MUTED");
        else
            Debug.Log("[VoiceSettings] ToggleMuteImmediate -> UNMUTED");
    }

    public async void SetMute(bool value)
    {
        if (VoiceTimeLimiter.LocalInstance != null)
        {
            bool success = VoiceTimeLimiter.LocalInstance.TrySetMuteState(value);

            if (!success && !value)
            {
                muteMicrophone = true;
                return;
            }
        }

        muteMicrophone = value;
        await ApplyAllAsync();
    }

    public async void SetInputVolume(int value)
    {
        inputDeviceVolume = Mathf.Clamp(value, -50, 50);
        await ApplyAllAsync();
    }

    private void Log(string message)
    {
        if (verboseLogs)
            Debug.Log(message);
    }
}