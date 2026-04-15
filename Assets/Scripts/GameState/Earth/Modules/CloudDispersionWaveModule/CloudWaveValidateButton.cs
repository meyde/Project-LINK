using UnityEngine;
using System.Collections;

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
    }

    public void OnHoverEnter()
    {
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