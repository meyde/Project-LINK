using UnityEngine;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class WaterModulePort : MonoBehaviour, MouseInteractionManager.IInteractable
{
    [Header("Port")]
    public ModulePortId portId;
    public WaterPortState currentState = WaterPortState.None;

    [Header("Connection")]
    public WaterPipe connectedPipe;

    [Header("Visual")]
    public Color noneColor = Color.white;
    public Color inputColor = Color.green;
    public Color outputColor = Color.red;
    public Color hoverTint = Color.yellow;

    private SpriteRenderer sr;
    private WaterModuleManager moduleManager;
    private Color baseColor;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        moduleManager = GetComponentInParent<WaterModuleManager>();
        RefreshVisual();
    }

    public void OnClick()
    {
        // Cycle : None -> Input -> Output -> None
        switch (currentState)
        {
            case WaterPortState.None:
                SetState(WaterPortState.Input);
                break;

            case WaterPortState.Input:
                SetState(WaterPortState.Output);
                break;

            case WaterPortState.Output:
                SetState(WaterPortState.None);
                break;
        }

        if (moduleManager != null)
            moduleManager.OnPortStateChanged(this);
    }

    public void SetState(WaterPortState newState)
    {
        currentState = newState;
        RefreshVisual();
    }

    public void RefreshVisual()
    {
        switch (currentState)
        {
            case WaterPortState.None:
                baseColor = noneColor;
                break;

            case WaterPortState.Input:
                baseColor = inputColor;
                break;

            case WaterPortState.Output:
                baseColor = outputColor;
                break;
        }

        if (sr != null)
            sr.color = baseColor;
    }

    public void OnHoverEnter()
    {
        if (sr != null)
            sr.color = hoverTint;
    }

    public void OnHoverExit()
    {
        if (sr != null)
            sr.color = baseColor;
    }
}