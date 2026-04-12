using UnityEngine;

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
        HandleClick();
    }

    void HandleHover()
    {
        Vector2 mouseWorldPos = cam.ScreenToWorldPoint(Input.mousePosition);

        RaycastHit2D hit = Physics2D.Raycast(mouseWorldPos, Vector2.zero, Mathf.Infinity, interactableLayer);

        if (hit.collider != null)
        {
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();

            if (interactable != null) 
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

        // Si rien touché
        if (currentHover != null)
        { 
            currentHover.OnHoverExit();
            currentHover = null;
        }
    }

    void HandleClick()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("Clique Souris Effectué");
            Vector2 mouseWorldPos = cam.ScreenToWorldPoint(Input.mousePosition);

            RaycastHit2D hit = Physics2D.Raycast(mouseWorldPos, Vector2.zero, Mathf.Infinity, interactableLayer);

            if (hit.collider != null)
            {
                IInteractable interactable = hit.collider.GetComponent<IInteractable>();

                if (interactable != null)
                {
                    interactable.OnClick();
                }
            }
        }
    }

    public interface IInteractable
    {
        void OnClick();
        void OnHoverEnter();
        void OnHoverExit();
    }
}
