using UnityEngine;

[CreateAssetMenu(fileName = "LeverCodes", menuName = "Scriptable Objects/LeverCodes")]
public class LeverCodes : ScriptableObject
{
    public int id;
    public int codeSize;
    public int[] values;
}
