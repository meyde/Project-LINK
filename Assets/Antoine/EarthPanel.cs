using Unity.Netcode;
using UnityEngine;

public class EarthPanel : NetworkBehaviour
{
    private NetworkPlayer player;

    private void Start()
    {
        player = GetComponent<NetworkPlayer>();
    }

    private void Update()
    {
        if (!IsOwner) return;
        if (player == null) return;
        if (!player.IsEarth()) return;

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            GameProblemSystem.Instance.TrySolveProblemServerRpc(ProblemType.Overheat);
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            GameProblemSystem.Instance.TrySolveProblemServerRpc(ProblemType.PowerFailure);
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            GameProblemSystem.Instance.TrySolveProblemServerRpc(ProblemType.CameraFailure);
        }
    }
}