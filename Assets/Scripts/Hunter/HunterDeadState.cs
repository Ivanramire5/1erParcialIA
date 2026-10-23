using UnityEngine;

public class HunterDeadState : State 
{
    private readonly FSMAgent _agent;
    private float _timer;

    public HunterDeadState(FSMAgent agent) => _agent = agent;

    public override void Enter()
    {
        Debug.Log("Cazador: MUERTO. Esperando respawn...");
        _timer = 0f;
        _agent.StopVelocity();
        
        // Lo volvemos "fantasma" para que los lobos no se traben
        _agent.GetComponent<Collider>().enabled = false;
        _agent.SetChildColor(Color.clear);
    }

    public override void Update()
    {
        _timer += Time.deltaTime;
        if (_timer >= _agent.RespawnTime)
        {
            _agent.boidsCollected = 0;
            _agent.transform.position = Vector3.zero; // Cambiar por tu posición de Start
            _agent.FSM.ChangeState(_agent.Search);
        }
    }

    public override void Exit()
    {
        // Vuelve a ser sólido y visible
        _agent.GetComponent<Collider>().enabled = true;
        _agent.SetChildColor(Color.white); 
        Debug.Log("Cazador: RESPAWN COMPLETADO.");
    }
}