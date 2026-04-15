using System.Threading.Tasks;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerLobbyData : NetworkBehaviour
{

    public NetworkVariable<FixedString32Bytes> Pseudo =
        new NetworkVariable<FixedString32Bytes>("Player");

    public NetworkVariable<FixedString64Bytes> LobbyCode =
    new NetworkVariable<FixedString64Bytes>(
        "",
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    public NetworkVariable<bool> IsReady =
        new NetworkVariable<bool>(false);

    public NetworkVariable<PlayerRole> SelectedRole =
        new NetworkVariable<PlayerRole>(
            PlayerRole.None,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );


    public void OnStartGame()
    {
        Debug.Log("Change Value");
        Pseudo.OnValueChanged -= OnPseudoChanged;
        LobbyCode.OnValueChanged -= OnLobbyCodeChanged;
        IsReady.OnValueChanged -= OnReadyChanged;
    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            LobbyUIManager.Instance.BindLocalPlayer(this);
        }

        Pseudo.OnValueChanged += OnPseudoChanged;
        LobbyCode.OnValueChanged -= OnLobbyCodeChanged;
        IsReady.OnValueChanged += OnReadyChanged;

        if (LobbyUIManager.Instance != null)
            LobbyUIManager.Instance.RefreshPlayerList();
    }

    public override void OnNetworkDespawn()
    {
        Pseudo.OnValueChanged -= OnPseudoChanged;
        LobbyCode.OnValueChanged -= OnLobbyCodeChanged;
        IsReady.OnValueChanged -= OnReadyChanged;

        if (LobbyUIManager.Instance != null)
            LobbyUIManager.Instance.RefreshPlayerList();
    }

    private void OnPseudoChanged(FixedString32Bytes oldValue, FixedString32Bytes newValue)
    {
        if (LobbyUIManager.Instance != null)
            LobbyUIManager.Instance.RefreshPlayerList();
    }

    private void OnLobbyCodeChanged(FixedString64Bytes oldValue, FixedString64Bytes newValue)
    {
        if (LobbyUIManager.Instance != null)
            LobbyUIManager.Instance.RefreshPlayerList();
    }

    private void OnReadyChanged(bool oldValue, bool newValue)
    {
        if (LobbyUIManager.Instance != null)
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