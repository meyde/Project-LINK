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

    [Header("Lumi�re d'�tat")]
    public SpriteRenderer statusLight;
    public Color neutralColor = Color.white;
    public Color successColor = Color.green;
    public Color failureColor = Color.red;

    [Header("Etat")]
    public int moduleId = 1;


    private GameManagerLocal gm;
    private ZoomableModule zm;

    private void Awake()
    {
        gm = FindFirstObjectByType<GameManagerLocal>();
        zm = gameObject.GetComponent<ZoomableModule>(); 
        AssignManagerToLevers();
        
    }
    public override void OnStarted()
    {
        PickRandomSignal();
        signalDisplay.sprite = currentSignal.signalSprite;
        Reset();
    }

    private void Reset()
    {
        foreach (CloudWaveLever lever in levers)
        {
            lever.SetState(false);
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
            Debug.LogWarning("Aucun CloudWaveSignalSO assign� au module.");
            return;
        }

        currentSignal = availableSignals[Random.Range(0, availableSignals.Length)];

        Debug.Log($"Signal choisi : {currentSignal.id}");


    }

    // � appeler depuis le bouton de validation
    public void ValidateLevers()
    {
        Debug.Log("Validation demand�e");
        int index = 0;
        List<int> falseInd = new();
        bool hasSucceeded = false;
        int[] tempIds = new int[5] { 0, 1, 2, 3, 4 };
        foreach (int eventId in gm.gmn.eventDataIds)
        {
            CatastrophicEvent cEvent = gm.allEvents[eventId];
            Debug.Log("event:");
            Debug.Log(eventId.ToString());
            for (int i = 0; i < cEvent.modules1.Length; i++)
            {
                if (cEvent.modules1[i] == moduleId)
                {
                    foreach (CloudWaveCodeSO code in availableCodes)
                    {
                        if (code.eventId == eventId && code.signalId == currentSignal.id)
                        {
                            if (CheckSolution(code))
                            {
                                hasSucceeded = true;
                                gm.EndModuleCheck(moduleId, true, index);
                                zm.CloseModule();
                                break;
                            }
                            else
                            {
                                falseInd.Add(code.eventId);
                            }
                        }
                    }
                }
            }
            index++;
        }
        if (!hasSucceeded)
        {
            if (falseInd.Count > 0)
            {
                gm.EndModuleCheck(moduleId, false, falseInd[0]);
            }
            else
            {
                if (gm.gmn.eventDataIds.Count > 0)
                {
                    gm.EndModuleCheck(moduleId, false, 0);
                }
            }
        }

    }

    public bool CheckSolution(CloudWaveCodeSO codeSo)
    {
        if (currentSignal == null)
        {
            Debug.LogWarning("Aucun signal courant.");
            return false;
        }

        if (codeSo.leverCode == null || codeSo.leverCode.Length == 0)
        {
            Debug.LogWarning($"Le signal {currentSignal.name} n'a pas de code.");
            return false;
        }

        if (levers == null || levers.Length == 0)
        {
            Debug.LogWarning("Aucun levier assign�.");
            return false;
        }

        if (codeSo.leverCode.Length != levers.Length)
        {
            Debug.LogWarning(
                $"Le signal {currentSignal.name} contient {codeSo.leverCode.Length} �tats, " +
                $"mais il y a {levers.Length} leviers."
            );

            return false;
        }

        for (int i = 0; i < levers.Length; i++)
        {
            bool expected = codeSo.leverCode[i];
            Debug.Log("expected "+expected.ToString());
            bool current = levers[i] != null && levers[i].isOn;
            Debug.Log("got"+current.ToString());

            if (current != expected)
            {
                return false;
            }
        }

        Debug.Log($"Module onde r�solu ! Signal : {currentSignal.id}");
        return true;
    }

}