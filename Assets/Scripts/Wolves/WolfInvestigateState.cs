using System.Collections.Generic;
using UnityEngine;

public class WolfInvestigateState : State
{
    private readonly WolfAgent _wolf;
    private List<Node> _path = new List<Node>();
    private int _pathIndex = 0;
    private Vector3 _targetPosition;

    public WolfInvestigateState(WolfAgent wolf)
    {
        _wolf = wolf;
    }

    public override void Enter()
    {
        Debug.Log("Lobo: Entrando a INVESTIGATE. Calculando ruta hacia la alerta.");
        _path.Clear();
        _pathIndex = 0;

        // Tomamos la última posición conocida transmitida por el aullido[cite: 1]
        if (WolfAgent.LastKnownHunterPosition.HasValue)
        {
            _targetPosition = WolfAgent.LastKnownHunterPosition.Value;
            CalculateAStarPath(_targetPosition);
        }
    }

    public override void Update()
    {
        // 1. SENSADO: Si durante la investigación el lobo logra ver al Cazador físicamente
        if (_wolf.DetectHunter(out Vector3 hunterPos))
        {
            // Si la posición del Cazador vuelve a ser detectada, actualizamos la información y alertamos[cite: 1]
            _wolf.Howl(hunterPos); 
            _wolf.FSM.ChangeState(_wolf.ChaseState);
            return;
        }

        // 2. NAVEGACIÓN A* HACIA LA ALERTA
        if (_path != null && _pathIndex < _path.Count)
        {
            Node targetNode = _path[_pathIndex];
            float dist = Vector3.Distance(_wolf.transform.position, targetNode.transform.position);

            if (dist <= 0.5f) // Distancia de aceptación del nodo
            {
                _pathIndex++; // Pasamos al siguiente nodo del camino
            }
            else
            {
                MoveTowards(targetNode.transform.position);
            }
        }
        else
        {
            // 3. LLEGÓ A LA ZONA Y EL CAZADOR NO ESTÁ
            // Una vez que llegan a la zona de detección, continúan su comportamiento de búsqueda[cite: 1]
            Debug.Log("Lobo: Zona investigada sin éxito. Retomando patrullaje.");
            _wolf.FSM.ChangeState(_wolf.PatrolState);
        }
    }

    private void CalculateAStarPath(Vector3 targetPos)
    {
        // NOTA: Asegurate de tener GetClosestNode en tu Graph.cs iterando sobre GetAllGridNodes
        Node startNode = Graph.Current.GetClosestNode(_wolf.transform.position);
        Node endNode = Graph.Current.GetClosestNode(targetPos);

        if (startNode != null && endNode != null)
        {
            // Feedback visual/consola exigido: Cuándo se está utilizando Pathfinding[cite: 1]
            Debug.Log("Lobo: Utilizando Pathfinding A* hacia la posición " + targetPos); 
            _path = Pathfinding.AStar(startNode, endNode); // Calculamos la ruta con el algoritmo del proyecto[cite: 1, 14]
        }
    }

    private void MoveTowards(Vector3 targetPos)
    {
        Vector3 dir = (targetPos - _wolf.transform.position).normalized;
        dir.y = 0f; // Evitamos rotaciones extrañas hacia el piso

        if (dir.sqrMagnitude > 0.01f)
        {
            Quaternion targetRot = Quaternion.LookRotation(dir);
            _wolf.transform.rotation = Quaternion.Slerp(_wolf.transform.rotation, targetRot, Time.deltaTime * 10f);
        }
        
        _wolf.transform.position += dir * _wolf.Speed * Time.deltaTime;
    }

    public override void Exit()
    {
        Debug.Log("Lobo: Saliendo de INVESTIGATE.");
        _path.Clear();
    }
}
