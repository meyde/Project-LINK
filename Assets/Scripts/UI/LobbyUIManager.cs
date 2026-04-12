using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class LobbyUIManager : MonoBehaviour
{
    public static LobbyUIManager Instance;

    public TMPro.TextMeshProUGUI playerListText;
    public GameObject startButton;

    [SerializeField] private string gameplaySceneName = "CharacterChoice";

    private PlayerLobbyData localPlayer;

    private void Awake()
    {
        Instance = this;
    }

    public void BindLocalPlayer(PlayerLobbyData player)
    {
        localPlayer = player;
    }

    public void OnPseudoChanged(string newPseudo)
    {
        localPlayer.SetPseudoServerRpc(newPseudo);
    }

    public void OnReadyClicked()
    {
        bool newState = !localPlayer.IsReady.Value;
        localPlayer.SetReadyServerRpc(newState);
    }

    public void RefreshPlayerList()
    {
        var players = FindObjectsByType<PlayerLobbyData>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        string text = "";
        bool allReady = true;

        foreach (var p in players)
        {
            text += $"{p.Pseudo.Value} - {(p.IsReady.Value ? "Ready" : "Not Ready")}\n";

            if (!p.IsReady.Value)
                allReady = false;
        }

        playerListText.text = text;

        // Host sees Start button only when all ready
        if (NetworkManager.Singleton.IsHost)
            startButton.SetActive(allReady);
    }

    public void OnStartGameClicked()
    {
        if (!NetworkManager.Singleton.IsHost)
            return;
        
        NetworkManager.Singleton.SceneManager.LoadScene(gameplaySceneName, LoadSceneMode.Single);
    }
}