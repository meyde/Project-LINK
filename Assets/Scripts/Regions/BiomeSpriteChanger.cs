using System.Collections;
using UnityEngine;

public class BiomeSpriteChanger : MonoBehaviour
{
    [Header("Renderer à modifier")]
    [SerializeField] private SpriteRenderer targetRenderer;

    [Header("Sprites par biome (index = biomeId)")]
    [SerializeField] private Sprite[] biomeSprites;
    [SerializeField] private GameObject[] FxObj;

[Header("Biome actuel")]
    [SerializeField] private int currentBiomeId;

    public SpriteRenderer[] thunderStrikes;
    [SerializeField] private float timeBetweenStrikes=10f;
    [SerializeField] private float strikesDuration = 0.5f ;
    [SerializeField] private float doubleStrikesIntervals = 2f;
    private Coroutine thunder;
    private void Awake()
    {
        if (targetRenderer == null)
            targetRenderer = GetComponent<SpriteRenderer>();

    }

    private void Start()
    {
        ApplyBiome(currentBiomeId);
    }

    public void SetBiome(int biomeId, int eventFx)
    {
        if (thunder != null) StopCoroutine(thunder);
        currentBiomeId = biomeId;
        ApplyBiome(currentBiomeId);
        foreach (GameObject go in FxObj)
        {
            go.SetActive(false);
        }
        switch (eventFx)
        {
            case 0:
                FxObj[0].gameObject.SetActive(true);
                Debug.Log("yellow storm, single");
                StormManager(0, false);
                break;
            case 1:
                FxObj[0].gameObject.SetActive(true);
                Debug.Log("blue Storm, single");
                StormManager(1,false);
                break;
            case 2:
                FxObj[0].gameObject.SetActive(true);
                Debug.Log("green storm, single");
                StormManager(2, false);
                break;
            case 3:
                FxObj[0].gameObject.SetActive(true);
                Debug.Log("yellow storm, double");
                StormManager(0, true);
                break;
            case 4:
                FxObj[0].gameObject.SetActive(true);
                Debug.Log("blue Storm, double");
                StormManager(1, true);
                break;
            case 5:
                FxObj[0].gameObject.SetActive(true);
                Debug.Log("green storm, double");
                StormManager(2, true);
                break;
            case 6:
                FxObj[1].gameObject.SetActive(true);
                Debug.Log("Tornado, no debris");
                break;
            case 7:
                FxObj[2].gameObject.SetActive(true);
                Debug.Log("Tornado, debris");
                break;
            case 8:
                FxObj[3].gameObject.SetActive(true);
                Debug.Log("blizzard,snow");
                break;
            case 9:
                FxObj[4].gameObject.SetActive(true);
                Debug.Log("Blizzard, ice");
                break;
            case 10:

                FxObj[5].gameObject.SetActive(true);
                Debug.Log("Blizzard, solid");
                break;
            case 11:
                FxObj[6].gameObject.SetActive(true);
                Debug.Log("forest fire");
                break;
        }
    }

    public void StormManager(int color, bool isDoubled)
    {
        foreach (SpriteRenderer sr in thunderStrikes)
        {
            sr.gameObject.SetActive(false);
        }
        SpriteRenderer[] thunderStrikesSr = new SpriteRenderer[3];
        for (int i = 0; i < 3; i++)
        {
            thunderStrikesSr[i] = thunderStrikes[3*color + i];
        }
        thunder = StartCoroutine(CyclingThunder(thunderStrikesSr,isDoubled));
    }
    public IEnumerator CyclingThunder(SpriteRenderer[] coloredStrikes, bool isDoubled)
    {
        while (true)
        {
            if (!isDoubled)
            {
                int choice = Random.Range(0, 3);
                coloredStrikes[choice].gameObject.SetActive(true);
                yield return new WaitForSeconds(strikesDuration);
                coloredStrikes[choice].gameObject.SetActive(false);
                yield return new WaitForSeconds(timeBetweenStrikes);
            }
            else
            {
                int choice = Random.Range(0, 3);
                coloredStrikes[choice].gameObject.SetActive(true);
                yield return new WaitForSeconds(strikesDuration);
                coloredStrikes[choice].gameObject.SetActive(false);
                yield return new WaitForSeconds(doubleStrikesIntervals);
                choice = Random.Range(0, 3);
                coloredStrikes[choice].gameObject.SetActive(true);
                yield return new WaitForSeconds(strikesDuration);
                coloredStrikes[choice].gameObject.SetActive(false);
                yield return new WaitForSeconds(timeBetweenStrikes);

            }
        }

    }




    public void eventOver()
    {
        Debug.Log("event Over, removing fx");
        foreach (GameObject go in FxObj)
        {
            go.SetActive(false);
        }
        if (thunder != null )StopCoroutine(thunder);
    }
    private void ApplyBiome(int biomeId)
    {
        if (targetRenderer == null)
        {
            Debug.LogWarning("[BiomeSprite] Aucun SpriteRenderer assigné.");
            return;
        }

        if (biomeId < 0 || biomeId >= biomeSprites.Length)
        {
            Debug.LogWarning($"[BiomeSprite] biomeId invalide : {biomeId}");
            return;
        }

        targetRenderer.sprite = biomeSprites[biomeId];
    }
}