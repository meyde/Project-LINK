using Unity.Netcode;
using TMPro;
using UnityEngine;
using Unity.Collections;

[GenerateSerializationForTypeAttribute(typeof(System.String))]
public class SharedText : NetworkBehaviour
{
    [SerializeField] private TMP_Text text;
    private NetworkVariable<FixedString128Bytes> sharedText = new NetworkVariable<FixedString128Bytes>(
        "",
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    void Awake()
    {
        sharedText.OnValueChanged += OnTextChanged;
        text.text = sharedText.Value.ToString();
    }
    private void OnTextChanged(FixedString128Bytes oldValue, FixedString128Bytes newValue)
    {
        text.text = newValue.ToString();
    }

    public void UpdateText(FixedString128Bytes newText)
    {
        if (IsServer)
        {
            sharedText.Value = newText;
        }
    }
}
