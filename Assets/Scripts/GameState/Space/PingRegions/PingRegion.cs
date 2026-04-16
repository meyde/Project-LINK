using UnityEngine;

public class PingRegion : MonoBehaviour
{
    [Header("ID de région")]
    public int regionId;

    [Header("Rendu")]
    [SerializeField] private SpriteRenderer targetRenderer;

    [Header("Glow Fade")]
    [SerializeField] private bool useUnscaledTime = true;
    [SerializeField] private float pulseSpeed = 2f;
    [SerializeField] private float inactiveAlpha = 0f;
    [SerializeField] private float activeMinAlpha = 0.25f;
    [SerializeField] private float activeMaxAlpha = 1f;

    [Header("Scale optionnel")]
    [SerializeField] private bool pulseScale = false;
    [SerializeField] private float scalePulseAmount = 0.08f;

    [Header("Audio Pulse")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip pulseSound;
    [SerializeField] private bool playSoundOnPulse = true;
    [SerializeField] private bool playFirstPulseImmediately = false;
    [Range(0f, 1f)]
    [SerializeField] private float pulseSoundVolume = 1f;
    [SerializeField] private float pulseTriggerThreshold = 0.9f;

    private bool isRegionActive = false;
    private Color baseColor;
    private Vector3 baseScale;
    private bool isInitialized = false;

    private bool pulseSoundArmed = true;

    private void Awake()
    {
        Initialize();
    }

    private void OnValidate()
    {
        if (targetRenderer == null)
            targetRenderer = GetComponent<SpriteRenderer>();

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        if (!isInitialized || !isRegionActive)
            return;

        float timeValue = useUnscaledTime ? Time.unscaledTime : Time.time;
        float t = (Mathf.Sin(timeValue * pulseSpeed) + 1f) * 0.5f;

        float currentAlpha = Mathf.Lerp(activeMinAlpha, activeMaxAlpha, t);
        ApplyAlpha(currentAlpha);

        if (pulseScale)
        {
            float scaleFactor = 1f + (t * scalePulseAmount);
            transform.localScale = baseScale * scaleFactor;
        }

        HandlePulseSound(t);
    }

    public void SetRegionActive(bool active)
    {
        if (!isInitialized)
            Initialize();

        if (!isInitialized)
            return;

        isRegionActive = active;

        if (isRegionActive)
        {
            ApplyAlpha(activeMaxAlpha);

            pulseSoundArmed = !playFirstPulseImmediately;

            if (playFirstPulseImmediately)
                PlayPulseSound();
        }
        else
        {
            ApplyInactiveVisual();
            pulseSoundArmed = true;
        }
    }

    private void Initialize()
    {
        if (targetRenderer == null)
            targetRenderer = GetComponent<SpriteRenderer>();

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (targetRenderer == null)
        {
            Debug.LogWarning($"[PingRegion] Aucun SpriteRenderer trouvé sur {name}. Ajoute-en un ou assigne targetRenderer.");
            isInitialized = false;
            return;
        }

        baseColor = targetRenderer.color;
        baseScale = transform.localScale;
        isInitialized = true;

        ApplyInactiveVisual();
    }

    private void HandlePulseSound(float t)
    {
        if (!playSoundOnPulse || audioSource == null || pulseSound == null)
            return;

        // On joue une seule fois quand le pulse arrive proche de son maximum
        if (t >= pulseTriggerThreshold && pulseSoundArmed)
        {
            PlayPulseSound();
            pulseSoundArmed = false;
        }

        // On réarme quand le pulse redescend suffisamment
        if (t <= 0.25f)
        {
            pulseSoundArmed = true;
        }
    }

    private void PlayPulseSound()
    {
        if (audioSource == null || pulseSound == null)
            return;

        audioSource.PlayOneShot(pulseSound, pulseSoundVolume);
    }

    private void ApplyInactiveVisual()
    {
        if (!isInitialized)
            return;

        ApplyAlpha(inactiveAlpha);
        transform.localScale = baseScale;
    }

    private void ApplyAlpha(float alpha)
    {
        if (!isInitialized || targetRenderer == null)
            return;

        Color c = baseColor;
        c.a = alpha;
        targetRenderer.color = c;
    }
}