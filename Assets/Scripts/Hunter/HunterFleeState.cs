using UnityEngine;
using System.Collections.Generic;

public class HunterFleeState : State 
{
    private readonly FSMAgent _agent;
    private List<Node> _path;
    private int _pathIndex;
    private bool _isSafe;
    private float _timer;

    public HunterFleeState(FSMAgent agent) => _agent = agent;

    public override void Enter()
    {
        Debug.Log("Cazador: ¡LOBO DETECTADO! Huyendo a la Zona Segura más cercana.");
        _isSafe = false;
        _timer = 0f;

        Node start = Graph.Current.GetClosestNode(_agent.transform.position, false);
        Node safeNode = null;
        float minDist = Mathf.Infinity;

        // Buscar el refugio más cercano
        foreach (var node in Graph.Current.AllNodes)
        {
            if (node.IsSafeZone) // Asegurate de tener public bool IsSafeZone; en Node.cs
            {
                float dist = Vector3.Distance(_agent.transform.position, node.transform.position);
                if (dist < minDist)
                {
                    minDist = dist;
                    safeNode = node;
                }
            }
        }

        if (safeNode != null)
        {
            _path = Pathfinding.AStar(start, safeNode, Pathfinding.HeuristicMethod.Distance);
            _pathIndex = 0;
        }
    }

    public override void Update()
    {
        if (_isSafe)
        {
            _agent.StopVelocity();
            _timer += Time.deltaTime;
            if (_timer >= _agent.SafeZoneWaitTime)
            {
                if (_agent.boidsCollected >= _agent.RequiredGathers)
                    _agent.FSM.ChangeState(_agent.Exit);
                else
                    _agent.FSM.ChangeState(_agent.Search);
            }
            return;
        }

        if (_path == null || _pathIndex >= _path.Count)
        {
            _isSafe = true; // Llegó al refugio
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