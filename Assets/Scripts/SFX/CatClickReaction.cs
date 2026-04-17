using System.Collections;
using UnityEngine;

public class CatInteractable : MonoBehaviour, MouseInteractionManager.IInteractable
{
    [Header("Références")]
    [SerializeField] private SpriteRenderer catSprite;
    [SerializeField] private AudioSource audioSource;

    [Header("Sons")]
    [SerializeField] private AudioClip meowSound;
    [SerializeField] private AudioClip purrSound;
    [SerializeField] private bool randomBetweenBoth = true;

    [Header("Volume")]
    [Range(0f, 1f)]
    [SerializeField] private float catVolume = 1f;

    [Header("Hover visuel")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color hoverColor = new Color(1f, 0.95f, 0.8f, 1f);

    [Header("Animation")]
    [SerializeField] private float animDuration = 0.12f;
    [SerializeField] private float scaleBoost = 0.08f;
    [SerializeField] private float rotationAmount = 6f;
    [SerializeField] private float moveAmount = 0.05f;
    [SerializeField] private bool useUnscaledTime = false;

    private Vector3 baseScale;
    private Vector3 baseLocalPosition;
    private Quaternion baseRotation;
    private Coroutine animRoutine;

    private void Awake()
    {
        if (catSprite == null)
            catSprite = GetComponent<SpriteRenderer>();

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        baseScale = transform.localScale;
        baseLocalPosition = transform.localPosition;
        baseRotation = transform.localRotation;

        if (catSprite != null)
            catSprite.color = normalColor;

        if (audioSource != null)
            audioSource.volume = catVolume;
    }

    public void OnClick()
    {
        PlayCatSound();

        if (animRoutine != null)
            StopCoroutine(animRoutine);

        animRoutine = StartCoroutine(PlayReaction());
    }

    public void OnHoverEnter()
    {
        if (catSprite != null)
            catSprite.color = hoverColor;
    }

    public void OnHoverExit()
    {
        if (catSprite != null)
            catSprite.color = normalColor;
    }

    public void SetCatVolume(float newVolume)
    {
        catVolume = Mathf.Clamp01(newVolume);

        if (audioSource != null)
            audioSource.volume = catVolume;
    }

    public float GetCatVolume()
    {
        return catVolume;
    }

    private void PlayCatSound()
    {
        if (audioSource == null)
            return;

        AudioClip clipToPlay = null;

        if (randomBetweenBoth)
        {
            bool playMeow = Random.value < 0.5f;

            if (playMeow && meowSound != null)
                clipToPlay = meowSound;
            else if (!playMeow && purrSound != null)
                clipToPlay = purrSound;
            else
                clipToPlay = meowSound != null ? meowSound : purrSound;
        }
        else
        {
            clipToPlay = meowSound != null ? meowSound : purrSound;
        }

        if (clipToPlay != null)
            audioSource.PlayOneShot(clipToPlay);
    }

    private IEnumerator PlayReaction()
    {
        float time = 0f;

        Vector3 targetScale = baseScale * (1f + scaleBoost);
        Vector3 targetPos = baseLocalPosition + new Vector3(
            Random.Range(-moveAmount, moveAmount),
            Random.Range(-moveAmount, moveAmount),
            0f
        );

        Quaternion targetRot = Quaternion.Euler(
            baseRotation.eulerAngles.x,
            baseRotation.eulerAngles.y,
            baseRotation.eulerAngles.z + Random.Range(-rotationAmount, rotationAmount)
        );

        while (time < animDuration)
        {
            time += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            float t = Mathf.Clamp01(time / animDuration);

            transform.localScale = Vector3.Lerp(baseScale, targetScale, t);
            transform.localPosition = Vector3.Lerp(baseLocalPosition, targetPos, t);
            transform.localRotation = Quaternion.Lerp(baseRotation, targetRot, t);

            yield return null;
        }

        time = 0f;

        while (time < animDuration)
        {
            time += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            float t = Mathf.Clamp01(time / animDuration);

            transform.localScale = Vector3.Lerp(targetScale, baseScale, t);
            transform.localPosition = Vector3.Lerp(targetPos, baseLocalPosition, t);
            transform.localRotation = Quaternion.Lerp(targetRot, baseRotation, t);

            yield return null;
        }

        transform.localScale = baseScale;
        transform.localPosition = baseLocalPosition;
        transform.localRotation = baseRotation;

        animRoutine = null;
    }
}