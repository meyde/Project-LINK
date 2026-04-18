using System.Collections;
using UnityEngine;

public class RegionTransitionScreen : MonoBehaviour
{
    public static RegionTransitionScreen Instance { get; private set; }

    [Header("Référence visuelle")]
    [SerializeField] private SpriteRenderer overlayRenderer;
    [SerializeField] private Camera targetCamera;
    [SerializeField] private float distanceFromCamera = 1f;

    [Header("Réglages")]
    [SerializeField] private float fadeOutDuration = 0.25f;
    [SerializeField] private float holdDuration = 0.1f;
    [SerializeField] private float fadeInDuration = 0.25f;

    private Coroutine transitionCoroutine;
    private bool isTransitioning;

    public bool IsTransitioning => isTransitioning;

    private void Awake()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;

        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (overlayRenderer != null)
        {
            Color c = overlayRenderer.color;
            c.a = 0f;
            overlayRenderer.color = c;
            overlayRenderer.gameObject.SetActive(true);
        }
    }

    private void Update()
    {
        UpdatePosition();
    }

    private void UpdatePosition()
    {
        if (targetCamera == null || overlayRenderer == null)
            return;

        // Position pile devant la caméra
        Vector3 pos = targetCamera.transform.position + targetCamera.transform.forward * distanceFromCamera;

        overlayRenderer.transform.position = pos;

        // Toujours face caméra
        overlayRenderer.transform.rotation = targetCamera.transform.rotation;
    }

    public void PlayTransition(System.Action onMiddleReached)
    {
        if (transitionCoroutine != null)
            StopCoroutine(transitionCoroutine);

        transitionCoroutine = StartCoroutine(TransitionRoutine(onMiddleReached));
    }

    private IEnumerator TransitionRoutine(System.Action onMiddleReached)
    {
        isTransitioning = true;

        yield return Fade(0f, 1f, fadeOutDuration);

        onMiddleReached?.Invoke();

        if (holdDuration > 0f)
            yield return new WaitForSeconds(holdDuration);

        yield return Fade(1f, 0f, fadeInDuration);

        isTransitioning = false;
        transitionCoroutine = null;
    }

    private IEnumerator Fade(float from, float to, float duration)
    {
        if (overlayRenderer == null)
            yield break;

        float time = 0f;
        Color c = overlayRenderer.color;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = duration <= 0f ? 1f : time / duration;
            c.a = Mathf.Lerp(from, to, t);
            overlayRenderer.color = c;
            yield return null;
        }

        c.a = to;
        overlayRenderer.color = c;
    }
}