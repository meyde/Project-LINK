using UnityEngine;
using UnityEngine.Audio;

public class DesktopIconOpener : MonoBehaviour, MouseInteractionManager.IInteractable
{
    [Header("Fenêtre à ouvrir")]
    [SerializeField] private GameObject targetWindow;

    [Header("Scroller à rafraîchir (optionnel)")]
    [SerializeField] private WorldImageScroller targetScroller;

    [Header("Hover visuel (optionnel)")]
    [SerializeField] private SpriteRenderer iconSprite;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color hoverColor = Color.yellow;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip clickSound;
    [SerializeField] private AudioClip hoverSound;

    private void Awake()
    {
        if (iconSprite == null)
            iconSprite = GetComponent<SpriteRenderer>();

        if (iconSprite != null)
            iconSprite.color = normalColor;

        if (targetScroller == null && targetWindow != null)
            targetScroller = targetWindow.GetComponentInChildren<WorldImageScroller>(true);
    }

    public void OnClick()
    {
        if (targetWindow != null)
            targetWindow.SetActive(true);

        if (targetScroller != null)
            targetScroller.RefreshScroll(true);

        if (audioSource != null && clickSound != null)
            audioSource.PlayOneShot(clickSound);
    }

    public void OnHoverEnter()
    {
        if (iconSprite != null)
            iconSprite.color = hoverColor;

        if (audioSource != null && hoverSound != null)
            audioSource.PlayOneShot(hoverSound);
    }

    public void OnHoverExit()
    {
        if (iconSprite != null)
            iconSprite.color = normalColor;
    }
}