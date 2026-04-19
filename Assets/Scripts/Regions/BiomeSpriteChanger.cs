using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class BiomeSpriteChanger : MonoBehaviour
{
    [Header("Renderer à modifier")]
    [SerializeField] private SpriteRenderer targetRenderer;

    [Header("Sprites par biome (index = biomeId)")]
    [SerializeField] private Sprite[] biomeSprites;
    [SerializeField] private GameObject[] FxObj;

    [Header("Biome actuel")]
    [SerializeField] private int currentBiomeId;

    [Header("Thunder Strikes")]
    public SpriteRenderer[] thunderStrikes;
    [SerializeField] private float timeBetweenStrikes = 10f;
    [SerializeField] private float strikesDuration = 0.5f;
    [SerializeField] private float doubleStrikesIntervals = 2f;

    [Header("Thunder Audio")]
    [SerializeField] private AudioSource thunderAudioSource;
    [SerializeField] private AudioClip thunderClip;
    [SerializeField][Range(0f, 1f)] private float thunderVolume = 1f;

    [Header("Materials d'événement")]
    [SerializeField] private Material defaultMaterial;
    [SerializeField] private Material heatMapMaterial;

    [Header("Event IDs Canicule")]
    [SerializeField] private int[] heatwaveEventIds = { 27, 28, 29 };

    [Header("Earthquake (tremblement)")]
    [SerializeField] private int[] shakeEventIds = { 24, 25, 26 };
    [SerializeField] private float shakeIntensity = 0.1f;
    [SerializeField] private float shakeSpeed = 25f;

    private Coroutine shakeCoroutine;
    private Vector3 originalPosition;

    private bool blockEventOver;
    private Coroutine thunder;

    private void Awake()
    {
        if (targetRenderer == null)
            targetRenderer = GetComponent<SpriteRenderer>();

        if (targetRenderer != null && defaultMaterial == null)
            defaultMaterial = targetRenderer.sharedMaterial;
    }

    private void Start()
    {
        ApplyBiome(currentBiomeId);
        ApplyMaterialFromEventId(-1);
    }

    public void SetBiome(int biomeId, int eventFx, int eventId = -1)
    {
        if (thunder != null)
            StopCoroutine(thunder);

        currentBiomeId = biomeId;
        ApplyBiome(currentBiomeId);
        ApplyMaterialFromEventId(eventId);

        // Gestion tremblement
        if (IsShakeEvent(eventId))
        {
            if (shakeCoroutine == null)
            {
                Debug.Log($"Event : Tremblement de terre (ID {eventId})");
                shakeCoroutine = StartCoroutine(Shake());
            }
        }
        else
        {
            StopShake();
        }

        if (!(blockEventOver && eventFx == -1))
        {
            foreach (GameObject go in FxObj)
                go.SetActive(false);
        }

        switch (eventFx)
        {
            case 0:
                FxObj[0].SetActive(true);
                Debug.Log("Event : Tempête jaune (simple)");
                StormManager(0, false);
                break;

            case 1:
                FxObj[0].SetActive(true);
                Debug.Log("Event : Tempête bleue (simple)");
                StormManager(1, false);
                break;

            case 2:
                FxObj[0].SetActive(true);
                Debug.Log("Event : Tempête verte (simple)");
                StormManager(2, false);
                break;

            case 3:
                FxObj[0].SetActive(true);
                Debug.Log("Event : Tempête jaune (double)");
                StormManager(0, true);
                break;

            case 4:
                FxObj[0].SetActive(true);
                Debug.Log("Event : Tempête bleue (double)");
                StormManager(1, true);
                break;

            case 5:
                FxObj[0].SetActive(true);
                Debug.Log("Event : Tempête verte (double)");
                StormManager(2, true);
                break;

            case 6:
                FxObj[1].SetActive(true);
                Debug.Log("Event : Tornade (sans débris)");
                break;

            case 7:
                FxObj[2].SetActive(true);
                Debug.Log("Event : Tornade (avec débris)");
                break;

            case 8:
                FxObj[3].SetActive(true);
                Debug.Log("Event : Blizzard (neige)");
                break;

            case 9:
                FxObj[4].SetActive(true);
                Debug.Log("Event : Blizzard (glace)");
                break;

            case 10:
                FxObj[5].SetActive(true);
                Debug.Log("Event : Blizzard (solide)");
                break;

            case 11:
                FxObj[6].SetActive(true);
                Debug.Log("Event : Incendie de forêt");
                break;
        }
    }

    private void ApplyMaterialFromEventId(int eventId)
    {
        if (targetRenderer == null)
            return;

        bool isHeatwave = IsHeatwaveEvent(eventId);

        if (isHeatwave)
        {
            if (heatMapMaterial != null)
            {
                targetRenderer.material = heatMapMaterial;
                Debug.Log($"Event : Canicule (ID {eventId}), HeatMap activé");
            }
        }
        else
        {
            if (defaultMaterial != null)
                targetRenderer.material = defaultMaterial;
            else
                targetRenderer.material = null;
        }
    }

    private bool IsHeatwaveEvent(int eventId)
    {
        for (int i = 0; i < heatwaveEventIds.Length; i++)
        {
            if (heatwaveEventIds[i] == eventId)
                return true;
        }

        return false;
    }

    private bool IsShakeEvent(int eventId)
    {
        for (int i = 0; i < shakeEventIds.Length; i++)
        {
            if (shakeEventIds[i] == eventId)
                return true;
        }

        return false;
    }

    private IEnumerator Shake()
    {
        originalPosition = transform.localPosition;

        while (true)
        {
            float offsetX = Random.Range(-1f, 1f) * shakeIntensity;
            float offsetY = Random.Range(-1f, 1f) * shakeIntensity;

            transform.localPosition = originalPosition + new Vector3(offsetX, offsetY, 0f);

            yield return new WaitForSeconds(1f / shakeSpeed);
        }
    }

    private void StopShake()
    {
        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
            shakeCoroutine = null;
            transform.localPosition = originalPosition;
        }
    }

    public void StormManager(int color, bool isDoubled)
    {
        foreach (SpriteRenderer sr in thunderStrikes)
            sr.gameObject.SetActive(false);

        SpriteRenderer[] thunderStrikesSr = new SpriteRenderer[3];

        for (int i = 0; i < 3; i++)
            thunderStrikesSr[i] = thunderStrikes[3 * color + i];

        thunder = StartCoroutine(CyclingThunder(thunderStrikesSr, isDoubled));
    }

    public IEnumerator CyclingThunder(SpriteRenderer[] coloredStrikes, bool isDoubled)
    {
        while (true)
        {
            int choice = Random.Range(0, 3);

            coloredStrikes[choice].gameObject.SetActive(true);
            StartCoroutine(PlayThunderSoundDelayed(Random.Range(0.1f, 0.6f)));

            yield return new WaitForSeconds(strikesDuration);

            coloredStrikes[choice].gameObject.SetActive(false);

            if (isDoubled)
            {
                yield return new WaitForSeconds(doubleStrikesIntervals);

                choice = Random.Range(0, 3);
                coloredStrikes[choice].gameObject.SetActive(true);
                StartCoroutine(PlayThunderSoundDelayed(Random.Range(0.1f, 0.6f)));

                yield return new WaitForSeconds(strikesDuration);

                coloredStrikes[choice].gameObject.SetActive(false);
            }

            yield return new WaitForSeconds(timeBetweenStrikes);
        }
    }

    private void PlayThunderSound()
    {
        if (thunderAudioSource == null || thunderClip == null)
            return;

        thunderAudioSource.PlayOneShot(thunderClip, thunderVolume);
    }

    private IEnumerator PlayThunderSoundDelayed(float delay)
    {
        yield return new WaitForSeconds(delay);
        PlayThunderSound();
    }

    public void BlockEventOver()
    {
        blockEventOver = true;
    }

    public void AllowEventOver()
    {
        blockEventOver = false;
    }

    public void eventOver()
    {
        if (blockEventOver)
            return;

        Debug.Log("Event terminé, nettoyage des FX");

        foreach (GameObject go in FxObj)
            go.SetActive(false);

        if (thunder != null)
        {
            StopCoroutine(thunder);
            thunder = null;
        }

        foreach (SpriteRenderer sr in thunderStrikes)
            sr.gameObject.SetActive(false);

        StopShake();

        ApplyMaterialFromEventId(-1);
    }

    private void ApplyBiome(int biomeId)
    {
        if (targetRenderer == null)
            return;

        if (biomeId < 0 || biomeId >= biomeSprites.Length)
            return;

        targetRenderer.sprite = biomeSprites[biomeId];
    }
}