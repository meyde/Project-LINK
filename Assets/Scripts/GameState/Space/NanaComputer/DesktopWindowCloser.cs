using UnityEngine;

public class DesktopWindowCloser : MonoBehaviour, MouseInteractionManager.IInteractable
{
    [Header("Fenêtre à fermer")]
    [SerializeField] private GameObject targetWindow;

    [Header("Hover visuel (optionnel)")]
    [SerializeField] private SpriteRenderer buttonSprite;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color hoverColor = Color.red;

    private void Awake()
    {
        if (buttonSprite == null)
            buttonSprite = GetComponent<SpriteRenderer>();

        if (buttonSprite != null)
            buttonSprite.color = normalColor;
    }

    public void OnClick()
    {
        if (targetWindow != null)
            targetWindow.SetActive(false);
    }

    public void OnHoverEnter()
    {
        if (buttonSprite != null)
            buttonSprite.color = hoverColor;
    }

    public void OnHoverExit()
    {
        if (buttonSprite != null)
            buttonSprite.color = normalColor;
    }
}