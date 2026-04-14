using UnityEngine;
using TMPro;

public class HeatModuleManager : MonoBehaviour
{
    [Header("Recette à résoudre")]
    public HeatModuleRecipe currentRecipe;

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

        bool correct =
            dialL.CurrentValue == currentRecipe.targetL &&
            dialH.CurrentValue == currentRecipe.targetH &&
            dialI.CurrentValue == currentRecipe.targetI;

        isSolved = correct;

        if (correct)
        {
            SetSuccessState();
            Debug.Log("Module chaleur résolu !");
        }
        else
        {
            SetFailureState();
        }
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

    public Vector3 GetCurrentHeatValues()
    {
        return new Vector3(dialL.CurrentValue, dialH.CurrentValue, dialI.CurrentValue);
    }
}