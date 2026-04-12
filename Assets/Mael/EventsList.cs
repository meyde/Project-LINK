using UnityEngine;
using NUnit.Framework;

[CreateAssetMenu(fileName = "EventsList", menuName = "Scriptable Objects/EventsList")]
public class EventsList : ScriptableObject
{
    public CatastrophicEvent[] eventList;

    public CatastrophicEvent randomEvent()
    {
        var index = Random.Range(0,eventList.Length);
        return eventList[index];
    }
}
