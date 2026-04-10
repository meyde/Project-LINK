using Unity.Netcode;
using UnityEngine;

public class NetworkMessageSystem : NetworkBehaviour
{
    public static NetworkMessageSystem Instance;

    private void Awake()
    {
        Instance = this;
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void SendMessageServerRpc(string message)
    {
        Debug.Log("Message reçu par serveur : " + message);
        ReceiveMessageClientRpc(message);
    }

    [ClientRpc]
    private void ReceiveMessageClientRpc(string message)
    {
        Debug.Log("Message envoyé à tous : " + message);

        TypewriterText[] texts = FindObjectsByType<TypewriterText>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (var t in texts)
        {
            if (t != null && t.gameObject.activeInHierarchy)
            {
                t.StartTyping(message);
            }
        }
    }
}