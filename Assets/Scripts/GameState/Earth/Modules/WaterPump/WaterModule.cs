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
    private int waterState;
    private GameManagerLocal gm;

    private void Awake()
    {
        gm=FindFirstObjectByType<GameManagerLocal>();
    }


    public override void OnStarted()
    {
        waterState = Random.Range(0, 8);
        waterSprite.sprite = sprites[waterState];
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
                    List<WaterCodeSO> codeList = new();
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
