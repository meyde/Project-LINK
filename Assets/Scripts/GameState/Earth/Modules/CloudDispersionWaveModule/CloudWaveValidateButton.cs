using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

public class CloudWaveValidateButton : MonoBehaviour, MouseInteractionManager.IInteractable
{
    [Header("Référence module")]
    public CloudWaveModuleManager moduleManager;

    [Header("Visuel du bouton")]
    public Transform buttonVisual;

    [Header("Animation")]
    public float pressDepth = 0.1f;
    public float pressSpeed = 10f;

    private Vector3 initialLocalPosition;
    private Coroutine pressRoutine;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip clickSound;
    [SerializeField] private AudioClip hoverSound;

    private void Start()
    {
        if (buttonVisual != null)
            initialLocalPosition = buttonVisual.localPosition;
    }

    public void OnClick()
    {
        Debug.Log("Bouton validation cliqué");

        if (moduleManager != null)
            moduleManager.ValidateLevers();
        else
            Debug.LogWarning("Aucun CloudWaveModuleManager assigné au bouton.");

        if (buttonVisual != null)
        {
            if (pressRoutine != null)
                StopCoroutine(pressRoutine);

            pressRoutine = StartCoroutine(PressAnimation());
        }

        if (audioSource != null && clickSound != null)
            audioSource.PlayOneShot(clickSound);
    }

    public void OnHoverEnter()
    {
        if (audioSource != null && hoverSound != null)
            audioSource.PlayOneShot(hoverSound);
    }

    public void OnHoverExit()
    {
    }

    private IEnumerator PressAnimation()
    {
        Vector3 pressedPosition = initialLocalPosition + Vector3.down * pressDepth;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * pressSpeed;
            buttonVisual.localPosition = Vector3.Lerp(initialLocalPosition, pressedPosition, t);
            yield return null;
        }

        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * pressSpeed;
            buttonVisual.localPosition = Vector3.Lerp(pressedPosition, initialLocalPosition, t);
            yield return null;
        }

        buttonVisual.localPosition = initialLocalPosition;
        pressRoutine = null;
    }
}