using Unity.Netcode;
using UnityEngine;

public class ProblemAutoStarter : NetworkBehaviour
{
    private bool started = false;

    private void Update()
    {
        if (!IsServer) return;
        if (started) return;

        if (NetworkManager.Singleton.ConnectedClients.Count >= 2)
        {
            started = true;
            GameProblemSystem.Instance.GenerateProblem(ProblemType.Overheat, 20f);
        }
    }
}