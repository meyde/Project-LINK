using UnityEngine;
using Unity.Netcode; 

public class squarecolor : NetworkBehaviour
{
    public NetworkVariable<Color> squareColor;


    void Awake()
    {
        squareColor = new NetworkVariable<Color>(new Color(1f, 0.4f, 0.2f),NetworkVariableReadPermission.Everyone,NetworkVariableWritePermission.Server);
    }

    // Update is called once per frame
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void CcRpc() { gameObject.GetComponent<SpriteRenderer>().color = squareColor.Value; }
    void Update()
    {
        CcRpc();
        
    }
}
