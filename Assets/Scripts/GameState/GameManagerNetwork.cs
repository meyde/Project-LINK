using System;
using System.Collections;
using System.Linq;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

public class GameManagerNetwork : NetworkBehaviour
{
    public static GameManagerNetwork Instance { get; private set; }

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
    public NetworkList<int> eventDataIds = new();
    public NetworkList<int> eventLives = new();
    public NetworkList<int> eventModulesDone = new();
    public NetworkList<int> eventCurrentLevel = new();
    public NetworkList<int> eventStates = new();
    public NetworkList<int> eventRegions = new();
    //public NetworkList<FixedList64Bytes<int>> eventModule1Option = new();
    //public NetworkList<FixedList64Bytes<int>> eventModule2Option = new();
    //public NetworkList<FixedList64Bytes<int>> eventModule3Option = new();

    public NetworkVariable<int> health = new(3);
    public NetworkVariable<int> successes = new(0);

    public NetworkList<int> pictoSpritesCurrent = new();
    [SerializeField] private Sprite[] pictoIcons;
    [SerializeField] private Color[] pictoColors;
    [SerializeField] private Sprite[] pictoNmbers;

    public int RequiredSuccesses => requiredSuccesses;

    [SerializeField] private float gameDuration = 300f; // dur�e totale de la partie en secondes
    public NetworkVariable<float> remainingGameTime = new(0f);

    private Coroutine gameTimerCoroutine;
    private bool gameEnded;

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
        if (!IsServer || gameStarted) return;

        gameStarted = true;
        gameEnded = false;
        successes.Value = 0;
        health.Value = 3;
        remainingGameTime.Value = gameDuration;

        eventCoroutine = StartCoroutine(EventGenerationRepeating());
        gameTimerCoroutine = StartCoroutine(GameTimerCoroutine());
    }

    public void Start()
    {
        if (Instance == null)
            Instance = this;

        OnStartGame();
    }
    public override void OnNetworkSpawn()
    {
    }

    public override void OnNetworkDespawn()
    {
        
    }

    private void Update()
    {
        
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
        while (eventRegions.Contains(allEvents[id].region))
        {
             id = eventList[Random.Range(0, eventList.Length)].eventId;
        }
        eventDataIds.Add(id);
        eventLives.Add(eventList[id].baseLife);
        eventModulesDone.Add(0);
        eventCurrentLevel.Add(1);
        eventStates.Add(0);
        FixedList64Bytes<int> module1Options = new();
        FixedList64Bytes<int> module2Options = new();
        FixedList64Bytes<int> module3Options = new();
        for (int i = 0; i < eventList[id].modules1.Count() ;i++ ) { module1Options.Add(0); }
        for (int i = 0; i < eventList[id].modules2.Count(); i++) { module2Options.Add(0); }
        for (int i = 0; i < eventList[id].modules3.Count(); i++) { module3Options.Add(0); }
        //eventModule1Option.Add(module1Options);
        //eventModule2Option.Add(module2Options);
        //eventModule3Option.Add(module3Options);
        eventRegions.Add(eventList[id].region);
        
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
    public void EventLoseLifeServerRpc(int eventIndex)
    {
        Debug.Log("Module 2chou�");
        if (eventDataIds.Count == 0)
        {
            return;
        }
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
        Debug.Log("EventEchou�");
        if (gameEnded) return;
        health.Value--;
        eventDataIds.RemoveAt(eventId);
        eventLives.RemoveAt(eventId);
        eventModulesDone.RemoveAt(eventId);
        //eventModule1Option.RemoveAt(eventId);
        //eventModule2Option.RemoveAt(eventId);
        //eventModule3Option.RemoveAt(eventId);
        eventStates.RemoveAt(eventId);
        eventCurrentLevel.RemoveAt(eventId);
        eventRegions.RemoveAt(eventId);
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
        if (gameEnded) return;
        successes.Value++;
        eventDataIds.RemoveAt(eventId);
        eventLives.RemoveAt(eventId);
        eventModulesDone.RemoveAt(eventId);
        //eventModule1Option.RemoveAt(eventId);
        //eventModule2Option.RemoveAt(eventId);
        //eventModule3Option.RemoveAt(eventId);
        eventStates.RemoveAt(eventId);
        eventCurrentLevel.RemoveAt(eventId);
        eventRegions.RemoveAt(eventId);
        Invoke("EventGeneration", Random.Range(5f, 10f));
        if (successes.Value >= requiredSuccesses)
        {
            OnGameWin();
        }
    }
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void OnIncrementRpc(int eventInd, int moduleId)
    {
        Debug.Log("Module Reussi");
        Debug.Log(eventInd.ToString());
        Debug.Log(moduleId.ToString());
        eventModulesDone[eventInd]++;
        Debug.Log("eventModulesDone atteind");
        CatastrophicEvent cEvent = allEvents[eventDataIds[eventInd]];
        Debug.Log("allEvents[eventDataIds[eventId] atteind");
        switch (eventCurrentLevel[eventInd])
        {
            case 1:
                //for (int i = 0; i < cEvent.modules1.Count(); i++)
                //{
                //    if (cEvent.modules1[i] == moduleId)
                //    {
                //        var curList = eventModule1Option[eventInd];
                //        curList[i] = Random.Range(0, 3);
                //        eventModule1Option[eventInd] = curList;
                //    }
                //}
                if (eventModulesDone[eventInd] == cEvent.modules1.Count())
                {
                    if ( cEvent.eventCategoryLevel == 1)
                    {
                        Debug.Log("SuccessRPC essayé");
                         OnSuccessRpc(eventInd);
                    }
                    else
                    {
                        eventCurrentLevel[eventInd]++;
                    }
                }
                    break;
            case 2:
                //for (int i = 0; i < cEvent.modules2.Count(); i++)
                //{
                //    if (cEvent.modules2[i] == moduleId)
                //    {
                //        var curList = eventModule2Option[eventInd];
                //        curList[i] = Random.Range(0, 3);
                //        eventModule1Option[eventInd] = curList;
                //    }
                //}
                if (eventModulesDone[eventInd] == cEvent.modules1.Count() + cEvent.modules2.Count()) 
                {
                    if (cEvent.eventCategoryLevel == 2)
                    {
                        Debug.Log("trying successRPC");
                        OnSuccessRpc(eventInd);
                    }
                    else
                    {
                        eventCurrentLevel[eventInd]++;
                    }
                }
                break;
            case 3:
                //for (int i = 0; i < cEvent.modules3.Count(); i++)
                //{
                //    if (cEvent.modules2[i] == moduleId)
                //    {
                //        var curList = eventModule3Option[eventInd];
                //        curList[i] = Random.Range(0, 3);
                //        eventModule1Option[eventInd] = curList;
                //    }
                //}
                if (eventModulesDone[eventInd] == cEvent.modulesCount ) 
                {
                    if (cEvent.eventCategoryLevel == 3)
                    {
                        OnSuccessRpc(eventInd);
                    }
                    else
                    {
                        Debug.Log("Not supposed to happen");
                        eventCurrentLevel[eventInd]++;
                    }
                }
                break;

        }

    }
}
