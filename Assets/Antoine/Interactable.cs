using System;
using UnityEngine;

public class Interactable : MonoBehaviour, MouseInteractionManager.IInteractable
{
    private SpriteRenderer sr;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

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
