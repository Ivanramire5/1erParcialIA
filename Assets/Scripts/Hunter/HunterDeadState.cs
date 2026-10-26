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
        _agent.StopVelocity();
        
        _agent.GetComponent<Collider>().enabled = false;
        _agent.SetVisible(false);
    }

    public override void Update()
    {
        _timer += Time.deltaTime;
        
        // Pasado RespawnTime, reaparece en la Zona inicial
        if (_timer >= _agent.RespawnTime)
        {
            _agent.boidsCollected = 0;
            _agent.currentHealth = _agent.maxHealth;
            
            // Lo teletransportamos al punto de spawn antes de hacerlo visible
            if (_agent.respawnPoint != null)
            {
                _agent.transform.position = _agent.respawnPoint.position;
            }
            else
            {
                _agent.transform.position = _agent.initialPosition; 
            }

            _agent.FSM.ChangeState(_agent.Search);
        }
    }

    public override void Exit()
    {
        _agent.GetComponent<Collider>().enabled = true;
        _agent.SetVisible(true); 
        Debug.Log("Cazador: RESPAWN COMPLETADO.");
    }
}