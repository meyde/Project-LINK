using UnityEngine;

public class PipeController : MonoBehaviour, MouseInteractionManager.IInteractable 
{
    public int orientation = 0;
    public int orientationLimit = 4;

    private SpriteRenderer sr;

    private void Awake()
    {
        sr = gameObject.GetComponent<SpriteRenderer>();
    }
    public void OnClick()
    {
        orientation = (orientation + 1) % orientationLimit;
        sr.transform.Rotate(new Vector3(0, 0, 90));
    }
    public void OnHoverEnter()
    {

    }

    public void OnHoverExit()
    {

    }
}
