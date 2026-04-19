using UnityEngine;

public class ModuleCloseOverlay : MonoBehaviour
{
    public void CloseCurrentModule()
    {
        ZoomableModule.CloseCurrentOpenModule();
    }
}