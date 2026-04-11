using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    [SerializeField] private MenuSync ms;
    public GameObject mainMenu;
    public GameObject hostUI;
    public GameObject joinUI;

    private void Awake()
    {
        Instance = this;
    }
    public void UnShow()
    {
        ms.SetScreenServerRpc(MenuSync.ScreenType.Main);
    }

    public void ShowScreen(MenuSync.ScreenType screen)
    {
        mainMenu.SetActive(screen == MenuSync.ScreenType.Main);
        hostUI.SetActive(screen == MenuSync.ScreenType.HostLobby);
    }
}
