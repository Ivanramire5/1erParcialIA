using UnityEngine;

public class IdleState : State
{
    private readonly FSMAgent _agent;
    readonly float _timeToChangeToPatrol = 3f;
    float _timer = 0f;

    public IdleState(FSMAgent agent) => _agent = agent;

    public override void Enter()
    {
        Debug.Log("Cazador: Entrando a estado IDLE");
        _timer = 0f;
    }

    public override void Update()
    {
        _timer += Time.deltaTime;
        if(_timer >= _timeToChangeToPatrol)
        {
            _agent.FSM.ChangeState(_agent.Patrol);
        }
    }

    public override void Exit()
    {
        Debug.LogError("Sali de idle");
    }

}
