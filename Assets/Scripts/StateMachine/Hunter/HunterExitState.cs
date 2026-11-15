using UnityEngine;
using System.Collections.Generic;

public class HunterExitState : State 
{
    private const float NodeArrivalDistance = 1f;

    private readonly FSMAgent _agent;
    private List<Node> _path;
    private int _pathIndex;

    public HunterExitState(FSMAgent agent) => _agent = agent;

    public override void Enter()
    {
        _agent.SetColor(Color.blue);
        Debug.Log("Cazador: ¡3 RECOLECCIONES LISTAS! Yendo a la Salida.");
        CalculatePathToExit();
    }

    public override void Update()
    {
        if (_agent.TryFleeFromWolf()) return;

        if (HasFinishedPath())
        {
            CompleteEscape();
            return;
        }

        FollowPath();
    }

    private void CalculatePathToExit()
    {
        Node exitNode = FindExitNode();
        if (exitNode == null) return;

        Node start = Graph.Current.GetClosestVisibleNode(_agent.transform.position);
        _path = Pathfinding.AStar(start, exitNode);
        _pathIndex = 0;
    }

    private Node FindExitNode()
    {
        foreach (Node node in Graph.Current.AllNodes)
        {
            if (node.IsExitNode) return node;
        }

        return null;
    }

    private bool HasFinishedPath()
    {
        return _path == null || _pathIndex >= _path.Count;
    }

    private void CompleteEscape()
    {
        Debug.Log("Cazador: ESCAPE EXITOSO. Reiniciando ciclo.");
        _agent.boidsCollected = 0;
        _agent.MoveToRespawn();
        _agent.FSM.ChangeState(_agent.Search);
    }

    private void FollowPath()
    {
        AgentMovement.MoveAlongPath(_agent.transform, _path, ref _pathIndex, _agent.Speed, NodeArrivalDistance);
    }

    public override void Exit() { }
}