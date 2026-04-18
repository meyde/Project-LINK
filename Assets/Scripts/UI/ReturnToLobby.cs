using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;

public class ReturnToLobby : MonoBehaviour
{
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private bool isReturning = false;

    public void ReturnToMenu()
    {
        if (isReturning)
            return;

        StartCoroutine(ReturnToMenuRoutine());
    }

    private IEnumerator ReturnToMenuRoutine()
    {
        isReturning = true;

        Debug.Log("[Return] Retour au MainMenu");

        // Quitter le vocal proprement
        if (VivoxManager.Instance != null)
        {
            Task leaveTask = null;

            try
            {
                leaveTask = VivoxManager.Instance.LeaveVoiceAsync();
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[Return] Erreur au lancement de LeaveVoiceAsync : {e}");
            }

            if (leaveTask != null)
            {
                while (!leaveTask.IsCompleted)
                    yield return null;

                if (leaveTask.IsFaulted)
                    Debug.LogWarning($"[Return] LeaveVoiceAsync a échoué : {leaveTask.Exception}");
            }
        }

        // Stop Netcode
        if (NetworkManager.Singleton != null)
        {
            Debug.Log("[Return] Shutdown réseau...");

            if (NetworkManager.Singleton.IsListening)
            {
                NetworkManager.Singleton.Shutdown();

                while (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
                    yield return null;
            }

            GameObject networkManagerObject = NetworkManager.Singleton.gameObject;
            Destroy(networkManagerObject);

            // Attendre la destruction effective
            yield return null;
        }

        // Attendre une frame pour laisser le shutdown se faire
        yield return null;

        // Reset global
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Debug.Log($"[Return] Chargement de la scène {mainMenuSceneName}");

        // Charger le menu
        SceneManager.LoadScene(mainMenuSceneName, LoadSceneMode.Single);

        isReturning = false;
    }
}