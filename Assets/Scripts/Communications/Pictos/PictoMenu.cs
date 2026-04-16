using UnityEngine;
using UnityEngine.Audio;

public class PictoMenu : Module, MouseInteractionManager.IInteractable
{
    [SerializeField] private GameObject[] pictos;


    public void OnClick()
    {
        foreach (GameObject picto in pictos)
        {
            picto.SetActive(true);
            gameObject.SetActive(false);
        }
    }

    public void OnHoverEnter()
    {

    }

    public void OnHoverExit()
    {
    }
}
