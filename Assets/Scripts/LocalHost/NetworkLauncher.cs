using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

public class NetworkLauncher : MonoBehaviour
{

    public void StartHost()
    {
        NetworkManager.Singleton.StartHost();
        Debug.Log("Host démarré");
    }


    public void StartClient()
    {
        NetworkManager.Singleton.StartClient();
        Debug.Log("Client démarré");
    }
}