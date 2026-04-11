using TMPro;
using Unity.Netcode;
using UnityEngine;

[GenerateSerializationForTypeAttribute(typeof(System.String))]
public class PseudoInput : MonoBehaviour
{
    [SerializeField] private SharedText pseudoLocation;
    [SerializeField] private TMP_InputField pseudoInput;

    public void writing()
    {
        pseudoLocation.UpdateText(pseudoInput.text.Trim());
    }
}
