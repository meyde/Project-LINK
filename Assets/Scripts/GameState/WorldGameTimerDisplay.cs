using UnityEngine;
using TMPro;

public class WorldGameTimerDisplay : MonoBehaviour
{
    [Header("Référence texte 3D dans la scène")]
    [SerializeField] private TextMeshPro timerText;

    [Header("Format")]
    [SerializeField] private string prefix = "Temps restant : ";

    private void Awake()
    {
        if (timerText == null)
            timerText = GetComponent<TextMeshPro>();
    }

    void Update()
    {
        float timeLeft = GameManagerNetwork.Instance.remainingGameTime.Value;

        if (timeLeft < 0f)
            timeLeft = 0f;

        int minutes = Mathf.FloorToInt(timeLeft / 60f);
        int seconds = Mathf.FloorToInt(timeLeft % 60f);

        timerText.text = $"{prefix}{minutes:00}:{seconds:00}";
    }
}
