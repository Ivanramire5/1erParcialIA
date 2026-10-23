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
        Debug.Log("Cazador: ¡3 RECOLECCIONES LISTAS! Yendo a la Salida.");
        Node start = Graph.Current.GetClosestNode(_agent.transform.position, false);
        Node exitNode = null;
        
        foreach (var node in Graph.Current.AllNodes)
        {
            if (node.IsExitNode) // Asegurate de tener public bool IsExitNode; en Node.cs
            {
                exitNode = node;
                break;
            }
        }

        if (exitNode != null)
        {
            _path = Pathfinding.AStar(start, exitNode, Pathfinding.HeuristicMethod.Distance);
            _pathIndex = 0;
        }
    }

    public override void Update()
    {
        if (_path == null || _pathIndex >= _path.Count)
        {
            Debug.Log("Cazador: ESCAPE EXITOSO. Reiniciando ciclo.");
            _agent.boidsCollected = 0;
            _agent.transform.position = Vector3.zero; // Cambiar por la posición de tu Zona Inicial
            _agent.FSM.ChangeState(_agent.Search);
            return;
        }

        Vector3 targetPos = _path[_pathIndex].transform.position;
        
        Vector3 agentFlatPos = new Vector3(_agent.transform.position.x, 0f, _agent.transform.position.z);
        Vector3 targetFlatPos = new Vector3(targetPos.x, 0f, targetPos.z);

        if (Vector3.Distance(agentFlatPos, targetFlatPos) < 1f)
            _pathIndex++;
        else
            _agent.SeekTo(targetPos);
    }

    public override void Exit() { }
}