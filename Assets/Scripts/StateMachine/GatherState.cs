using UnityEngine;

public class GatherState : State
{
    private readonly FSMAgent _agent;

    private Transform _deadBoid;
    
    private float _gatherTimer;
    private float _timeToGather = 2.5f; //Tiempo de duración de la recolección
    private float _reachDistance = 1.5f; //Distancia para considerar que llegó al boid

    public GatherState(FSMAgent agent) 
    {
        _agent = agent;
    }

    public override void Enter()
    {
        Debug.Log("Cazador: Entrando a estado GATHER");
        _deadBoid = _agent.DeadTarget; //Tomamos el boid muerto desde el FSMAgent
        _gatherTimer = 0f;
    }

    public override void Update()
    {
        // Si el objetivo deja de estar disponible antes de completar la recolección, el HUNTER abandona la acción y regresa al comportamiento correspondiente.
        if (_deadBoid == null || !_deadBoid.gameObject.activeInHierarchy)
        {
            _agent.FSM.ChangeState(_agent.Patrol);
            return;
        }

        float distance = Vector3.Distance(_agent.transform.position, _deadBoid.position);

        // Dirigirse hacia el agente eliminado[cite: 1].
        if (distance > _reachDistance)
        {
            Vector3 dir = (_deadBoid.position - _agent.transform.position).normalized;
            _agent.transform.position += dir * _agent.Speed * Time.deltaTime;
            _agent.transform.forward = dir;
        }
        else
        {
            // Ejecuta la acción de recolección durante un tiempo determinado.
            _gatherTimer += Time.deltaTime;
            if (_gatherTimer >= _timeToGather)
            {
                
                _deadBoid.gameObject.SetActive(false); 
                Debug.Log("Boid recolectado exitosamente.");
                
               
                _agent.FSM.ChangeState(_agent.Patrol);
            }
        }
    }

    public override void Exit()
    {
        Debug.Log("Cazador: Saliendo de estado GATHER");
        _deadBoid = null;
    }
}
