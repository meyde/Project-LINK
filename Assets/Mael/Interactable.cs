using System;
using UnityEngine;

public class Interactable : MonoBehaviour, MouseInteractionManager.IInteractable
{
    private SpriteRenderer sr;

    public void OnClick()
    {
        Debug.Log("Objet Cliqué" + gameObject.name);
    }

    public void OnHoverEnter()
    {
        sr.color = Color.yellow;
    }

    public void OnHoverExit()
    {
        sr.color= Color.white;
    }

}
