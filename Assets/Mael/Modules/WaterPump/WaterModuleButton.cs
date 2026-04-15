using UnityEngine;

public class WaterModuleButton : MonoBehaviour, MouseInteractionManager.IInteractable 
{
    [SerializeField] private WaterModule wm;
    public void OnClick()
    {
        wm.Validate();
    }
    public void OnHoverEnter()
    {

    }

    public void OnHoverExit()
    {

    }
}
