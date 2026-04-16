using System.Collections.Generic;
using UnityEngine;

public class MapRegionManager : MonoBehaviour
{
    private PingRegion[] regions;

    private void Awake()
    {
        regions = GetComponentsInChildren<PingRegion>(true);
    }

    public void UpdateRegions(List<int> activeRegionIds)
    {
        if (regions == null || regions.Length == 0)
            regions = GetComponentsInChildren<PingRegion>(true);

        for (int i = 0; i < regions.Length; i++)
        {
            if (regions[i] == null)
                continue;

            bool shouldBeActive = activeRegionIds.Contains(regions[i].regionId);
            regions[i].SetRegionActive(shouldBeActive);
        }
    }
}