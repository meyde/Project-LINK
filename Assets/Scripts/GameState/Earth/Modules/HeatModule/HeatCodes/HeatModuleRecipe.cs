using UnityEngine;

[CreateAssetMenu(fileName = "HeatModuleRecipe", menuName = "Scriptable Objects/Heat Module Recipe")]
public class HeatModuleRecipe : ScriptableObject
{
    [Header("Valeurs cibles (0 à 3)")]
    public int reciepeId;
    [Range(0, 2)] public int targetL;
    [Range(0, 2)] public int targetH;
    [Range(0, 2)] public int targetI;

    [Header("Optionnel : nom de la recette")]
    public string recipeName;
}