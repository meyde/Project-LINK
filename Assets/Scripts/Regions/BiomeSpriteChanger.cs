using System.Collections;
using UnityEngine;

public class BiomeSpriteChanger : MonoBehaviour
{
    [Header("Renderer à modifier")]
    [SerializeField] private SpriteRenderer targetRenderer;

    [Header("Sprites par biome (index = biomeId)")]
    [SerializeField] private Sprite[] biomeSprites;
    [SerializeField] private string[] fxTypes;
    [SerializeField] private GameObject[] FxObj;
    [SerializeField] private SpriteRenderer[] ThunderStrikesSr;

[Header("Biome actuel")]
    [SerializeField] private int currentBiomeId;

    public SpriteRenderer[] thunderStrikes;
    [SerializeField] private float timeBetweenStrikes;
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
        StopCoroutine(thunder);
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
                Debug.Log("Tornado, no debris");
                break;
            case 7:
                Debug.Log("Tornado, debris");
                break;
            case 8:
                Debug.Log("blizzard,snow");
                break;
            case 9:
                Debug.Log("Blizzard, ice");
                break;
            case 10:
                Debug.Log("Blizzard, solid");
                break;
            case 11:
                Debug.Log("forest fire");
                break;
        }
    }

    public void StormManager(int color, bool isDoubled)
    {
        SpriteRenderer[] thunderStrikes = new SpriteRenderer[3];
        for (int i = 0; i < 3; i++)
        {
            thunderStrikes[i] = ThunderStrikesSr[color + i];
        }
        thunder = StartCoroutine(cyclingThunder(thunderStrikes,isDoubled));
    }
    public IEnumerator cyclingThunder(SpriteRenderer[] coloredStrikes, bool isDoubled)
    {
        while (true)
        {
            if (!isDoubled)
            {int choice = Random.Range(0, 3);
            coloredStrikes[choice].gameObject.SetActive(true);
            yield return new WaitForSeconds(strikesDuration);
            coloredStrikes[choice].gameObject.SetActive(false);
            yield return new WaitForSeconds(timeBetweenStrikes);
            }
            else
            {
                if (!isDoubled)
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

    }




    public void eventOver()
    {
        Debug.Log("event Over, removing fx");
        foreach (GameObject go in FxObj)
        {
            go.SetActive(false);
        }
        StopCoroutine(thunder);
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