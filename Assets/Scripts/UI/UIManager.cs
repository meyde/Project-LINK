using Unity.Netcode;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    [SerializeField] private MenuSync ms;
    [SerializeField] private RelayManager relayManager;
    public GameObject mainMenu;
    public GameObject hostUI;
    public GameObject joinUI;

    private void Awake()
    {
        Instance = this;
    }
    public void UnShow()
    {
        if (relayManager != null)
        {
            relayManager.CloseHostWindow();
        }

        ShowScreen(MenuSync.ScreenType.Main);
    }

    public void ShowScreen(MenuSync.ScreenType screen)
    {
        mainMenu.SetActive(screen == MenuSync.ScreenType.Main);
        hostUI.SetActive(screen == MenuSync.ScreenType.HostLobby);

        if (joinUI != null)
            joinUI.SetActive(false);
    }
}
