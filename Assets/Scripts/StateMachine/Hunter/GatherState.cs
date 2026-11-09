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
        _agent.SetChildColor(Color.yellow);
        Debug.Log("Cazador: Entrando a GATHER");
        _deadBoid = _agent.DeadTarget;
        _gatherTimer = 0f;
    }

    public override void Update()
    {
        if (_deadBoid == null || !_deadBoid.gameObject.activeInHierarchy)
        {
            _agent.FSM.ChangeState(_agent.Search);
            return;
        }

        Vector3 agentFlatPos = new Vector3(_agent.transform.position.x, 0f, _agent.transform.position.z);
        Vector3 targetFlatPos = new Vector3(_deadBoid.position.x, 0f, _deadBoid.position.z);
        float distance = Vector3.Distance(agentFlatPos, targetFlatPos);

        if (distance > _reachDistance)
        {

            Vector3 dir = (targetFlatPos - agentFlatPos).normalized;

            if (dir.sqrMagnitude > 0.01f)
            {
                Quaternion targetRot = Quaternion.LookRotation(dir);
                _agent.transform.rotation = Quaternion.Slerp(_agent.transform.rotation, targetRot, Time.deltaTime * 15f);
            }

            _agent.transform.position += dir * _agent.Speed * Time.deltaTime;
        }
        else
        {
            _agent.StopVelocity();

            _gatherTimer += Time.deltaTime;
            if (_gatherTimer >= _timeToGather)
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

                if (_agent.boidsCollected >= _agent.RequiredGathers)
                {
                    _agent.FSM.ChangeState(_agent.Exit);
                }
                else
                {
                    _agent.FSM.ChangeState(_agent.Search);
                }
            }
        }
    }

    public override void Exit()
    {
        Debug.Log("Cazador: Saliendo de GATHER");
        _deadBoid = null;
    }
}