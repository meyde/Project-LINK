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

    [Header("Module Feedback Audio")]
    [SerializeField] private AudioSource feedbackAudioSource;
    [SerializeField] private AudioClip moduleSuccessClip;
    [SerializeField] private AudioClip moduleFailClip;
    [SerializeField][Range(0f, 1f)] private float successVolume = 1f;
    [SerializeField][Range(0f, 1f)] private float failVolume = 1f;

    [Header("Event FX Audio")]
    [SerializeField] private AudioSource fxAudioSource;
    private int currentLoopingFxType = -1;
    private int currentLoopingEventId = -1;

    [System.Serializable]
    public struct FxSound
    {
        public int fxType;
        public AudioClip clip;
    }

    [SerializeField] private FxSound[] fxSounds;

    private void Awake()
    {
        gmn = FindFirstObjectByType<GameManagerNetwork>();

        if (gmn == null)
        {
            Debug.LogError("[GameManagerLocal] GameManagerNetwork introuvable.");
            return;
        }

        allEvents = gmn.allEvents;

        if (mapRegionManagers == null || mapRegionManagers.Length == 0)
            mapRegionManagers = FindObjectsByType<MapRegionManager>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        if (gmn != null)
            gmn.events.OnListChanged += OnEventsChanged;

        // Si aucune AudioSource n'est assignée, on essaie de récupérer celle du GameObject
        if (feedbackAudioSource == null)
            feedbackAudioSource = GetComponent<AudioSource>();
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

    private void OnEventsChanged(NetworkListEvent<CEventRuntimeData> change)
    {
        Debug.Log("Event list changed");

        if (change.Type == NetworkListEvent<CEventRuntimeData>.EventType.Add)
        {
            CEventRuntimeData cEventData = change.Value;
            CatastrophicEvent evnt = allEvents[cEventData.eventId];
            OnNewEvent(evnt);
        }

        RefreshCurrentRegionVisualAndAudio();
        RefreshMapRegions();
    }

    public void ChangeRegion(int regionSelected)
    {
        currentRegion = regionSelected;

        if (regionBiomeDatabase != null)
            currentBiome = regionBiomeDatabase.GetBiome(currentRegion);

        RefreshCurrentRegionVisualAndAudio();

        Debug.Log($"Région: {currentRegion} | Biome: {currentBiome}");
    }

    private AudioClip GetFxClip(int fxType)
    {
        foreach (var fx in fxSounds)
        {
            if (fx.fxType == fxType)
                return fx.clip;
        }
        return null;
    }
    private void RefreshCurrentRegionVisualAndAudio()
    {
        int eventFx = -1;
        CEventRuntimeData? activeEventData = null;

        foreach (CEventRuntimeData cEventData in gmn.events)
        {
            if (cEventData.state != 1)
                continue;

            CatastrophicEvent cEvent = allEvents[cEventData.eventId];

            if (cEvent.region == currentRegion)
            {
                eventFx = cEvent.fxType;
                activeEventData = cEventData;
                break;
            }
        }

        if (bsc != null)
            bsc.SetBiome(currentBiome, eventFx);

        if (activeEventData.HasValue)
        {
            CatastrophicEvent activeEvent = allEvents[activeEventData.Value.eventId];
            PlayOrUpdateLoopingEventFx(activeEvent.eventId, activeEvent.fxType);
        }
        else
        {
            StopEventFxSound();
        }
    }

    private void RefreshMapRegions()
    {
        if (mapRegionManagers == null || mapRegionManagers.Length == 0)
            return;

        Debug.Log("Nombre de MAP trouvées : " + mapRegionManagers.Length);
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
        RefreshCurrentRegionVisualAndAudio();
    }

    private void PlayOrUpdateLoopingEventFx(int eventId, int fxType)
    {
        if (fxAudioSource == null)
            return;

        AudioClip clip = GetFxClip(fxType);

        if (clip == null)
        {
            Debug.LogWarning($"Aucun son trouvé pour fxType {fxType}");
            StopEventFxSound();
            return;
        }

        bool sameEvent = currentLoopingEventId == eventId;
        bool sameFx = currentLoopingFxType == fxType;
        bool sameClip = fxAudioSource.clip == clip;

        if (fxAudioSource.isPlaying && sameEvent && sameFx && sameClip)
            return;

        fxAudioSource.Stop();
        fxAudioSource.clip = clip;
        fxAudioSource.loop = true;
        fxAudioSource.Play();

        currentLoopingEventId = eventId;
        currentLoopingFxType = fxType;
    }
    private void StopEventFxSound()
    {
        if (fxAudioSource == null)
            return;

        if (fxAudioSource.isPlaying)
            fxAudioSource.Stop();

        fxAudioSource.clip = null;
        fxAudioSource.loop = false;
        currentLoopingFxType = -1;
        currentLoopingEventId = -1;
    }

    public IEnumerator CountDown(CatastrophicEvent cEvent)
    {
        yield return new WaitForSeconds(cEvent.eventDuration);
        Debug.Log("Event timer out");

        int eventInd = gmn.FindIndexFromId(cEvent.eventId);
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

    private void PlayModuleSuccessSound()
    {
        if (feedbackAudioSource == null || moduleSuccessClip == null)
            return;

        feedbackAudioSource.PlayOneShot(moduleSuccessClip, successVolume);
    }

    private void PlayModuleFailSound()
    {
        if (feedbackAudioSource == null || moduleFailClip == null)
            return;

        feedbackAudioSource.PlayOneShot(moduleFailClip, failVolume);
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
                PlayModuleFailSound();
                return;
            }
            else
            {
                Debug.Log("Found. It loses a life.");
                PlayModuleFailSound();
                gmn.EventLoseLifeServerRpc(eventId);
                return;
            }
        }

        CatastrophicEvent cEvent = allEvents[eventId];
        CEventRuntimeData cEventData = gmn.events[eventInd];

        if (!state)
        {
            Debug.Log($"wrong solution from module. Losing a life on the event:{eventId}");
            PlayModuleFailSound();
            gmn.EventLoseLifeServerRpc(eventId);
        }
        else
        {
            if (currentRegion != cEvent.region)
            {
                Debug.Log($"Right solution from module for event {eventId} but wrong region for it.");
                PlayModuleFailSound();
                gmn.EventLoseLifeServerRpc(eventId);
            }
            else
            {
                Debug.Log($"Module {moduleId} succeeded for event {eventId}. incrementing the event.");
                PlayModuleSuccessSound();
                gmn.OnIncrementRpc(eventId, moduleId);
            }
        }
    }
}