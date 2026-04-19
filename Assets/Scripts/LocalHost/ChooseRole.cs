using Unity.Netcode;
using UnityEngine;

public class ChooseRole : NetworkBehaviour
{
    public NetworkVariable<PlayerRole> Role = new NetworkVariable<PlayerRole>(
        PlayerRole.None,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    public override void OnNetworkSpawn()
    {
        Role.OnValueChanged += OnRoleChanged;

        if (IsServer)
        {
            SyncRoleFromLobby();
        }

        OnRoleChanged(Role.Value, Role.Value);
    }

    public override void OnNetworkDespawn()
    {
        Role.OnValueChanged -= OnRoleChanged;
    }

    private void SyncRoleFromLobby()
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
                Debug.Log($"[ChooseRole] Client {OwnerClientId} -> rôle synchronisé : {Role.Value}");
                return;
            }
        }

        Debug.LogWarning($"[ChooseRole] Aucun PlayerLobbyData trouvé pour client {OwnerClientId}");
    }

    private void OnRoleChanged(PlayerRole oldRole, PlayerRole newRole)
    {
        Debug.Log($"[ChooseRole] Client {OwnerClientId} -> rôle : {newRole}");
    }

    public bool HasRole(PlayerRole role)
    {
        return Role.Value == role;
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