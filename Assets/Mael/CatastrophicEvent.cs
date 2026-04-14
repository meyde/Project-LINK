using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;


[CreateAssetMenu(fileName = "CatastrophicEvent", menuName = "Scriptable Objects/CatastrophicEvent")]
public class CatastrophicEvent : ScriptableObject
{
    public int eventId;
    public string eventName;
    public string localisation;
    public string type;
    public int[] modules; //in order of completion
    public int[] moduleState; //in order of completion.
    public int eventCategoryLevel;
    public int eventDuration;
    public int baseLife = 2;
    public int Captor1Info;
    public int Captor2Info;
    public int Captor3Info;
    public int Captor4Info;
}
