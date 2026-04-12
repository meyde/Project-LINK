using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(SpriteRenderer))]
public class PictogramInteractable : MonoBehaviour, MouseInteractionManager.IInteractable
{
    [Header("Références")]
    [SerializeField] private PictogramSystem pictogramSystem;
    [SerializeField] private SpriteRenderer spriteRenderer;

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

        Debug.Log($"[PictogramInteractable] Click sur {gameObject.name}");
        pictogramSystem.TrySendPictogram(spriteRenderer);
    }

    public void OnHoverEnter()
    {
    }

    public void OnHoverExit()
    {
    }
}