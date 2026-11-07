using UnityEngine;

public class FleeState : State
{
    private Boid _boid;
    private Transform _currentHunter;

    public FleeState(Boid boid) { _boid = boid; }

    public override void Enter()
    {
        _boid.SetColor(Color.cyan);

        
        _currentHunter = _boid.Sensor.GetClosestVisibleHunter(_boid.transform.position);
    }

    public override void Update()
    {
        if (_boid.IsDead) return;

        
        if (_currentHunter == null || !_currentHunter.gameObject.activeInHierarchy)
        {
            _currentHunter = _boid.Sensor.GetClosestVisibleHunter(_boid.transform.position);

            if (_currentHunter == null)
            {
                _boid.FSM.ChangeState(_boid.StateFlock);
                return;
            }
        }

        
        float distanceToHunter = Vector3.Distance(_boid.transform.position, _currentHunter.position);

        if (distanceToHunter > _boid.ViewRadius)
        {
            _boid.FSM.ChangeState(_boid.StateFlock);
            return;
        }

        
        Vector3 steering = _boid.CalculateFlee(_currentHunter.position)
         + _boid.CalculateSeparation(Boid.AllAgents, _boid.SeparationRadius) * 1.5f;

        _boid.ApplyMovement(steering);
    }

    public override void Exit()
    {
        _currentHunter = null;
    }
}