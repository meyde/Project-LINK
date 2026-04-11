using System.Threading.Tasks;
using UnityEngine;
using Unity.Services.Vivox;

[DisallowMultipleComponent]
public class VivoxVoiceSettings : MonoBehaviour
{
    [Header("Auto Apply")]
    [SerializeField] private bool applyOnStart = true;

    [Header("Keyboard Mute")]
    [SerializeField] private bool enableKeyboardMuteToggle = true;
    [SerializeField] private KeyCode muteToggleKey = KeyCode.K;
    [SerializeField] private bool debugKeyLogs = true;

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

    private void Update()
    {
        if (!enableKeyboardMuteToggle)
            return;

        if (!Input.GetKeyDown(muteToggleKey))
            return;

        if (debugKeyLogs)
            Debug.Log("[VoiceSettings] Touche détectée : " + muteToggleKey);

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

        // ON GARDE seulement le volume d'entrée ici
        VivoxService.Instance.SetInputDeviceVolume(inputDeviceVolume);
        Log("[VoiceSettings] Volume entrée appliqué : " + inputDeviceVolume);

        // ON GARDE seulement le mute micro ici
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

        await Task.CompletedTask;
    }

    public void ToggleMuteImmediate()
    {
        muteMicrophone = !muteMicrophone;

        if (muteMicrophone)
        {
            VivoxService.Instance.MuteInputDevice();
            Debug.Log("[VoiceSettings] ToggleMuteImmediate -> MUTED");
        }
        else
        {
            VivoxService.Instance.UnmuteInputDevice();
            Debug.Log("[VoiceSettings] ToggleMuteImmediate -> UNMUTED");
        }
    }

    public async void SetMute(bool value)
    {
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