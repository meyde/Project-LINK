using UnityEngine;
using UnityEngine.Audio;

public class WaterModuleButton : MonoBehaviour, MouseInteractionManager.IInteractable 
{
    [SerializeField] private WaterModule wm;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip clickSound;
    [SerializeField] private AudioClip hoverSound;
    public void OnClick()
    {
        wm.Validate();

        if (audioSource != null && clickSound != null)
            audioSource.PlayOneShot(clickSound);
    }
    public void OnHoverEnter()
    {
        if (audioSource != null && hoverSound != null)
            audioSource.PlayOneShot(hoverSound);
    }

    public void OnHoverExit()
    {

    }
}
