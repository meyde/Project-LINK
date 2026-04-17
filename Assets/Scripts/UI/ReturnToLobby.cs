using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;
using System.Threading.Tasks;

public class ReturnToLobby : MonoBehaviour
{
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private bool isReturning = false;

    public async void ReturnToMenu()
    {
        if (isReturning)
            return;

        isReturning = true;

        Debug.Log("[Return] Retour au MainMenu");

        // Quitter le vocal si présent
        if (VivoxManager.Instance != null)
        {
            await VivoxManager.Instance.LeaveVoiceAsync();
        }

        // Stop Netcode proprement
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
        {
            NetworkManager.Singleton.Shutdown();
            Debug.Log("[Return] Network shutdown");
        }

        await Task.Yield();

        // Reset global
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Charger le MainMenu
        SceneManager.LoadScene(mainMenuSceneName, LoadSceneMode.Single);
    }
}