using UnityEngine;
using System.Collections.Generic;

public class HunterExitState : State 
{
    private readonly FSMAgent _agent;
    private List<Node> _path;
    private int _pathIndex;

    public HunterExitState(FSMAgent agent) => _agent = agent;

    public override void Enter()
    {
        _agent.SetChildColor(Color.blue);
        Debug.Log("Cazador: ¡3 RECOLECCIONES LISTAS! Yendo a la Salida.");
        Node start = Graph.Current.GetClosestNode(_agent.transform.position, false);
        Node exitNode = null;
        
        foreach (var node in Graph.Current.AllNodes)
        {
            if (node.IsExitNode) 
            {
                exitNode = node;
                break;
            }
        }

        if (exitNode != null)
        {
            
            _path = Pathfinding.AStar(start, exitNode);
            _pathIndex = 0;
        }
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

        if (_path == null || _pathIndex >= _path.Count)
        {
            Debug.Log("Cazador: ESCAPE EXITOSO. Reiniciando ciclo.");
            _agent.boidsCollected = 0;
            _agent.transform.position = Vector3.zero; 
            _agent.FSM.ChangeState(_agent.Search);
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

    public override void Exit() { }
}