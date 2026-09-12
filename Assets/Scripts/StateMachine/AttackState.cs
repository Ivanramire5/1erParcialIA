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

        // 1. Si el boid ya est� muerto antes de atacar, cambiar a Gather inmediatamente
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
        Debug.Log("Ataque Cuerpo a Cuerpo ejecutado!");
        _agent.currentMeleeTBATimer = 0f;

        if (boid != null)
    {
        boid.TakeDamage(_agent.MeleeAttackDamage);

        if (boid.IsDead)
        {
            _agent.DeadTarget = _targetBoid;
            _agent.FSM.ChangeState(_agent.Gather);
        }
        else
        {
            _agent.FSM.ChangeState(_agent.Patrol);
        }
    }
    }

    private void PerformRangedAttack()
    {
        Debug.Log("Disparo A Distancia ejecutado!");
        _agent.currentRangeTBATimer = 0f;
        _agent.StopVelocity();
        _agent.FireBullet(_targetBoid);

        PursueTarget();
    }

    private void PursueTarget()
    {
        if (_targetBoid == null) return;

        Vector3 dir = (_targetBoid.position - _agent.transform.position).normalized;
        dir.y = 0f; 

        if (dir.sqrMagnitude > 0.01f)
        {
            
            Quaternion targetRot = Quaternion.LookRotation(dir);
            _agent.transform.rotation = Quaternion.Slerp(_agent.transform.rotation, targetRot, Time.deltaTime * 10f);
        }

        _agent.transform.position += dir * _agent.Speed * Time.deltaTime;
    }

    public override void Exit()
    {
        Debug.Log("Cazador: Saliendo de ATTACK");
    }
}