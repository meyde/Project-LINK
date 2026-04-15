using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using System.Collections;

public class LobbyUIManager : MonoBehaviour
{
    public static LobbyUIManager Instance;

    public TMPro.TextMeshProUGUI playerListText;
    public TMPro.TextMeshProUGUI lobbyCodeText;
    public GameObject startButton;

    [SerializeField] private string gameplaySceneName = "CharacterChoice";

    private PlayerLobbyData localPlayer;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        RefreshPlayerList();
    }

    private void OnEnable()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientListChanged;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientListChanged;
        }
    }

    private void OnDisable()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientListChanged;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientListChanged;
        }
    }

    private void OnClientListChanged(ulong clientId)
    {
        StartCoroutine(RefreshPlayerListNextFrame());
    }

    private IEnumerator RefreshPlayerListNextFrame()
    {
        yield return null;
        RefreshPlayerList();
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

        string currentLobbyCode = "";
        string text = "";
        bool allReady = true;

        foreach (var p in players)
        {
            if (string.IsNullOrEmpty(currentLobbyCode) && !string.IsNullOrEmpty(p.LobbyCode.Value.ToString()))
                currentLobbyCode = p.LobbyCode.Value.ToString();

            text += $"{p.Pseudo.Value} - {(p.IsReady.Value ? "Ready" : "Not Ready")}\n";

            if (!p.IsReady.Value)
                allReady = false;
        }

        playerListText.text = text;

        if (lobbyCodeText != null)
            lobbyCodeText.text = currentLobbyCode;

        // Host sees Start button only when all ready
        if (NetworkManager.Singleton.IsHost)
            startButton.SetActive(allReady);
    }

    public void OnStartGameClicked()
    {
        var players = FindObjectsByType<PlayerLobbyData>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (var p in players)
        {
            p.OnStartGame();
        }

        if (!NetworkManager.Singleton.IsHost)
            return;
        
        NetworkManager.Singleton.SceneManager.LoadScene(gameplaySceneName, LoadSceneMode.Single);
    }
}