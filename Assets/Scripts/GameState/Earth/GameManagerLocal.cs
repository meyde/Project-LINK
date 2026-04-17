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
            gmn.events.OnListChanged += OnEventsChanged;
    }
    private void OnEventsChanged(NetworkListEvent<CEventRuntimeData> change)
    {
        Debug.Log("New event Detected");
        if (change.Type == NetworkListEvent<CEventRuntimeData>.EventType.Add)
        {
            CEventRuntimeData cEventData = change.Value; 
            CatastrophicEvent evnt = allEvents[cEventData.eventId];
            OnNewEvent(evnt);
        }
        RefreshMapRegions();
    }
    private void Start()
    {
        RefreshMapRegions();
    }

    private void OnDisable()
    {
        if (gmn != null)
            gmn.events.OnListChanged -= OnEventsChanged;
    }

    public void ChangeRegion(int regionSelected)
    {
        currentRegion = regionSelected;
        int eventFx = -1;
        if (regionBiomeDatabase != null)
        {
            currentBiome = regionBiomeDatabase.GetBiome(currentRegion);
            foreach (CEventRuntimeData cEventData in gmn.events)
            {
                if (cEventData.state != 1) { continue; }
                CatastrophicEvent cEvent = allEvents[cEventData.eventId];
                if (cEvent.region == currentRegion)
                {
                    eventFx = cEvent.fxType;
                }
            }
            bsc.SetBiome(currentBiome, eventFx);
        }

        Debug.Log($"R�gion: {currentRegion} | Biome: {currentBiome}");
    }

    

    private void RefreshMapRegions()
    {
        if (gmn == null || allEvents == null || mapRegionManagers == null || mapRegionManagers.Length == 0)
            return;

        Debug.Log("Nombre de MAP trouvées : " + mapRegionManagers.Length);

        List<int> activeRegions = new List<int>();

        foreach (int regionId in gmn.occupiedRegions)
        {
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
        int eventInd =gmn.FindIndexFromId(cEvent.eventId);
        if (eventInd == -1)
        {
            Debug.Log("Event Never Occured");
            yield break;
        }
        CEventRuntimeData cEventData = gmn.events[eventInd];


        if (cEventData.state == 2)
        {
            Debug.Log("event succeeded");
            yield break;
        }
        if (cEventData.state == 3)
        {
            Debug.Log("Event Previously failed");
            yield break;
        }
        if (cEventData.state == 1)
        {
            Debug.Log("Failing event");
            gmn.OnFailureRpc(cEvent.eventId);
        }
    }

    public void EndModuleCheck(int moduleId, bool state, int eventId)
    {
        int eventInd = gmn.FindIndexFromId(eventId);
        if (eventInd == -1)
        {
            Debug.Log("the module tried to fail or succeed in an event not occuring, checking if there is any event occuring.");
            eventId = gmn.FindIdOfFirstActivated();
            eventInd = gmn.FindIndexFromId(eventId);
            if (eventInd == -1)
            {
                Debug.Log("none found, nothing happens");
                return;
            }
            else
            {
                Debug.Log("Found. It loses a life.");
                gmn.EventLoseLifeServerRpc(eventId);
            }

        }
        CatastrophicEvent cEvent = allEvents[eventId];
        CEventRuntimeData cEventData = gmn.events[eventInd];
        if (!state)
        {
            Debug.Log($"wrong solution from module. Losing a life on the event:{eventId}");
            gmn.EventLoseLifeServerRpc(eventId);
        }
        else
        {
            if (currentRegion != cEvent.region)
            {
                Debug.Log($"Right solution from module for event {eventId} but wrong region for it.");
                gmn.EventLoseLifeServerRpc(eventId);
            }
            else
            {
                Debug.Log($"Module {moduleId} succeeded for event {eventId}. incrementing the event.");
                gmn.OnIncrementRpc(eventId,moduleId);
            }
        }
    }
}