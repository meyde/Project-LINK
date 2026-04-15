using UnityEngine;

public class BorderPipeController : MonoBehaviour, MouseInteractionManager.IInteractable
{
    public int state = 0;
    [SerializeField] private Sprite[] srList;
    private SpriteRenderer sr;


    public void Awake()
    {
        sr= gameObject.GetComponent<SpriteRenderer>() ;
    }
    public void OnClick()
    {
        state = state + 1 % 3;
        sr.sprite = srList[state];
    }
    public void OnHoverEnter()
    {

    }

    public void OnHoverExit()
    {

    }

}
