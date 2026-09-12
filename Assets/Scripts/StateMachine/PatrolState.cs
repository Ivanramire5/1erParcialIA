using UnityEngine;
using System.Collections.Generic;

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
        Debug.Log("Cazador: Entrando a Patrol");
        _agent.SetChildColor(Color.blue);
    }

    public override void Update()
    {
        Transform aliveBoid = null;
        Transform deadBoid = null;

        //  Sensado prioritario: evaluamos el entorno antes de tomar decisiones de movimiento
        _agent.SenseEnvironment(_agent._viewRadius, ref aliveBoid, ref deadBoid);

      
        if (deadBoid != null)
        {
            _agent.DeadTarget = deadBoid;
            _agent.FSM.ChangeState(_agent.Gather);
            return;
        }

     
        bool canAttack = _agent.currentMeleeTBATimer >= _agent.MeleeTBA || _agent.currentRangeTBATimer >= _agent.RangeTBA;


        if (aliveBoid != null && canAttack)
        {
            _agent.CurrentTarget = aliveBoid;
            _agent.FSM.ChangeState(_agent.Attack);
            return;
        }
        else
        {

            Patroling();
        }
    }

    private void Patroling()
    {
        Transform nextWaypoint = _data.waypoints[_currentIndex];

        if (Vector3.Distance(_agent.transform.position, nextWaypoint.position) <= _data.waypointCheckDistance)
        {
            _currentIndex = (_currentIndex + 1) % _data.waypoints.Count;
            nextWaypoint = _data.waypoints[_currentIndex];
        }

        _agent.SeekTo(nextWaypoint.position);
        _agent.TrySpawnInterestObject();
    }

    public override void Exit()
    {
        Debug.Log("Cazador: Saliendo de Patrol");
    }
}