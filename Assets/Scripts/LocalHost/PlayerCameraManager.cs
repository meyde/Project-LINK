using UnityEngine;
using Unity.Netcode;

public class PlayerCameraManager : NetworkBehaviour
{
    [Header("Cameras")]
    [SerializeField] private Camera cameraPlayer1;
    [SerializeField] private Camera cameraPlayer2;

    [Header("Objects To Toggle")]
    [SerializeField] private GameObject objectForSpacePlayer;
    [SerializeField] private GameObject objectForEarthPlayer;

    private void Start()
    {
        SetupCamera();
    }

    private void SetupCamera()
    {
        if (!IsClient)
            return;

        if (NetworkManager.Singleton == null)
        {
            Debug.LogError("[CameraManager] NetworkManager null");
            return;
        }

        var playerObject = NetworkManager.Singleton.LocalClient.PlayerObject;
        if (playerObject == null)
        {
            Debug.LogError("[CameraManager] PlayerObject null");
            return;
        }

        PlayerLobbyData lobbyData = playerObject.GetComponent<PlayerLobbyData>();
        if (lobbyData == null)
        {
            Debug.LogError("[CameraManager] PlayerLobbyData manquant");
            return;
        }

        var role = lobbyData.SelectedRole.Value;

        Debug.Log($"[CameraManager] Role local = {role}");

        if (role == PlayerRole.Space)
        {
            cameraPlayer1.gameObject.SetActive(true);
            cameraPlayer2.gameObject.SetActive(false);

            objectForSpacePlayer.SetActive(true);
            objectForEarthPlayer.SetActive(false);
        }
        else
        {
            cameraPlayer1.gameObject.SetActive(false);
            cameraPlayer2.gameObject.SetActive(true);

            objectForSpacePlayer.SetActive(false);
            objectForEarthPlayer.SetActive(true);
        }
    }
}