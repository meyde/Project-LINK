using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Editor;

public class movement : NetworkBehaviour
{
    public squarecolor square;

    
    void Awake()
    {
        square = FindFirstObjectByType<squarecolor>();
    }

    // Update is called once per frame
    void Update()
    {
        if (IsOwner)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                var newColor = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
                square.squareColor.Value = newColor;
                print("pong");
            }

            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                transform.position = new Vector3(transform.position.x, transform.position.y + 1, 0);
            }
            ;
        }

    }
}
