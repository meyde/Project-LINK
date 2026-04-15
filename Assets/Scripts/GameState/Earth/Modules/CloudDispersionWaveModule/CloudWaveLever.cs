using UnityEngine;

public class CloudWaveLever : MonoBehaviour, MouseInteractionManager.IInteractable
{
    [Header("Références")]
    public CloudWaveModuleManager moduleManager;

    [Header("Etat")]
    public bool isOn = false;

    [Header("Visuel rotation")]
    public Transform leverVisual;
    public float onAngle = -35f;
    public float offAngle = 35f;

    [Header("Visuel sprite (optionnel)")]
    public SpriteRenderer leverSpriteRenderer;
    public Sprite offSprite;
    public Sprite onSprite;

    [Header("Hover")]
    public SpriteRenderer sr;
    public Color normalColor = Color.white;
    public Color hoverColor = Color.yellow;


    private void Start()
    {
        RefreshVisual();
    }

    public void OnClick()
    {
        Debug.Log("Levier cliqué : " + gameObject.name);
        ToggleLever();
    }

    public void OnHoverEnter()
    {
        if (sr != null)
            sr.color = hoverColor;
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
        if (leverVisual != null)
        {
            float z = isOn ? onAngle : offAngle;
            leverVisual.localRotation = Quaternion.Euler(0f, 0f, z);
        }
        else
        {
            Debug.LogWarning($"leverVisual non assigné sur {gameObject.name}");
        }

        if (leverSpriteRenderer != null)
        {
            if (isOn && onSprite != null)
                leverSpriteRenderer.sprite = onSprite;
            else if (!isOn && offSprite != null)
                leverSpriteRenderer.sprite = offSprite;
        }
    }
}