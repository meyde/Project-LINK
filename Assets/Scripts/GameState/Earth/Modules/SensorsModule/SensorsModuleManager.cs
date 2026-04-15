using System.Collections;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class CatastrophicEventStateModule : Module
{
    [Header("R�f�rences")]
    [SerializeField] private GameManagerLocal gameManagerLocal;

    [Header("Capteurs (TextMeshPro 3D)")]
    [SerializeField] private TextMeshPro windText;
    [SerializeField] private TextMeshPro temperatureText;
    [SerializeField] private TextMeshPro intensityText;
    [SerializeField] private TextMeshPro oxygenText;

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
            gameManagerLocal.gmn.eventDataIds.OnListChanged += OnEventListChanged;
        }
    }

    private void TryUnsubscribe()
    {
        if (gameManagerLocal != null && gameManagerLocal.gmn != null)
        {
            gameManagerLocal.gmn.eventDataIds.OnListChanged -= OnEventListChanged;
        }
    }

    private void OnEventListChanged(NetworkListEvent<int> changeEvent)
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
            ShowNoSignal();
            return;
        }

        int windValue = GetRandomSensorValue(regionEvent.windSpeed, regionEvent.windSpeedMax);
        int temperatureValue = GetRandomSensorValue(regionEvent.temperature, regionEvent.temperatureMax);
        int intensityValue = GetRandomSensorValue(regionEvent.intensity, regionEvent.intensityMax);
        int oxygenValue = GetRandomSensorValue(regionEvent.oxygenLevel, regionEvent.oxygenLevelMax);

        windText.text = $"Vent : {windValue}";
        temperatureText.text = $"Temp�rature : {temperatureValue}";
        intensityText.text = $"Intensit� : {intensityValue}";
        oxygenText.text = $"Oxyg�ne : {oxygenValue}";
    }

    private CatastrophicEvent GetActiveEventForCurrentRegion()
    {
        if (gameManagerLocal.gmn.eventDataIds == null || gameManagerLocal.gmn.eventDataIds.Count == 0)
            return null;

        int currentRegion = gameManagerLocal.currentRegion;

        for (int i = 0; i < gameManagerLocal.gmn.eventDataIds.Count; i++)
        {
            int eventIndex = gameManagerLocal.gmn.eventDataIds[i];

            if (eventIndex < 0 || eventIndex >= gameManagerLocal.allEvents.Length)
                continue;

            CatastrophicEvent ev = gameManagerLocal.allEvents[eventIndex];

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
        windText.text = "Vent : --";
        temperatureText.text = "Temp�rature : --";
        intensityText.text = "Intensit� : --";
        oxygenText.text = "Oxyg�ne : --";
    }

    private void ClearTexts()
    {
        windText.text = "Vent : --";
        temperatureText.text = "Temp�rature : --";
        intensityText.text = "Intensit� : --";
        oxygenText.text = "Oxyg�ne : --";
    }
}