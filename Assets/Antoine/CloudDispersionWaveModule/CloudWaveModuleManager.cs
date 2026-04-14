using UnityEngine;
using TMPro;
using NUnit.Framework;
using System.Collections.Generic;

public class CloudWaveModuleManager : Module 
{
    [Header("Base de signaux")]
    public CloudWaveSignalSO[] availableSignals;
    public CloudWaveCodeSO[] availableCodes;

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


    private GameManagerLocal gm;
    private List<CloudWaveCodeSO> possibleCodes;

    private void Awake()
    {
        gm = FindFirstObjectByType<GameManagerLocal>();
    }
    public override void  OnStarted()
    {
        AssignManagerToLevers();
        PickRandomSignal();
        SetNeutralState();
        int signalId = currentSignal.id;
        foreach (CloudWaveCodeSO code in availableCodes)
        {
            for(int i=0; i< gm.gmn.eventDataIds.Count;i++)
            {
                if (gm.gmn.eventDataIds[i] == code.eventId && signalId == code.signalId)
                {
                    possibleCodes.Add(code);
                }
            }
        }
        

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
            idText.text = currentSignal.id.ToString();
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
        foreach(CloudWaveCodeSO code in possibleCodes)
        {
            CheckSolution(code);
        }
        
    }

    public void CheckSolution(CloudWaveCodeSO codeSo)
    {
        if (currentSignal == null)
        {
            Debug.LogWarning("Aucun signal courant.");
            return;
        }

        if (codeSo.leverCode == null || codeSo.leverCode.Length == 0)
        {
            Debug.LogWarning($"Le signal {currentSignal.name} n'a pas de code.");
            return;
        }

        if (levers == null || levers.Length == 0)
        {
            Debug.LogWarning("Aucun levier assigné.");
            return;
        }

        if (codeSo.leverCode.Length != levers.Length)
        {
            Debug.LogWarning(
                $"Le signal {currentSignal.name} contient {codeSo.leverCode.Length} états, " +
                $"mais il y a {levers.Length} leviers."
            );

            isSolved = false;
            return;
        }

        for (int i = 0; i < levers.Length; i++)
        {
            bool expected = codeSo.leverCode[i];
            bool current = levers[i] != null && levers[i].IsOn;

            if (current != expected)
            {
                isSolved = false;
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