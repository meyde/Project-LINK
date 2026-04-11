using System.Threading.Tasks;
using UnityEngine;
using Unity.Services.Vivox;

[DisallowMultipleComponent]
public class VivoxVoiceSettings : MonoBehaviour
{
    [Header("Auto Apply")]
    [SerializeField] private bool applyOnStart = true;
    [SerializeField] private bool applyContinuouslyInEditor = true;
    [SerializeField] private bool reapplyWhenChannelChanges = true;

    [Header("Device")]
    [Tooltip("-50 à 50. 0 = volume normal Vivox pour l'entrée.")]
    [Range(-50, 50)]
    [SerializeField] private int inputDeviceVolume = -10;

    [Tooltip("-50 à 50. 0 = volume normal Vivox pour la sortie.")]
    [Range(-50, 50)]
    [SerializeField] private int outputDeviceVolume = -15;

    [Header("Channel")]
    [Tooltip("-50 à 50. 0 = normal. Affecte le channel courant.")]
    [Range(-50, 50)]
    [SerializeField] private int currentChannelVolume = -15;

    [Header("Micro")]
    [SerializeField] private bool muteMicrophone = false;

    [Header("Debug")]
    [SerializeField] private bool verboseLogs = true;

    private int _lastInputDeviceVolume;
    private int _lastOutputDeviceVolume;
    private int _lastCurrentChannelVolume;
    private bool _lastMuteMicrophone;
    private string _lastChannelName;

    private async void Start()
    {
        if (applyOnStart)
            await ApplyAllAsync();
    }

    private async void Update()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying || !applyContinuouslyInEditor)
            return;

        bool valuesChanged = HasValuesChanged();
        bool channelChanged = false;

        string currentChannel = GetCurrentChannelName();
        if (reapplyWhenChannelChanges && currentChannel != _lastChannelName)
            channelChanged = true;

        if (valuesChanged || channelChanged)
            await ApplyAllAsync();
#endif
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
            CacheCurrentValues();
            return;
        }

        string channelName = GetCurrentChannelName();

        // Volumes device
        VivoxService.Instance.SetInputDeviceVolume(inputDeviceVolume);
        Log("[VoiceSettings] Volume entrée appliqué : " + inputDeviceVolume);

        VivoxService.Instance.SetOutputDeviceVolume(outputDeviceVolume);
        Log("[VoiceSettings] Volume sortie appliqué : " + outputDeviceVolume);

        // Mute micro
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

        // Volume channel
        if (!string.IsNullOrWhiteSpace(channelName))
        {
            await VivoxService.Instance.SetChannelVolumeAsync(channelName, currentChannelVolume);
            Log("[VoiceSettings] Volume du channel appliqué : " + currentChannelVolume + " sur " + channelName);
        }
        else
        {
            Log("[VoiceSettings] Aucun channel Vivox courant, volume de channel non appliqué.");
        }

        CacheCurrentValues();
        _lastChannelName = channelName;
    }

    public async void SetInputVolume(int value)
    {
        inputDeviceVolume = Mathf.Clamp(value, -50, 50);
        await ApplyAllAsync();
    }

    public async void SetOutputVolume(int value)
    {
        outputDeviceVolume = Mathf.Clamp(value, -50, 50);
        await ApplyAllAsync();
    }

    public async void SetChannelVolume(int value)
    {
        currentChannelVolume = Mathf.Clamp(value, -50, 50);
        await ApplyAllAsync();
    }

    public async void SetMute(bool value)
    {
        muteMicrophone = value;
        await ApplyAllAsync();
    }

    private string GetCurrentChannelName()
    {
        return VivoxManager.Instance != null ? VivoxManager.Instance.CurrentChannelName : null;
    }

    private bool HasValuesChanged()
    {
        return _lastInputDeviceVolume != inputDeviceVolume
            || _lastOutputDeviceVolume != outputDeviceVolume
            || _lastCurrentChannelVolume != currentChannelVolume
            || _lastMuteMicrophone != muteMicrophone;
    }

    private void CacheCurrentValues()
    {
        _lastInputDeviceVolume = inputDeviceVolume;
        _lastOutputDeviceVolume = outputDeviceVolume;
        _lastCurrentChannelVolume = currentChannelVolume;
        _lastMuteMicrophone = muteMicrophone;
    }

    private void Log(string message)
    {
        if (verboseLogs)
            Debug.Log(message);
    }
}