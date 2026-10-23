using System.Collections.Generic;
using UnityEngine;

public class WolfPatrolState : State
{
    private readonly WolfAgent _wolf;
    private int _waypointIndex = 0;

    private List<Node> _currentPath = new List<Node>();
    private int _pathIndex = 0;

    public WolfPatrolState(WolfAgent wolf)
    {
        _wolf = wolf;
    }

    public override void Enter()
    {
        Debug.Log("Lobo: Entrando a estado PATROL");
        _currentPath.Clear();
    }

    public override void Update()
    {
        // 1. SENSADO VISUAL DIRECTO[cite: 2, 13]
        if (_wolf.DetectHunter(out Vector3 hunterPos))
        {
            _wolf.Howl(hunterPos);
            _wolf.FSM.ChangeState(_wolf.ChaseState);
            return;
        }

        // 2. SENSADO AUDITIVO (Alerta global de otro lobo)[cite: 2, 13]
        if (WolfAgent.IsAlerted)
        {
            _wolf.FSM.ChangeState(_wolf.InvestigateState);
            return;
        }

        // 3. LÓGICA DE PATRULLAJE Y NAVEGACIÓN[cite: 13]
        PatrolBehavior();
    }

    private void PatrolBehavior()
    {
        if (_wolf.waypoints == null || _wolf.waypoints.Count == 0) return;

        Transform targetWaypoint = _wolf.waypoints[_waypointIndex];
        float distanceToWaypoint = Vector3.Distance(_wolf.transform.position, targetWaypoint.position);

        if (distanceToWaypoint <= _wolf.WaypointCheckDistance)
        {
            _waypointIndex = (_waypointIndex + 1) % _wolf.waypoints.Count;
            targetWaypoint = _wolf.waypoints[_waypointIndex];
            _currentPath.Clear();
        }

        // 4. VERIFICAR LÍNEA DE VISIÓN AL WAYPOINT[cite: 2, 13]
        bool hasLineOfSight = !Physics.Linecast(_wolf.transform.position, targetWaypoint.position, _wolf.WallMask);

        if (hasLineOfSight)
        {
            _currentPath.Clear();
            MoveTowards(targetWaypoint.position);
        }
        else
        {
            if (_currentPath == null || _currentPath.Count == 0)
            {
                Node startNode = Graph.Current.GetClosestNode(_wolf.transform.position);
                Node endNode = Graph.Current.GetClosestNode(targetWaypoint.position);

                _currentPath = Pathfinding.AStar(startNode, endNode);
                _wolf.CurrentActivePath = _currentPath;
                _pathIndex = 0;
            }

            if (_currentPath != null && _pathIndex < _currentPath.Count)
            {
                Node targetNode = _currentPath[_pathIndex];

                // Distancia plana (ignorando Y)
                Vector3 wolfPosFlat = new Vector3(_wolf.transform.position.x, 0f, _wolf.transform.position.z);
                Vector3 targetPosFlat = new Vector3(targetNode.transform.position.x, 0f, targetNode.transform.position.z);
                float distToNode = Vector3.Distance(wolfPosFlat, targetPosFlat);

                if (distToNode <= 0.5f)
                {
                    _pathIndex++;
                }
                else
                {
                    MoveTowards(targetNode.transform.position);
                }
            }
        }
    }

    private void MoveTowards(Vector3 targetPos)
    {
        Vector3 dir = (targetPos - _wolf.transform.position).normalized;
        dir.y = 0f;

        if (dir.sqrMagnitude > 0.01f)
        {
            Quaternion targetRot = Quaternion.LookRotation(dir);
            _wolf.transform.rotation = Quaternion.Slerp(_wolf.transform.rotation, targetRot, Time.deltaTime * 10f);
        }

        _wolf.transform.position += dir * _wolf.Speed * Time.deltaTime;
    }

    public override void Exit()
    {
        Debug.Log("Lobo: Saliendo de PATROL");
        _currentPath.Clear();
        _wolf.CurrentActivePath.Clear();
    }
}