using NUnit.Framework;
using System.Collections;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class LeverModule : MonoBehaviour, MouseInteractionManager.IInteractable
{
    [Header("Data")]
    [SerializeField] private LeverCodes[] codes;
    [SerializeField] private int moduleId =0;

    [Header("Lever Settings")]
    [SerializeField] private int minValue = 0;
    [SerializeField] private int maxValue = 20;
    [SerializeField] private int startValue = 10;
    [SerializeField] private float dragStepThreshold = 1f;
    [SerializeField] private float startPosY;
    private int value;
    private float accumulatedY = 0f;
    private int previousStep = 0;
    private int[] code=new int[3];
    private int currentIndex;
    private int optionColor;
    private int codeDone = -1;
    private WaitForFixedUpdate WaitForFixedUpdate = new();
    private GameManagerLocal gm;


    private void Awake()
    {
        value = startValue;
        startPosY= gameObject.transform.position.y;
        gm = FindFirstObjectByType<GameManagerLocal>();
    }

    public void OnClick()
    {
        StartCoroutine(Dragging());
    }

    public void OnHoverEnter()
    {
    }

    public void OnHoverExit()
    {
    }

    public void ModuleEnd()
    {
        foreach (LeverCodes levercode in codes)
        {
            if (code==levercode.values)
            {
                codeDone = levercode.id;
            }
        }
        optionColor = Random.Range(0, 3);
        gm.EndModuleCheck(moduleId, codeDone, optionColor);
    }

    public IEnumerator Dragging()
    {
        currentIndex = 0;
        while (Mouse.current != null && Mouse.current.leftButton.isPressed)
        {
            float deltaY = Mouse.current.delta.ReadValue().y;
            accumulatedY += deltaY;
            
            
            while (Mathf.Abs(accumulatedY) > dragStepThreshold)
            {
                int step = (int)Mathf.Sign(accumulatedY);
                if ((step * previousStep) != 0 && step != previousStep)
                {
                    code[currentIndex] = value;
                    accumulatedY = 0f;
                    currentIndex++;
                }
                
                value = Mathf.Clamp(value - step, minValue, maxValue);
                accumulatedY -= step*dragStepThreshold;
                previousStep = step;
            }
            gameObject.transform.position = new Vector3(gameObject.transform.position.x,(-0.4f*value+4)+startPosY , 0);

            yield return WaitForFixedUpdate;
        }
        code[currentIndex] = value;
        ModuleEnd();
    }
}