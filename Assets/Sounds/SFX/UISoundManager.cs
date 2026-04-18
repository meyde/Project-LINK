using UnityEngine;

public class UISoundManager : MonoBehaviour
{
    public static UISoundManager Instance { get; private set; }

    [Header("Audio Source")]
    [SerializeField] private AudioSource audioSource;

    [Header("UI Sounds")]
    [SerializeField] private AudioClip clickSound;
    [SerializeField] private AudioClip hoverSound;
    [SerializeField] private AudioClip backSound;
    [SerializeField] private AudioClip openPanelSound;
    [SerializeField] private AudioClip closePanelSound;

    [Header("Volume")]
    [Range(0f, 1f)]
    [SerializeField] private float uiVolume = 1f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    public void PlayClick()
    {
        Play(clickSound);
    }

    public void PlayHover()
    {
        Play(hoverSound);
    }

    public void PlayBack()
    {
        Play(backSound);
    }

    public void PlayOpenPanel()
    {
        Play(openPanelSound);
    }

    public void PlayClosePanel()
    {
        Play(closePanelSound);
    }

    public void PlayCustom(AudioClip clip)
    {
        Play(clip);
    }

    private void Play(AudioClip clip)
    {
        if (audioSource == null || clip == null)
            return;

        audioSource.PlayOneShot(clip, uiVolume);
    }
}