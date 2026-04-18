using UnityEngine;

public class Pistons : MonoBehaviour, MouseInteractionManager.IInteractable
{
    public int pistonId;
    private int state;
    [SerializeField] private CoolingModule cm;
    [SerializeField] private Sprite[] leverStates;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip clickSound;
    [SerializeField] private AudioClip hoverSound;
    public void OnClick() 
    {
        state = (state + 1) % 5;
        gameObject.GetComponent<SpriteRenderer>().sprite= leverStates[state];
        cm.OnPistonChange(pistonId);

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
