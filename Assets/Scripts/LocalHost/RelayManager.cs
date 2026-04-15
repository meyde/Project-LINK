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

    public async void CreateGame()
    {
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
    }

    public async void JoinGame()
    {
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
    }

    public async void LeaveGame()
    {
        // Quitter le channel vocal avant de fermer la partie
        if (VivoxManager.Instance != null)
            await VivoxManager.Instance.LeaveVoiceAsync();

        if (networkManager != null && networkManager.IsListening)
        {
            // Arrêter le réseau et fermer la partie
            networkManager.Shutdown();
            Debug.Log("[Relay] Partie fermée.");
        }
    }
}