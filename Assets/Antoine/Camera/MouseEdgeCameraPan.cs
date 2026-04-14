using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Camera))]
public class MouseEdgeCameraPan : MonoBehaviour
{
    [Header("Défilement horizontal")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float smoothTime = 0.08f;

    [Header("Zone de détection des bords")]
    [Tooltip("Pourcentage de l'écran utilisé comme zone active sur les côtés. 0.05 = 5%")]
    [Range(0.01f, 0.3f)]
    [SerializeField] private float edgePercent = 0.05f;

    [Header("Limites")]
    [Tooltip("Position X centrale de départ de la caméra")]
    [SerializeField] private float baseCenterX = 0f;

    [Tooltip("Moitié d'écran supplémentaire à gauche et à droite = 0.5")]
    [SerializeField] private float extraScreenHalfEachSide = 0.5f;

    private Camera cam;
    private Vector3 velocity;

    private float minX;
    private float maxX;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        RecalculateLimits();
    }

    private void Update()
    {
        if (Mouse.current == null)
            return;

        Vector3 targetPosition = transform.position;
        Vector2 mousePos = Mouse.current.position.ReadValue();

        float leftEdge = Screen.width * edgePercent;
        float rightEdge = Screen.width * (1f - edgePercent);

        float moveX = 0f;

        if (mousePos.x <= leftEdge)
        {
            float t = 1f - Mathf.Clamp01(mousePos.x / leftEdge);
            moveX = -t;
        }
        else if (mousePos.x >= rightEdge)
        {
            float t = Mathf.Clamp01((mousePos.x - rightEdge) / (Screen.width - rightEdge));
            moveX = t;
        }

        targetPosition.x += moveX * moveSpeed * Time.deltaTime;
        targetPosition.x = Mathf.Clamp(targetPosition.x, minX, maxX);

        transform.position = Vector3.SmoothDamp(
            transform.position,
            targetPosition,
            ref velocity,
            smoothTime
        );
    }

    private void OnValidate()
    {
        if (cam == null)
            cam = GetComponent<Camera>();

        if (cam != null)
            RecalculateLimits();
    }

    private void RecalculateLimits()
    {
        if (cam == null)
            return;

        float visibleHeight = cam.orthographicSize * 2f;
        float visibleWidth = visibleHeight * cam.aspect;

        float horizontalOffset = visibleWidth * extraScreenHalfEachSide;

        minX = baseCenterX - horizontalOffset;
        maxX = baseCenterX + horizontalOffset;
    }

    public void SetBaseCenter(float newBaseCenterX)
    {
        baseCenterX = newBaseCenterX;
        RecalculateLimits();
    }
}