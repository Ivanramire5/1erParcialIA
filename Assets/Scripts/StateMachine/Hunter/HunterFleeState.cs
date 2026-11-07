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
        _agent.SetChildColor(Color.magenta);

        Node start = Graph.Current.GetClosestNode(_agent.transform.position);
        List<Node> bestPath = null;
        float minPathCost = Mathf.Infinity;

        foreach (var node in Graph.Current.AllNodes)
        {
            if (node.IsSafeZone) 
            {
                // Llamada limpia solo con start y node
                List<Node> currentPath = Pathfinding.AStar(start, node);
                
                if (currentPath != null && currentPath.Count > 0)
                {
                    if (currentPath.Count < minPathCost)
                    {
                        minPathCost = currentPath.Count;
                        bestPath = currentPath;
                    }
                }
            }
        }

        if (bestPath != null)
        {
            _path = bestPath;
            _pathIndex = 0;
        }
        else
        {
            Debug.LogWarning("Cazador: No se encontró ruta a ninguna Zona Segura.");
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
                Debug.Log("Cazador: Tiempo de espera en Zona Segura terminado. Retomando actividad.");
                
                if (_agent.boidsCollected >= _agent.RequiredGathers)
                {
                    _agent.FSM.ChangeState(_agent.Exit);
                }
                else
                {
                    _agent.FSM.ChangeState(_agent.Search);
                }
            }
            return;
        }

        
        if (_path == null || _pathIndex >= _path.Count)
        {
            
            _isSafe = true; 
            return;
        }

      
        Vector3 targetPos = _path[_pathIndex].transform.position;
        Vector3 agentFlatPos = new Vector3(_agent.transform.position.x, 0f, _agent.transform.position.z);
        Vector3 targetFlatPos = new Vector3(targetPos.x, 0f, targetPos.z);

        
        float checkDistance = (_pathIndex == _path.Count - 1) ? 2.5f : 1f;

        if (Vector3.Distance(agentFlatPos, targetFlatPos) < checkDistance)
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

    public override void Exit() 
    {
        Debug.Log("Cazador: Saliendo del estado FLEE.");
    }
}