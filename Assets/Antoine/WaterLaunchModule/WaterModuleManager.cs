using System.Collections.Generic;
using UnityEngine;

public class WaterModuleManager : MonoBehaviour
{
    [Header("Contexte courant")]
    public int currentCatastropheCaseId;
    public WaterColorType currentWaterColor;
    public WaterGasType currentWaterGas;

    [Header("Rules")]
    public List<WaterModuleRuleSO> rules = new List<WaterModuleRuleSO>();

    [Header("Scene References")]
    public List<WaterModulePort> ports = new List<WaterModulePort>();
    public List<WaterPipe> pipes = new List<WaterPipe>();

    private Dictionary<Vector2Int, WaterPipe> pipeGrid = new Dictionary<Vector2Int, WaterPipe>();

    private void Awake()
    {
        BuildPipeGrid();
    }

    private void Start()
    {
        ValidateModule();
    }

    private void BuildPipeGrid()
    {
        pipeGrid.Clear();

        foreach (var pipe in pipes)
        {
            if (pipe == null)
                continue;

            if (!pipeGrid.ContainsKey(pipe.gridPosition))
            {
                pipeGrid.Add(pipe.gridPosition, pipe);
            }
            else
            {
                Debug.LogWarning($"Deux pipes ont la même gridPosition : {pipe.gridPosition}");
            }
        }
    }

    public void OnPortStateChanged(WaterModulePort changedPort)
    {
        EnforceSingleInputAndOutput(changedPort);
        ValidateModule();
    }

    private void EnforceSingleInputAndOutput(WaterModulePort changedPort)
    {
        if (changedPort.currentState == WaterPortState.Input)
        {
            foreach (var port in ports)
            {
                if (port != changedPort && port.currentState == WaterPortState.Input)
                {
                    port.SetState(WaterPortState.None);
                }
            }
        }
        else if (changedPort.currentState == WaterPortState.Output)
        {
            foreach (var port in ports)
            {
                if (port != changedPort && port.currentState == WaterPortState.Output)
                {
                    port.SetState(WaterPortState.None);
                }
            }
        }
    }

    public void ValidateModule()
    {
        foreach (var pipe in pipes)
        {
            if (pipe != null)
                pipe.SetPathHighlight(false);
        }

        WaterModuleRuleSO rule = GetCurrentRule();

        if (rule == null)
        {
            Debug.LogWarning("Aucune règle trouvée pour la configuration actuelle.");
            return;
        }

        WaterModulePort inputPort = null;
        WaterModulePort outputPort = null;

        foreach (var port in ports)
        {
            if (port == null)
                continue;

            if (port.currentState == WaterPortState.Input)
                inputPort = port;
            else if (port.currentState == WaterPortState.Output)
                outputPort = port;
        }

        if (inputPort == null || outputPort == null)
        {
            Debug.Log("Module incomplet : il faut un Input et un Output.");
            return;
        }

        bool portsAreCorrect =
            inputPort.portId == rule.expectedInputPort &&
            outputPort.portId == rule.expectedOutputPort;

        if (!portsAreCorrect)
        {
            Debug.Log("Les ports choisis ne correspondent pas à la consigne.");
            return;
        }

        bool pathExists = CheckPathBetweenPorts(inputPort, outputPort);

        if (pathExists)
        {
            Debug.Log("Module d'eau résolu correctement !");
        }
        else
        {
            Debug.Log("Aucun chemin valide entre Input et Output.");
        }
    }

    private WaterModuleRuleSO GetCurrentRule()
    {
        foreach (var rule in rules)
        {
            if (rule.catastropheCaseId == currentCatastropheCaseId &&
                rule.waterColor == currentWaterColor &&
                rule.waterGas == currentWaterGas)
            {
                return rule;
            }
        }

        return null;
    }

    private bool CheckPathBetweenPorts(WaterModulePort inputPort, WaterModulePort outputPort)
    {
        if (inputPort.connectedPipe == null)
        {
            Debug.LogWarning($"Le port {inputPort.portId} n'a pas de connectedPipe.");
            return false;
        }

        if (outputPort.connectedPipe == null)
        {
            Debug.LogWarning($"Le port {outputPort.portId} n'a pas de connectedPipe.");
            return false;
        }

        HashSet<WaterPipe> visited = new HashSet<WaterPipe>();

        WaterPipe startPipe = inputPort.connectedPipe;
        WaterPipe endPipe = outputPort.connectedPipe;

        return SearchPath(startPipe, endPipe, visited);
    }

    private bool SearchPath(WaterPipe current, WaterPipe target, HashSet<WaterPipe> visited)
    {
        if (current == null)
            return false;

        if (visited.Contains(current))
            return false;

        visited.Add(current);
        current.SetPathHighlight(true);

        if (current == target)
            return true;

        List<PipeDirection> openings = current.GetOpenDirections();

        foreach (var dir in openings)
        {
            Vector2Int nextPos = current.gridPosition + DirectionToVector(dir);

            if (!pipeGrid.TryGetValue(nextPos, out WaterPipe nextPipe))
                continue;

            PipeDirection opposite = GetOpposite(dir);

            if (!nextPipe.IsOpenTo(opposite))
                continue;

            if (SearchPath(nextPipe, target, visited))
                return true;
        }

        current.SetPathHighlight(false);
        return false;
    }

    private Vector2Int DirectionToVector(PipeDirection dir)
    {
        switch (dir)
        {
            case PipeDirection.Up: return new Vector2Int(0, -1);
            case PipeDirection.Right: return new Vector2Int(1, 0);
            case PipeDirection.Down: return new Vector2Int(0, 1);
            case PipeDirection.Left: return new Vector2Int(-1, 0);
        }

        return Vector2Int.zero;
    }

    private PipeDirection GetOpposite(PipeDirection dir)
    {
        switch (dir)
        {
            case PipeDirection.Up: return PipeDirection.Down;
            case PipeDirection.Right: return PipeDirection.Left;
            case PipeDirection.Down: return PipeDirection.Up;
            case PipeDirection.Left: return PipeDirection.Right;
        }

        return PipeDirection.Up;
    }
}