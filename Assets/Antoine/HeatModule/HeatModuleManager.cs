using UnityEngine;
using TMPro;

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

        // Choisis UNE des 2 lignes selon ce que tu veux :

        // 1) Si tu veux remettre en blanc quand on modifie :
        // SetNeutralState();

        // 2) Si tu veux ne rien changer visuellement tant qu'on n'a pas revalidé :
        // ne rien faire
    }

    private void UpdateVisuals()
    {
        if (stateText == null)
            return;

        stateText.text =
            $"L : {dialL.CurrentValue}/3\n" +
            $"H : {dialH.CurrentValue}/3\n" +
            $"I : {dialI.CurrentValue}/3";
    }

    public void Validate()
    {
        hasValidated = true;
        CheckSolution();
    }

    public void CheckSolution()
    {
        if (currentRecipe == null)
        {
            Debug.LogWarning("Aucune recette assignée.");
            return;
        }

        if (!hasValidated)
            return;

        int returnState = -1;
        foreach (HeatModuleRecipe r in allRecipes)
        {
            if (dialL.CurrentValue == r.targetL &&
            dialH.CurrentValue == r.targetH &&
            dialI.CurrentValue == r.targetI)
            {
                returnState = r.reciepeId;
            }
        }

        gm.EndModuleCheck(2,false, 0);


        
    }

    public Vector3 GetCurrentHeatValues()
    {
        return new Vector3(dialL.CurrentValue, dialH.CurrentValue, dialI.CurrentValue);
    }
}