using System.Collections.Generic;
using UnityEngine;

public class WolfInvestigateState : State
{
    private readonly WolfAgent _wolf;
    private List<Node> _path = new List<Node>();
    private int _pathIndex = 0;
    private Vector3 _targetPosition;

    // Variables para el delay de olfateo
    private bool _isSniffing = false;
    private float _sniffTimer = 0f;
    private readonly float _sniffDuration = 2.5f; // Tiempo que se queda oliendo en el lugar

    public WolfInvestigateState(WolfAgent wolf)
    {
        _wolf = wolf;
    }

    public override void Enter()
    {
        Debug.Log("Lobo: Entrando a INVESTIGATE. Calculando ruta hacia la alerta.");
        _wolf.CambiarColorSemaforo(Color.yellow);
        _path.Clear();
        _pathIndex = 0;
        _isSniffing = false;
        _sniffTimer = 0f;

        if (WolfAgent.LastKnownHunterPosition.HasValue)
        {
            _targetPosition = WolfAgent.LastKnownHunterPosition.Value;
            CalculateAStarPath(_targetPosition);
        }
    }

    public override void Update()
    {
        // 1. SENSADO: Si durante la investigación u olfateo ve al cazador, vuelve a CHASE
        if (_wolf.DetectHunter(out Vector3 hunterPos))
        {
            _wolf.Howl(hunterPos);
            _wolf.FSM.ChangeState(_wolf.ChaseState);
            return;
        }

        // 2. NAVEGACIÓN A* HACIA LA ALERTA
        if (_path != null && _pathIndex < _path.Count)
        {
            Node targetNode = _path[_pathIndex];

            Vector3 wolfPosFlat = new Vector3(_wolf.transform.position.x, 0f, _wolf.transform.position.z);
            Vector3 targetPosFlat = new Vector3(targetNode.transform.position.x, 0f, targetNode.transform.position.z);
            float dist = Vector3.Distance(wolfPosFlat, targetPosFlat);

            if (dist <= 0.5f)
            {
                _pathIndex++;
            }
            else
            {
                MoveTowards(targetNode.transform.position);
            }
        }
        else
        {
            // 3. LLEGÓ A LA ZONA: Comienza la fase de olfateo
            if (!_isSniffing)
            {
                _isSniffing = true;
                _sniffTimer = 0f;
                Debug.Log("Lobo: Llegó al punto de alerta. Olfateando el área...");
                if (_wolf.CurrentActivePath != null) _wolf.CurrentActivePath.Clear();
            }

            _sniffTimer += Time.deltaTime;

            // Se queda oliendo durante el tiempo configurado
            if (_sniffTimer >= _sniffDuration)
            {
                Debug.Log("Lobo: Fin del olfateo sin éxito. Retomando patrullaje.");
                WolfAgent.ResetAlert();
                _wolf.FSM.ChangeState(_wolf.PatrolState);
            }
        }
    }

    private void CalculateAStarPath(Vector3 targetPos)
    {
        // Los lobos excluyen nodos de zonas seguras al calcular A*[cite: 5, 8]
        Node startNode = Graph.Current.GetClosestNode(_wolf.transform.position, excludeSafeZones: true);
        Node endNode = Graph.Current.GetClosestNode(targetPos, excludeSafeZones: true);

        if (startNode != null && endNode != null)
        {
            _path = Pathfinding.AStar(startNode, endNode, Pathfinding.HeuristicMethod.Distance, node => !node.IsSafeZone);
            _wolf.CurrentActivePath = _path;
        }
    }

    private void MoveTowards(Vector3 targetPos)
    {
        Vector3 dir = (targetPos - _wolf.transform.position).normalized;
        dir.y = 0f;

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
        if (_wolf.CurrentActivePath != null) _wolf.CurrentActivePath.Clear();
    }
}