using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(SpriteRenderer))]
public class PictogramInteractable : MonoBehaviour, MouseInteractionManager.IInteractable
{
    [Header("Références")]
    [SerializeField] private PictogramSystem pictogramSystem;
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip clickSound;
    [SerializeField] private AudioClip hoverSound;

    private void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (pictogramSystem == null)
            pictogramSystem = FindFirstObjectByType<PictogramSystem>();
    }

    public void OnClick()
    {
        if (pictogramSystem == null)
        {
            Debug.LogWarning($"[PictogramInteractable] Aucun PictogramSystem trouvé sur {gameObject.name}");
            return;
        }

        if (spriteRenderer == null)
        {
            Debug.LogWarning($"[PictogramInteractable] Aucun SpriteRenderer trouvé sur {gameObject.name}");
            return;
        }

        if (audioSource != null && clickSound != null)
            audioSource.PlayOneShot(clickSound);

        Debug.Log($"[PictogramInteractable] Click sur {gameObject.name}");
        pictogramSystem.TrySendPictogram(spriteRenderer);
    }

    public void OnHoverEnter()
    {
        if (audioSource != null && hoverSound != null)
            audioSource.PlayOneShot(hoverSound);
    }

    public void OnHoverExit()
    {
    }
}