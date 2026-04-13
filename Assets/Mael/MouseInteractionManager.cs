using UnityEngine;
using UnityEngine.InputSystem;

public class MouseInteractionManager : MonoBehaviour
{
    [Header("Settings")]
    public LayerMask interactableLayer;

    private Camera cam;
    private IInteractable currentHover;

    void Start()
    {
        cam = Camera.main;
    }

    void Update()
    {
        HandleHover();
    }

    void HandleHover()
    {
        Vector2 mouseWorldPos = cam.ScreenToWorldPoint(Mouse.current.position.ReadValue());

        RaycastHit2D hit = Physics2D.Raycast(mouseWorldPos, Vector2.zero, Mathf.Infinity, interactableLayer);

        if (hit.collider != null)
        {
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();

            if (IsInteractableUsable(interactable))
            {
                if (currentHover != interactable)
                {
                    currentHover?.OnHoverExit();
                    currentHover = interactable;
                    currentHover.OnHoverEnter();
                }
                return;
            }
        }

        if (currentHover != null)
        {
            currentHover.OnHoverExit();
            currentHover = null;
        }
    }

    public void OnLeftClick()
    {
        Debug.Log("Clique Souris Effectué");

        Vector2 mouseWorldPos = cam.ScreenToWorldPoint(Mouse.current.position.ReadValue());

        RaycastHit2D hit = Physics2D.Raycast(mouseWorldPos, Vector2.zero, Mathf.Infinity, interactableLayer);

        // CAS 1 : un module est déjà ouvert
        if (ZoomableModule.CurrentOpenModule != null)
        {
            // Si on clique sur le module ouvert ou un de ses enfants, on ne ferme pas
            if (hit.collider != null)
            {
                Transform clickedTransform = hit.collider.transform;

                if (clickedTransform == ZoomableModule.CurrentOpenModule.transform ||
                    clickedTransform.IsChildOf(ZoomableModule.CurrentOpenModule.transform))
                {
                    IInteractable clickedInteractable = hit.collider.GetComponent<IInteractable>();

                    if (IsInteractableUsable(clickedInteractable))
                    {
                        Debug.Log("Clic sur le module ouvert ou un enfant interactable");
                        clickedInteractable.OnClick();
                    }

                    return;
                }
            }

            // Sinon : clic à l'extérieur, on ferme le module courant
            Debug.Log("Clic hors du module ouvert -> fermeture");
            ZoomableModule.CloseCurrentOpenModule();
            return;
        }

        // CAS 2 : aucun module ouvert => comportement normal
        if (hit.collider != null)
        {
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();

            if (IsInteractableUsable(interactable))
            {
                Debug.Log("objet cliqué");
                interactable.OnClick();
            }
        }
    }

    private bool IsInteractableUsable(IInteractable interactable)
    {
        if (interactable == null)
            return false;

        MonoBehaviour behaviour = interactable as MonoBehaviour;

        if (behaviour == null)
            return false;

        if (!behaviour.isActiveAndEnabled)
            return false;

        return true;
    }

    public interface IInteractable
    {
        void OnClick();
        void OnHoverEnter();
        void OnHoverExit();
    }
}