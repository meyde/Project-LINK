using Unity.Netcode;
using UnityEngine;

public class MapRegionManager : MonoBehaviour
{
    private PingRegion[] regions;
    private GameManagerNetwork gameManager;
    private bool isSubscribed = false;

    private void Awake()
    {
        regions = GetComponentsInChildren<PingRegion>(true);
    }

    private void Start()
    {
        TryBindToGameManager();
        RefreshRegions();
    }

    private void OnEnable()
    {
        TryBindToGameManager();
        RefreshRegions();
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    private void OnDestroy()
    {
        Unsubscribe();
    }

    private void TryBindToGameManager()
    {
        if (isSubscribed)
            return;

        gameManager = GameManagerNetwork.Instance;

        if (gameManager == null)
            return;

        // Sécurise le tableau si besoin
        if (regions == null || regions.Length == 0)
            regions = GetComponentsInChildren<PingRegion>(true);

        // Si la NetworkList n'est pas encore créée/spawnée, on attendra le prochain passage
        if (gameManager.occupiedRegions == null)
            return;

        gameManager.occupiedRegions.OnListChanged += OnOccupiedRegionsChanged;
        isSubscribed = true;
    }

    private void Unsubscribe()
    {
        if (!isSubscribed || gameManager == null || gameManager.occupiedRegions == null)
            return;

        gameManager.occupiedRegions.OnListChanged -= OnOccupiedRegionsChanged;
        isSubscribed = false;
    }

    private void OnOccupiedRegionsChanged(NetworkListEvent<int> changeEvent)
    {
        RefreshRegions();
    }

    public void RefreshRegions()
    {
        if (regions == null || regions.Length == 0)
            regions = GetComponentsInChildren<PingRegion>(true);

        if (gameManager == null)
            gameManager = GameManagerNetwork.Instance;

        if (gameManager == null || gameManager.occupiedRegions == null)
        {
            SetAllRegionsInactive();
            return;
        }

        for (int i = 0; i < regions.Length; i++)
        {
            if (regions[i] == null)
                continue;

            bool shouldBeActive = gameManager.occupiedRegions.Contains(regions[i].regionId);
            regions[i].SetRegionActive(shouldBeActive);
        }
    }

    private void SetAllRegionsInactive()
    {
        if (regions == null)
            return;

        for (int i = 0; i < regions.Length; i++)
        {
            if (regions[i] == null)
                continue;

            regions[i].SetRegionActive(false);
        }
    }
}