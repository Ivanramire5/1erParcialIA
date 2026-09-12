using UnityEngine;

public class AttackState : State
{
    private readonly FSMAgent _agent;
    public Transform _targetBoid;

    public AttackState(FSMAgent agent)
    {
        _agent = agent;
    }

    public override void Enter()
    {
        Debug.Log("Cazador: Entrando a ATTACK!");
        _agent.SetChildColor(Color.red);
        _targetBoid = _agent.CurrentTarget;
    }

    public override void Update()
    {
        if (_targetBoid == null)
        {
            _agent.FSM.ChangeState(_agent.Patrol);
            return;
        }

        TargetAgent boid = _targetBoid.GetComponent<TargetAgent>();

        // 1. Si el boid ya está muerto antes de atacar, cambiar a Gather inmediatamente
        if (boid != null && boid.IsDead)
        {
            _agent.DeadTarget = _targetBoid;
            _agent.FSM.ChangeState(_agent.Gather);
            return;
        }

        float distanceToTarget = Vector3.Distance(_agent.transform.position, _targetBoid.position);

        bool canMelee = _agent.currentMeleeTBATimer >= _agent.MeleeTBA;
        bool canRange = _agent.currentRangeTBATimer >= _agent.RangeTBA;

        if (distanceToTarget <= _agent.MeleeAttackRadius && canMelee)
        {
            PerformMeleeAttack(boid);
        }
        else if (distanceToTarget <= _agent.RangeAttackRadius && canRange)
        {
            PerformRangedAttack();
        }
        else
        {
            PursueTarget();
        }
    }

    private void PerformMeleeAttack(TargetAgent boid)
    {
        Debug.Log("¡Ataque Cuerpo a Cuerpo ejecutado!");
        _agent.currentMeleeTBATimer = 0f;

        if (boid != null)
        {
            _agent.StopVelocity();
            boid.TakeDamage(_agent.MeleeAttackDamage);

            // 2. Si el golpe actual provocó la muerte, pasar a Gather
            if (boid.IsDead)
            {
                _agent.DeadTarget = _targetBoid;
                _agent.FSM.ChangeState(_agent.Gather);
            }
        }
    }

    private void PerformRangedAttack()
    {
        Debug.Log("¡Disparo A Distancia ejecutado!");
        _agent.currentRangeTBATimer = 0f;
        _agent.StopVelocity();
        _agent.FireBullet(_targetBoid);

        PursueTarget();
    }

    private void PursueTarget()
    {
        _agent.SeekTo(_targetBoid.position);
    }

    public override void Exit()
    {
        Debug.Log("Cazador: Saliendo de ATTACK");
    }
}