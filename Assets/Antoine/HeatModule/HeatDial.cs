using UnityEngine;
using TMPro;

public class HeatDial : MonoBehaviour, MouseInteractionManager.IInteractable
{
    [Header("Infos")]
    public string dialId; // "L", "H", "I"

    [Header("Valeur actuelle")]
    [Range(0, 3)]
    [SerializeField] private int currentValue = 0;

    [Header("Affichage")]
    public TextMeshPro valueText;
    public Transform rotatingVisual;
    public float rotationStep = 90f;

    [Header("Hover")]
    public SpriteRenderer sr;
    public Color normalColor = Color.white;
    public Color hoverColor = Color.yellow;

    private HeatModuleManager manager;

    public int CurrentValue => currentValue;

    public void Initialize(HeatModuleManager moduleManager, string id)
    {
        manager = moduleManager;
        dialId = id;
        RefreshVisual();
    }

    public void OnClick()
    {
        currentValue++;
        if (currentValue > 3)
            currentValue = 0;

        RefreshVisual();

        Debug.Log($"Podomètre {dialId} -> valeur {currentValue}");

        if (manager != null)
            manager.OnDialValueChanged();
    }

    public void OnHoverEnter()
    {
        if (sr != null)
            sr.color = hoverColor;
    }

    public void OnHoverExit()
    {
        if (sr != null)
            sr.color = normalColor;
    }

    private void RefreshVisual()
    {
        if (valueText != null)
            valueText.text = $"{dialId} : {currentValue}";

        if (rotatingVisual != null)
        {
            rotatingVisual.localRotation = Quaternion.Euler(0f, 0f, -currentValue * rotationStep);
        }
    }

    public void SetValue(int value)
    {
        currentValue = Mathf.Clamp(value, 0, 3);
        RefreshVisual();

        if (manager != null)
            manager.OnDialValueChanged();
    }
}