using UnityEngine;
using TMPro;

public class WorldEventProgressDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshPro eventsText;
    [SerializeField] private string prefix = "Events résolus : ";

    private int lastDisplayedSuccesses = -1;

    void Update()
    {
        var gm = GameManagerNetwork.Instance;
        if (gm == null || eventsText == null)
            return;

        int currentSuccesses = gm.successes.Value;

        lastDisplayedSuccesses = currentSuccesses;
        eventsText.text = $"{prefix}{currentSuccesses} / {gm.RequiredSuccesses}";
    }
}
