using UnityEngine;

public class HeatValidateButton : MonoBehaviour, MouseInteractionManager.IInteractable
{
    [Header("Référence au module")]
    public HeatModuleManager module;

    [Header("Visuel (optionnel)")]
    public Transform buttonVisual;
    public float pressDepth = 0.1f;
    public float pressSpeed = 10f;

    private Vector3 initialPosition;

    private void Start()
    {
        if (buttonVisual != null)
            initialPosition = buttonVisual.localPosition;
    }

    public void OnClick()
    {
        if (module != null)
        {
            module.Validate();
            Debug.Log("Bouton validé !");
        }

        // petit effet visuel
        if (buttonVisual != null)
        {
            StopAllCoroutines();
            StartCoroutine(PressAnimation());
        }
    }

    public void OnHoverEnter()
    {
        
    }

    public void OnHoverExit()
    {
        
    }

    private System.Collections.IEnumerator PressAnimation()
    {
        Vector3 pressedPos = initialPosition + Vector3.down * pressDepth;

        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime * pressSpeed;
            buttonVisual.localPosition = Vector3.Lerp(initialPosition, pressedPos, t);
            yield return null;
        }

        t = 0;
        while (t < 1)
        {
            t += Time.deltaTime * pressSpeed;
            buttonVisual.localPosition = Vector3.Lerp(pressedPos, initialPosition, t);
            yield return null;
        }
    }
}