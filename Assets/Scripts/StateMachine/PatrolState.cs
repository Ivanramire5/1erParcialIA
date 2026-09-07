using System.Collections.Generic;
using UnityEngine;

public class PatrolState : State
{
    private FSMAgent _agent; 
    private PatrolData _data; 
    private int _currentIndex = 0;

    public PatrolState(PatrolData data, FSMAgent agent)
    {
        _data = data;
        _agent = agent;
    }

    public override void Enter()
    {
        Debug.LogError("Entre a Patrol");
    }

    public override void Update()
    {
        Transform nextWaypoint = _data.waypoints[_currentIndex];
        
        if (Vector3.Distance(nextWaypoint.position, _data.transform.position) <= _data.waypointCheckDistance)
        {
            _currentIndex = _currentIndex + 1 < _data.waypoints.Count ? _currentIndex + 1 : 0;
            nextWaypoint = _data.waypoints[_currentIndex];
        }

        Vector3 dir = nextWaypoint.position - _data.transform.position;

        _data.transform.position += _agent.Speed * Time.deltaTime * dir.normalized;
        _data.transform.forward = dir;
    }

    public override void Exit()
    {
        Debug.LogError("Sali de Patrol");
    }
}
