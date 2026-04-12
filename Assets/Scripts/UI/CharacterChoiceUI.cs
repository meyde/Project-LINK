using UnityEngine;
using UnityEngine.UI;

public class CharacterChoiceUI : MonoBehaviour
{
    private PlayerLobbyData localPlayer;

    [Header("Buttons")]
    [SerializeField] private Button spaceButton;
    [SerializeField] private Button earthButton;

    private void Start()
    {
        FindLocalPlayer();
        RefreshButtons();
        InvokeRepeating(nameof(RefreshButtons), 0.5f, 0.5f); // refresh auto
    }

    private void FindLocalPlayer()
    {
        var players = FindObjectsByType<PlayerLobbyData>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None
        );

        foreach (var p in players)
        {
            if (p.IsOwner)
            {
                localPlayer = p;
                return;
            }
        }
    }

    public void SelectSpace()
    {
        if (localPlayer == null) return;

        if (IsRoleTaken(PlayerRole.Space)) return;

        localPlayer.SetRoleServerRpc(PlayerRole.Space);
    }

    public void SelectEarth()
    {
        if (localPlayer == null) return;

        if (IsRoleTaken(PlayerRole.Earth)) return;

        localPlayer.SetRoleServerRpc(PlayerRole.Earth);
    }

    private bool IsRoleTaken(PlayerRole role)
    {
        var players = FindObjectsByType<PlayerLobbyData>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None
        );

        foreach (var p in players)
        {
            if (p.SelectedRole.Value == role)
                return true;
        }

        return false;
    }

    private void RefreshButtons()
    {
        bool spaceTaken = IsRoleTaken(PlayerRole.Space);
        bool earthTaken = IsRoleTaken(PlayerRole.Earth);

        spaceButton.interactable = !spaceTaken;
        earthButton.interactable = !earthTaken;
    }
}