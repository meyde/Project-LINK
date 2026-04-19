using UnityEngine;

[CreateAssetMenu(fileName = "PistonModesSO", menuName = "Scriptable Objects/PistonModesSO")]
public class PistonModesSO : ScriptableObject
{
    public int pistonModeId;
    public int eventId;
    public int ledOption;
    public int randomPistonId;
    public int leftFanSpeedObj;
    public int rightFanSpeedObj;
    public int leftFanTurnDirectionObj;
    public int rightFanTurnDirectionObj;

}
