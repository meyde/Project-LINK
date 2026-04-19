using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CharacterChoiceUI : MonoBehaviour
{
    private PlayerLobbyData localPlayer;

    [Header("Buttons")]
    [SerializeField] private Button spaceButton;
    [SerializeField] private Button earthButton;
    [SerializeField] private GameObject startButton;

    [SerializeField] private string gameplaySceneName = "GameScene";

    private void Start()
    {
        FindLocalPlayer();

        RefreshUI();

        // Refresh automatique toutes les 0.5 secondes
        // (utile car les NetworkVariables changent en async(Merci les docs))
        InvokeRepeating(nameof(RefreshUI), 0.5f, 0.5f);
    }

    private void FindLocalPlayer()
    {
        var players = FindObjectsByType<PlayerLobbyData>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None
        );

        foreach (var p in players)
        {
            // IsOwner = ce client contrôle cet objet
            if (p.IsOwner)
            {
                localPlayer = p;
                return;
            }
        }
    }

    public void SelectSpace()
    {
        SelectRole(PlayerRole.Space);
    }

    public void SelectEarth()
    {
        SelectRole(PlayerRole.Earth);
    }

    private void SelectRole(PlayerRole role)
    {
        if (localPlayer == null)
            return;

        // Si un autre joueur a déjà ce rôle → on bloque
        if (IsRoleTakenByAnotherPlayer(role))
            return;

        // Envoi du rôle au serveur
        localPlayer.SetRoleServerRpc(role);

        // Dès qu’un rôle est choisi, le joueur est prêt automatiquement
        if (!localPlayer.IsReady.Value)
            localPlayer.SetReadyServerRpc(true);

        // Mise à jour immédiate de l’UI
        RefreshUI();
    }

    private bool IsRoleTakenByAnotherPlayer(PlayerRole role)
    {
        var players = FindObjectsByType<PlayerLobbyData>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None
        );

        foreach (var p in players)
        {
            // On ignore le joueur local (il peut garder son rôle)
            if (p == localPlayer)
                continue;

            if (p.SelectedRole.Value == role)
                return true;
        }

        return false;
    }

    public void OnReadyClicked()
    {
        if (localPlayer == null)
            return;

        bool newState = !localPlayer.IsReady.Value;

        localPlayer.SetReadyServerRpc(newState);

        RefreshUI();
    }

    private void RefreshUI()
    {
        if (localPlayer == null)
            FindLocalPlayer();

        // Vérifie si les rôles sont pris par d'autres
        bool spaceTakenByOther = IsRoleTakenByAnotherPlayer(PlayerRole.Space);
        bool earthTakenByOther = IsRoleTakenByAnotherPlayer(PlayerRole.Earth);

        // Vérifie si le joueur local possède déjà ces rôles
        bool localIsSpace = localPlayer != null && localPlayer.SelectedRole.Value == PlayerRole.Space;
        bool localIsEarth = localPlayer != null && localPlayer.SelectedRole.Value == PlayerRole.Earth;

        // Active/désactive les boutons
        if (spaceButton != null)
            spaceButton.interactable = !spaceTakenByOther || localIsSpace;

        if (earthButton != null)
            earthButton.interactable = !earthTakenByOther || localIsEarth;

        RefreshStartButton();
    }

    private void RefreshStartButton()
    {
        var players = FindObjectsByType<PlayerLobbyData>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None
        );

        bool allReady = true;

        foreach (var p in players)
        {
            // Vérifie qu’un rôle est choisi
            bool hasRole = p.SelectedRole.Value != PlayerRole.None;

            // Si un joueur n’est pas prêt ou sans rôle, on le bloque
            if (!p.IsReady.Value || !hasRole)
            {
                allReady = false;
                break;
            }
        }

        if (startButton == null)
            return;

        // Seul le host peut voir le bouton Start
        if (NetworkManager.Singleton.IsHost)
            startButton.SetActive(allReady);
        else
            startButton.SetActive(false);
    }

    public void OnStartGameClicked()
    {
        if (!NetworkManager.Singleton.IsHost)
            return;

        var players = FindObjectsByType<PlayerLobbyData>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None
        );

        foreach (var p in players)
        {
            p.OnStartGame();
        }

        NetworkManager.Singleton.SceneManager.LoadScene(gameplaySceneName, LoadSceneMode.Single);
    }
}