using UnityEngine;

public class CoolModuleValidateBtn : MonoBehaviour, MouseInteractionManager.IInteractable
{
    [SerializeField] private CoolingModule cm;
    public void OnClick()
    {
        cm.OnValidation();
    }

    public void OnHoverEnter()
    {

    }

    public void OnHoverExit()
    {

    }
}
