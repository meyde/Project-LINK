using UnityEngine;

[CreateAssetMenu(fileName = "WaterCodeSO", menuName = "Scriptable Objects/WaterCodeSO")]
public class WaterCodeSO : ScriptableObject
{
    public int waterCodeId;
    public int eventId;
    public int waterStateId;

    public int[] pipeOrientations;
    public int[] borderStates;
}
