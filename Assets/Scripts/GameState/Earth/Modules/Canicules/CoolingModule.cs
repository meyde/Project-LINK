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
    [SerializeField] private int leftFanSpeedPiston;
    [SerializeField] private int rightFanSpeedPiston;
    [SerializeField] private int leftFanTurnDirectionPiston;
    [SerializeField] private int rightFanTurnDirectionPiston;
    [SerializeField] private int randomPiston;
    private int leftFanSpeed = 0 ;
    private int rightFanSpeed = 0;
    private int leftFanTurnDirection = 0;
    private int rightFanTurnDirection = 0;
    private int leftFanSpeedObj;
    private int rightFanSpeedObj;
    private int leftFanTurnDirectionObj;
    private int rightFanTurnDirectionObj;


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
                leftFanSpeedPiston = pistonMode.leftFanSpeedPistonId;
                leftFanTurnDirectionPiston = pistonMode.leftFanTurnDirectionId;
                rightFanSpeedPiston = pistonMode.rightFanSpeedPistonId;
                rightFanTurnDirectionPiston = pistonMode.rightFanTurnDirectionId;
                randomPiston = pistonMode.randomPistonId;
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
            leftFan.Refreshrotation(leftFanSpeed, leftFanTurnDirection / 2);
        }
        if (pistonId == rightFanSpeedPiston)
        {
            rightFanSpeed = (rightFanSpeed + 1) % 5;
            rightFan.Refreshrotation(rightFanSpeed, rightFanTurnDirection / 2);
        }
        if (pistonId == leftFanTurnDirectionPiston)
        {
            leftFanTurnDirection = (leftFanTurnDirection + 1) % 5;
            leftFan.Refreshrotation(leftFanSpeed, leftFanTurnDirection / 2);
        }
        if (pistonId == rightFanTurnDirectionPiston)
        {
            rightFanTurnDirection = (rightFanTurnDirection + 1) % 5;
            rightFan.Refreshrotation(rightFanSpeed, rightFanTurnDirection / 2);
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
            (leftFanTurnDirection / 2) == leftFanTurnDirectionObj &&
            (rightFanTurnDirection / 2) == rightFanTurnDirectionObj;

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
