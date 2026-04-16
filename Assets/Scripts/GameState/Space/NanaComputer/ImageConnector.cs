using UnityEngine;

public class ImageConnector : MonoBehaviour
{
    [System.Serializable]
    public class ImageLink
    {
        public Transform imageRoot;
        public Transform topPoint;
        public Transform bottomPoint;
    }

    [SerializeField] private ImageLink[] images;

    [Header("Espacement entre images")]
    [SerializeField] private float padding = 0.1f;

    private void Start()
    {
        ConnectImages();
    }

    [ContextMenu("Connect Images")]
    public void ConnectImages()
    {
        if (images == null || images.Length <= 1)
            return;

        for (int i = 1; i < images.Length; i++)
        {
            ImageLink previous = images[i - 1];
            ImageLink current = images[i];

            if (previous == null || current == null ||
                previous.imageRoot == null || previous.topPoint == null || previous.bottomPoint == null ||
                current.imageRoot == null || current.topPoint == null || current.bottomPoint == null)
            {
                Debug.LogWarning($"[ImageConnector] Référence manquante à l'index {i}.");
                continue;
            }

            Vector3 previousBottom = previous.bottomPoint.position;
            Vector3 currentTop = current.topPoint.position;

            Vector3 downDirection = (previous.bottomPoint.position - previous.topPoint.position).normalized;
            Vector3 targetTopPosition = previousBottom + downDirection * padding;

            Vector3 offset = currentTop - current.imageRoot.position;
            current.imageRoot.position = targetTopPosition - offset;
        }
    }
}