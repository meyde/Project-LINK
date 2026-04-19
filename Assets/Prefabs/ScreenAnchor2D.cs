using UnityEngine;

public class ScreenAnchor2D : MonoBehaviour
{
    public Camera targetCamera;
    public Vector2 viewportPosition = new Vector2(0.5f, 0.9f);
    // (0.5,0.9) = haut centre

    public float distanceFromCamera = 10f;

    private void LateUpdate()
    {
        if (targetCamera == null) return;

        Vector3 worldPos = targetCamera.ViewportToWorldPoint(
            new Vector3(viewportPosition.x, viewportPosition.y, distanceFromCamera)
        );

        worldPos.z = 0f; // important en 2D
        transform.position = worldPos;
    }
}