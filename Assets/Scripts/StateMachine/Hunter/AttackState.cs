using UnityEngine;

public class AttackState : State
{
    private readonly FSMAgent _agent;
    private Transform _targetBoid;

    public AttackState(FSMAgent agent)
    {
        _agent = agent;
    }

    public override void Enter()
    {
        Debug.Log("Cazador: Entrando a ATTACK!");
        _agent.SetColor(Color.red);
        _targetBoid = _agent.CurrentTarget;
    }

    public override void Update()
    {
        if (_agent.TryFleeFromWolf()) return;

        Boid boid = GetTargetBoid();
        if (boid == null || !_agent.CanSee(_targetBoid))
        {
            _agent.FSM.ChangeState(_agent.Search);
            return;
        }

        if (boid.IsDead)
        {
            StartGathering();
            return;
        }

        AttackOrPursue(boid);
    }

    private Boid GetTargetBoid()
    {
        if (_targetBoid == null || !_targetBoid.gameObject.activeInHierarchy) return null;

        return _targetBoid.GetComponent<Boid>();
    }

    private void StartGathering()
    {
        _agent.DeadTarget = _targetBoid;
        _agent.FSM.ChangeState(_agent.Gather);
    }

    private void AttackOrPursue(Boid boid)
    {
        HunterWeapon weapon = _agent.Weapon;
        float distance = Vector3.Distance(_agent.transform.position, _targetBoid.position);

        if (weapon.CanMeleeAttack && distance <= weapon.MeleeRadius)
        {
            PerformMeleeAttack(boid);
        }
        else if (weapon.CanRangeAttack && distance <= weapon.RangeRadius)
        {
            PerformRangedAttack();
        }
        else if (weapon.CanMeleeAttack || weapon.CanRangeAttack)
        {
            PursueTarget();
        }
        else
        {
            FaceTarget();
        }
    }

    private void PerformMeleeAttack(Boid boid)
    {
        Debug.Log("Ataque Cuerpo a Cuerpo ejecutado!");

        _agent.Weapon.MeleeAttack(boid);

        if (boid.IsDead)
        {
            StartGathering();
        }
    }

    private void PerformRangedAttack()
    {
        Debug.Log("Disparo A Distancia ejecutado!");

        _agent.Weapon.ShootAt(_targetBoid);
    }

    private void PursueTarget()
    {
        AgentMovement.MoveTowards(_agent.transform, _targetBoid.position, _agent.Speed);
    }

    private void FaceTarget()
    {
        AgentMovement.FaceTowards(_agent.transform, _targetBoid.position);
    }

    public override void Exit()
    {
        Debug.Log("Cazador: Saliendo de ATTACK");
    }
}