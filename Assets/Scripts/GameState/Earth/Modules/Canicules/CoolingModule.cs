using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.InputSystem.PlayerInput;

public class CoolingModule : Module
{
    private int moduleId = 5;
    private CEventRuntimeData? activeEvent = null;
    [SerializeField] private GameManagerLocal gm;
    [SerializeField] private PistonModesSO[] allModes;
    [SerializeField] private FanController leftFan;
    [SerializeField] private FanController rightFan;
    private bool autoLose;
    [SerializeField] private int leftFanSpeedPiston=0;
    [SerializeField] private int rightFanSpeedPiston=1;
    [SerializeField] private int leftFanTurnDirectionPiston=2;
    [SerializeField] private int rightFanTurnDirectionPiston=3;
    [SerializeField] private int randomPiston=4;
    [SerializeField] private int leftFanSpeed = 0 ;
    [SerializeField] private int rightFanSpeed = 0;
    [SerializeField] private int leftFanTurnDirection = 0;
    [SerializeField] private int rightFanTurnDirection = 0;
    [SerializeField] private int leftFanSpeedObj;
    [SerializeField] private int rightFanSpeedObj;
    [SerializeField] private int leftFanTurnDirectionObj;
    [SerializeField] private int rightFanTurnDirectionObj;

    private int TrueSign(float value)
    {
        if (value > 0) return 1;
        if (value < 0) return -1;
        return 0;
    }
    public override void OnStarted()
    {
        int currReg = gm.currentRegion;
        foreach (CEventRuntimeData cEventData in gm.gmn.events)
        {
            if (cEventData.state != 1) { continue; }
            CatastrophicEvent cEvent = gm.allEvents[cEventData.eventId];
            if (cEvent.region == currReg && cEventData.module1Option != -1 && cEvent.modules2.Contains(moduleId))
            {
                Debug.Log($"event {cEvent.eventId} is occuring in this region, need this module and has done its first module");

                activeEvent = cEventData;
            }
        }
        if (!activeEvent.HasValue)
        {
            Debug.Log($" No event occuring found in this region that has done it's first module. any validation henceforth will FAIL");
            autoLose = true;
            return;
        }

        CEventRuntimeData currentCEventData = activeEvent.Value;
        foreach (PistonModesSO pistonMode in allModes)
        {
            if(pistonMode.eventId ==currentCEventData.eventId && pistonMode.ledOption == currentCEventData.module1Option)
            {
                Debug.Log("shuffling");
                int bomb = pistonMode.randomPistonId;
                int[] pistons = new int[4];
                int index = 0;
                for (int i=0; i<5; i++)
                {
                    if (i == bomb) continue;
                    pistons[index] = i; index++;
                }
                var rng = new System.Random();
                rng.Shuffle(pistons);
                leftFanSpeedPiston = pistons[0];
                leftFanTurnDirectionPiston = pistons[1];
                rightFanSpeedPiston = pistons[2];
                rightFanTurnDirectionPiston = pistons[3];
                randomPiston = bomb;
                leftFanSpeedObj = pistonMode.leftFanSpeedObj;
                leftFanTurnDirectionObj = pistonMode.leftFanTurnDirectionObj;
                rightFanSpeedObj = pistonMode.rightFanSpeedObj;
                rightFanTurnDirectionObj = pistonMode.rightFanTurnDirectionObj;
            }
        }   
    }

    public void OnPistonChange(int pistonId)
    {
        if (pistonId == leftFanSpeedPiston)
        {
            leftFanSpeed = (leftFanSpeed + 1) % 5;
            leftFan.Refreshrotation(leftFanSpeed, leftFanTurnDirection);
        }
        if (pistonId == rightFanSpeedPiston)
        {
            rightFanSpeed = (rightFanSpeed + 1) % 5;
            rightFan.Refreshrotation(rightFanSpeed, rightFanTurnDirection );
        }
        if (pistonId == leftFanTurnDirectionPiston)
        {
            leftFanTurnDirection = (leftFanTurnDirection + 1) % 5;
            leftFan.Refreshrotation(leftFanSpeed, leftFanTurnDirection);
        }
        if (pistonId == rightFanTurnDirectionPiston)
        {
            rightFanTurnDirection = (rightFanTurnDirection + 1) % 5;
            rightFan.Refreshrotation(rightFanSpeed, rightFanTurnDirection);
        }
        if (pistonId == randomPiston)
        {
            int newid = Random.Range(0, 4);
            while (newid == randomPiston)
            {
                newid = Random.Range(0, 4);
            }
            OnPistonChange(newid);
        }

    }


    public void OnValidation()
    {
        if (autoLose)
        {
            Debug.Log("Validated when no event was found. Losing life.");
            gm.EndModuleCheck(moduleId, false, -1);
        }
        bool vaildation = leftFanSpeed == leftFanSpeedObj &&
            rightFanSpeed == rightFanSpeedObj &&
            TrueSign(leftFanTurnDirection - 2) == leftFanTurnDirectionObj &&
            TrueSign(rightFanTurnDirection - 2) == rightFanTurnDirectionObj;

        if (vaildation)
        {
            Debug.Log("code correct trouvé");
            gm.EndModuleCheck(moduleId,true,activeEvent.Value.eventId);
            return;
        }
        else
        {
            Debug.Log("error in code found");
            gm.EndModuleCheck(moduleId,false,activeEvent.Value.eventId);
        }
    }





}
