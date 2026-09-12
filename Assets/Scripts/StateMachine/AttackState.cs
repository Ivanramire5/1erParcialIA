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
        Debug.Log("Cazador: Entrando a ATTACK!");
        _targetBoid = _agent.CurrentTarget;
    }

    public override void Update()
    {
        if (_targetBoid == null || !IsTargetInView())
        {
            _agent.FSM.ChangeState(_agent.Patrol);
            return;
        }

        float distanceToTarget = Vector3.Distance(_agent.transform.position, _targetBoid.position);

        bool canMelee = _agent.currentMeleeTBATimer >= _agent.MeleeTBA;
        bool canRange = _agent.currentRangeTBATimer >= _agent.RangeTBA;

        // Prioridad al Melee si estamos cerca y lo tenemos disponible
        if (distanceToTarget <= _agent.MeleeAttackRadius && canMelee)
        {
            PerformMeleeAttack();
        }
        // Si no, verificamos si podemos disparar a distancia
        else if (distanceToTarget <= _agent.RangeAttackRadius && canRange)
        {
            PerformRangedAttack();
        }
        // Si estamos lejos o no tenemos ataques listos, lo perseguimos
        else
        {
            PursueTarget();
        }
    }

    private void PerformMeleeAttack()
    {
        Debug.Log("¡Ataque Cuerpo a Cuerpo ejecutado!");
        _agent.currentMeleeTBATimer = 0f;

        TargetAgent boid = _targetBoid.GetComponent<TargetAgent>();
        if (boid != null)
        {
            // Daño instantáneo
            boid.TakeDamage(_agent.MeleeAttackDamage);

            if (boid.IsDead)
            {
                _agent.DeadTarget = _targetBoid;
                _agent.FSM.ChangeState(_agent.Gather);
                return;
            }
        }

        // Si sobrevive, volvemos a patrulla
        _agent.FSM.ChangeState(_agent.Patrol);
    }

    private void PerformRangedAttack()
    {
        Debug.Log("¡Disparo A Distancia ejecutado!");
        _agent.currentRangeTBATimer = 0f;

        // Instanciamos la bala física
        _agent.FireBullet(_targetBoid);

        // Volvemos a patrulla de inmediato. 
        // Cuando la bala mate al Boid, el Cazador lo verá muerto en el estado Patrol y lo irá a buscar.
        _agent.FSM.ChangeState(_agent.Patrol);
    }

    private void PursueTarget()
    {
        _agent.SeekTo(_targetBoid.position);
    }

    private bool IsTargetInView()
    {
        return Vector3.Distance(_agent.transform.position, _targetBoid.position) <= _viewRadius;
    }

    public override void Exit()
    {
        Debug.Log("Cazador: Saliendo de ATTACK");
    }
}