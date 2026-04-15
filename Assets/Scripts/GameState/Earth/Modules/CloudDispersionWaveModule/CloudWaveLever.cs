using UnityEngine;
using UnityEngine.Audio;

public class CloudWaveLever : MonoBehaviour, MouseInteractionManager.IInteractable
{
    [Header("Références")]
    public CloudWaveModuleManager moduleManager;

    [Header("Etat")]
    public bool isOn = false;


    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip clickSound;
    [SerializeField] private AudioClip hoverSound;

    [Header("Visuel sprite (optionnel)")]
    public SpriteRenderer leverSpriteRenderer;
    public Sprite offSprite;
    public Sprite onSprite;

    [Header("Hover")]
    public SpriteRenderer sr;
    public Color normalColor = Color.white;
    public Color hoverColor = Color.yellow;


    private void Awake()
    {
        leverSpriteRenderer = GetComponent<SpriteRenderer>();
        RefreshVisual();
        
    }

    public void OnClick()
    {
        Debug.Log("Levier cliqué : " + gameObject.name);

        if (audioSource != null && clickSound != null)
            audioSource.PlayOneShot(clickSound);

        ToggleLever();
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

    public void ToggleLever()
    {
        isOn = !isOn;
        Debug.Log($"Etat levier {gameObject.name} -> {(isOn ? "ON" : "OFF")}");

        RefreshVisual();
    }

    public void SetState(bool newState)
    {
        isOn = newState;
        RefreshVisual();
    }

    private void RefreshVisual()
    {
        if (isOn)
        {
            leverSpriteRenderer.flipY = false;
        }
        else
        {
            leverSpriteRenderer.flipY = true;
        }
    }
}