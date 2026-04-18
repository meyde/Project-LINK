using System;
using System.Collections;
using UnityEngine;

public class RegionTransitionScreen : MonoBehaviour
{
    public static RegionTransitionScreen Instance { get; private set; }

    [Header("Renderers")]
    [SerializeField] private SpriteRenderer backgroundRenderer;
    [SerializeField] private SpriteRenderer animationRenderer;

    [Header("Sprites")]
    [SerializeField] private Sprite backgroundSprite;
    [SerializeField] private Sprite[] animationFrames;

    [Header("Animation")]
    [SerializeField] private float frameRate = 12f;
    [SerializeField] private bool loop = true;

    [Header("Fade")]
    [SerializeField] private float fadeOutDuration = 0.25f;
    [SerializeField] private float holdDuration = 0.1f;
    [SerializeField] private float fadeInDuration = 0.25f;

    [Header("Placement")]
    [SerializeField] private bool followCamera = true;
    [SerializeField] private Camera targetCamera;
    [SerializeField] private float distance = 1f;

    private Coroutine transitionRoutine;
    private Coroutine animRoutine;
    private int currentFrame;

    private void Awake()
    {
        Instance = this;

        if (targetCamera == null)
            targetCamera = Camera.main;

        if (backgroundRenderer != null)
            backgroundRenderer.sprite = backgroundSprite;

        ResetAlpha(0f);
    }

    private void LateUpdate()
    {
        if (!followCamera || targetCamera == null)
            return;

        transform.position = targetCamera.transform.position + Vector3.forward * distance;
    }

    public void PlayTransition(Action onMid)
    {
        if (transitionRoutine != null)
            StopCoroutine(transitionRoutine);

        transitionRoutine = StartCoroutine(Transition(onMid));
    }

    private IEnumerator Transition(Action onMid)
    {
        StartAnimation();

        yield return Fade(0, 1, fadeOutDuration);

        onMid?.Invoke();

        yield return new WaitForSeconds(holdDuration);

        yield return Fade(1, 0, fadeInDuration);

        StopAnimation();
    }

    private IEnumerator Fade(float from, float to, float duration)
    {
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            float a = Mathf.Lerp(from, to, t / duration);
            SetAlpha(a);
            yield return null;
        }

        SetAlpha(to);
    }

    private void SetAlpha(float a)
    {
        if (backgroundRenderer != null)
        {
            var c = backgroundRenderer.color;
            c.a = a;
            backgroundRenderer.color = c;
        }

        if (animationRenderer != null)
        {
            var c = animationRenderer.color;
            c.a = a;
            animationRenderer.color = c;
        }
    }

    private void ResetAlpha(float a)
    {
        SetAlpha(a);
    }

    private void StartAnimation()
    {
        if (animationFrames == null || animationFrames.Length == 0)
            return;

        currentFrame = 0;
        animationRenderer.sprite = animationFrames[0];

        if (animRoutine != null)
            StopCoroutine(animRoutine);

        animRoutine = StartCoroutine(Animate());
    }

    private void StopAnimation()
    {
        if (animRoutine != null)
            StopCoroutine(animRoutine);
    }

    private IEnumerator Animate()
    {
        float delay = 1f / frameRate;

        while (true)
        {
            yield return new WaitForSeconds(delay);

            currentFrame++;

            if (currentFrame >= animationFrames.Length)
            {
                if (loop)
                    currentFrame = 0;
                else
                    yield break;
            }

            animationRenderer.sprite = animationFrames[currentFrame];
        }
    }
}