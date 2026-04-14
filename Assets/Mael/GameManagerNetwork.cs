using UnityEngine;
using Unity.Netcode;
using System.Linq;
using System.Collections.Generic;
using System.Collections;
using Unity.Collections;
public class GameManagerNetwork : NetworkBehaviour
{
    [Header("Events")]
    [SerializeField] private CatastrophicEvent[] level1Events;
    [SerializeField] private CatastrophicEvent[] level2Events;
    [SerializeField] private CatastrophicEvent[] level3Events;
    [Header("data")]
    [SerializeField] private int approximateTimeForEvents = 45;
    [SerializeField] private int randomisationTime = 30;
    [SerializeField] private int requiredSuccesses = 4;
    private int gameLevel;

    public NetworkList<int> eventDataIds = new();
    public NetworkList<int> eventLives = new();
    public NetworkList<int> eventLastModules = new();
    public NetworkList<int> eventStates = new();
    public NetworkVariable<int> health = new(3);
    public NetworkVariable<int> successes = new(0);
    public NetworkList<int> eventModuleOption = new();

    private Coroutine eventCoroutine;


    private CatastrophicEvent[] GetEventsLevel()
    {
        return gameLevel switch
        {
            1 => level1Events,
            2 => level2Events,
            3 => level3Events,
            _ => level1Events
        };
    }

    public void OnStartGame()
    {
        if (!IsServer) return;
        eventCoroutine = StartCoroutine(EventGeneration());
    }

    public void Start()
    {
        OnStartGame();
    }
    private void OnGameLoss()
    {
        Debug.Log("Game Lost");
        if (eventCoroutine != null) { StopCoroutine(eventCoroutine); }
    }
    private void OnGameWin()
    {
        Debug.Log("Game Won");
        if (eventCoroutine != null) { StopCoroutine(eventCoroutine); }
    }
    public IEnumerator EventGeneration()
    {
        while (true)
        {
            var eventList = GetEventsLevel();
            int id = Random.Range(0, eventList.Length);
            eventDataIds.Add(id);
            eventLives.Add(eventList[id].baseLife);
            eventLastModules.Add(-1);
            eventStates.Add(0);
            eventModuleOption.Add(0);
            
            yield return new WaitForSeconds(Random.Range(0, randomisationTime) + approximateTimeForEvents);
        }
    }
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void EventLoseLifeServerRpc(int eventIndex)
    {

        if (eventIndex < 0 ) 
        {
            eventLives[0]--;
            if (eventLives[0]<=0) {  OnFailureRpc(0); }
        }
        else
        {
            eventLives[eventIndex]--;
            if (eventLives[eventIndex] <= 0) { OnFailureRpc(eventIndex); }
        }

          
    }
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void OnFailureRpc(int eventId)
    {
        health.Value--;
        eventDataIds.RemoveAt(eventId);
        eventLives.RemoveAt(eventId);
        eventLastModules.RemoveAt(eventId);
        eventStates.RemoveAt(eventId);
        eventModuleOption.RemoveAt(eventId);
        if (health.Value < 1) 
        {
            OnGameLoss(); 
        }
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void OnSuccessRpc(int eventId)
    {
        successes.Value++;
        eventDataIds.RemoveAt(eventId);
        eventLives.RemoveAt(eventId);
        eventLastModules.RemoveAt(eventId);
        eventStates.RemoveAt(eventId);
        eventModuleOption.RemoveAt(eventId);
        if (successes.Value >= requiredSuccesses)
        {
            OnGameWin();
        }
    }
}
