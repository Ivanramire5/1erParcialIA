using UnityEngine;

public class DeadState : State
{
    private Boid _boid;
    public DeadState(Boid boid) { _boid = boid; }

    public override void Enter()
    {
        _boid.PrepareForDeath();
    }

    public override void Update()
    {
    }

    public override void Exit() { }
}