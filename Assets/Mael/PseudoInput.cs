using TMPro;
using UnityEngine;

public class PseudoInput : MonoBehaviour
{
    [SerializeField] private TMP_InputField pseudoInput;

    public void OnValidatePseudo()
    {
        string newPseudo = pseudoInput.text.Trim();

        if (string.IsNullOrEmpty(newPseudo)) return;

        // Envoie au serveur via PlayerLobbyData
        LobbyUIManager.Instance.OnPseudoChanged(newPseudo);
    }
}