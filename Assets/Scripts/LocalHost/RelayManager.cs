using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

using Unity.Netcode;
using Unity.Netcode.Transports.UTP;

using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;

public class RelayManager : MonoBehaviour
{
    [Header("Network")]
    [SerializeField] private NetworkManager networkManager;
    [SerializeField] private UnityTransport unityTransport;

    [Header("UI")]
    [SerializeField] private TMP_Text joinCodeText;
    [SerializeField] private TMP_InputField joinCodeInput;
    [SerializeField] private Button hostButton;
    [SerializeField] private Button joinButton;

    private bool isCreatingGame = false;
    private bool isLeavingGame = false;

    private string currentJoinCode = "";
    private bool hostSessionActive = false;

    private async void Start()
    {
        await InitializeUnityServices();

        if (hostButton != null)
            hostButton.onClick.AddListener(() => CreateGame());

        if (joinButton != null)
            joinButton.onClick.AddListener(() => JoinGame());
    }

    private async Task InitializeUnityServices()
    {
        if (UnityServices.State != ServicesInitializationState.Initialized)
        {
            await UnityServices.InitializeAsync();
        }

        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }

        Debug.Log("[Relay] Unity Services initialisés et connexion anonyme OK");
    }

    public async void CloseHostWindow()
    {
        // Quitter le channel vocal sans fermer la partie host
        if (VivoxManager.Instance != null)
            await VivoxManager.Instance.LeaveVoiceAsync();

        if (joinCodeText != null)
            joinCodeText.text = currentJoinCode;
    }

    public async void CreateGame()
    {
        if (isCreatingGame || isLeavingGame)
            return;

        if (networkManager == null || unityTransport == null)
        {
            Debug.LogError("[Relay] NetworkManager ou UnityTransport manquant.");
            return;
        }

        // Si on est déjà host, on ne recrée pas la partie, on réaffiche juste le code existant.
        if (networkManager.IsHost && networkManager.IsListening && hostSessionActive)
        {
            if (joinCodeText != null)
                joinCodeText.text = currentJoinCode;

            if (UIManager.Instance != null)
                UIManager.Instance.ShowScreen(MenuSync.ScreenType.HostLobby);

            Debug.Log("[Relay] Partie host déjà active, réouverture avec le code : " + currentJoinCode);

            //Vérifier que le VivoxManager est présent avant de tenter de rejoindre le channel vocal
            if (VivoxManager.Instance == null)
            {
                Debug.LogError("[Relay] Aucun VivoxManager trouvé dans la scène.");
                return;
            }

            // Rejoindre le channel vocal avec le même code que la partie
            await VivoxManager.Instance.JoinVoiceFromCodeAsync(currentJoinCode);
            return;
        }

        if (networkManager.IsListening)
        {
            Debug.LogWarning("[Relay] Une session réseau est déjà active.");
            return;
        }

        isCreatingGame = true;

        try
        {
            int maxConnections = 1; // 1 client + le host = 2 joueurs au total

            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxConnections);
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            unityTransport.SetRelayServerData(
                allocation.RelayServer.IpV4,
                (ushort)allocation.RelayServer.Port,
                allocation.AllocationIdBytes,
                allocation.Key,
                allocation.ConnectionData,
                allocation.ConnectionData, // HostConnectionData = ConnectionData pour l'host
                false
            );

            bool started = networkManager.StartHost();

            if (!started)
            {
                Debug.LogError("[Relay] Échec du démarrage host.");
                return;
            }

            currentJoinCode = joinCode;
            hostSessionActive = true;

            var players = FindObjectsByType<PlayerLobbyData>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            foreach (var p in players)
            {
                if (p.OwnerClientId == NetworkManager.Singleton.LocalClientId)
                {
                    p.LobbyCode.Value = joinCode;
                    break;
                }
            }

            if (joinCodeText != null)
                joinCodeText.text = joinCode;

            Debug.Log("[Relay] Partie créée. Code : " + joinCode);

            //Vérifier que le VivoxManager est présent avant de tenter de rejoindre le channel vocal
            if (VivoxManager.Instance == null)
            {
                Debug.LogError("[Relay] Aucun VivoxManager trouvé dans la scène.");
                return;
            }

            // Rejoindre le channel vocal avec le même code que la partie
            await VivoxManager.Instance.JoinVoiceFromCodeAsync(joinCode);
        }
        catch (RelayServiceException e)
        {
            Debug.LogError("[Relay] Erreur Relay CreateGame : " + e);
        }
        finally
        {
            isCreatingGame = false;
        }
    }

    public async void JoinGame()
    {
        if (isCreatingGame || isLeavingGame)
            return;

        if (networkManager == null || unityTransport == null)
        {
            Debug.LogError("[Relay] NetworkManager ou UnityTransport manquant.");
            return;
        }

        isCreatingGame = true;

        try
        {
            string joinCode = joinCodeInput != null
                ? joinCodeInput.text.Trim().ToUpperInvariant()
                : string.Empty;

            if (string.IsNullOrEmpty(joinCode))
            {
                Debug.LogWarning("[Relay] Aucun code entré");
                return;
            }

            // Si on a déjà une session active (host/client), on la ferme complètement avant de rejoindre une autre partie.
            if (networkManager.IsListening)
            {
                Debug.Log("[Relay] Une session est déjà active, fermeture avant la reconnexion...");
                LeaveGame();

                while (isLeavingGame)
                    await Task.Yield();
            }

            // Rejoindre l'allocation Relay avec le code entré
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

            unityTransport.SetRelayServerData(
                joinAllocation.RelayServer.IpV4,
                (ushort)joinAllocation.RelayServer.Port,
                joinAllocation.AllocationIdBytes,
                joinAllocation.Key,
                joinAllocation.ConnectionData,
                joinAllocation.HostConnectionData,
                false
            );

            bool started = networkManager.StartClient();

            if (!started)
            {
                Debug.LogError("[Relay] Échec du démarrage client.");
                return;
            }

            hostSessionActive = false;
            currentJoinCode = "";

            Debug.Log("[Relay] Connexion à la partie avec le code : " + joinCode);

            if (VivoxManager.Instance == null)
            {
                Debug.LogError("[Relay] Aucun VivoxManager trouvé dans la scène.");
                return;
            }

            await VivoxManager.Instance.JoinVoiceFromCodeAsync(joinCode);
        }
        catch (RelayServiceException e)
        {
            Debug.LogError("[Relay] Erreur Relay JoinGame : " + e);
        }
        finally
        {
            isCreatingGame = false;
        }
    }

    public async void LeaveGame()
    {
        if (isLeavingGame)
            return;

        isLeavingGame = true;

        try
        {
            // Quitter le channel vocal avant de fermer la partie
            if (VivoxManager.Instance != null)
                await VivoxManager.Instance.LeaveVoiceAsync();

            if (networkManager != null && networkManager.IsListening)
            {
                // Vider le code côté réseau AVANT le shutdown
                var players = FindObjectsByType<PlayerLobbyData>(FindObjectsInactive.Include, FindObjectsSortMode.None);

                foreach (var p in players)
                {
                    if (p.OwnerClientId == NetworkManager.Singleton.LocalClientId)
                    {
                        p.LobbyCode.Value = "";
                        break;
                    }
                }

                // Arrêter le réseau et fermer la partie
                networkManager.Shutdown();
                Debug.Log("[Relay] Partie fermée.");
            }

            await Task.Yield();

            currentJoinCode = "";
            hostSessionActive = false;

            if (joinCodeText != null)
                joinCodeText.text = "";

            if (joinCodeInput != null)
                joinCodeInput.text = "";
        }
        finally
        {
            isLeavingGame = false;
        }
    }
}