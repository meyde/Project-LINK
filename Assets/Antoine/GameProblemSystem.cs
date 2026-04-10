using Unity.Netcode;
using UnityEngine;

public class GameProblemSystem : NetworkBehaviour
{
    public static GameProblemSystem Instance;

    public NetworkVariable<int> CurrentProblemId = new NetworkVariable<int>(
        0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    public NetworkVariable<ProblemType> CurrentProblemType = new NetworkVariable<ProblemType>(
        ProblemType.None,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    public NetworkVariable<ProblemState> CurrentProblemState = new NetworkVariable<ProblemState>(
        ProblemState.Waiting,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    public NetworkVariable<float> RemainingTime = new NetworkVariable<float>(
        0f,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private float serverTimer;
    private bool timerRunning;

    private void Awake()
    {
        Instance = this;
    }

    public override void OnNetworkSpawn()
    {
        CurrentProblemType.OnValueChanged += OnProblemTypeChanged;
        CurrentProblemState.OnValueChanged += OnProblemStateChanged;
        RemainingTime.OnValueChanged += OnTimerChanged;
    }

    public override void OnNetworkDespawn()
    {
        CurrentProblemType.OnValueChanged -= OnProblemTypeChanged;
        CurrentProblemState.OnValueChanged -= OnProblemStateChanged;
        RemainingTime.OnValueChanged -= OnTimerChanged;
    }

    private void Update()
    {
        if (!IsServer) return;

        if (!timerRunning) return;

        serverTimer -= Time.deltaTime;
        if (serverTimer < 0f) serverTimer = 0f;

        RemainingTime.Value = serverTimer;

        if (serverTimer <= 0f)
        {
            timerRunning = false;
            FailProblem();
        }
    }

    [ContextMenu("Generate Test Problem")]
    public void GenerateTestProblem()
    {
        if (!IsServer) return;

        GenerateProblem(ProblemType.Overheat, 20f);
    }

    public void GenerateProblem(ProblemType type, float duration)
    {
        if (!IsServer) return;

        CurrentProblemId.Value++;
        CurrentProblemType.Value = type;
        CurrentProblemState.Value = ProblemState.Appeared;

        serverTimer = duration;
        RemainingTime.Value = duration;
        timerRunning = true;

        Debug.Log($"Problème généré : {type}");
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void ReportProblemServerRpc()
    {
        if (CurrentProblemState.Value != ProblemState.Appeared)
            return;

        CurrentProblemState.Value = ProblemState.Reported;
        Debug.Log("Problème signalé");
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void TrySolveProblemServerRpc(ProblemType attemptedSolution)
    {
        if (CurrentProblemState.Value != ProblemState.Reported &&
            CurrentProblemState.Value != ProblemState.Resolving)
            return;

        CurrentProblemState.Value = ProblemState.Resolving;

        if (attemptedSolution == CurrentProblemType.Value)
        {
            SolveProblem();
        }
        else
        {
            WrongActionClientRpc();
        }
    }

    private void SolveProblem()
    {
        CurrentProblemState.Value = ProblemState.Solved;
        timerRunning = false;
        RemainingTime.Value = 0f;
        SolvedClientRpc();
    }

    private void FailProblem()
    {
        CurrentProblemState.Value = ProblemState.Failed;
        FailedClientRpc();
    }

    private void OnProblemTypeChanged(ProblemType oldValue, ProblemType newValue)
    {
        Debug.Log($"Type problème : {newValue}");
    }

    private void OnProblemStateChanged(ProblemState oldValue, ProblemState newValue)
    {
        Debug.Log($"État problème : {newValue}");
    }

    private void OnTimerChanged(float oldValue, float newValue)
    {
        // Pour l’UI plus tard
    }

    [ClientRpc]
    private void SolvedClientRpc()
    {
        Debug.Log("Problème résolu pour tous");
    }

    [ClientRpc]
    private void FailedClientRpc()
    {
        Debug.Log("Problème échoué pour tous");
    }

    [ClientRpc]
    private void WrongActionClientRpc()
    {
        Debug.Log("Mauvaise action");
    }
}