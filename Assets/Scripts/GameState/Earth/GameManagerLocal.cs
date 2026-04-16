using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class GameManagerLocal : MonoBehaviour
{
    public GameManagerNetwork gmn;
    public CatastrophicEvent[] allEvents;
    [Header("Region / Biome")]
    [SerializeField] private RegionBiomeDatabase regionBiomeDatabase;
    [SerializeField] private BiomeSpriteChanger bsc;
    public int currentRegion;
    public int currentBiome;
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

    public void ChangeRegion(int regionSelected)
    {
        currentRegion = regionSelected;

        if (regionBiomeDatabase != null)
        {
            currentBiome = regionBiomeDatabase.GetBiome(currentRegion);
            bsc.SetBiome(currentBiome);
        }

        Debug.Log($"Région: {currentRegion} | Biome: {currentBiome}");
    }

    private void OnEventsChanged(NetworkListEvent<int> change)
    {
        if (change.Type == NetworkListEvent<int>.EventType.Add)
        {
            int id = gmn.eventDataIds[change.Index];

            CatastrophicEvent evnt = allEvents[id];


            OnNewEvent(evnt);
        }
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
            gmn.OnFailureRpc(pos);
        }
        
    }
    public void EndModuleCheck ( int moduleId, bool state, int eventPos)
    {
        if (eventPos > -1)
        {
            if (state)
            {
                gmn.OnIncrementRpc(eventPos, moduleId);
            }
            else
            {
                gmn.EventLoseLifeServerRpc(eventPos);
            }
        }
        else
        {
            gmn.EventLoseLifeServerRpc(eventPos);
        }
    }

}
