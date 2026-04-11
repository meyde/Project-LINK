using Unity.Collections;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class PlayerLobbyData : NetworkBehaviour
{
    public NetworkVariable<FixedString32Bytes> Pseudo =
        new NetworkVariable<FixedString32Bytes>("Player");

    public NetworkVariable<bool> IsReady =
        new NetworkVariable<bool>(false);

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            // Tell UI that this is the local player
            LobbyUIManager.Instance.BindLocalPlayer(this);
        }

        // Update UI whenever values change
        Pseudo.OnValueChanged += (_, __) => LobbyUIManager.Instance.RefreshPlayerList();
        IsReady.OnValueChanged += (_, __) => LobbyUIManager.Instance.RefreshPlayerList();

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
}