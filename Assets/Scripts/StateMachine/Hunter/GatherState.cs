using UnityEngine;

public class GatherState : State
{
    private readonly FSMAgent _agent;
    private Transform _deadBoid;

    private float _gatherTimer;
    private float _timeToGather = 2f;
    private float _reachDistance = 1.3f;

    public GatherState(FSMAgent agent)
    {
        _agent = agent;
    }

    public override void Enter()
    {
        _agent.SetColor(Color.yellow);
        Debug.Log("Cazador: Entrando a GATHER");
        _deadBoid = _agent.DeadTarget;
        _gatherTimer = 0f;
    }

    public override void Update()
    {
        if (_agent.TryFleeFromWolf()) return;

        if (!IsTargetAvailable())
        {
            _agent.FSM.ChangeState(_agent.Search);
            return;
        }

        if (IsTargetInReach())
        {
            GatherTarget();
        }
        else
        {
            MoveTowardsTarget();
        }
    }

    private bool IsTargetAvailable()
    {
        return _deadBoid != null && _deadBoid.gameObject.activeInHierarchy;
    }

    private bool IsTargetInReach()
    {
        return AgentMovement.FlatDistance(_agent.transform.position, _deadBoid.position) <= _reachDistance;
    }

    private void MoveTowardsTarget()
    {
        AgentMovement.MoveTowards(_agent.transform, _deadBoid.position, _agent.Speed);
    }

    private void GatherTarget()
    {
        _gatherTimer += Time.deltaTime;

        if (_gatherTimer < _timeToGather) return;

        CollectDeadBoid();
        _agent.ContinueHuntOrExit();
    }

    private void CollectDeadBoid()
    {
        Debug.Log("Boid recolectado exitosamente.");

        Boid boidScript = _deadBoid.GetComponent<Boid>();
        if (boidScript != null)
        {
            boidScript.OnCollected();
        }
        else
        {
            _deadBoid.gameObject.SetActive(false);
        }

        _agent.boidsCollected++;
        Debug.Log($"Boids recolectados: {_agent.boidsCollected}/{_agent.RequiredGathers}");
    }

    public override void Exit()
    {
        Debug.Log("Cazador: Saliendo de GATHER");
        _deadBoid = null;
    }
}