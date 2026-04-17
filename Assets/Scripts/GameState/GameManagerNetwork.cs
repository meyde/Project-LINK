using System.Collections;
using System.Linq;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class GameManagerNetwork : NetworkBehaviour
{
    public static GameManagerNetwork Instance { get; private set; }
    [SerializeField] BiomeSpriteChanger bsc;

    [Header("Events")]

    public CatastrophicEvent[] allEvents;
    [SerializeField] private CatastrophicEvent[] level1Events;
    [SerializeField] private CatastrophicEvent[] level2Events;
    [SerializeField] private CatastrophicEvent[] level3Events;
    [Header("data")]
    [SerializeField] private int approximateTimeForEvents = 45;
    [SerializeField] private int randomisationTime = 30;
    [SerializeField] private int requiredSuccesses = 4;
    [SerializeField] private float timeBeforeStart = 30f;
    private int gameLevel;
    private bool gameStarted;

    public NetworkList<CEventRuntimeData> events = new(); 
    public NetworkList<int> occupiedRegions;

    public NetworkVariable<int> health = new(3);
    public NetworkVariable<int> successes = new(0);
    [SerializeField] private Sprite[] pictoIcons;
    [SerializeField] private Color[] pictoColors;
    [SerializeField] private Sprite[] pictoNmbers;

    public int RequiredSuccesses => requiredSuccesses;

    [SerializeField] private float gameDuration = 300f; // dur�e totale de la partie en secondes
    public NetworkVariable<float> remainingGameTime = new(0f);

    private Coroutine gameTimerCoroutine;
    private bool gameEnded;

    private Coroutine eventCoroutine;

    public override void OnNetworkSpawn()
    {
        if (Instance == null)
            Instance = this;

        OnStartGame();
    }

    public void OnStartGame()
    {
        if (!IsServer || gameStarted) return;

        gameStarted = true;
        gameEnded = false;
        successes.Value = 0;
        health.Value = 3;
        remainingGameTime.Value = gameDuration;

        eventCoroutine = StartCoroutine(EventGenerationRepeating());
        gameTimerCoroutine = StartCoroutine(GameTimerCoroutine());
    }
    public int FindIndexFromId(int eventId)
    {
        Debug.Log($"Trying to access the index of the event {eventId} ");
        for (int i = 0; i < events.Count; i++)
        {
            if (events[i].eventId == eventId)
            {
                Debug.Log($"Found it: {i}");
                return i;
            }
                
        }

        return -1;
    }

    public int FindIdOfFirstActivated()
    {
        foreach (CEventRuntimeData cEventData  in events)
        {
            if (cEventData.state == 1)
            {
                return cEventData.eventId;
            }
        }
        return -1;
    }
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





    private void OnGameLoss()
    {
        if (gameEnded) return;
        gameEnded = true;

        Debug.Log("Game Lost");

        if (eventCoroutine != null)
            StopCoroutine(eventCoroutine);

        if (gameTimerCoroutine != null)
            StopCoroutine(gameTimerCoroutine);
    }
    private void OnGameWin()
    {
        if (gameEnded) return;
        gameEnded = true;

        Debug.Log("Game Won");

        if (eventCoroutine != null)
            StopCoroutine(eventCoroutine);

        if (gameTimerCoroutine != null)
            StopCoroutine(gameTimerCoroutine);
    }

    private void EventGeneration()
    {
        if (gameEnded) return;
        var eventList = GetEventsLevel();
        int id = eventList[Random.Range(0, eventList.Length)].eventId;
        while (occupiedRegions.Contains(allEvents[id].region))
        {
             id = eventList[Random.Range(0, eventList.Length)].eventId;
        }
        CEventRuntimeData evnt = new()
        {
            eventId = id,
            currentLevel = 1,
            state = 1,
            modulesDone = 0,
            eventLives = allEvents[id].baseLife
        };
        events.Add(evnt);
        occupiedRegions.Add(eventList[id].region);
        
    }
    private IEnumerator GameTimerCoroutine()
    {
        while (remainingGameTime.Value > 0f && !gameEnded)
        {
            yield return null;
            remainingGameTime.Value -= Time.deltaTime;
        }

        if (gameEnded) yield break;

        remainingGameTime.Value = 0f;

        if (successes.Value >= requiredSuccesses)
            OnGameWin();
        else
            OnGameLoss();
    }
    public IEnumerator EventGenerationRepeating()
    {
        yield return new WaitForSeconds(timeBeforeStart);

        while (true)
        {
            EventGeneration();
            yield return new WaitForSeconds(Random.Range(0, randomisationTime) + approximateTimeForEvents);
        }
    }
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void EventLoseLifeServerRpc(int eventId)
    {
        Debug.Log($"Module échoué, l'event {eventId} perd une vie");
        int eventInd = FindIndexFromId(eventId);
        if (eventInd < 0) 
        {
            Debug.Log("tried to lose a life on an unactivated event");
            return; 
        }
        CEventRuntimeData modifiedEvent = events[eventInd];
        modifiedEvent.eventLives--;
        events[eventInd] = modifiedEvent;
        if (modifiedEvent.eventLives == 0)
        {
            OnFailureRpc(eventId);
        }
    }
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void OnFailureRpc(int eventId)
    {
        Debug.Log("EventEchou�");
        if (gameEnded) return;
        health.Value--;
        int eventInd = FindIndexFromId(eventId);
        if (eventInd < 0) Debug.Log(" tried to fail an event not activated yet.");
        CEventRuntimeData modifiedEvent = events[eventId];
        modifiedEvent.state = 3;
        events[eventId] = modifiedEvent;
        occupiedRegions.Remove(allEvents[modifiedEvent.eventId].region);
        bsc.eventOver();
        Invoke("EventGeneration", Random.Range(5f, 10f));
        if (health.Value < 1) 
        {
            OnGameLoss(); 
        }
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void OnSuccessRpc(int eventId)
    {
        Debug.Log("Event Reussi");
        int eventInd = FindIndexFromId(eventId);
        if (eventInd < 0) { Debug.Log(" tried to succeed an event not activated yet."); return; }
        CEventRuntimeData modifiedEvent = events[eventInd];
        modifiedEvent.state = 2;
        events[eventInd] = modifiedEvent;
        occupiedRegions.Remove(allEvents[modifiedEvent.eventId].region);
        if (gameEnded) return;
        successes.Value++;
        bsc.eventOver();
        Invoke("EventGeneration", Random.Range(5f, 10f));
        if (successes.Value >= requiredSuccesses)
        {
            OnGameWin();
        }
    }
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void OnIncrementRpc(int eventId, int moduleId)
    {
        Debug.Log($"Module {moduleId} Reussi, succes ajouté à l'event d'id:{eventId}");
        int eventInd = FindIndexFromId(eventId);
        CatastrophicEvent cEvent = allEvents[eventId];
        CEventRuntimeData cEventData = events[eventInd];
        cEventData.modulesDone++;
        events[eventInd] = cEventData;
        switch (cEventData.currentLevel)
        {
            case 1:
                if (cEventData.modulesDone == cEvent.modules1.Count())
                {
                    if ( cEvent.eventCategoryLevel == 1)
                    {
                        Debug.Log($"Event {eventId} a terminé tout ses modules.");
                         OnSuccessRpc(eventId);
                    }
                    else
                    {
                        Debug.Log($"Event {eventId} a terminé tout ses modules de niveau 1 et monte en niveau.");
                        cEventData.currentLevel++;
                        events[eventInd] = cEventData;
                    }
                }
                    break;
            case 2:
                if (cEventData.modulesDone == cEvent.modules1.Count() + cEvent.modules2.Count()) 
                {
                    if (cEvent.eventCategoryLevel == 2)
                    {
                        Debug.Log($"Event {eventId} a terminé tout ses modules.");
                        OnSuccessRpc(eventId);
                    }
                    else
                    {
                        Debug.Log($"Event {eventId} a terminé tout ses modules de niveau 2 et monte en niveau.");
                        cEventData.currentLevel++;
                        events[eventInd] = cEventData;
                    }
                }
                break;
            case 3:
                if (cEventData.modulesDone == cEvent.modulesCount ) 
                {
                    if (cEvent.eventCategoryLevel == 3)
                    {
                        Debug.Log($"Event {eventId} a terminé tout ses modules.");
                        OnSuccessRpc(eventId);
                    }
                    else
                    {
                        Debug.Log("IMPOSSIBLE, tous les modules ont été effectués, mais l'event n'a jamais été validé.");
                    }
                }
                break;

        }

    }
}
