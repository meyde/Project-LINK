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

    private bool isRegionActive = false;
    private Color baseColor;
    private Vector3 baseScale;
    private bool isInitialized = false;

    private void Awake()
    {
        Initialize();
    }

    private void OnValidate()
    {
        if (targetRenderer == null)
            targetRenderer = GetComponent<SpriteRenderer>();
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
        }
        else
        {
            ApplyInactiveVisual();
        }
    }

    private void Initialize()
    {
        if (targetRenderer == null)
            targetRenderer = GetComponent<SpriteRenderer>();

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