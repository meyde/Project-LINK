using UnityEngine;

public class Module : MonoBehaviour
{
    [SerializeField] private GameObject diode;
    [SerializeField] private Sprite[] DiodeSprites;
    public virtual void OnStarted()
    {
        return;
    }

    public virtual int ChooseOption()
    {
        int option =Random.Range(0, 3);
        diode.GetComponent<SpriteRenderer>().sprite = DiodeSprites[option];

        return option;
    }
}
