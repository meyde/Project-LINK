using System.Collections;
using UnityEngine;
using TMPro;

public class TypewriterText : MonoBehaviour
{
    public TextMeshProUGUI textUI;
    public float typingSpeed = 0.05f;

    private Coroutine typingCoroutine;

    public void StartTyping(string message)
    {
        if (!gameObject.activeInHierarchy)
        {
            Debug.LogWarning($"{name} est inactif, impossible de lancer le texte.");
            return;
        }

        if (textUI == null)
        {
            Debug.LogError("textUI n'est pas assigné dans TypewriterText.");
            return;
        }

        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine = StartCoroutine(TypeText(message));
    }

    private IEnumerator TypeText(string message)
    {
        textUI.text = "";

        foreach (char letter in message)
        {
            textUI.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }
    }
}