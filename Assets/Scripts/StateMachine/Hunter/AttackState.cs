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
        if (_targetBoid == null || !_targetBoid.gameObject.activeInHierarchy)
        {
            _agent.FSM.ChangeState(_agent.Search);
            return;
        }

        Boid boid = _targetBoid.GetComponent<Boid>();
        if (boid == null)
        {
            _agent.FSM.ChangeState(_agent.Search);
            return;
        }

        // Si ya está muerto, pasamos directo al estado Gather
        if (boid.IsDead)
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

    private void PerformMeleeAttack(Boid boid)
    {
        Debug.Log("Ataque Cuerpo a Cuerpo ejecutado!");

        // Reseteamos el timer corporativo usando el método seguro del FSMAgent
        _agent.ResetMeleeTimer();
        _agent.StopVelocity();

        boid.TakeDamage(_agent.MeleeAttackDamage);

        if (boid.IsDead)
        {
            _agent.DeadTarget = _targetBoid;
            _agent.FSM.ChangeState(_agent.Gather);
        }
    }

    private void PerformRangedAttack()
    {
        Debug.Log("Disparo A Distancia ejecutado!");
        
        // Reseteamos el timer a distancia usando el método seguro del FSMAgent
        _agent.ResetRangeTimer();
        _agent.StopVelocity();
        _agent.FireBullet(_targetBoid);

        PursueTarget();
    }

    private void PursueTarget()
    {
        if (_targetBoid == null) return;

        Vector3 agentFlatPos = new Vector3(_agent.transform.position.x, 0f, _agent.transform.position.z);
        Vector3 targetFlatPos = new Vector3(_targetBoid.position.x, 0f, _targetBoid.position.z);

        Vector3 dir = (targetFlatPos - agentFlatPos).normalized;

        if (dir.sqrMagnitude > 0.01f)
        {
            Quaternion targetRot = Quaternion.LookRotation(dir);
            _agent.transform.rotation = Quaternion.Slerp(_agent.transform.rotation, targetRot, Time.deltaTime * 15f);
        }

        _agent.transform.position += dir * _agent.Speed * Time.deltaTime;
    }

    public override void Exit()
    {
        Debug.Log("Cazador: Saliendo de ATTACK");
    }
}