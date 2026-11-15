using UnityEngine;
using System.Collections.Generic;

public class HunterFleeState : State 
{
    private readonly FSMAgent _agent;
    private List<Node> _path;
    private int _pathIndex;
    private bool _isSafe;
    private float _timer;

    public HunterFleeState(FSMAgent agent) => _agent = agent;

    public override void Enter()
    {
        Debug.Log("Cazador: ¡LOBO DETECTADO! Huyendo a la Zona Segura más cercana.");
        _isSafe = false;
        _timer = 0f;
        _agent.SetColor(Color.magenta);
        CalculatePathToClosestSafeZone();
    }

    public override void Update()
    {
        if (_isSafe)
        {
            WaitInSafeZone();
            return;
        }

        if (HasFinishedPath())
        {
            _isSafe = true;
            return;
        }

        FollowPath();
    }

    private void CalculatePathToClosestSafeZone()
    {
        Node start = Graph.Current.GetClosestVisibleNode(_agent.transform.position);
        List<Node> shortestPath = FindShortestPathToSafeZone(start);

        if (shortestPath == null)
        {
            Debug.LogWarning("Cazador: No se encontró ruta a ninguna Zona Segura.");
            return;
        }

        _path = shortestPath;
        _pathIndex = 0;
    }

    private List<Node> FindShortestPathToSafeZone(Node start)
    {
        List<Node> shortestPath = null;

        foreach (Node node in Graph.Current.AllNodes)
        {
            if (!node.IsSafeZone) continue;

            List<Node> candidate = Pathfinding.AStar(start, node);
            if (candidate.Count == 0) continue;

            if (shortestPath == null || candidate.Count < shortestPath.Count)
            {
                shortestPath = candidate;
            }
        }

        return shortestPath;
    }

    private bool HasFinishedPath()
    {
        return _path == null || _pathIndex >= _path.Count;
    }

    private void WaitInSafeZone()
    {
        _timer += Time.deltaTime;

        if (_timer < _agent.SafeZoneWaitTime) return;

        Debug.Log("Cazador: Tiempo de espera en Zona Segura terminado. Retomando actividad.");
        _agent.ContinueHuntOrExit();
    }

    private void FollowPath()
    {
        AgentMovement.MoveAlongPath(_agent.transform, _path, ref _pathIndex, _agent.Speed, GetArrivalDistance());
    }

    private float GetArrivalDistance()
    {
        bool isLastNode = _pathIndex == _path.Count - 1;
        return isLastNode ? 2.5f : 1f;
    }

    public override void Exit() 
    {
        Debug.Log("Cazador: Saliendo del estado FLEE.");
    }
}