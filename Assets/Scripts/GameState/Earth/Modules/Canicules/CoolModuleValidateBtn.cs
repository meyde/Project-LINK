using UnityEngine;

public class CoolModuleValidateBtn : MonoBehaviour, MouseInteractionManager.IInteractable
{
    [SerializeField] private CoolingModule cm;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip clickSound;
    [SerializeField] private AudioClip hoverSound;

    public void OnClick()
    {
        cm.OnValidation();

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
