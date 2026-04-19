using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WaterModule : Module
{
    [SerializeField] private int moduleId = 3;
    [SerializeField] private PipeController[] pipeList;
    [SerializeField] private BorderPipeController[] borderPipeList;
    [SerializeField] private WaterCodeSO[] codeList;
    [SerializeField] private SpriteRenderer waterSprite;
    [SerializeField] private Sprite[] sprites;
    private int waterState=0;
    private GameManagerLocal gm;
    private List<int> availableStates = new();
    private ZoomableModule zm;
    private void Awake()
    {
        gm=FindFirstObjectByType<GameManagerLocal>();
        zm = gameObject.GetComponent<ZoomableModule>();
    }


    public override void OnStarted()
    {
        Debug.Log("started water module");
        foreach (CEventRuntimeData cEventData in gm.gmn.events)
        {
            if (cEventData.state != 1) { continue; }
            CatastrophicEvent cEvent = gm.allEvents[cEventData.eventId];
            for (int i = 0; i < cEvent.modules1.Length; i++)
            {
                if (cEvent.modules1[i] == moduleId)
                {
                    Debug.Log("Looking at each code, trying to find the correct one");
                    foreach (WaterCodeSO codeSo in codeList)
                    {
                        if (cEvent.modulesState1[i] == codeSo.waterCodeId && codeSo.eventId == cEvent.eventId && cEvent.region == gm.currentRegion)
                        {
                            Debug.Log($"Found a state, adding it: {codeSo.waterStateId}");
                            availableStates.Add(codeSo.waterStateId);
                        }
                    }
                }
            }

        }
        if (availableStates.Count > 0)
        {
            Debug.Log("at least one state found.");
            waterState = availableStates[Random.Range(0, availableStates.Count)];
            Debug.Log($"state chosen: {waterState}");
            waterSprite.sprite = sprites[waterState];
        }
        else
        {
            Debug.Log("No state found, using pure random.");
            waterState = Random.Range(0, sprites.Count());
            waterSprite.sprite = sprites[waterState];
        }
    }

    public void Validate()
    {
        List<int> falseInd = new();
        bool hasSucceeded = false;
        foreach (CEventRuntimeData cEventData in gm.gmn.events)
        {
            if (cEventData.state != 1) { continue; }
            CatastrophicEvent cEvent = gm.allEvents[cEventData.eventId];
            for (int i = 0; i < cEvent.modules1.Length; i++)
            {
                if (cEvent.modules1[i] == moduleId)
                {
                    Debug.Log($"event {cEvent.eventId} has the module in its required list.");
                    foreach (WaterCodeSO code in codeList)
                    {
                        if (code.eventId == cEvent.eventId && code.waterStateId == waterState)
                        {
                            if (CheckCode(code.pipeOrientations, code.borderStates))
                            {
                                Debug.Log($"Event has succeeded, validating.");
                                hasSucceeded = true;
                                gm.EndModuleCheck(moduleId, true, cEvent.eventId);
                                zm.CloseModule();
                            }
                            else
                            {
                                Debug.Log("Event has failed. adding it to list of failed events.");
                                falseInd.Add(code.eventId);
                            }
                        }
                    }
                }
            }
        }
        if (!hasSucceeded)
        {
            Debug.Log("module failed.");
            if (falseInd.Count > 0)
            {
                Debug.Log("an event needing this module is occuring, yet the code was not matched. Failing oldest event ");
                gm.EndModuleCheck(moduleId, false, falseInd[0]);
            }
            else
            {
                Debug.Log("No event needing this module is occuring. Failing oldest active event.");
                gm.EndModuleCheck(moduleId, false, -1);
                
            }
        }
    }

    private bool CheckCode(int[] pipeOrientation, int[] borderState)
    {
        for (int i =0; i<pipeList.Count();i++) 
        {
            if (pipeList[i].orientation != pipeOrientation[i] )
            {
                return false;
            }
        }
        for (int i = 0; i < borderPipeList.Count(); i++)
        {
            if (borderPipeList[i].state != borderState[i])
            {
                return false;
            }
        }
        return true;
    }


}
