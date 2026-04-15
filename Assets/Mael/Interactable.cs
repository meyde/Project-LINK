using System;
using UnityEngine;

public class Interactable : MonoBehaviour, MouseInteractionManager.IInteractable
{
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip clickSound;
    [SerializeField] private AudioClip hoverSound;

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

        if (audioSource != null && clickSound != null)
            audioSource.PlayOneShot(clickSound);
    }

    public void OnHoverEnter()
    {
        sr.color = Color.yellow;

        if (audioSource != null && hoverSound != null)
            audioSource.PlayOneShot(hoverSound);
    }

    public void OnHoverExit()
    {
        sr.color= Color.white;
    }

}
