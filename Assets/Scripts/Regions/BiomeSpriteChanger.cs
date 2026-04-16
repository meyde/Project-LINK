using UnityEngine;

public class BiomeSpriteChanger : MonoBehaviour
{
    [Header("Renderer à modifier")]
    [SerializeField] private SpriteRenderer targetRenderer;

    [Header("Sprites par biome (index = biomeId)")]
    [SerializeField] private Sprite[] biomeSprites;
    [SerializeField] private string[] fxTypes;

[Header("Biome actuel")]
    [SerializeField] private int currentBiomeId;


    private void Awake()
    {
        if (targetRenderer == null)
            targetRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        ApplyBiome(currentBiomeId);
    }

    public void SetBiome(int biomeId, string eventFx)
    {
        currentBiomeId = biomeId;
        ApplyBiome(currentBiomeId);
        int fxInd = -1;

        int ind = 0;
        foreach (string  fxType in fxTypes)
        {
            if (fxType == eventFx)
            {
                fxInd = ind;
            }
            ind++;
        }


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