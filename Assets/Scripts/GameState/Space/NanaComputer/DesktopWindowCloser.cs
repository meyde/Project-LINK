using UnityEngine;
using UnityEngine.Audio;

public class DesktopWindowCloser : MonoBehaviour, MouseInteractionManager.IInteractable
{
    [Header("Fenêtre à fermer")]
    [SerializeField] private GameObject targetWindow;

    [Header("Scroller à reset (optionnel)")]
    [SerializeField] private WorldImageScroller targetScroller;

    [Header("Hover visuel (optionnel)")]
    [SerializeField] private SpriteRenderer buttonSprite;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color hoverColor = Color.red;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip clickSound;
    [SerializeField] private AudioClip hoverSound;

    private void Awake()
    {
        if (buttonSprite == null)
            buttonSprite = GetComponent<SpriteRenderer>();

        if (buttonSprite != null)
            buttonSprite.color = normalColor;

        if (targetScroller == null && targetWindow != null)
            targetScroller = targetWindow.GetComponentInChildren<WorldImageScroller>(true);
    }

    public void OnClick()
    {
        if (targetScroller != null)
            targetScroller.ResetToBasePosition();

        if (targetWindow != null)
            targetWindow.SetActive(false);

        if (audioSource != null && clickSound != null)
            audioSource.PlayOneShot(clickSound);
    }

    public void OnHoverEnter()
    {
        if (buttonSprite != null)
            buttonSprite.color = hoverColor;

        if (audioSource != null && hoverSound != null)
            audioSource.PlayOneShot(hoverSound);
    }

    public void OnHoverExit()
    {
        if (buttonSprite != null)
            buttonSprite.color = normalColor;
    }
}