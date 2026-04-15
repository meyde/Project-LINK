using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HeatModuleManager : Module
{
    [Header("Recette à résoudre")]
    private HeatModuleRecipe currentRecipe;
    public HeatModuleRecipe[] allRecipes;

    [Header("Podomètres")]
    public HeatDial dialL;
    public HeatDial dialH;
    public HeatDial dialI;

    [Header("Feedback visuel")]
    public TextMeshPro stateText;
    [SerializeField] SpriteRenderer sr;
    [SerializeField] Sprite[] sprites;

    [Tooltip("Unique lumière d'état du module")]
    public SpriteRenderer statusLight;

    [Header("Couleurs")]
    public Color neutralColor = Color.white;
    public Color successColor = Color.green;
    public Color failureColor = Color.red;

    [Header("Etat du module")]
    public bool isSolved;

    private bool hasValidated = false;

    private GameManagerLocal gm;

    private int moduleId = 2;
    private void Awake()
    {
        gm = FindFirstObjectByType<GameManagerLocal>();
    }

    private void Start()
    {
        if (dialL != null) dialL.Initialize(this, "L");
        if (dialH != null) dialH.Initialize(this, "H");
        if (dialI != null) dialI.Initialize(this, "I");

        if (statusLight == null)
            Debug.LogError("StatusLight non assigné dans l'inspecteur.");

        UpdateVisuals();
    }

    public void OnDialValueChanged()
    {
        UpdateVisuals();
        hasValidated = false;
    }

    private void UpdateVisuals()
    {

        if(dialL.CurrentValue == 0)
        {
            if (dialH.CurrentValue == 0)
            {
                if (dialI.CurrentValue == 0)
                {
                    sr.sprite = sprites[0];
                }
                else if (dialI.CurrentValue == 1)
                {
                    sr.sprite = sprites[1];
                }
                else
                {
                    sr.sprite = sprites[2];
                }
            }
            else if (dialH.CurrentValue == 1)
            {
                if (dialI.CurrentValue == 0)
                {
                    sr.sprite = sprites[3];
                }
                else if (dialI.CurrentValue == 1)
                {
                    sr.sprite = sprites[4];
                }
                else
                {
                    sr.sprite = sprites[5];
                }
            }
            else
            {
                if (dialI.CurrentValue == 0)
                {
                    sr.sprite = sprites[6];
                }
                else if (dialI.CurrentValue == 1)
                {
                    sr.sprite = sprites[7];
                }
                else
                {
                    sr.sprite = sprites[8];
                }
            }
        }
        else if (dialL.CurrentValue ==1)
        {
            if (dialH.CurrentValue == 0)
            {
                if (dialI.CurrentValue == 0)
                {
                    sr.sprite = sprites[9];
                }
                else if (dialI.CurrentValue == 1)
                {
                    sr.sprite = sprites[10];
                }
                else
                {
                    sr.sprite = sprites[11];
                }
            }
            else if (dialH.CurrentValue == 1)
            {
                if (dialI.CurrentValue == 0)
                {
                    sr.sprite = sprites[12];
                }
                else if (dialI.CurrentValue == 1)
                {
                    sr.sprite = sprites[13];
                }
                else
                {
                    sr.sprite = sprites[14];
                }
            }
            else
            {
                if (dialI.CurrentValue == 0)
                {
                    sr.sprite = sprites[15];
                }
                else if (dialI.CurrentValue == 1)
                {
                    sr.sprite = sprites[16];
                }
                else
                {
                    sr.sprite = sprites[17];
                }
            }
        }
        else
        {
            if (dialH.CurrentValue == 0)
            {
                if (dialI.CurrentValue == 0)
                {
                    sr.sprite = sprites[18];
                }
                else if (dialI.CurrentValue == 1)
                {
                    sr.sprite = sprites[19];
                }
                else
                {
                    sr.sprite = sprites[20];
                }
            }
            else if (dialH.CurrentValue == 1)
            {
                if (dialI.CurrentValue == 0)
                {
                    sr.sprite = sprites[21];
                }
                else if (dialI.CurrentValue == 1)
                {
                    sr.sprite = sprites[22];
                }
                else
                {
                    sr.sprite = sprites[23];
                }
            }
            else
            {
                if (dialI.CurrentValue == 0)
                {
                    sr.sprite = sprites[24];
                }
                else if (dialI.CurrentValue == 1)
                {
                    sr.sprite = sprites[25];
                }
                else
                {
                    sr.sprite = sprites[26];
                }
            }
        }
    }

    public void Validate()
    {
        hasValidated = true;
        int r=CheckSolution();
        int index = 0;
        List<int> falseInd = new();
        bool hasSucceeded = false;
        foreach (int eventId in gm.gmn.eventDataIds)
        {
            CatastrophicEvent cEvent = gm.allEvents[eventId];
            for (int i = 0; i < cEvent.modules1.Length; i++)
            {
                if (cEvent.modules1[i] == moduleId)
                {
                    if (cEvent.modulesState1[i] == r)
                    {
                        hasSucceeded = true;
                        gm.EndModuleCheck(moduleId, true, index);
                    }
                    else
                    {
                        falseInd.Add(i);
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

    public int CheckSolution()
    {
        if (currentRecipe == null)
        {
            Debug.LogWarning("Aucune recette assignée.");
            return -1;
        }

        if (!hasValidated)
            return -1;

        
        foreach (HeatModuleRecipe r in allRecipes)
        {
            if (dialL.CurrentValue == r.targetL &&
            dialH.CurrentValue == r.targetH &&
            dialI.CurrentValue == r.targetI)
            {
                return r.reciepeId;
            }
            
        }
        return -1;

        


        
    }

    public Vector3 GetCurrentHeatValues()
    {
        return new Vector3(dialL.CurrentValue, dialH.CurrentValue, dialI.CurrentValue);
    }
}