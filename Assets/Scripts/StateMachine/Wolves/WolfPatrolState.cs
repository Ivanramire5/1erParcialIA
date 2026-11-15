using UnityEngine;

public class WolfPatrolState : State
{
    private const float NodeArrivalDistance = 0.5f;

    private readonly WolfAgent _wolf;
    private int _waypointIndex = 0;
    private int _pathIndex = 0;

    public WolfPatrolState(WolfAgent wolf)
    {
        _wolf = wolf;
    }

    public override void Enter()
    {
        Debug.Log("Lobo: Entrando a estado PATROL");
        _wolf.SetColor(Color.green);
    }

    public override void Update()
    {
        if (_wolf.DetectHunter(out Vector3 hunterPos))
        {
            _wolf.Howl(hunterPos);
            _wolf.FSM.ChangeState(_wolf.ChaseState);
            return;
        }

        if (WolfAgent.IsAlerted)
        {
            _wolf.FSM.ChangeState(_wolf.InvestigateState);
            return;
        }

        PatrolBehavior();
    }

    private void PatrolBehavior()
    {
        if (_wolf.waypoints == null || _wolf.waypoints.Count == 0) return;

        if (HasReachedWaypoint())
        {
            GoToNextWaypoint();
        }

        FollowPathTo(GetCurrentWaypoint().position);
    }

    private Transform GetCurrentWaypoint()
    {
        return _wolf.waypoints[_waypointIndex];
    }

    private bool HasReachedWaypoint()
    {
        float distance = Vector3.Distance(_wolf.transform.position, GetCurrentWaypoint().position);
        return distance <= _wolf.WaypointCheckDistance;
    }

    private void GoToNextWaypoint()
    {
        _waypointIndex = (_waypointIndex + 1) % _wolf.waypoints.Count;
        _wolf.ClearPath();
    }

    private void FollowPathTo(Vector3 destination)
    {
        if (_wolf.CurrentActivePath.Count == 0)
        {
            CalculatePathTo(destination);
        }

        if (HasNodesLeftInPath())
        {
            AgentMovement.MoveAlongPath(_wolf.transform, _wolf.CurrentActivePath, ref _pathIndex, _wolf.Speed, NodeArrivalDistance);
            return;
        }

        AgentMovement.MoveTowards(_wolf.transform, destination, _wolf.Speed);
    }

    private bool HasNodesLeftInPath()
    {
        return _pathIndex < _wolf.CurrentActivePath.Count;
    }

    private void CalculatePathTo(Vector3 destination)
    {
        _wolf.CalculatePathTo(destination);
        _pathIndex = 0;
    }

    public override void Exit()
    {
        Debug.Log("Lobo: Saliendo de PATROL");
        _wolf.ClearPath();
    }
}