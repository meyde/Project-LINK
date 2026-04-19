using UnityEngine;

public class BorderPipeController : MonoBehaviour, MouseInteractionManager.IInteractable
{
    public int state = 0;
    [SerializeField] private Sprite[] srList;
    private SpriteRenderer sr;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip clickSound;
    [SerializeField] private AudioClip hoverSound;
    public void Awake()
    {
        sr= gameObject.GetComponent<SpriteRenderer>() ;
    }
    public void OnClick()
    {
        state = (state + 1) % 3;
        sr.sprite = srList[state];

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
