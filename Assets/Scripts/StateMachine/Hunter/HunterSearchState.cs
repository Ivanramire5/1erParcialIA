using UnityEngine;
using System.Collections.Generic;

public class HunterSearchState : State 
{
    private const float NodeArrivalDistance = 1f;

    private readonly FSMAgent _agent;
    private List<Node> _path;
    private int _pathIndex;

    public HunterSearchState(FSMAgent agent) => _agent = agent;

    public override void Enter()
    {
        _agent.GetComponent<Collider>().enabled = true;
        _agent.SetVisible(true);
        Debug.Log("Cazador: Entrando a Search (Navegando con A*)");
        _agent.SetColor(Color.blue);
        CalculateNewPath();
    }

    public override void Update()
    {
        if (_agent.TryFleeFromWolf()) return;

        if (TryStartGathering(_agent.GetVisibleDeadBoid())) return;
        if (TryStartAttacking(_agent.GetVisibleAliveBoid())) return;

        FollowPath();
        _agent.TrapSpawner.TrySpawnTrap();
    }

    private bool TryStartGathering(Transform deadBoid)
    {
        if (deadBoid == null) return false;

        _agent.DeadTarget = deadBoid;
        _agent.FSM.ChangeState(_agent.Gather);
        return true;
    }

    private bool TryStartAttacking(Transform aliveBoid)
    {
        if (aliveBoid == null) return false;
        if (!_agent.Weapon.CanMeleeAttack && !_agent.Weapon.CanRangeAttack) return false;

        _agent.CurrentTarget = aliveBoid;
        _agent.FSM.ChangeState(_agent.Attack);
        return true;
    }

    private void FollowPath()
    {
        if (HasFinishedPath())
        {
            CalculateNewPath();
            return;
        }

        AgentMovement.MoveAlongPath(_agent.transform, _path, ref _pathIndex, _agent.Speed, NodeArrivalDistance);
    }

    private bool HasFinishedPath()
    {
        return _path == null || _pathIndex >= _path.Count;
    }

    private void CalculateNewPath()
    {
        Node start = Graph.Current.GetClosestVisibleNode(_agent.transform.position);
        Node end = PickHuntingNode(start);

        _path = Pathfinding.AStar(start, end);
        _pathIndex = 0;
    }

    private Node PickHuntingNode(Node current)
    {
        List<Node> huntingNodes = new List<Node>();

        foreach (Node node in Graph.Current.AllNodes)
        {
            if (node.IsHuntingNode && !node.Blocked && node != current)
            {
                huntingNodes.Add(node);
            }
        }

        return huntingNodes[Random.Range(0, huntingNodes.Count)];
    }

    public override void Exit() { }
}