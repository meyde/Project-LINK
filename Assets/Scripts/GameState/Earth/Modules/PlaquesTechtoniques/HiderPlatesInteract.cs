using UnityEngine;
using UnityEngine.Audio;

public class HiderPlatesInteract : MonoBehaviour, MouseInteractionManager.IInteractable
{
    public int blockerId;
    public int[] neighboors;
    [SerializeField] private HiderPlates hm;
    public void OnClick()
    {
        hm.TryChangingDeactivatedBlocker(blockerId);
    }
    public void OnHoverEnter()
    {

    }

    public void OnHoverExit()
    {

    }
}
