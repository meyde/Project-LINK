using UnityEngine;

[CreateAssetMenu(fileName = "WaterModuleRule", menuName = "Game/Water Module Rule")]
public class WaterModuleRuleSO : ScriptableObject
{
    [Header("Contexte")]
    public int catastropheCaseId;
    public WaterColorType waterColor;
    public WaterGasType waterGas;

    [Header("Ports attendus")]
    public ModulePortId expectedInputPort;
    public ModulePortId expectedOutputPort;
}