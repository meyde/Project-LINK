using UnityEngine;

public class HiderPlates : MonoBehaviour
{
    [SerializeField] HiderPlatesInteract[] blockers;

    private int emptyBlocker ;
    private void Awake()
    {
        emptyBlocker = 15;
        blockers[15].gameObject.SetActive(false);
    }
    public void TryChangingDeactivatedBlocker(int blockerId)
    {
        bool hasChanged = false;
        int[] neighboors = blockers[blockerId].neighboors;
        foreach (int neighboor in neighboors)
        {
            if (neighboor == emptyBlocker)
            {
                hasChanged = true;
                blockers[emptyBlocker].gameObject.SetActive(true);
                blockers[blockerId].gameObject.SetActive(false);
            }
        }
        if (hasChanged)
        {
            Debug.Log("Blocker Moved");
        }

    }
}
