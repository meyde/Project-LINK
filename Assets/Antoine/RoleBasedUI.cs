using Unity.Netcode;
using UnityEngine;

public class RoleBasedUI : NetworkBehaviour
{
    public GameObject spaceUI;
    public GameObject earthUI;

    private NetworkPlayer player;

    private void Start()
    {
        player = GetComponent<NetworkPlayer>();
        RefreshUI();
    }

    private void Update()
    {
        if (!IsOwner) return;
        RefreshUI();
    }

    private void RefreshUI()
    {
        if (player == null) return;

        bool isSpace = player.IsSpace();
        bool isEarth = player.IsEarth();

        if (spaceUI != null)
        {
            spaceUI.SetActive(IsOwner && isSpace);
        }

        if (earthUI != null) { 
            earthUI.SetActive(IsOwner && isEarth);
        }
    }
}