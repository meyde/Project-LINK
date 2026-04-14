using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class VoiceTimeLimiter : NetworkBehaviour
{
    public static VoiceTimeLimiter LocalInstance { get; private set; }

    [Header("Voice Time")]
    [SerializeField] private float maxVoiceDuration = 60f;

    [Header("Scene Restriction")]
    [SerializeField] private string targetSceneName = "GameScene";

    [Header("Bar Prefab")]
    [SerializeField] private GameObject barPrefab;

    [Header("Bar Position In Camera View")]
    [SerializeField] private Vector2 viewportPosition = new Vector2(0.5f, 0.9f);
    [SerializeField] private float cameraDistance = 10f;

    [Header("Debug")]
    [SerializeField] private bool verboseLogs = true;

    private PlayerLobbyData localLobbyData;

    private float remainingVoiceTime;
    private bool timerRunning;
    private bool voiceLocked;
    private bool initialized;

    private Transform barRoot;
    private Transform barFill;
    private Vector3 initialFillScale;

    public float RemainingVoiceTime => remainingVoiceTime;
    public float MaxVoiceDuration => maxVoiceDuration;
    public bool IsTimerRunning => timerRunning;
    public bool IsVoiceLocked => voiceLocked;
    public bool CanUnmute => !voiceLocked && remainingVoiceTime > 0f;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
            return;

        LocalInstance = this;
        remainingVoiceTime = maxVoiceDuration;

        StartCoroutine(WaitForTargetSceneAndInitialize());
    }

    public override void OnNetworkDespawn()
    {
        if (LocalInstance == this)
            LocalInstance = null;
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
            yield break;
        }

        if (verboseLogs)
            Debug.Log("[VoiceLimiter] Joueur Space détecté dans GameScene -> initialisation.");

        SpawnBar();
        UpdateBar();

        ForceMicrophoneState(false);
        timerRunning = true;

        StartCoroutine(VoiceCountdownRoutine());
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

    private IEnumerator VoiceCountdownRoutine()
    {
        while (!voiceLocked)
        {
            if (timerRunning)
            {
                remainingVoiceTime -= Time.deltaTime;

                if (remainingVoiceTime <= 0f)
                {
                    remainingVoiceTime = 0f;
                    timerRunning = false;
                    voiceLocked = true;

                    ForceMicrophoneState(true);
                    UpdateBar();

                    if (verboseLogs)
                        Debug.Log("[VoiceLimiter] Temps écoulé -> parole verrouillée.");

                    yield break;
                }

                UpdateBar();
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
            timerRunning = false;
            ForceMicrophoneState(true);

            if (verboseLogs)
                Debug.Log("[VoiceLimiter] Mute manuel -> timer en pause.");

            return true;
        }

        if (!CanUnmute)
        {
            ForceMicrophoneState(true);

            if (verboseLogs)
                Debug.Log("[VoiceLimiter] Unmute refusé -> temps restant nul ou parole verrouillée.");

            return false;
        }

        ForceMicrophoneState(false);
        timerRunning = true;

        if (verboseLogs)
            Debug.Log("[VoiceLimiter] Unmute autorisé -> timer reprend.");

        return true;
    }

    private void ForceMicrophoneState(bool muted)
    {
        if (VivoxManager.Instance == null)
            return;

        VivoxManager.Instance.MuteMicrophone(muted);
    }

    private void SpawnBar()
    {
        if (barPrefab == null)
            return;

        GameObject barInstance = Instantiate(barPrefab);
        barRoot = barInstance.transform;
        barFill = barRoot.Find("BarFill");

        if (barFill != null)
            initialFillScale = barFill.localScale;

        ScreenAnchor2D anchor = barInstance.GetComponent<ScreenAnchor2D>();
        if (anchor == null)
            anchor = barInstance.AddComponent<ScreenAnchor2D>();

        Camera targetCam = FindLocalPlayerCamera();
        anchor.targetCamera = targetCam;
        anchor.viewportPosition = viewportPosition;
        anchor.distanceFromCamera = cameraDistance;
    }

    private Camera FindLocalPlayerCamera()
    {
        Camera cam = GetComponentInChildren<Camera>(true);
        if (cam != null)
            return cam;

        return Camera.main;
    }

    private void UpdateBar()
    {
        if (barFill == null || maxVoiceDuration <= 0f)
            return;

        float normalized = Mathf.Clamp01(remainingVoiceTime / maxVoiceDuration);

        Vector3 scale = initialFillScale;
        scale.x = initialFillScale.x * normalized;
        barFill.localScale = scale;
    }
}