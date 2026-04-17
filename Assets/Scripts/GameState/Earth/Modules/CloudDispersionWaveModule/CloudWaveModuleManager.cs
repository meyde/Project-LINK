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
        List<int> falseId = new();
        bool hasSucceeded = false;
        foreach (CEventRuntimeData cEventData in gm.gmn.events)
        {
            if (cEventData.state != 1) { continue; }
            CatastrophicEvent cEvent = gm.allEvents[cEventData.eventId];
            Debug.Log($"event:{cEvent.eventId}");
            for (int i = 0; i < cEvent.modules1.Length; i++)
            {
                if (cEvent.modules1[i] == moduleId)
                {
                    Debug.Log("Event needing module found, checking codes.");
                    foreach (CloudWaveCodeSO code in availableCodes)
                    {
                        if (code.eventId == cEvent.eventId && code.signalId == currentSignal.id)
                        {
                            if (CheckSolution(code))
                            {
                                Debug.Log("Good code found. Validating");
                                hasSucceeded = true;
                                gm.EndModuleCheck(moduleId, true, cEvent.eventId);
                                zm.CloseModule();
                                break;
                            }
                            else
                            {
                                Debug.Log("Wrong code.");
                                falseId.Add(code.eventId);
                            }
                        }
                    }
                }
            }
        }
        if (!hasSucceeded)
        {
            if (falseId.Count > 0)
            {
                Debug.Log("an event needing this module is occuring, yet the code was not matched. Failing oldest event ");
                gm.EndModuleCheck(moduleId, false, falseId[0]);
            }
            else
            {
                Debug.Log("No event needing this module is occuring. Failing oldest active event.");
                gm.EndModuleCheck(moduleId, false, -1);

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