using UnityEngine;

[CreateAssetMenu(fileName = "CloudWaveSignal", menuName = "Modules/Cloud Wave Signal")]
public class CloudWaveSignalSO : ScriptableObject
{
    [Header("Identifiant")]
    public int id;

    [Header("Affichage")]
    public Sprite signalSprite;
}