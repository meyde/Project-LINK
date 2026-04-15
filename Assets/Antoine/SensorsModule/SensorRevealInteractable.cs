using UnityEngine;
using UnityEngine.Audio;

public class SensorRevealInteractable : MonoBehaviour, MouseInteractionManager.IInteractable
{
    [Header("Références")]
    [SerializeField] private SpriteRenderer sr;
    [SerializeField] private CatastrophicEventStateModule sensorsModule;

    [Header("Couleurs")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color hoverColor = Color.yellow;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip clickSound;
    [SerializeField] private AudioClip hoverSound;

    private void Awake()
    {
        if (sr == null)
            sr = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        if (sr != null)
            sr.color = normalColor;
    }

    public void OnClick()
    {
        Debug.Log("Bouton capteurs cliqué : " + gameObject.name);

        if (sensorsModule != null)
        {
            if (audioSource != null && clickSound != null)
                audioSource.PlayOneShot(clickSound);
            sensorsModule.RevealSensorsTemporarily();
        }
        else
        {
            Debug.LogWarning("Aucun CatastrophicEventStateModule assigné sur " + gameObject.name);
        }
    }

    public void OnHoverEnter()
    {
        if (sr != null)
            sr.color = hoverColor;

        if (audioSource != null && hoverSound != null)
            audioSource.PlayOneShot(hoverSound);
    }

    public void OnHoverExit()
    {
        if (sr != null)
            sr.color = normalColor;
    }
}