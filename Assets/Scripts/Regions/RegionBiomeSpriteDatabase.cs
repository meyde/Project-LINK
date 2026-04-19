using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RegionBiomeDatabase", menuName = "Scriptable Objects/Region Biome Database")]
public class RegionBiomeDatabase : ScriptableObject
{
    [Serializable]
    public class RegionBiomeEntry
    {
        public int regionId;
        public int biomeId;
    }

    public List<RegionBiomeEntry> entries = new();

    public int GetBiome(int regionId)
    {
        for (int i = 0; i < entries.Count; i++)
        {
            if (entries[i].regionId == regionId)
                return entries[i].biomeId;
        }

        Debug.LogWarning($"Aucun biome trouvé pour la région {regionId}");
        return -1;
    }
}