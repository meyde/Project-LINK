using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class CatastrophicEventStateModule : MonoBehaviour
{
    [Header("Références")]
    [SerializeField] private GameManagerLocal gameManagerLocal;

    [Header("Capteurs (TextMeshPro 3D)")]
    [SerializeField] private TextMeshPro windText;
    [SerializeField] private TextMeshPro temperatureText;
    [SerializeField] private TextMeshPro intensityText;
    [SerializeField] private TextMeshPro oxygenText;

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
        RefreshSensors();
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
        RefreshSensors();
    }

    public void RefreshSensors()
    {
        if (gameManagerLocal == null || gameManagerLocal.gmn == null)
            return;

        if (gameManagerLocal.gmn.eventDataIds.Count == 0)
        {
            ClearTexts();
            return;
        }

        int id = gameManagerLocal.gmn.eventDataIds[0];

        if (id < 0 || id >= gameManagerLocal.allEvents.Length)
            return;

        CatastrophicEvent ev = gameManagerLocal.allEvents[id];
        if (ev == null)
            return;

        windText.text = $"Vent : {ev.windSpeed}";
        temperatureText.text = $"Température : {ev.temperature}";
        intensityText.text = $"Intensité : {ev.intensity}";
        oxygenText.text = $"Oxygène : {ev.oxygenLevel}";
    }

    private void ClearTexts()
    {
        windText.text = "Vent : --";
        temperatureText.text = "Température : --";
        intensityText.text = "Intensité : --";
        oxygenText.text = "Oxygène : --";
    }
}