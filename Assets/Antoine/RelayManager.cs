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

        Debug.Log("Unity Services initialisés et connexion anonyme OK");
    }

    public async void CreateGame()
    {
        try
        {
            int maxConnections = 3; // 3 clients + le host = 4 joueurs au total

            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxConnections);
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            // Correction ici : utiliser la bonne surcharge de SetRelayServerData
            unityTransport.SetRelayServerData(
                allocation.RelayServer.IpV4,
                (ushort)allocation.RelayServer.Port,
                allocation.AllocationIdBytes,
                allocation.Key,
                allocation.ConnectionData,
                allocation.ConnectionData, // Pour l'host, HostConnectionData = ConnectionData
                false
            );

            networkManager.StartHost();

            if (joinCodeText != null)
                joinCodeText.text = "Code : " + joinCode;

            Debug.Log("Partie créée. Code : " + joinCode);
        }
        catch (RelayServiceException e)
        {
            Debug.LogError("Erreur Relay CreateGame : " + e);
        }
    }

    public async void JoinGame()
    {
        try
        {
            string joinCode = joinCodeInput.text.Trim().ToUpper();

            if (string.IsNullOrEmpty(joinCode))
            {
                Debug.LogWarning("Aucun code entré");
                return;
            }

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

            networkManager.StartClient();

            Debug.Log("Connexion à la partie avec le code : " + joinCode);
        }
        catch (RelayServiceException e)
        {
            Debug.LogError("Erreur Relay JoinGame : " + e);
        }
    }
}