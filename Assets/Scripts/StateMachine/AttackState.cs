using UnityEngine;

public class AttackState : State
{
    private readonly FSMAgent _agent;
    public Transform _targetBoid;
    private float _viewRadius = 15f;

    public AttackState(FSMAgent agent)
    {
        _agent = agent;
    }

    public override void Enter()
    {
        Debug.Log("Cazador entrando a estado ATTACK!!!");

        _targetBoid = _agent.CurrentTarget;
    }

    public override void Update()
    {
        //Verificamos si perdimos el objetivo o si se fue de nuestro rango de vision.
        if (_targetBoid == null || !IsTargetInView())
        {
            
            _agent.FSM.ChangeState(_agent.Patrol);
            return;
        }

        float distanceToTarget = Vector3.Distance(_agent.transform.position, _targetBoid.position);
        //Cuando el objetivo se encuentra dentro de MeleeAttackRadius, deberia perseguirlo hasta encontrarse en rango para realizar un ataque cuerpo a cuerpo.
        if (distanceToTarget <= _agent.MeleeAttackRadius)
        {
            PerformAttack("Cuerpo a Cuerpo");
        }
        else if (distanceToTarget <= _agent.RangeAttackRadius)
        {
            PerformAttack("A Distancia");
        }
        else
        {
            PursueTarget();
        }
    }

    private void PerformAttack(string type)
    {
        Debug.Log($"Ataque {type} exitoso!");
        
        //Al realizar un ataque exitoso el agente reinicia el temporizador TBA.
        _agent.currentTBATimer = 0f; 
        
        //Al realizar un ataque exitoso el agente abandona el estado Attack.
        _agent.FSM.ChangeState(_agent.Patrol); 
    }

    private void PursueTarget()
    {

        Vector3 dir = (_targetBoid.position - _agent.transform.position).normalized;
        _agent.transform.position += dir * _agent.Speed * Time.deltaTime;
        _agent.transform.forward = dir;
    }

    private bool IsTargetInView()
    {
        return Vector3.Distance(_agent.transform.position, _targetBoid.position) <= _viewRadius;
    }

    public override void Exit()
    {
        Debug.Log("Cazador: Saliendo de estado ATTACK");
    }

}
