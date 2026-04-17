using System.Collections.Generic;
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

    [Header("Espacement entre images")]
    [SerializeField] private float padding = 0.1f;

    [Header("Scroller lié")]
    [SerializeField] private WorldImageScroller linkedScroller;
    [SerializeField] private bool snapToTopAfterConnect = true;

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = false;

    [SerializeField] private List<ImageLink> images = new List<ImageLink>();

    private void Awake()
    {
        if (linkedScroller == null)
            linkedScroller = GetComponentInParent<WorldImageScroller>(true);
    }

    private void Start()
    {
        ConnectImages();
    }

    [ContextMenu("Rebuild From Hierarchy")]
    public void RebuildFromHierarchy()
    {
        images.Clear();

        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);

            Transform topPoint = child.Find("TopPoint");
            Transform bottomPoint = child.Find("BottomPoint");

            if (topPoint == null || bottomPoint == null)
            {
                if (showDebugLogs)
                    Debug.LogWarning($"[ImageConnector] {child.name} ignoré : TopPoint ou BottomPoint manquant.");
                continue;
            }

            images.Add(new ImageLink
            {
                imageRoot = child,
                topPoint = topPoint,
                bottomPoint = bottomPoint
            });
        }

        if (showDebugLogs)
            Debug.Log($"[ImageConnector] {images.Count} image(s) récupérées depuis la hiérarchie.");
    }

    [ContextMenu("Connect Images")]
    public void ConnectImages()
    {
        RebuildFromHierarchy();

        if (images == null || images.Count <= 1)
        {
            RefreshScroller();
            return;
        }

        for (int i = 1; i < images.Count; i++)
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

            if (showDebugLogs)
                Debug.Log($"[ImageConnector] {previous.imageRoot.name} -> {current.imageRoot.name}");
        }

        RefreshScroller();
    }

    private void RefreshScroller()
    {
        if (linkedScroller == null)
            linkedScroller = GetComponentInParent<WorldImageScroller>(true);

        if (linkedScroller == null)
        {
            Debug.LogWarning("[ImageConnector] Aucun WorldImageScroller trouvé.");
            return;
        }

        linkedScroller.RecalculateBounds();

        if (snapToTopAfterConnect)
            linkedScroller.SnapToTop();
    }
}