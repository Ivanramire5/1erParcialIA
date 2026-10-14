using UnityEngine;

public class InteractState : State
{
    private Boid _boid;
    private Transform _targetObject;

    public InteractState(Boid boid) { _boid = boid; }

    public override void Enter() { _boid.SetColor(Color.yellow); }
    public override void Update()
    {
        if (_boid.IsDead) return;

        Transform hunter = _boid.Sensor.GetClosestVisibleHunter(_boid.transform.position);
        if (hunter != null)
        {
            _boid.FSM.ChangeState(_boid.StateFlee);
            return;
        }

        _targetObject = _boid.Sensor.GetClosestVisibleObject(_boid.transform.position);

        if (_targetObject == null)
        {
            _boid.FSM.ChangeState(_boid.StateFlock);
            return;
        }

        Vector3 steering = _boid.CalculateArrive(_targetObject.position)
                         + _boid.CalculateSeparation(Boid.AllAgents, _boid.SeparationRadius) * 1.5f;
        _boid.ApplyMovement(steering);
        _boid.InteractWithObject(_targetObject);
    }
    public override void Exit() { }
}