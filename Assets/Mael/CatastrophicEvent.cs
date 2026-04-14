using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;


[CreateAssetMenu(fileName = "CatastrophicEvent", menuName = "Scriptable Objects/CatastrophicEvent")]
public class CatastrophicEvent : ScriptableObject
{
    public int eventId;
    public string eventName;
    public string type;
    public int region;
    public int modulesCount;
    public int[] modules1; 
    public int[] modules2;
    public int[] modules3;
    public int[] modulesState1;
    public int[] modulesState2;
    public int[] modulesState3;
    public int eventCategoryLevel;
    public int eventDuration;
    public int baseLife = 2;
    public int WindSpeed;
    public int Temperature;
    public int Intensity;
    public int OxygenLevel;
}
