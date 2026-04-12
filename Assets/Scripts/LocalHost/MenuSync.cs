using Unity.Netcode;
using UnityEngine;

public class MenuSync : NetworkBehaviour
{
    public enum ScreenType { Main, HostLobby }

    public NetworkVariable<ScreenType> CurrentScreen =
        new NetworkVariable<ScreenType>(ScreenType.Main);

    private void Start()
    {
        CurrentScreen.OnValueChanged += OnScreenChanged;
    }
    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            // Host sets the initial screen when the object is ready
            CurrentScreen.Value = ScreenType.HostLobby;
        }

        // All clients update UI when they spawn
        UIManager.Instance.ShowScreen(CurrentScreen.Value);
    }
    private void OnScreenChanged(ScreenType oldValue, ScreenType newValue)
    {
        UIManager.Instance.ShowScreen(newValue);
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void SetScreenServerRpc(ScreenType screen)
    {
        CurrentScreen.Value = screen;
    }
}