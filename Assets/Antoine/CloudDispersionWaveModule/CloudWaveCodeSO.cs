using UnityEngine;

[CreateAssetMenu(fileName = "CloudWaveCodeSO", menuName = "Scriptable Objects/CloudWaveCodeSO")]
public class CloudWaveCodeSO : ScriptableObject
{
    public int eventId;
    public int signalId;

    public bool[] leverCode;
}
