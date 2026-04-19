using UnityEngine;

public class RegionSelector : MonoBehaviour, MouseInteractionManager.IInteractable 
{
    [SerializeField] private int regionId;
    private GameManagerLocal gm;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip clickSound;
    [SerializeField] private AudioClip hoverSound;

    private void Awake()
    {
        gm= FindFirstObjectByType<GameManagerLocal>();
    }
    public void OnClick()
    {
        gm.ChangeRegion(regionId);

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
