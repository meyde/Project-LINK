using UnityEngine;

public class ColorPickerTecPlate : MonoBehaviour , MouseInteractionManager.IInteractable 
{
    [SerializeField] TectonicPlatesModule tm;
    [SerializeField] int colorToSet;
    public void OnClick()
    {
        tm.colorState = colorToSet;
        tm.Randomizer();
    }
    
    public void OnHoverEnter()
    {

    }

    public void OnHoverExit()
    {

    }


}
