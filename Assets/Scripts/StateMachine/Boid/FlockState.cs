using UnityEngine;

public class FlockState : State
{
    private Boid _boid;
    public FlockState(Boid boid) { _boid = boid; }

    public override void Enter() { _boid.SetColor(Color.green); }
    public override void Update()
    {
        if (_boid.IsDead) return;

        Transform hunter = _boid.Sensor.GetClosestVisibleHunter(_boid.transform.position);
        if (hunter != null)
        {
            _boid.FSM.ChangeState(_boid.StateFlee);
            return;
        }

        Transform obj = _boid.Sensor.GetClosestVisibleObject(_boid.transform.position);
        if (obj != null)
        {
            _boid.FSM.ChangeState(_boid.StateInteract);
            return;
        }

        Vector3 steering = _boid.CalculateFlocking();
        _boid.ApplyMovement(steering);
    }
    public override void Exit() { }
}

