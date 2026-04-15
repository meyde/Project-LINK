using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class LeverModule : Module, MouseInteractionManager.IInteractable
{
    [Header("Data")]
    [SerializeField] private LeverCodes[] codes;
    [SerializeField] private int moduleId = 0;
    [SerializeField] private int[] yArray;

    [Header("Lever Settings")]
    [SerializeField] private int minValue = 0;
    [SerializeField] private int maxValue = 20;
    [SerializeField] private int startValue = 10;
    [SerializeField] private float dragStepThreshold = 1f;
    private int value;
    private float accumulatedY = 0f;
    private int previousStep = 0;
    private List<int> code=new();
    private int optionColor;
    private int codeDone = -1;
    private WaitForFixedUpdate WaitForFixedUpdate = new();
    private GameManagerLocal gm;


    private void Awake()
    {
        value = startValue;
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
        int index = 0;
        List<int> falseInd = new();
        bool hasSucceeded = false;
        foreach (int eventId in gm.gmn.eventDataIds)
        {
            CatastrophicEvent cEvent = gm.allEvents[eventId];
            for (int i = 0; i < cEvent.modules1.Length; i++)
            {
                if (cEvent.modules1[i] == moduleId)
                {
                    if (cEvent.modulesState1[i] == codeDone)
                    {
                        hasSucceeded = true;
                        gm.EndModuleCheck(moduleId, true, index);
                    }
                    else
                    {
                        falseInd.Add(i);
                    }
                }
            }
            index++;
        }
        if (!hasSucceeded)
        {
            if (falseInd.Count > 0)
            {
                gm.EndModuleCheck(moduleId, false, falseInd[0]);
            }
            else
            {
                if (gm.gmn.eventDataIds.Count > 0)
                {
                    gm.EndModuleCheck(moduleId, false, 0);
                }
            }
        }



    }

    public IEnumerator Dragging()
    {
        while (Mouse.current != null && Mouse.current.leftButton.isPressed)
        {
            float deltaY = Mouse.current.delta.ReadValue().y;
            accumulatedY += deltaY;
            
            
            while (Mathf.Abs(accumulatedY) > dragStepThreshold)
            {
                int step = (int)Mathf.Sign(accumulatedY);
                if ((step * previousStep) != 0 && step != previousStep)
                {
                    code.Add(value);
                    accumulatedY = 0f;
                }
                
                value = Mathf.Clamp(value - step, minValue, maxValue);
                accumulatedY -= step*dragStepThreshold;
                previousStep = step;
            }
            gameObject.transform.position = new Vector3(gameObject.transform.position.x, yArray[value] , 0);

            yield return WaitForFixedUpdate;
        }
        code.Add(value);
        ModuleEnd();
    }
}