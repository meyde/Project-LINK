using UnityEngine;
using TMPro;

public class WorldEventProgressDisplay : MonoBehaviour
{
    [Header("Earth UI")]
    [SerializeField] private TextMeshPro earthSuccessText;
    [SerializeField] private TextMeshPro earthFailText;

    [Header("Space UI")]
    [SerializeField] private TextMeshPro spaceSuccessText;
    [SerializeField] private TextMeshPro spaceFailText;

    [Header("Settings")]
    [SerializeField] private int maxHealth = 3;

    private int lastSuccess = -1;
    private int lastFail = -1;

    private void Update()
    {
        var gm = GameManagerNetwork.Instance;
        if (gm == null)
            return;

        int success = gm.successes.Value;
        int fail = maxHealth - gm.health.Value;

        // Update seulement si changement
        if (success != lastSuccess || fail != lastFail)
        {
            lastSuccess = success;
            lastFail = fail;

            string successText = $"Succès : {success}/{gm.RequiredSuccesses}";
            string failText = $"Échecs : {fail}/{maxHealth}";

            // Earth
            if (earthSuccessText != null)
                earthSuccessText.text = successText;

            if (earthFailText != null)
                earthFailText.text = failText;

            // Space
            if (spaceSuccessText != null)
                spaceSuccessText.text = successText;

            if (spaceFailText != null)
                spaceFailText.text = failText;
        }
    }
}