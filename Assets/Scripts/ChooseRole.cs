using Unity.Netcode;
using UnityEngine;

public class ChooseRole : NetworkBehaviour
{
    public NetworkVariable<PlayerRole> Role =
        new NetworkVariable<PlayerRole>(
            PlayerRole.None,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            AssignRoleFromLobby();
        }

        Role.OnValueChanged += OnRoleChanged;
        OnRoleChanged(Role.Value, Role.Value);
    }

    public override void OnNetworkDespawn()
    {
        Role.OnValueChanged -= OnRoleChanged;
    }

    private void AssignRoleFromLobby()
    {
        PlayerLobbyData[] players = FindObjectsByType<PlayerLobbyData>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None
        );

        foreach (var player in players)
        {
            if (player.OwnerClientId == OwnerClientId)
            {
                Role.Value = player.SelectedRole.Value;
                Debug.Log($"Rôle récupéré pour client {OwnerClientId} : {Role.Value}");
                return;
            }
        }

        Debug.LogWarning($"Aucun PlayerLobbyData trouvé pour le client {OwnerClientId}");
    }

    private void OnRoleChanged(PlayerRole oldRole, PlayerRole newRole)
    {
        Debug.Log($"Client {OwnerClientId} -> rôle : {newRole}");
    }

    public bool IsSpace()
    {
        return Role.Value == PlayerRole.Space;
    }

    public bool IsEarth()
    {
        return Role.Value == PlayerRole.Earth;
    }
}