using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using NUnit.Framework;

public class LeverModule : Module, MouseInteractionManager.IInteractable
{
    [Header("Data")]
    [SerializeField] private LeverCodes[] codes;
    [SerializeField] private int moduleId = 0;
    [SerializeField] private float[] yArray;

    [Header("Lever Settings")]
    [SerializeField] private int minValue = 0;
    [SerializeField] private int maxValue = 20;
    [SerializeField] private int startValue = 10;
    [SerializeField] private float dragStepThreshold = 1f;
    private int value;
    private float accumulatedY = 0f;
    private int previousStep = 0;
    private List<int> code=new();
    private int codeDone = -1;
    private WaitForFixedUpdate WaitForFixedUpdate = new();
    private GameManagerLocal gm;
    [SerializeField] private ZoomableModule zm;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip changeSound;
    [SerializeField] private AudioClip hoverSound;

    private void Awake()
    {
        value = startValue;
        gm = FindFirstObjectByType<GameManagerLocal>();
        if (zm == null)
        {

            zm = gameObject.GetComponent<ZoomableModule>();
        }
    }

    public void OnClick()
    {
        StartCoroutine(Dragging());
    }

    public void OnHoverEnter()
    {
        if (audioSource != null && hoverSound != null)
            audioSource.PlayOneShot(hoverSound);
    }

    public void OnHoverExit()
    {
    }





    public void ModuleEnd()
    {
        Debug.Log("Lever module code submitted.");
        foreach (LeverCodes levercode in codes)
        {
            var tempInd = levercode.id;
            int ind = 0;
            foreach (int i in levercode.values)
            {
                if (ind >= code.Count) { tempInd = -1; break; }
                if (i != code[ind])
                {
                    tempInd = -1;
                }
                ind++;
                if (tempInd != -1)
                {
                    codeDone = tempInd;
                    break;
                }

            }
        }

        List<int> falseId = new();
        bool hasSucceeded = false;
        foreach (CEventRuntimeData cEventData in gm.gmn.events)
        {
            if (cEventData.state != 1) {  continue; }
            CatastrophicEvent cEvent = gm.allEvents[cEventData.eventId];
            for (int i = 0; i < cEvent.modules1.Length; i++)
            {
                if (cEvent.modules1[i] == moduleId)
                {
                    if (cEvent.modulesState1[i] == codeDone)
                    {
                        Debug.Log($"code found in occuring events for event: {cEvent.eventId}");
                        hasSucceeded = true;
                        gm.EndModuleCheck(moduleId, true, cEvent.eventId);
                        zm.CloseModule();
                    }
                    else
                    {
                        falseId.Add(cEvent.eventId);
                    }
                }
            }
        }
        if (!hasSucceeded)
        {
            if (falseId.Count > 0)
            {
                Debug.Log("an event needing this module is occuring, yet the code was not matched. Failing oldest event ");
                gm.EndModuleCheck(moduleId, false, falseId[0]);
            }
            else
            {
                Debug.Log("No event needing this module is occuring. Failing oldest active event.");
                gm.EndModuleCheck(moduleId, false, -1);
                
            }
        }

        value = 10;
        gameObject.transform.localPosition = new Vector3(gameObject.transform.localPosition.x, yArray[value], 0);

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

                int oldValue = value;
                value = Mathf.Clamp(value - step, minValue, maxValue);

                if (value != oldValue)
                {
                    PlayChangeSound();
                }

                accumulatedY -= step * dragStepThreshold;
                previousStep = step;
            }

            gameObject.transform.localPosition = new Vector3(
                gameObject.transform.localPosition.x,
                yArray[value],
                0
            );

            yield return WaitForFixedUpdate;
        }

        code.Add(value);
        ModuleEnd();
    }

    private void PlayChangeSound()
    {
        if (audioSource == null || changeSound == null)
            return;

        audioSource.pitch = Random.Range(0.97f, 1.03f);
        audioSource.PlayOneShot(changeSound);
    }
}