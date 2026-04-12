using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class GameManagerProto : MonoBehaviour
{
    [SerializeField] private EventsList level1List;

    private CatastrophicEvent currentEvent;
    private int[] modulesDone;
    private int[] modulesSolutions;

    private void CreateLevel1Event()
    {
        var evnt = level1List.randomEvent();
        currentEvent = evnt;
        evnt.state = 1;
    }
    private bool isEventFixed()
    {
        return (modulesDone ==currentEvent.modules && modulesSolutions== currentEvent.moduleState);
    }
    public void onSucceedEvent()
    {
        print("youhou");
    }
    public void onFailEvent()
    {
        print(":(");
    }
    public IEnumerator CountDown()
    {
        yield return new WaitForSeconds(currentEvent.eventDuration);
        if (isEventFixed()) { currentEvent.state = 2; onSucceedEvent(); } else { currentEvent.state = 3; onFailEvent(); }
        
    }







}
