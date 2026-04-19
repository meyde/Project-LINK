using UnityEngine;

public class FanController : MonoBehaviour
{
    [SerializeField] private int intensity;
    private int turnSpeed;
    private int turnDirection;
    private int rotation;

    public void Update()
    {
        gameObject.transform.Rotate(0f, 0f, rotation);
    }


    public void Refreshrotation(int newSpeed, int newDirection)
    {
        turnSpeed = newSpeed;
        turnDirection = newDirection;
        rotation = intensity * turnSpeed * (turnDirection - 1);
    }

}
