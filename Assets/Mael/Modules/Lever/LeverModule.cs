using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class LeverModule : MonoBehaviour, MouseInteractionManager.IInteractable
{
    private LeverCodes currentcode;
    [SerializeField] private SpriteRenderer[] spritelist;
    private int LeverPosition;
    private MouseInteractionManager mim;
    [SerializeField] private InputAction mouseClick;
    private WaitForFixedUpdate WaitForFixedUpdate = new();
    private void Awake()
    {
        mim = FindFirstObjectByType<MouseInteractionManager>();
    }

    public void Update()
    {

    }
    public IEnumerator Draging()
    {
        while (Mouse.current.leftButton.IsPressed())
        {
            DeltaControl delta = Mouse.current.delta;
            if (Mathf.Abs(delta.value.y) > 3)
            {
                if (delta.value.y <0) { Debug.Log("goDown"); }
                if (delta.value.y > 0) { Debug.Log("goUp"); }
            }

            yield return WaitForFixedUpdate;
        }
    }

    public void OnClick() 
    {
        StartCoroutine(Draging());
    }

    public void OnHoverEnter()
    {

    }

    public void OnHoverExit()
    {

    }



}
