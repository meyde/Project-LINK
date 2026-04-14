using UnityEngine;

public class DesktopIconOpener : MonoBehaviour, MouseInteractionManager.IInteractable
{
    [Header("Fenêtre à ouvrir")]
    [SerializeField] private GameObject targetWindow;

    [Header("Hover visuel (optionnel)")]
    [SerializeField] private SpriteRenderer iconSprite;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color hoverColor = Color.yellow;

    private void Awake()
    {
        if (iconSprite == null)
            iconSprite = GetComponent<SpriteRenderer>();

        if (iconSprite != null)
            iconSprite.color = normalColor;
    }

    public void OnClick()
    {
        if (targetWindow != null)
            targetWindow.SetActive(true);
    }

    public void OnHoverEnter()
    {
        if (iconSprite != null)
            iconSprite.color = hoverColor;
    }

    public void OnHoverExit()
    {
        if (iconSprite != null)
            iconSprite.color = normalColor;
    }
}