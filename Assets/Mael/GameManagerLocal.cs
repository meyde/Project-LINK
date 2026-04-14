using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Rendering;

public class GameManagerLocal : MonoBehaviour
{
    public GameManagerNetwork gmn;
    public CatastrophicEvent[] allEvents;
    private void Awake()
    {
        gmn = FindFirstObjectByType<GameManagerNetwork>();

        if (gmn != null)
        {
            gmn.eventDataIds.OnListChanged += OnEventsChanged;
        }
    }

    private void OnDisable()
    {
        if (gmn != null)
            gmn.eventDataIds.OnListChanged -= OnEventsChanged;
    }

    private void OnEventsChanged(NetworkListEvent<int> change)
    {
        if (change.Type == NetworkListEvent<int>.EventType.Add)
        {
            int id = gmn.eventDataIds[change.Index];

            CatastrophicEvent evnt = allEvents[id];


            OnNewEvent(evnt);
        }
    }
    public void OnNewEvent (CatastrophicEvent cEvent)
    {
        StartCoroutine(CountDown(cEvent));
    }
    public IEnumerator CountDown(CatastrophicEvent cEvent)
    {
        yield return new WaitForSeconds(cEvent.eventDuration);
        int pos = -1; 
        for (int i=0; i< gmn.eventDataIds.Count;i++)  
        {
            if (gmn.eventDataIds[i] == cEvent.eventId) { pos = i; }
        }
        if (pos == -1)
            yield break;
        if (gmn.eventStates[pos] != 2 )
        {
            gmn.OnFailureRpc(pos);
        }
        
    }
    public void EndModuleCheck ( bool state, int eventPos)
    {
        if (eventPos > -1)
        {
            CatastrophicEvent cEvent = allEvents[gmn.eventDataIds[eventPos]];
            if (state)
            {
                //if (gmn.eventModulesDone[eventPos] == cEvent.modules.Count() - 1)
                //{
                //    gmn.OnSuccessRpc(eventPos);
                //}
                //else
                //{
                //    gmn.OnIncrementRpc(eventPos);
                //}
            }
            else
            {
                gmn.EventLoseLifeServerRpc(eventPos);
            }
        }
        else
        {
            gmn.EventLoseLifeServerRpc(eventPos);
        }
    }

}
