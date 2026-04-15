using UnityEngine;

public class RegionSelector : MonoBehaviour, MouseInteractionManager.IInteractable 
{
    [SerializeField] private int regionId;
    private GameManagerLocal gm;

    private void Awake()
    {
        gm= FindFirstObjectByType<GameManagerLocal>();
    }
    public void OnClick()
    {
        gm.ChangeRegion(regionId);
    }

    public void OnHoverEnter()
    {

    }
    public void OnHoverExit()
    {

    }


}
