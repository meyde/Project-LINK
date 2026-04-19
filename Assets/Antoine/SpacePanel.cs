using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SpacePanel : NetworkBehaviour
{
    private NetworkPlayer player;
    public TMP_InputField inputField;

    private void Start()
    {
        player = GetComponent<NetworkPlayer>();
    }

    private void Update()
    {
        if (!IsOwner) return;
        if (player == null) return;
        if (!player.IsSpace()) return;

        if (Input.GetKeyDown(KeyCode.R))
        {
            GameProblemSystem.Instance.ReportProblemServerRpc();
        }

        if (Input.GetKeyDown(KeyCode.Return))
        {
            SendInputMessage();
        }
    }

    public void OnClickChangeColor()
    {
        if (!IsOwner) return;
        if (player == null) return;
        if (!player.IsSpace()) return;

        if (ColorSyncObject.Instance != null)
        {
            ColorSyncObject.Instance.ChangeColorServerRpc();
        }
    }

    public void SendInputMessage()
    {
        if (!IsOwner) return;
        if (player == null) return;
        if (!player.IsSpace()) return;

        string message = inputField.text;

        if (string.IsNullOrWhiteSpace(message)) return;

        NetworkMessageSystem.Instance.SendMessageServerRpc(message);

        inputField.text = ""; // reset champ
    }
}
