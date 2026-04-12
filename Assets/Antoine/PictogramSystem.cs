using Unity.Netcode;
using UnityEngine;

public class PictogramSystem : NetworkBehaviour
{
    [System.Serializable]
    public class PictogramPair
    {
        public SpriteRenderer spaceRenderer;
        public SpriteRenderer earthRenderer;
    }

    [Header("Paires liées")]
    [SerializeField] private PictogramPair[] pictogramPairs;

    public void TrySendPictogram(SpriteRenderer clickedSpaceRenderer)
    {
        Debug.Log($"[TRY SEND] Client local = {NetworkManager.Singleton.LocalClientId}");

        if (!IsSpawned)
        {
            Debug.LogError("[TRY SEND] STOP -> PictogramSystem NetworkObject non spawn");
            return;
        }

        if (clickedSpaceRenderer == null)
        {
            Debug.LogError("[TRY SEND] STOP -> clickedSpaceRenderer est null");
            return;
        }

        if (!LocalPlayerIsSpace())
        {
            Debug.LogWarning("[TRY SEND] STOP -> Le joueur local n'est pas Space");
            return;
        }

        int pairIndex = GetPairIndexFromSpaceRenderer(clickedSpaceRenderer);
        if (pairIndex < 0)
        {
            Debug.LogError($"[TRY SEND] STOP -> Aucun pairIndex trouvé pour {clickedSpaceRenderer.name}");
            return;
        }

        Debug.Log($"[TRY SEND] OK -> {clickedSpaceRenderer.name} correspond à pairIndex = {pairIndex}");
        RequestPictogramChangeRpc(pairIndex);
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void RequestPictogramChangeRpc(int pairIndex, RpcParams rpcParams = default)
    {
        ulong senderClientId = rpcParams.Receive.SenderClientId;

        Debug.Log($"[SERVER RPC] sender = {senderClientId} | pairIndex = {pairIndex}");

        if (!ClientIsSpace(senderClientId))
        {
            Debug.LogWarning("[SERVER RPC] STOP -> Le client émetteur n'est pas Space");
            return;
        }

        if (!IsValidPairIndex(pairIndex))
        {
            Debug.LogWarning("[SERVER RPC] STOP -> pairIndex invalide : " + pairIndex);
            return;
        }

        Color syncedColor = GetRandomColor();

        ApplyColorLocal(pairIndex, syncedColor);
        ApplyPictogramColorClientRpc(pairIndex, syncedColor);

        Debug.Log($"[SERVER RPC] Couleur envoyée pour pairIndex {pairIndex} : {syncedColor}");
    }

    [Rpc(SendTo.NotServer)]
    private void ApplyPictogramColorClientRpc(int pairIndex, Color syncedColor)
    {
        Debug.Log($"[CLIENT RPC] localClientId = {NetworkManager.Singleton.LocalClientId} | pairIndex = {pairIndex}");
        ApplyColorLocal(pairIndex, syncedColor);
    }

    private void ApplyColorLocal(int pairIndex, Color syncedColor)
    {
        if (!IsValidPairIndex(pairIndex))
        {
            Debug.LogWarning("[APPLY] pairIndex invalide : " + pairIndex);
            return;
        }

        PictogramPair pair = pictogramPairs[pairIndex];
        if (pair == null)
        {
            Debug.LogWarning("[APPLY] Paire null à l'index " + pairIndex);
            return;
        }

        if (pair.spaceRenderer != null)
        {
            pair.spaceRenderer.color = syncedColor;
            Debug.Log($"[APPLY] Space mis à jour : {pair.spaceRenderer.name}");
        }
        else
        {
            Debug.LogWarning("[APPLY] spaceRenderer null pour pairIndex " + pairIndex);
        }

        if (pair.earthRenderer != null)
        {
            pair.earthRenderer.color = syncedColor;
            Debug.Log($"[APPLY] Earth mis à jour : {pair.earthRenderer.name}");
        }
        else
        {
            Debug.LogWarning("[APPLY] earthRenderer null pour pairIndex " + pairIndex);
        }
    }

    private int GetPairIndexFromSpaceRenderer(SpriteRenderer clickedSpaceRenderer)
    {
        if (clickedSpaceRenderer == null || pictogramPairs == null)
            return -1;

        for (int i = 0; i < pictogramPairs.Length; i++)
        {
            PictogramPair pair = pictogramPairs[i];
            if (pair == null)
                continue;

            if (pair.spaceRenderer == clickedSpaceRenderer)
                return i;
        }

        return -1;
    }

    private bool IsValidPairIndex(int pairIndex)
    {
        return pictogramPairs != null
            && pictogramPairs.Length > 0
            && pairIndex >= 0
            && pairIndex < pictogramPairs.Length;
    }

    private bool LocalPlayerIsSpace()
    {
        if (NetworkManager.Singleton == null)
        {
            Debug.LogError("[ROLE CHECK] NetworkManager.Singleton est null");
            return false;
        }

        if (NetworkManager.Singleton.LocalClient == null)
        {
            Debug.LogError("[ROLE CHECK] LocalClient est null");
            return false;
        }

        NetworkObject playerObject = NetworkManager.Singleton.LocalClient.PlayerObject;
        if (playerObject == null)
        {
            Debug.LogError("[ROLE CHECK] LocalClient.PlayerObject est null");
            return false;
        }

        PlayerLobbyData lobbyData = playerObject.GetComponent<PlayerLobbyData>();
        if (lobbyData == null)
        {
            Debug.LogError("[ROLE CHECK] Aucun PlayerLobbyData sur le PlayerObject local : " + playerObject.name);
            return false;
        }

        Debug.Log($"[ROLE CHECK] Local player = {playerObject.name} | Role = {lobbyData.SelectedRole.Value}");
        return lobbyData.SelectedRole.Value == PlayerRole.Space;
    }

    private bool ClientIsSpace(ulong clientId)
    {
        if (NetworkManager.Singleton == null)
        {
            Debug.LogError("[ROLE CHECK SERVER] NetworkManager.Singleton est null");
            return false;
        }

        foreach (NetworkObject netObj in NetworkManager.Singleton.SpawnManager.SpawnedObjectsList)
        {
            if (netObj.OwnerClientId != clientId)
                continue;

            PlayerLobbyData lobbyData = netObj.GetComponent<PlayerLobbyData>();
            if (lobbyData != null)
            {
                Debug.Log($"[ROLE CHECK SERVER] Client {clientId} | Object = {netObj.name} | Role = {lobbyData.SelectedRole.Value}");
                return lobbyData.SelectedRole.Value == PlayerRole.Space;
            }
        }

        Debug.LogError("[ROLE CHECK SERVER] Aucun PlayerLobbyData trouvé pour le client " + clientId);
        return false;
    }

    private Color GetRandomColor()
    {
        return new Color(
            Random.Range(0.25f, 1f),
            Random.Range(0.25f, 1f),
            Random.Range(0.25f, 1f),
            1f
        );
    }
}