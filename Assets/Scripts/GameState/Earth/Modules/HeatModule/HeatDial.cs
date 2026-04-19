using TMPro;
using UnityEngine;
using UnityEngine.Audio;

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

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip clickSound;
    [SerializeField] private AudioClip hoverSound;

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
        currentValue = (currentValue + 1) % 3;

        if (audioSource != null && clickSound != null)
            audioSource.PlayOneShot(clickSound);

        RefreshVisual();

        Debug.Log($"Podomètre {dialId} -> valeur {currentValue}");

        if (manager != null)
            manager.OnDialValueChanged();
    }

    public void OnHoverEnter()
    {
        if (sr != null)
            sr.color = hoverColor;

        if (audioSource != null && hoverSound != null)
            audioSource.PlayOneShot(hoverSound);
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
            rotatingVisual.localRotation = Quaternion.Euler(0f, 0f,180 +( -currentValue * rotationStep));
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