using System;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Vivox;
using Unity.Services.Multiplayer;
//using VivoxUnity;

public class VivoxManager : MonoBehaviour
{
    public static VivoxManager Instance { get; private set; }

    [Header("Vivox")]
    [SerializeField] private string channelPrefix = "match_";

    private bool _vivoxInitialized;
    private bool _vivoxLoggedIn;
    private string _currentChannelName;

    public string CurrentChannelName => _currentChannelName;

    private void Awake()
    {
        Debug.Log("[Vivox] Awake VivoxManager");

        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public async Task InitializeAsync()
    {
        Debug.Log("[Vivox] Début InitializeAsync");

        try
        {
            if (UnityServices.State != ServicesInitializationState.Initialized)
            {
                await UnityServices.InitializeAsync();
                Debug.Log("[Vivox] Unity Services initialisés.");
            }

            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
                Debug.Log("[Vivox] Auth anonyme réussie.");
            }

            if (!_vivoxInitialized)
            {
                await VivoxService.Instance.InitializeAsync();
                _vivoxInitialized = true;
                Debug.Log("[Vivox] Service initialisé.");
            }

            if (!_vivoxLoggedIn)
            {
                await VivoxService.Instance.LoginAsync();
                _vivoxLoggedIn = true;
                Debug.Log("[Vivox] Login réussi.");
            }
        }
        catch (Exception e)
        {
            Debug.LogError("[Vivox] Erreur InitializeAsync : " + e);
        }
    }

    public async Task JoinVoiceForSessionAsync(ISession session)
    {
        if (session == null)
        {
            Debug.LogError("[Vivox] Session null.");
            return;
        }

        string sessionCode = session.Code;
        string sessionId = session.Id;

        Debug.Log($"[Vivox] JoinVoiceForSessionAsync -> id={sessionId}, code={sessionCode}");

        string rawChannelId = !string.IsNullOrWhiteSpace(sessionCode) ? sessionCode : sessionId;
        await JoinVoiceFromCodeAsync(rawChannelId);
    }

    public async Task JoinVoiceFromCodeAsync(string partyCode)
    {
        if (string.IsNullOrWhiteSpace(partyCode))
        {
            Debug.LogError("[Vivox] Party code vide.");
            return;
        }

        await InitializeAsync();

        string channelName = BuildChannelName(partyCode);

        try
        {
            if (!string.IsNullOrEmpty(_currentChannelName))
            {
                if (_currentChannelName == channelName)
                {
                    Debug.Log("[Vivox] Déjà dans le channel : " + _currentChannelName);
                    return;
                }

                await VivoxService.Instance.LeaveChannelAsync(_currentChannelName);
                Debug.Log("[Vivox] Ancien channel quitté : " + _currentChannelName);
            }

            await VivoxService.Instance.JoinGroupChannelAsync(
                channelName,
                ChatCapability.AudioOnly
            );

            _currentChannelName = channelName;
            Debug.Log("[Vivox] Channel rejoint : " + _currentChannelName);
        }
        catch (Exception e)
        {
            Debug.LogError("[Vivox] Erreur JoinVoiceFromCodeAsync : " + e);
        }
    }

    public async Task LeaveVoiceAsync()
    {
        try
        {
            if (string.IsNullOrEmpty(_currentChannelName))
                return;

            await VivoxService.Instance.LeaveChannelAsync(_currentChannelName);
            Debug.Log("[Vivox] Channel quitté : " + _currentChannelName);
            _currentChannelName = null;
        }
        catch (Exception e)
        {
            Debug.LogError("[Vivox] Erreur LeaveVoiceAsync : " + e);
        }
    }

    public void MuteMicrophone(bool muted)
    {
        if (muted)
        {
            VivoxService.Instance.MuteInputDevice();
            Debug.Log("[Vivox] Micro coupé");
        }
        else
        {
            VivoxService.Instance.UnmuteInputDevice();
            Debug.Log("[Vivox] Micro activé");
        }
    }

    private string BuildChannelName(string partyCode)
    {
        return $"{channelPrefix}{partyCode.Trim().ToUpperInvariant()}";
    }
}