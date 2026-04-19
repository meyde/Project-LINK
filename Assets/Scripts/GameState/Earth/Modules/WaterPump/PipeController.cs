using UnityEngine;

public class PipeController : MonoBehaviour, MouseInteractionManager.IInteractable 
{
    public int orientation = 0;
    public int orientationLimit = 4;

    private SpriteRenderer sr;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip clickSound;
    [SerializeField] private AudioClip hoverSound;

    private void Awake()
    {
        sr = gameObject.GetComponent<SpriteRenderer>();
    }
    public void OnClick()
    {
        orientation = (orientation + 1) % orientationLimit;
        sr.transform.localRotation= Quaternion.Euler(0, 0, -orientation*90);

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
