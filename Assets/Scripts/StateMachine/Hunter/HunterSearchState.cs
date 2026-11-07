using UnityEngine;
using System.Collections.Generic;

public class HunterSearchState : State 
{
    private readonly FSMAgent _agent;
    private List<Node> _path;
    private int _pathIndex;

    public HunterSearchState(FSMAgent agent) => _agent = agent;

    public override void Enter()
    {
        Debug.Log("Cazador: Entrando a Search (Navegando con A*)");
        _agent.SetChildColor(Color.blue);
        CalculateNewPath();
    }

    public override void Update()
    {
        Transform aliveBoid = null, deadBoid = null, wolf = null;
        

        _agent.SenseEnvironment(_agent._viewRadius, ref aliveBoid, ref deadBoid, ref wolf);

        
        if (wolf != null)
        {
            _agent.ThreatTarget = wolf;
            _agent.FSM.ChangeState(_agent.Flee);
            return;
        }
        if (deadBoid != null)
        {
            _agent.DeadTarget = deadBoid;
            _agent.FSM.ChangeState(_agent.Gather);
            return;
        }

        
        bool canAttack = _agent.currentMeleeTBATimer >= _agent.MeleeTBA || _agent.currentRangeTBATimer >= _agent.RangeTBA;
        
        if (aliveBoid != null && canAttack)
        {
            _agent.CurrentTarget = aliveBoid;
            _agent.FSM.ChangeState(_agent.Attack);
            return;
        }


        FollowPath();
        

        _agent.TrySpawnInterestObject();
    }

    private void FollowPath()
    {
        if (_path == null || _pathIndex >= _path.Count)
        {
            CalculateNewPath(); 
            return;
        }

        Vector3 targetPos = _path[_pathIndex].transform.position;
        Vector3 agentFlatPos = new Vector3(_agent.transform.position.x, 0f, _agent.transform.position.z);
        Vector3 targetFlatPos = new Vector3(targetPos.x, 0f, targetPos.z);

        if (Vector3.Distance(agentFlatPos, targetFlatPos) < 1f)
        {
            _pathIndex++; 
        }
        else
        {
            
            Vector3 dir = (targetFlatPos - agentFlatPos).normalized;
            
            if (dir.sqrMagnitude > 0.01f)
            {
                
                Quaternion targetRot = Quaternion.LookRotation(dir);
                _agent.transform.rotation = Quaternion.Slerp(_agent.transform.rotation, targetRot, Time.deltaTime * 15f);
            }
            
            _agent.transform.position += dir * _agent.Speed * Time.deltaTime;
        }
    }

    private void CalculateNewPath()
    {
        Node start = Graph.Current.GetClosestNode(_agent.transform.position, false);
        
        Node end = Graph.Current.AllNodes[Random.Range(0, Graph.Current.AllNodes.Count)];
        
        // Llamamos al AStar SOLO con el nodo de inicio y fin, sin el Enum
        _path = Pathfinding.AStar(start, end);
        _pathIndex = 0;
    }

    public override void Exit() { }
}