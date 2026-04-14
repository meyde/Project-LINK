using UnityEngine;
using TMPro;

public class CloudWaveModuleManager : MonoBehaviour
{
    [Header("Base de signaux")]
    public CloudWaveSignalSO[] availableSignals;

    [Header("Signal actuellement choisi")]
    public CloudWaveSignalSO currentSignal;

    [Header("Affichage")]
    public SpriteRenderer signalDisplay;
    public TextMeshPro idText;

    [Header("Leviers")]
    public CloudWaveLever[] levers;

    [Header("Lumière d'état")]
    public SpriteRenderer statusLight;
    public Color neutralColor = Color.white;
    public Color successColor = Color.green;
    public Color failureColor = Color.red;

    [Header("Etat")]
    public int moduleId = 1;
    public bool isSolved;

    private void Start()
    {
        AssignManagerToLevers();
        PickRandomSignal();
        SetNeutralState();
    }

    private void AssignManagerToLevers()
    {
        if (levers == null)
            return;

        for (int i = 0; i < levers.Length; i++)
        {
            if (levers[i] != null)
                levers[i].moduleManager = this;
        }
    }

    public void PickRandomSignal()
    {
        if (availableSignals == null || availableSignals.Length == 0)
        {
            Debug.LogWarning("Aucun CloudWaveSignalSO assigné au module.");
            return;
        }

        currentSignal = availableSignals[Random.Range(0, availableSignals.Length)];

        Debug.Log($"Signal choisi : {currentSignal.id}");

        ApplyCurrentSignalVisual();
        ResetLevers(false);
        isSolved = false;
        SetNeutralState();
    }

    private void ApplyCurrentSignalVisual()
    {
        if (currentSignal == null)
        {
            Debug.LogWarning("Aucun signal à afficher.");
            return;
        }

        // Affichage du sprite central
        if (signalDisplay != null)
        {
            signalDisplay.sprite = currentSignal.signalSprite;
        }
        else
        {
            Debug.LogWarning("SignalDisplay non assigné !");
        }

        // Affichage ID (optionnel)
        if (idText != null)
        {
            idText.text = currentSignal.id;
        }
    }

    public void OnLeverStateChanged()
    {
        // Quand un levier change, on remet juste la light en neutre.
        // La vraie validation se fait uniquement avec le bouton.
        SetNeutralState();
        isSolved = false;
    }

    // À appeler depuis le bouton de validation
    public void ValidateLevers()
    {
        Debug.Log("Validation demandée");
        CheckSolution();
    }

    public void CheckSolution()
    {
        if (currentSignal == null)
        {
            Debug.LogWarning("Aucun signal courant.");
            return;
        }

        if (currentSignal.leverCode == null || currentSignal.leverCode.Length == 0)
        {
            Debug.LogWarning($"Le signal {currentSignal.name} n'a pas de code.");
            return;
        }

        if (levers == null || levers.Length == 0)
        {
            Debug.LogWarning("Aucun levier assigné.");
            return;
        }

        if (currentSignal.leverCode.Length != levers.Length)
        {
            Debug.LogWarning(
                $"Le signal {currentSignal.name} contient {currentSignal.leverCode.Length} états, " +
                $"mais il y a {levers.Length} leviers."
            );

            isSolved = false;
            SetFailureState();
            return;
        }

        for (int i = 0; i < levers.Length; i++)
        {
            bool expected = currentSignal.leverCode[i];
            bool current = levers[i] != null && levers[i].IsOn;

            if (current != expected)
            {
                isSolved = false;
                SetFailureState();
                return;
            }
        }

        isSolved = true;
        SetSuccessState();
        Debug.Log($"Module onde résolu ! Signal : {currentSignal.id}");
    }

    public void ResetLevers(bool notifyManager = true)
    {
        if (levers == null)
            return;

        for (int i = 0; i < levers.Length; i++)
        {
            if (levers[i] != null)
                levers[i].SetState(false, false);
        }

        isSolved = false;

        if (notifyManager)
            OnLeverStateChanged();
    }

    private void SetNeutralState()
    {
        if (statusLight != null)
            statusLight.color = neutralColor;
    }

    private void SetSuccessState()
    {
        if (statusLight != null)
            statusLight.color = successColor;
    }

    private void SetFailureState()
    {
        if (statusLight != null)
            statusLight.color = failureColor;
    }
}