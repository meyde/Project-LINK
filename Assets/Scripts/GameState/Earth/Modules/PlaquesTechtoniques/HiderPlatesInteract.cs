using UnityEngine;

public class HiderPlatesInteract : MonoBehaviour, MouseInteractionManager.IInteractable
{
    [HideInInspector] public int blockerId;
    [HideInInspector] public int[] neighboors;

    [SerializeField] private HiderPlates hm;

    public void SetManager(HiderPlates manager, int id)
    {
        hm = manager;
        blockerId = id;
    }

    public void OnClick()
    {
        Debug.Log($"[HIDER] Click sur {name} | blockerId = {blockerId}");

        if (hm == null)
        {
            Debug.LogWarning($"[HIDER] Aucun manager assigné sur {name}");
            return;
        }

        hm.TryChangingDeactivatedBlocker(blockerId);
    }

    public void OnHoverEnter()
    {
    }

    public void OnHoverExit()
    {
    }
}