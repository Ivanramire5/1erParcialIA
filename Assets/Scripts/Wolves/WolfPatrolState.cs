using System.Collections.Generic;
using UnityEngine;

public class WolfPatrolState : State
{
    private readonly WolfAgent _wolf;
    private int _waypointIndex = 0;

    //Variables para el pathfinding
    private List<Node> _currentPath = new List<Node>();
    private int _pathIndex = 0;
    public WolfPatrolState(WolfAgent wolf) 
    {
        _wolf = wolf;
    }
    public override void Enter()
    {
        Debug.Log("Lobo: Entrando a estado PATROL");
        _currentPath.Clear();
    }

    public override void Update()
    {
        // ¿El lobo ve al cazador directamente? (SENSADO VISUAL)
        if (_wolf.DetectHunter(out Vector3 hunterPos))
        {
            _wolf.Howl(hunterPos); // Dispara la alerta global[cite: 2]
            _wolf.FSM.ChangeState(_wolf.ChaseState);
            return;
        }

        // ¿El lobo ha sido alertado por otro lobo? (SENSADO AUDITIVO)
        if (WolfAgent.IsAlerted)
        {
            // Si escuchamos la alerta pero no lo vemos directamente, vamos a investigar la última posición conocida[cite: 2]
            _wolf.FSM.ChangeState(_wolf.InvestigateState);
            return;
        }

        // 3. LÓGICA DE PATRULLAJE Y NAVEGACIÓN
        PatrolBehavior();
    }
    private void PatrolBehavior()
    {
        if (_wolf.waypoints == null || _wolf.waypoints.Count == 0) return;

        Transform targetWaypoint = _wolf.waypoints[_waypointIndex];
        float distanceToWaypoint = Vector3.Distance(_wolf.transform.position, targetWaypoint.position);

        // Comprobamos si llegamos al waypoint para pasar al siguiente (recorriendo en bucle)[cite: 2]
        if (distanceToWaypoint <= _wolf.WaypointCheckDistance)
        {
            _waypointIndex = (_waypointIndex + 1) % _wolf.waypoints.Count;
            targetWaypoint = _wolf.waypoints[_waypointIndex];
            _currentPath.Clear(); // Limpiamos el camino viejo
        }

        // 4. VERIFICAR LÍNEA DE VISIÓN AL WAYPOINT
        // Trazamos un rayo para ver si hay un muro entre el lobo y su destino[cite: 2]
        bool hasLineOfSight = !Physics.Linecast(_wolf.transform.position, targetWaypoint.position, _wolf.WallMask);

        if (hasLineOfSight)
        {
            // Si lo vemos directamente, anulamos cualquier camino viejo y vamos en línea recta
            _currentPath.Clear(); 
            MoveTowards(targetWaypoint.position);
        }
        else
        {
            // Si no hay línea de visión, usamos Pathfinding A*[cite: 2, 13]
            if (_currentPath == null || _currentPath.Count == 0)
            {
                // Buscamos los nodos más cercanos en nuestro grafo propio
                Node startNode = Graph.Current.GetClosestNode(_wolf.transform.position);
                Node endNode = Graph.Current.GetClosestNode(targetWaypoint.position);
                
                // Calculamos la ruta usando la clase estática que ya tienen armada[cite: 13]
                _currentPath = Pathfinding.AStar(startNode, endNode);
                _pathIndex = 0;
            }

            // Recorremos los nodos que nos devolvió el A*
            if (_currentPath != null && _pathIndex < _currentPath.Count)
            {
                Node targetNode = _currentPath[_pathIndex];
                float distToNode = Vector3.Distance(_wolf.transform.position, targetNode.transform.position);
                
                if (distToNode <= 0.5f) // Distancia de aceptación del nodo
                {
                    _pathIndex++; // Pasamos al siguiente nodo del camino
                }
                else
                {
                    MoveTowards(targetNode.transform.position);
                }
            }
        }
    }
    private void MoveTowards(Vector3 targetPos)
    {
        Vector3 dir = (targetPos - _wolf.transform.position).normalized;
        dir.y = 0f; // Evitamos que rote hacia el piso

        if (dir.sqrMagnitude > 0.01f)
        {
            // Rotación suavizada para evitar el "efecto trompo" que el profe corrigió antes[cite: 4]
            Quaternion targetRot = Quaternion.LookRotation(dir);
            _wolf.transform.rotation = Quaternion.Slerp(_wolf.transform.rotation, targetRot, Time.deltaTime * 10f);
        }

        // Desplazamiento
        _wolf.transform.position += dir * _wolf.Speed * Time.deltaTime;
    }

    public override void Exit()
    {
        Debug.Log("Lobo: Saliendo de PATROL");
        _currentPath.Clear();
    }

}
