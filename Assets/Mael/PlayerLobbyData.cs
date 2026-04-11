using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerLobbyData : NetworkBehaviour
{

    public NetworkVariable<FixedString32Bytes> Pseudo =
        new NetworkVariable<FixedString32Bytes>("Player");

    public NetworkVariable<bool> IsReady =
        new NetworkVariable<bool>(false);

    public NetworkVariable<PlayerRole> SelectedRole =
        new NetworkVariable<PlayerRole>(
            PlayerRole.None,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            LobbyUIManager.Instance.BindLocalPlayer(this);
        }

        Pseudo.OnValueChanged += (_, __) => LobbyUIManager.Instance.RefreshPlayerList();
        IsReady.OnValueChanged += (_, __) => LobbyUIManager.Instance.RefreshPlayerList();
        SelectedRole.OnValueChanged += (_, __) => LobbyUIManager.Instance.RefreshPlayerList();

        LobbyUIManager.Instance.RefreshPlayerList();
    }

    [ServerRpc]
    public void SetPseudoServerRpc(string newPseudo)
    {
        Pseudo.Value = newPseudo;
    }

    [ServerRpc]
    public void SetReadyServerRpc(bool ready)
    {
        IsReady.Value = ready;
    }

    [ServerRpc]
    public void SetRoleServerRpc(PlayerRole newRole)
    {
        SelectedRole.Value = newRole;
    }
}