using Unity.Netcode;
using UnityEngine;

public class ColorSyncObject : NetworkBehaviour
{
    public static ColorSyncObject Instance;

    public NetworkVariable<Color> syncedColor = new NetworkVariable<Color>(
        Color.white,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private SpriteRenderer sr;

    private void Awake()
    {
        Instance = this;
        sr = GetComponent<SpriteRenderer>();
    }

    public override void OnNetworkSpawn()
    {
        syncedColor.OnValueChanged += OnColorChanged;
        ApplyColor(syncedColor.Value);
    }

    public override void OnNetworkDespawn()
    {
        syncedColor.OnValueChanged -= OnColorChanged;
    }

    private void OnColorChanged(Color oldColor, Color newColor)
    {
        ApplyColor(newColor);
    }

    private void ApplyColor(Color color)
    {
        if (sr != null)
            sr.color = color;
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void ChangeColorServerRpc()
    {
        // Toggle simple entre rouge et bleu
        if (syncedColor.Value == Color.red)
            syncedColor.Value = Color.blue;
        else
            syncedColor.Value = Color.red;

        Debug.Log("Couleur changée côté serveur !");
    }
}