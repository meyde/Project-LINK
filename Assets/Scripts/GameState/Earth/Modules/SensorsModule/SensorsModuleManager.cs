using System.Collections;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class SensorsModuleManager : Module
{
    [Header("Références")]
    [SerializeField] private GameManagerLocal gameManagerLocal;

    [Header("Capteurs (TextMeshPro 3D)")]
    [SerializeField] private int captorId;
    [SerializeField] private TextMeshPro captorText;

    [Header("Affichage")]
    [SerializeField] private float displayDuration = 5f;
    [SerializeField] private bool clearOnStart = true;

    private Coroutine revealCoroutine;

    private void Awake()
    {
        if (gameManagerLocal == null)
        {
            gameManagerLocal = FindFirstObjectByType<GameManagerLocal>();
        }
    }

    private void Start()
    {
        TrySubscribe();

        if (clearOnStart)
            ClearTexts();
    }

    private void OnDestroy()
    {
        TryUnsubscribe();
    }

    private void TrySubscribe()
    {
        if (gameManagerLocal != null && gameManagerLocal.gmn != null)
        {
            gameManagerLocal.gmn.events.OnListChanged += OnEventListChanged;
        }
    }

    private void TryUnsubscribe()
    {
        if (gameManagerLocal != null && gameManagerLocal.gmn != null)
        {
            gameManagerLocal.gmn.events.OnListChanged -= OnEventListChanged;
        }
    }

    private void OnEventListChanged(NetworkListEvent<CEventRuntimeData> changeEvent)
    {
        ClearTexts();
    }

    public void RevealSensorsTemporarily()
    {
        if (revealCoroutine != null)
        {
            StopCoroutine(revealCoroutine);
        }

        revealCoroutine = StartCoroutine(RevealSensorsRoutine());
    }

    private IEnumerator RevealSensorsRoutine()
    {
        ShowRandomizedSensorsForCurrentRegion();

        yield return new WaitForSeconds(displayDuration);

        ClearTexts();
        revealCoroutine = null;
    }

    private void ShowRandomizedSensorsForCurrentRegion()
    {
        if (gameManagerLocal == null || gameManagerLocal.gmn == null)
        {
            ClearTexts();
            return;
        }

        CatastrophicEvent regionEvent = GetActiveEventForCurrentRegion();

        if (regionEvent == null)
        {
            Debug.Log("no event found in region");
            ShowNoSignal();
            return;
        }

        switch (captorId)
        {
            case 0:
                int windValue = GetRandomSensorValue(regionEvent.windSpeed, regionEvent.windSpeedMax);
                captorText.text = $"Vent : {windValue}";
                break;
            case 1:
                int temperatureValue = GetRandomSensorValue(regionEvent.temperature, regionEvent.temperatureMax);
                captorText.text = $"Température : {temperatureValue}";
                break;
            case 2:
                int intensityValue = GetRandomSensorValue(regionEvent.intensity, regionEvent.intensityMax);
                captorText.text = $"Intensité : {intensityValue}";
                break;
            case 3:
                int oxygenValue = GetRandomSensorValue(regionEvent.oxygenLevel, regionEvent.oxygenLevelMax);
                captorText.text = $"Oxygène : {oxygenValue}";
                break;

        }
        
        
        

        
        
        
        
    }

    private CatastrophicEvent GetActiveEventForCurrentRegion()
    {
        if (gameManagerLocal.gmn.events == null || gameManagerLocal.gmn.events.Count == 0)
            return null;

        int currentRegion = gameManagerLocal.currentRegion;

        foreach (CEventRuntimeData cEventData in gameManagerLocal.gmn.events)
        {
            int eventId = cEventData.eventId;
            CatastrophicEvent ev = gameManagerLocal.gmn.allEvents[eventId];
            Debug.Log($"Event{ev.eventId} is occuring in region: {ev.region}");
            if (ev == null)
                continue;

            if (ev.region == currentRegion)
                return ev;
        }

        return null;
    }

    private int GetRandomSensorValue(int baseValue, int variation)
    {
        int min = Mathf.Min(baseValue, variation);
        int max = Mathf.Max(baseValue, variation);

        return Random.Range(min, max + 1);
    }

    private void ShowNoSignal()
    {
        captorText.text = "------";
    }

    private void ClearTexts()
    {
        captorText.text = "------";
    }
}