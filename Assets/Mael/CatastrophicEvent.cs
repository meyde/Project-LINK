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
    public int[] module1; 
    public int[] module2;
    public int[] module3;
    public int[] moduleState1;
    public int[] moduleState2;
    public int[] moduleState3;
    public int eventCategoryLevel;
    public int eventDuration;
    public int baseLife = 2;
    public int WindSpeed;
    public int Temperature;
    public int Intensity;
    public int OxygenLevel;
}
