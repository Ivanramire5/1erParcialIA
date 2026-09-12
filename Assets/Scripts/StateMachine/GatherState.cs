using UnityEngine;

public class GatherState : State
{
    private readonly FSMAgent _agent;
    private Transform _deadBoid;

    private float _gatherTimer;
    private float _timeToGather = 2.5f;
    private float _reachDistance = 1.5f;

    public GatherState(FSMAgent agent)
    {
        _agent = agent;
    }

    public override void Enter()
    {
        Debug.Log("Cazador: Entrando a GATHER");
        _deadBoid = _agent.DeadTarget;
        _gatherTimer = 0f;
    }

    public override void Update()
    {
        if (_deadBoid == null || !_deadBoid.gameObject.activeInHierarchy)
        {
            _agent.FSM.ChangeState(_agent.Patrol);
            return;
        }

        float distance = Vector3.Distance(_agent.transform.position, _deadBoid.position);

        if (distance > _reachDistance)
        {
            _agent.SeekTo(_deadBoid.position);
        }
        else
        {
            _agent.StopVelocity();

            _gatherTimer += Time.deltaTime;
            if (_gatherTimer >= _timeToGather)
            {
                Debug.Log("Boid recolectado exitosamente.");

                TargetAgent boidScript = _deadBoid.GetComponent<TargetAgent>();
                if (boidScript != null)
                {
                    boidScript.OnCollected();
                }
                else
                {
                    _deadBoid.gameObject.SetActive(false);
                }

                _agent.FSM.ChangeState(_agent.Patrol);
            }
        }
    }

    public override void Exit()
    {
        Debug.Log("Cazador: Saliendo de GATHER");
        _deadBoid = null;
    }
}