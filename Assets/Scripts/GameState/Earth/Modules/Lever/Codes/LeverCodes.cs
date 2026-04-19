using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "LeverCodes", menuName = "Scriptable Objects/LeverCodes")]
public class LeverCodes : ScriptableObject
{
    public int id;
    public int codeSize;
    public List<int> values;
}
