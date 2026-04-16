using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameManagerLocal : MonoBehaviour
{
    public GameManagerNetwork gmn;
    public CatastrophicEvent[] allEvents;

    [Header("Region / Biome")]
    [SerializeField] private RegionBiomeDatabase regionBiomeDatabase;
    [SerializeField] private BiomeSpriteChanger bsc;

    [Header("Maps")]
    [SerializeField] private MapRegionManager[] mapRegionManagers;

    public int currentRegion;
    public int currentBiome;

    private void Awake()
    {
        gmn = FindFirstObjectByType<GameManagerNetwork>();
        allEvents= gmn.allEvents;

        if (mapRegionManagers == null || mapRegionManagers.Length == 0)
            mapRegionManagers = FindObjectsByType<MapRegionManager>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        if (gmn != null)
            gmn.eventDataIds.OnListChanged += OnEventsChanged;
    }

    private void Start()
    {
        RefreshMapRegions();
    }

    private void OnDisable()
    {
        if (gmn != null)
            gmn.eventDataIds.OnListChanged -= OnEventsChanged;
    }

    public void ChangeRegion(int regionSelected)
    {
        currentRegion = regionSelected;
        int eventFx = -1;
        if (regionBiomeDatabase != null)
        {
            currentBiome = regionBiomeDatabase.GetBiome(currentRegion);
            foreach (int eventInd in gmn.eventDataIds)
            {
                CatastrophicEvent cEvent = allEvents[eventInd];
                if (cEvent.region == currentRegion)
                {
                    eventFx = cEvent.fxType;
                }
            }
            bsc.SetBiome(currentBiome, eventFx);
        }

        Debug.Log($"R�gion: {currentRegion} | Biome: {currentBiome}");
    }

    private void OnEventsChanged(NetworkListEvent<int> change)
    {
        if (change.Type == NetworkListEvent<int>.EventType.Add)
        {
            int id = gmn.eventDataIds[change.Index];
            CatastrophicEvent evnt = allEvents[id];
            OnNewEvent(evnt);
        }

        RefreshMapRegions();
    }

    private void RefreshMapRegions()
    {
        if (gmn == null || allEvents == null || mapRegionManagers == null || mapRegionManagers.Length == 0)
            return;

        Debug.Log("Nombre de MAP trouvées : " + mapRegionManagers.Length);

        List<int> activeRegions = new List<int>();

        for (int i = 0; i < gmn.eventDataIds.Count; i++)
        {
            int eventDataId = gmn.eventDataIds[i];

            CatastrophicEvent currentEvent = allEvents[eventDataId];

            int regionId = currentEvent.region;

            // Évite les doublons si jamais plusieurs events pointent la même région
            if (!activeRegions.Contains(regionId))
                activeRegions.Add(regionId);
        }

        for (int i = 0; i < mapRegionManagers.Length; i++)
        {
            if (mapRegionManagers[i] != null)
                mapRegionManagers[i].UpdateRegions(activeRegions);
        }
    }

    public void RegisterMap(MapRegionManager map)
    {
        if (map == null)
            return;

        List<MapRegionManager> maps = new List<MapRegionManager>();

        if (mapRegionManagers != null)
            maps.AddRange(mapRegionManagers);

        if (!maps.Contains(map))
            maps.Add(map);

        mapRegionManagers = maps.ToArray();

        RefreshMapRegions();
    }

    public void UnregisterMap(MapRegionManager map)
    {
        if (map == null || mapRegionManagers == null)
            return;

        List<MapRegionManager> maps = new List<MapRegionManager>(mapRegionManagers);
        maps.Remove(map);
        mapRegionManagers = maps.ToArray();
    }

    public void OnNewEvent(CatastrophicEvent cEvent)
    {
        StartCoroutine(CountDown(cEvent));
    }

    public IEnumerator CountDown(CatastrophicEvent cEvent)
    {
        yield return new WaitForSeconds(cEvent.eventDuration);
        Debug.Log("Event timer out");
        int pos = -1;

        for (int i = 0; i < gmn.eventDataIds.Count; i++)
        {
            if (gmn.eventDataIds[i] == cEvent.eventId)
                pos = i;
        }

        if (pos == -1)
        {
            Debug.Log("Event Not occuring");
            yield break;
        }


        if (gmn.eventStates[pos] != 2)
            Debug.Log("event failed");
            gmn.OnFailureRpc(pos);
    }

    public void EndModuleCheck(int moduleId, bool state, int eventPos)
    {
        if (eventPos > -1)
        {
            if (state && currentRegion == gmn.eventRegions[eventPos])

            {
                Debug.Log("GoodModule");
                gmn.OnIncrementRpc(eventPos, moduleId);
            }
            else
            {
                if (currentRegion != gmn.eventRegions[eventPos])
                {
                    Debug.Log("WrongRegion");
                }
                if (!state)
                {
                    Debug.Log("WrongModulestate");
                }

                gmn.EventLoseLifeServerRpc(eventPos);
            }
        }
        else
        {
            Debug.Log("no event found");
            gmn.EventLoseLifeServerRpc(eventPos);
        }
    }
}