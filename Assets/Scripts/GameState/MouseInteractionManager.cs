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

        IInteractable interactable = GetClosestInteractableAtPoint(mouseWorldPos);

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

        if (currentHover != null)
        {
            currentHover.OnHoverExit();
            currentHover = null;
        }
    }

    public void OnLeftClick()
    {
        if (RegionTransitionScreen.Instance != null && RegionTransitionScreen.Instance.IsTransitioning)
            return;
        Debug.Log("Clique Souris Effectué");

        Vector2 mouseWorldPos = cam.ScreenToWorldPoint(Mouse.current.position.ReadValue());

        Collider2D clickedCollider = GetClosestColliderAtPoint(mouseWorldPos);
        IInteractable clickedInteractable = GetClosestInteractableAtPoint(mouseWorldPos);

        // CAS 1 : un module est déjà ouvert
        if (ZoomableModule.CurrentOpenModule != null)
        {
            // Si on clique sur le module ouvert ou un de ses enfants, on ne ferme pas
            if (clickedCollider != null)
            {
                Transform clickedTransform = clickedCollider.transform;

                if (clickedTransform == ZoomableModule.CurrentOpenModule.transform ||
                    clickedTransform.IsChildOf(ZoomableModule.CurrentOpenModule.transform))
                {
                    if (IsInteractableUsable(clickedInteractable))
                    {
                        Debug.Log("Clic sur le module ouvert ou un enfant interactable");
                        clickedInteractable.OnClick();
                    }
                    else
                    {
                        Debug.Log("Clic dans le module ouvert, sans interactable enfant prioritaire");
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
        if (IsInteractableUsable(clickedInteractable))
        {
            Debug.Log("Objet cliqué");
            clickedInteractable.OnClick();
        }
    }

    private Collider2D GetClosestColliderAtPoint(Vector2 worldPoint)
    {
        Collider2D[] hits = Physics2D.OverlapPointAll(worldPoint, interactableLayer);

        if (hits == null || hits.Length == 0)
            return null;

        Collider2D closest = null;
        float bestDistance = float.MaxValue;

        for (int i = 0; i < hits.Length; i++)
        {
            Collider2D hit = hits[i];

            if (hit == null)
                continue;

            Vector2 closestPoint = hit.ClosestPoint(worldPoint);
            float sqrDistance = (closestPoint - worldPoint).sqrMagnitude;

            if (closest == null || sqrDistance < bestDistance)
            {
                closest = hit;
                bestDistance = sqrDistance;
            }
        }

        return closest;
    }

    private IInteractable GetClosestInteractableAtPoint(Vector2 worldPoint)
    {
        Collider2D[] hits = Physics2D.OverlapPointAll(worldPoint, interactableLayer);

        if (hits == null || hits.Length == 0)
            return null;

        IInteractable closestInteractable = null;
        float bestDistance = float.MaxValue;

        for (int i = 0; i < hits.Length; i++)
        {
            Collider2D hit = hits[i];

            if (hit == null)
                continue;

            IInteractable interactable = hit.GetComponentInParent<IInteractable>();

            if (!IsInteractableUsable(interactable))
                continue;

            // Si un module est déjà ouvert, on ignore le ZoomableModule de ce module pour laisser ses enfants (autres Interactables) être choisis à la place.
            if (ZoomableModule.CurrentOpenModule != null &&
                interactable is ZoomableModule zoomable &&
                zoomable == ZoomableModule.CurrentOpenModule)
            {
                continue;
            }

            Vector2 closestPoint = hit.ClosestPoint(worldPoint);
            float sqrDistance = (closestPoint - worldPoint).sqrMagnitude;

            if (closestInteractable == null || sqrDistance < bestDistance)
            {
                closestInteractable = interactable;
                bestDistance = sqrDistance;
            }
        }

        return closestInteractable;
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