using UnityEngine;

public class VoiceBarSceneController : MonoBehaviour
{
    [Header("Références")]
    [SerializeField] private Transform barFill;
    [SerializeField] private SpriteRenderer barFillRenderer;

    [Header("Direction de vidage")]
    [SerializeField] private bool drainFromTopToBottom = true;

    [Header("Blink")]
    [SerializeField] private float blinkSpeed = 8f;
    [SerializeField] private float blinkMinAlpha = 0.3f;
    [SerializeField] private float blinkMaxAlpha = 1f;

    private Vector3 initialFillScale;
    private Vector3 initialFillLocalPosition;
    private Color initialColor;

    private void Awake()
    {
        if (barFill == null)
            barFill = transform;

        if (barFillRenderer == null && barFill != null)
            barFillRenderer = barFill.GetComponent<SpriteRenderer>();

        if (barFill != null)
        {
            initialFillScale = barFill.localScale;
            initialFillLocalPosition = barFill.localPosition;
        }

        if (barFillRenderer != null)
            initialColor = barFillRenderer.color;
    }

    private void Update()
    {
        VoiceTimeLimiter limiter = VoiceTimeLimiter.LocalInstance;

        if (limiter == null || !limiter.IsActiveForLocalPlayer)
        {
            SetVisible(false);
            return;
        }

        SetVisible(true);
        UpdateBar(limiter);
    }

    private void SetVisible(bool visible)
    {
        if (barFillRenderer != null)
            barFillRenderer.enabled = visible;
    }

    private void UpdateBar(VoiceTimeLimiter limiter)
    {
        if (barFill == null || limiter.MaxVoiceDuration <= 0f)
            return;

        float normalized = Mathf.Clamp01(limiter.RemainingVoiceTime / limiter.MaxVoiceDuration);

        Vector3 scale = initialFillScale;
        scale.y = initialFillScale.y * normalized;
        barFill.localScale = scale;

        if (drainFromTopToBottom)
        {
            float lostHeight = initialFillScale.y - scale.y;

            Vector3 pos = initialFillLocalPosition;
            pos.y = initialFillLocalPosition.y - (lostHeight * 0.5f);
            barFill.localPosition = pos;
        }
        else
        {
            barFill.localPosition = initialFillLocalPosition;
        }

        if (barFillRenderer == null)
            return;

        if (limiter.IsInCriticalZone)
        {
            float blink = Mathf.Sin(Time.time * blinkSpeed) * 0.5f + 0.5f;
            float alpha = Mathf.Lerp(blinkMinAlpha, blinkMaxAlpha, blink);
            SetBarAlpha(alpha);
        }
        else
        {
            SetBarAlpha(1f);
        }
    }

    private void SetBarAlpha(float alpha)
    {
        if (barFillRenderer == null)
            return;

        Color c = initialColor;
        c.a = alpha;
        barFillRenderer.color = c;
    }
}