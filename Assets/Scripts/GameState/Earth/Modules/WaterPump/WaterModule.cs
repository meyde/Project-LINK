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

    private void Awake()
    {
        gm=FindFirstObjectByType<GameManagerLocal>();
    }


    public override void OnStarted()
    {
        foreach (int eventId in gm.gmn.eventDataIds)
        {
            CatastrophicEvent cEvent = gm.allEvents[eventId];
            for (int i = 0; i < cEvent.modules1.Length; i++)
            {
                if (cEvent.modules1[i] == moduleId)
                {
                    foreach (WaterCodeSO codeSo in codeList)
                    {
                        if (cEvent.modulesState1[i] == codeSo.waterStateId)
                        {
                            availableStates.Add(codeSo.waterStateId);
                        }
                    }
                }
            }

        }
        if (availableStates.Count > 0)
        {
            waterState = availableStates[Random.Range(0, availableStates.Count)];
            waterSprite.sprite = sprites[waterState];
        }
        else
        {
            waterState = 0;
            waterSprite.sprite = sprites[0];
        }
    }

    public void Validate()
    {
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
                    foreach (WaterCodeSO code in codeList)
                    {
                        if (code.eventId == eventId && code.waterStateId == waterState)
                        {
                            if (CheckCode(code.pipeOrientations, code.borderStates))
                            {
                                hasSucceeded = true;
                                gm.EndModuleCheck(moduleId, true, index);
                            }
                            else
                            {
                                falseInd.Add(code.eventId);
                            }
                        }
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
