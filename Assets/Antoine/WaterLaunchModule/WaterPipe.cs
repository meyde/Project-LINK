using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class WaterPipe : MonoBehaviour, MouseInteractionManager.IInteractable
{
    [Header("Pipe")]
    public PipeShapeType shapeType;
    [Range(0, 3)] public int rotationStep; // 0,1,2,3 => 0,90,180,270

    [Header("Grid Position")]
    public Vector2Int gridPosition;

    [Header("Visual")]
    public Color normalColor = Color.white;
    public Color hoverColor = Color.yellow;
    public Color activePathColor = Color.cyan;

    private SpriteRenderer sr;
    private WaterModuleManager moduleManager;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        moduleManager = GetComponentInParent<WaterModuleManager>();
        ApplyRotation();
        SetPathHighlight(false);
    }

    public void OnClick()
    {
        rotationStep = (rotationStep + 1) % 4;
        ApplyRotation();
        moduleManager?.ValidateModule();
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

    public void SetPathHighlight(bool active)
    {
        if (sr != null)
            sr.color = active ? activePathColor : normalColor;
    }

    private void ApplyRotation()
    {
        transform.rotation = Quaternion.Euler(0f, 0f, -90f * rotationStep);
    }

    public List<PipeDirection> GetOpenDirections()
    {
        List<PipeDirection> dirs = new List<PipeDirection>();

        if (shapeType == PipeShapeType.Straight)
        {
            // base = Up / Down
            if (rotationStep % 2 == 0)
            {
                dirs.Add(PipeDirection.Up);
                dirs.Add(PipeDirection.Down);
            }
            else
            {
                dirs.Add(PipeDirection.Left);
                dirs.Add(PipeDirection.Right);
            }
        }
        else if (shapeType == PipeShapeType.Corner)
        {
            // base = Up / Right
            switch (rotationStep)
            {
                case 0:
                    dirs.Add(PipeDirection.Up);
                    dirs.Add(PipeDirection.Right);
                    break;
                case 1:
                    dirs.Add(PipeDirection.Right);
                    dirs.Add(PipeDirection.Down);
                    break;
                case 2:
                    dirs.Add(PipeDirection.Down);
                    dirs.Add(PipeDirection.Left);
                    break;
                case 3:
                    dirs.Add(PipeDirection.Left);
                    dirs.Add(PipeDirection.Up);
                    break;
            }
        }

        return dirs;
    }

    public bool IsOpenTo(PipeDirection dir)
    {
        return GetOpenDirections().Contains(dir);
    }
}