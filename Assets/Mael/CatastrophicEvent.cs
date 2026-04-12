using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;


[CreateAssetMenu(fileName = "CatastrophicEvent", menuName = "Scriptable Objects/CatastrophicEvent")]
public class CatastrophicEvent : ScriptableObject
{
    public string eventName;
    public string localisation;
    public string type;
    public int[] modules; //in order of completion
    public int[] moduleState; //in order of completion.
    public int eventCategoryLevel;
    public int eventDuration;
    public int state;//0:inactive, 1: occuring, 2:suceeded, 3: failed.
}
