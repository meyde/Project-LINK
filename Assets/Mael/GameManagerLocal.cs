using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Rendering;

public class GameManagerLocal : MonoBehaviour
{
    public GameManagerNetwork gmn;
    public List<CatastrophicEvent> currentEvents;
    [SerializeField] private CatastrophicEvent[] allEvents;
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

            currentEvents.Add(evnt);

            OnNewEvent(evnt);
        }
    }
    private int IsRightModule(int module, int state)
    {
        for (int i=0; i < gmn.eventDataIds.Count; i++) 
        {
            CatastrophicEvent events = currentEvents[i];

            if (events.modules[gmn.eventLastModules[i]+1] == module && events.moduleState[gmn.eventLastModules[i] + 1] == state)
            {
                return i;
            }
            ; 

        }
        return -1;
    }
    private int EventOfModule(int module)
    {
        for (int i = 0; i < gmn.eventDataIds.Count; i++)
        {
            CatastrophicEvent events = currentEvents[i];
            if (events.modules[gmn.eventLastModules[i] + 1] == module)
            {
                return i;
            }
        }
        return -1;
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
            Debug.Log("gml:ligne 86");
            gmn.OnFailureRpc(pos);
        }
        
    }
    public void EndModuleCheck (int module, int state, int option)
    {
        int index = IsRightModule(module, state);
        
        if (index>-1)
        {
            CatastrophicEvent eventDone = currentEvents[index];
            if (gmn.eventLastModules[index] == eventDone.modules.Count()-1)
            {
                gmn.eventStates[index] = 2;
                gmn.OnSuccessRpc(index);
            }
            else
            {

                gmn.eventModuleOption[index] += option;
                gmn.eventLastModules[index]++;
                
            }


        }
        else
        {
            int eventInd = EventOfModule(module);
            gmn.EventLoseLifeServerRpc(eventInd);
        }
    }

}
