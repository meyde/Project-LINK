using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class VoiceTimeLimiter : NetworkBehaviour
{
    public static VoiceTimeLimiter LocalInstance { get; private set; }

    [Header("Voice Time")]
    [SerializeField] private float maxVoiceDuration = 60f;

    [Header("Recharge")]
    [SerializeField] private float rechargeAmount = 10f;
    [SerializeField] private float rechargeDuration = 5f;

    [Header("Scene Restriction")]
    [SerializeField] private string targetSceneName = "GameScene";

    [Header("Audio Warning")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip warningSound;
    [SerializeField] private float triggerTime = 10f;
    [Range(0f, 1f)]
    [SerializeField] private float warningVolume = 1f;

    [Header("Debug")]
    [SerializeField] private bool verboseLogs = true;

    private PlayerLobbyData localLobbyData;

    private float remainingVoiceTime;
    private bool timerRunning;
    private bool voiceLocked;
    private bool initialized;
    private bool isActiveForLocalPlayer;

    private bool warningPlayed;
    private bool requiresRechargeBeforeUnmute;

    public float RemainingVoiceTime => remainingVoiceTime;
    public float MaxVoiceDuration => maxVoiceDuration;
    public bool IsTimerRunning => timerRunning;
    public bool IsVoiceLocked => voiceLocked;
    public bool IsActiveForLocalPlayer => isActiveForLocalPlayer;
    public bool IsInCriticalZone => remainingVoiceTime <= triggerTime && !voiceLocked;
    public float CriticalTriggerTime => triggerTime;

    public bool CanUnmute
    {
        get
        {
            if (voiceLocked)
                return false;

            if (requiresRechargeBeforeUnmute)
                return remainingVoiceTime >= triggerTime;

            return remainingVoiceTime > 0f;
        }
    }

    private float RechargePerSecond
    {
        get
        {
            if (rechargeDuration <= 0f)
                return 0f;

            return rechargeAmount / rechargeDuration;
        }
    }

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
            return;

        LocalInstance = this;
        remainingVoiceTime = maxVoiceDuration;
        requiresRechargeBeforeUnmute = false;
        isActiveForLocalPlayer = false;

        StartCoroutine(WaitForTargetSceneAndInitialize());
    }

    public override void OnNetworkDespawn()
    {
        if (LocalInstance == this)
            LocalInstance = null;
    }

    private void SetupAudioSource()
    {
        if (audioSource != null)
            return;

        audioSource = FindFirstObjectByType<GameManagerLocal>()?.GetComponent<AudioSource>();

        if (audioSource == null)
            Debug.LogWarning("[VoiceLimiter] GameManagerLocal / AudioSource introuvable.");
    }

    private IEnumerator WaitForTargetSceneAndInitialize()
    {
        while (SceneManager.GetActiveScene().name != targetSceneName)
            yield return null;

        if (initialized)
            yield break;

        initialized = true;
        yield return InitializeForLocalPlayer();
    }

    private IEnumerator InitializeForLocalPlayer()
    {
        yield return WaitForLocalLobbyData();

        SetupAudioSource();

        if (localLobbyData == null)
        {
            Debug.LogError("[VoiceLimiter] Impossible de trouver PlayerLobbyData local.");
            yield break;
        }

        while (localLobbyData.SelectedRole.Value == PlayerRole.None)
            yield return null;

        if (localLobbyData.SelectedRole.Value != PlayerRole.Space)
        {
            if (verboseLogs)
                Debug.Log("[VoiceLimiter] Joueur local non Space -> système inactif.");

            isActiveForLocalPlayer = false;
            yield break;
        }

        if (verboseLogs)
            Debug.Log("[VoiceLimiter] Joueur Space détecté dans GameScene -> initialisation.");

        isActiveForLocalPlayer = true;

        ForceMicrophoneState(false);
        timerRunning = true;

        StartCoroutine(VoiceRoutine());
    }

    private IEnumerator WaitForLocalLobbyData()
    {
        while (localLobbyData == null)
        {
            PlayerLobbyData[] lobbyPlayers = FindObjectsByType<PlayerLobbyData>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None
            );

            for (int i = 0; i < lobbyPlayers.Length; i++)
            {
                if (lobbyPlayers[i].OwnerClientId == OwnerClientId)
                {
                    localLobbyData = lobbyPlayers[i];
                    break;
                }
            }

            if (localLobbyData == null)
                yield return null;
        }
    }

    private IEnumerator VoiceRoutine()
    {
        while (true)
        {
            if (timerRunning && !voiceLocked)
            {
                remainingVoiceTime -= Time.deltaTime;

                if (!warningPlayed && remainingVoiceTime <= triggerTime)
                {
                    warningPlayed = true;

                    if (audioSource != null && warningSound != null)
                    {
                        audioSource.clip = warningSound;
                        audioSource.volume = warningVolume;
                        audioSource.loop = false;
                        audioSource.Play();
                    }

                    if (verboseLogs)
                        Debug.Log($"[VoiceLimiter] Warning sonore déclenché à {triggerTime}s restantes.");
                }

                if (remainingVoiceTime <= 0f)
                {
                    remainingVoiceTime = 0f;
                    timerRunning = false;
                    voiceLocked = true;
                    requiresRechargeBeforeUnmute = false;

                    ForceMicrophoneState(true);

                    if (verboseLogs)
                        Debug.Log("[VoiceLimiter] Temps écoulé -> parole verrouillée et batterie vide.");

                    yield return null;
                    continue;
                }
            }
            else if (!timerRunning && !voiceLocked && remainingVoiceTime > 0f)
            {
                float rechargeRate = RechargePerSecond;

                if (rechargeRate > 0f && remainingVoiceTime < maxVoiceDuration)
                {
                    remainingVoiceTime += rechargeRate * Time.deltaTime;
                    remainingVoiceTime = Mathf.Min(remainingVoiceTime, maxVoiceDuration);

                    if (remainingVoiceTime > triggerTime)
                        warningPlayed = false;

                    if (requiresRechargeBeforeUnmute && remainingVoiceTime >= triggerTime)
                    {
                        requiresRechargeBeforeUnmute = false;

                        if (verboseLogs)
                            Debug.Log("[VoiceLimiter] Recharge suffisante -> unmute à nouveau autorisé.");
                    }
                }
            }

            yield return null;
        }
    }

    public bool TrySetMuteState(bool muted)
    {
        if (!IsOwner)
            return false;

        if (muted)
        {
            if (audioSource != null && audioSource.isPlaying)
                audioSource.Stop();

            if (remainingVoiceTime <= triggerTime)
            {
                requiresRechargeBeforeUnmute = true;

                if (verboseLogs)
                    Debug.Log("[VoiceLimiter] Mute en zone critique -> unmute bloqué jusqu'à recharge au seuil.");
            }

            timerRunning = false;
            ForceMicrophoneState(true);

            if (verboseLogs)
                Debug.Log("[VoiceLimiter] Mute manuel -> consommation stoppée, recharge autorisée si batterie non vide.");

            return true;
        }

        if (!CanUnmute)
        {
            ForceMicrophoneState(true);

            if (verboseLogs)
            {
                if (voiceLocked)
                    Debug.Log("[VoiceLimiter] Unmute refusé -> parole verrouillée.");
                else if (requiresRechargeBeforeUnmute)
                    Debug.Log($"[VoiceLimiter] Unmute refusé -> recharge requise jusqu'à au moins {triggerTime} secondes.");
                else
                    Debug.Log("[VoiceLimiter] Unmute refusé -> temps restant nul.");
            }

            return false;
        }

        ForceMicrophoneState(false);
        timerRunning = true;

        if (verboseLogs)
            Debug.Log("[VoiceLimiter] Unmute autorisé -> consommation reprend.");

        return true;
    }

    private void ForceMicrophoneState(bool muted)
    {
        if (VivoxManager.Instance == null)
            return;

        VivoxManager.Instance.MuteMicrophone(muted);
    }
}