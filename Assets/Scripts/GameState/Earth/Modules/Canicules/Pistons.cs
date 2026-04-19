using UnityEngine;

public class Pistons : MonoBehaviour, MouseInteractionManager.IInteractable
{
    public int pistonId;
    private int state;
    [SerializeField] private CoolingModule cm;
    [SerializeField] private Sprite[] leverStates;
    public void OnClick() 
    {
        state = (state + 1) % 5;
        gameObject.GetComponent<SpriteRenderer>().sprite= leverStates[state];
        cm.OnPistonChange(pistonId);
    }

    public void OnHoverEnter()
    {

    }

    public void OnHoverExit()
    {

    }

}
