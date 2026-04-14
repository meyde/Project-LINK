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

    [Header("Limites locales Y")]
    [Tooltip("Position qui affiche le HAUT de l'image en premier.")]
    [SerializeField] private float topY = -0.75f;

    [Tooltip("Position qui affiche le BAS de l'image.")]
    [SerializeField] private float bottomY = 5f;

    [Header("Sens")]
    [SerializeField] private bool invertWheel = false;
    [SerializeField] private bool invertDrag = false;

    private bool isDragging = false;
    private Vector2 lastMouseWorldPos;

    private void Awake()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;

        ValidateBounds();

        if (resetToTopOnEnable)
            SnapToTop();
    }

    private void OnEnable()
    {
        ValidateBounds();

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

        if (showDebug)
            Debug.Log($"Local Y = {contentToScroll.localPosition.y} | TopY = {topY} | BottomY = {bottomY}");
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

        localPos.y = Mathf.Clamp(localPos.y, topY, bottomY);

        contentToScroll.localPosition = localPos;
    }

    public void SnapToTop()
    {
        if (contentToScroll == null)
            return;

        Vector3 pos = contentToScroll.localPosition;
        pos.y = topY;
        contentToScroll.localPosition = pos;
    }

    public void SnapToBottom()
    {
        if (contentToScroll == null)
            return;

        Vector3 pos = contentToScroll.localPosition;
        pos.y = bottomY;
        contentToScroll.localPosition = pos;
    }

    private void ValidateBounds()
    {
        if (topY > bottomY)
        {
            float temp = topY;
            topY = bottomY;
            bottomY = temp;
        }
    }
}