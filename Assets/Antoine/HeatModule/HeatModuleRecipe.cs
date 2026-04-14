using UnityEngine;

[CreateAssetMenu(fileName = "HeatModuleRecipe", menuName = "Modules/Heat Module Recipe")]
public class HeatModuleRecipe : ScriptableObject
{
    [Header("Valeurs cibles (0 à 3)")]
    [Range(0, 3)] public int targetL;
    [Range(0, 3)] public int targetH;
    [Range(0, 3)] public int targetI;

    [Header("Optionnel : nom de la recette")]
    public string recipeName;
}