using UnityEngine;
using UnityEngine.Rendering;

public class ScreenZoomOrder : MonoBehaviour
{
    [SerializeField] private SortingGroup sortingGroup;
    [SerializeField] private int normalOrder = 0;
    [SerializeField] private int zoomedOrder = 100;

    private void Awake()
    {
        if (sortingGroup == null)
            sortingGroup = GetComponent<SortingGroup>();
    }

    public void SetZoomed(bool zoomed)
    {
        if (sortingGroup != null)
            sortingGroup.sortingOrder = zoomed ? zoomedOrder : normalOrder;
    }
}