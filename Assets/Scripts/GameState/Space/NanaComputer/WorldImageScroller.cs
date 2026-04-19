using UnityEngine;
using UnityEngine.InputSystem;

public class WorldImageScroller : MonoBehaviour
{
    [Header("Références")]
    [SerializeField] private Camera targetCamera;
    [SerializeField] private Transform contentToScroll;
    [SerializeField] private Collider2D interactionArea;

    [Header("Debug")]
    [SerializeField] private bool showDebug = false;

    [Header("Position de départ")]
    [SerializeField] private bool resetToTopOnEnable = true;

    [Header("Scroll molette")]
    [SerializeField] private float wheelScrollSpeed = 0.01f;

    [Header("Drag souris")]
    [SerializeField] private float dragSensitivity = 1f;

    [Header("Sens")]
    [SerializeField] private bool invertWheel = false;
    [SerializeField] private bool invertDrag = false;

    [Header("Bords externes")]
    [SerializeField] private float topOuterPadding = 0.25f;
    [SerializeField] private float bottomOuterPadding = 0.25f;

    private bool isDragging = false;
    private Vector2 lastMouseWorldPos;

    private float minLocalY;
    private float maxLocalY;

    private bool basePositionSaved = false;
    private Vector3 baseLocalPosition;

    private void Awake()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;

        SaveBaseLocalPositionIfNeeded();
    }

    private void Start()
    {
        SaveBaseLocalPositionIfNeeded();
        RecalculateBounds();

        if (resetToTopOnEnable)
            SnapToTop();
    }

    private void OnEnable()
    {
        SaveBaseLocalPositionIfNeeded();
        RecalculateBounds();

        if (resetToTopOnEnable)
            SnapToTop();
    }

    private void Update()
    {
        if (targetCamera == null || contentToScroll == null || interactionArea == null || Mouse.current == null)
            return;

        Vector2 mouseWorldPos = targetCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue());

        HandleWheelScroll(mouseWorldPos);
        HandleDrag(mouseWorldPos);
    }

    private void OnDisable()
    {
        isDragging = false;
        lastMouseWorldPos = Vector2.zero;
    }

    public void RefreshScroll(bool snapToTop = true)
    {
        if (contentToScroll == null)
            return;

        isDragging = false;
        RecalculateBounds();

        if (snapToTop)
            SnapToTop();
    }

    private void SaveBaseLocalPositionIfNeeded()
    {
        if (contentToScroll == null || basePositionSaved)
            return;

        baseLocalPosition = contentToScroll.localPosition;
        basePositionSaved = true;
    }

    private void HandleWheelScroll(Vector2 mouseWorldPos)
    {
        if (!interactionArea.OverlapPoint(mouseWorldPos))
            return;

        float wheelDelta = Mouse.current.scroll.ReadValue().y;

        if (Mathf.Abs(wheelDelta) < 0.01f)
            return;

        if (invertWheel)
            wheelDelta = -wheelDelta;

        MoveContent(wheelDelta * wheelScrollSpeed);
    }

    private void HandleDrag(Vector2 mouseWorldPos)
    {
        bool isPressed = Mouse.current.leftButton.isPressed;
        bool pressedThisFrame = Mouse.current.leftButton.wasPressedThisFrame;
        bool releasedThisFrame = Mouse.current.leftButton.wasReleasedThisFrame;

        if (pressedThisFrame && interactionArea.OverlapPoint(mouseWorldPos))
        {
            isDragging = true;
            lastMouseWorldPos = mouseWorldPos;
        }

        if (releasedThisFrame)
            isDragging = false;

        if (!isDragging || !isPressed)
            return;

        float deltaY = mouseWorldPos.y - lastMouseWorldPos.y;

        if (invertDrag)
            deltaY = -deltaY;

        MoveContent(deltaY * dragSensitivity);
        lastMouseWorldPos = mouseWorldPos;
    }

    private void MoveContent(float delta)
    {
        Vector3 localPos = contentToScroll.localPosition;
        localPos.y += delta;
        localPos.y = Mathf.Clamp(localPos.y, minLocalY, maxLocalY);
        contentToScroll.localPosition = localPos;
    }

    public void SnapToTop()
    {
        if (contentToScroll == null)
            return;

        Vector3 pos = contentToScroll.localPosition;
        pos.y = minLocalY;
        contentToScroll.localPosition = pos;
    }

    public void SnapToBottom()
    {
        if (contentToScroll == null)
            return;

        Vector3 pos = contentToScroll.localPosition;
        pos.y = maxLocalY;
        contentToScroll.localPosition = pos;
    }

    public void ResetToBasePosition()
    {
        if (contentToScroll == null)
            return;

        contentToScroll.localPosition = baseLocalPosition;
    }

    [ContextMenu("Recalculate Bounds")]
    [ContextMenu("Recalculate Bounds")]
    public void RecalculateBounds()
    {
        if (contentToScroll == null || interactionArea == null)
        {
            Debug.LogWarning("[WorldImageScroller] contentToScroll ou interactionArea manquant.");
            return;
        }

        SpriteRenderer[] renderers = contentToScroll.GetComponentsInChildren<SpriteRenderer>();

        if (renderers.Length == 0)
        {
            Debug.LogWarning("[WorldImageScroller] Aucun SpriteRenderer trouvé dans le contenu.");
            return;
        }

        float contentMinY = float.MaxValue;
        float contentMaxY = float.MinValue;

        for (int i = 0; i < renderers.Length; i++)
        {
            Bounds b = renderers[i].bounds;
            contentMinY = Mathf.Min(contentMinY, b.min.y);
            contentMaxY = Mathf.Max(contentMaxY, b.max.y);
        }

        Bounds areaBounds = interactionArea.bounds;
        float areaMinY = areaBounds.min.y;
        float areaMaxY = areaBounds.max.y;

        // Position du contenu en world au repos
        float baseWorldY = contentToScroll.parent != null
            ? contentToScroll.parent.TransformPoint(baseLocalPosition).y
            : baseLocalPosition.y;

        // Limite haute : le haut du contenu arrive juste au haut de la zone visible
        float minWorldY = baseWorldY + (areaMaxY - contentMaxY) - topOuterPadding;

        // Limite basse : le bas du contenu arrive juste au bas de la zone visible
        float maxWorldY = baseWorldY + (areaMinY - contentMinY) + bottomOuterPadding;

        // Conversion world -> local
        if (contentToScroll.parent != null)
        {
            minLocalY = contentToScroll.parent.InverseTransformPoint(
                new Vector3(0f, minWorldY, 0f)
            ).y;

            maxLocalY = contentToScroll.parent.InverseTransformPoint(
                new Vector3(0f, maxWorldY, 0f)
            ).y;
        }
        else
        {
            minLocalY = minWorldY;
            maxLocalY = maxWorldY;
        }

        if (minLocalY > maxLocalY)
        {
            float temp = minLocalY;
            minLocalY = maxLocalY;
            maxLocalY = temp;
        }

        if (showDebug)
        {
            Debug.Log(
                $"[WorldImageScroller] contentMinY={contentMinY}, contentMaxY={contentMaxY}, areaMinY={areaMinY}, areaMaxY={areaMaxY}, minLocalY={minLocalY}, maxLocalY={maxLocalY}"
            );
        }
    }
}