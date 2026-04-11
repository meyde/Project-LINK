using Unity.Netcode;
using UnityEngine;

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