using Unity.Netcode;
using UnityEngine;

public class NetworkPlayer : NetworkBehaviour
{
    public NetworkVariable<PlayerRole> Role = new NetworkVariable<PlayerRole>(
        PlayerRole.None,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            AssignRole();
        }

        Role.OnValueChanged += OnRoleChanged;
        OnRoleChanged(Role.Value, Role.Value);
    }

    public override void OnNetworkDespawn()
    {
        Role.OnValueChanged -= OnRoleChanged;
    }

    private void AssignRole()
    {
        // Host = Space, second joueur = Earth

        if (OwnerClientId == 0)
            Role.Value = PlayerRole.Space;
        else
            Role.Value = PlayerRole.Earth;
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