using System.Collections;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class LeverModule : NetworkBehaviour, MouseInteractionManager.IInteractable
{
    [Header("Data")]
    [SerializeField] private LeverCodes currentCode;

    [Header("Visual")]
    [SerializeField] private TextMeshPro valueText;

    [Header("Lever Settings")]
    [SerializeField] private int minValue = 0;
    [SerializeField] private int maxValue = 20;
    [SerializeField] private float dragStepThreshold = 25f;
    [SerializeField] private bool invertDirection = true;

    private WaitForFixedUpdate waitForFixedUpdate = new();

    private bool isDragging = false;
    private float accumulatedY = 0f;

    private void Awake()
    {
        ClampSettings();
    }

    private void OnValidate()
    {
        ClampSettings();
        UpdateValueTextEditorSafe();
    }

    public void OnClick()
    {
        if (!isDragging)
            StartCoroutine(Draging());
    }

    public void OnHoverEnter()
    {
    }

    public void OnHoverExit()
    {
    }

    public IEnumerator Draging()
    {
        isDragging = true;
        accumulatedY = 0f;

        while (Mouse.current != null && Mouse.current.leftButton.isPressed)
        {
            float deltaY = Mouse.current.delta.ReadValue().y;
            accumulatedY += deltaY;

            int upValue = invertDirection ? -1 : +1;
            int downValue = invertDirection ? +1 : -1;

            while (accumulatedY >= dragStepThreshold)
            {
                accumulatedY -= dragStepThreshold;
            }

            while (accumulatedY <= -dragStepThreshold)
            {
                accumulatedY += dragStepThreshold;
            }

            yield return waitForFixedUpdate;
        }

        isDragging = false;
        accumulatedY = 0f;
    }

    private void OnLeverValueChanged(int oldValue, int newValue)
    {
        ApplyLeverValue(newValue);
    }

    private void ApplyLeverValue(int value)
    {
        UpdateValueText(value);
    }

    private void UpdateValueText(int value)
    {
        if (valueText != null)
            valueText.text = $"Valeur du levier : {value}";
    }

    private void ClampSettings()
    {
        if (maxValue < minValue)
            maxValue = minValue;
    }

    private void UpdateValueTextEditorSafe()
    {
        if (valueText != null)
            valueText.text = "Valeur du levier : 0";
    }
}