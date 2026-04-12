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
    public int Captor1Info;
    public int Captor2Info;
    public int Captor3Info;
    public int Captor4Info;
    public int Captor5Info;
    public int Captor6Info;
    public int state;//0:inactive, 1: occuring, 2:suceeded, 3: failed.
}
