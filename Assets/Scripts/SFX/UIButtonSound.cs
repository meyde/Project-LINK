using UnityEngine;
using UnityEngine.EventSystems;

public class UIButtonSound : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    [Header("Options")]
    [SerializeField] private bool playHoverSound = true;
    [SerializeField] private bool playClickSound = true;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!playHoverSound)
            return;

        if (UISoundManager.Instance != null)
            UISoundManager.Instance.PlayHover();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!playClickSound)
            return;

        if (UISoundManager.Instance != null)
            UISoundManager.Instance.PlayClick();
    }
}