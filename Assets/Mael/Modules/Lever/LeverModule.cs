using NUnit.Framework;
using System.Collections;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class LeverModule : MonoBehaviour, MouseInteractionManager.IInteractable
{
    [Header("Data")]
    [SerializeField] private LeverCodes currentCode;

    [Header("Lever Settings")]
    [SerializeField] private int minValue = 0;
    [SerializeField] private int maxValue = 20;
    [SerializeField] private int startValue = 10;
    [SerializeField] private float dragStepThreshold = 1f;
    [SerializeField] private float startPosY;
    [SerializeField] private int value;
    [SerializeField] private float accumulatedY = 0f;
    private int previousStep = 0;
    private int[] code;
    private int currentIndex;
    private WaitForFixedUpdate WaitForFixedUpdate = new();


    private void Awake()
    {
        value = startValue;
        startPosY= gameObject.transform.position.y;
        code = currentCode.values;
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

    public void Success()
    {
        Debug.Log("Success!");
    }

    public void Failure()
    {
        Debug.Log("Failure!");
    }
    public IEnumerator Dragging()
    {
        while (Mouse.current != null && Mouse.current.leftButton.isPressed)
        {
            float deltaY = Mouse.current.delta.ReadValue().y;
            accumulatedY += deltaY;
            int currentStep = (int)Mathf.Sign(accumulatedY);
            if ((currentStep * previousStep) != 0 && currentStep != previousStep)
            {
                if (currentIndex != currentCode.codeSize - 1)
                {
                    if (value != code[currentIndex])
                    {
                        Failure();
                    }
                    currentIndex++;
                }
                accumulatedY = 0f;
            }
            previousStep = currentStep;
            while (Mathf.Abs(accumulatedY) > dragStepThreshold)
            {
                int step = (int)Mathf.Sign(accumulatedY);
                value = Mathf.Clamp(value + step, minValue, maxValue);
                accumulatedY -= step*dragStepThreshold;
            }
            gameObject.transform.position = new Vector3(gameObject.transform.position.x,(0.4f*value-4)+startPosY , 0);

            yield return WaitForFixedUpdate;
        }
        if (value == code[currentIndex])
        {
            Success();
        }
        else
        {
            Failure();
        }
    }
}