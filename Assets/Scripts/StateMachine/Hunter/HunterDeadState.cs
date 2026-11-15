using UnityEngine;

public class HunterDeadState : State 
{
    private readonly FSMAgent _agent;
    private float _timer;

    public HunterDeadState(FSMAgent agent) => _agent = agent;

    public override void Enter()
    {
        Debug.Log("Cazador: MUERTO. Desapareciendo...");
        _timer = 0f;

        _agent.GetComponent<Collider>().enabled = false;
        _agent.SetVisible(false);
    }

    public override void Update()
    {
        _timer += Time.deltaTime;
        if (_timer < _agent.RespawnTime) return;

        Respawn();
    }

    private void Respawn()
    {
        _agent.boidsCollected = 0;
        _agent.currentHealth = _agent.maxHealth;
        _agent.MoveToRespawn();
        _agent.FSM.ChangeState(_agent.Search);
    }

    public override void Exit()
    {
        Debug.Log("Cazador: RESPAWN COMPLETADO.");
    }
}
