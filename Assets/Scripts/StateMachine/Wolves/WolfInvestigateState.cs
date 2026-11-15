using UnityEngine;

public class WolfInvestigateState : State
{
    private const float NodeArrivalDistance = 0.5f;

    private readonly WolfAgent _wolf;
    private int _pathIndex = 0;

    private bool _isSniffing = false;
    private float _sniffTimer = 0f;
    private readonly float _sniffDuration = 2.5f;

    public WolfInvestigateState(WolfAgent wolf)
    {
        _wolf = wolf;
    }

    public override void Enter()
    {
        Debug.Log("Lobo: Entrando a INVESTIGATE. Calculando ruta hacia la alerta.");
        _wolf.SetColor(Color.yellow);

        _pathIndex = 0;
        _isSniffing = false;
        _sniffTimer = 0f;

        if (WolfAgent.LastKnownHunterPosition.HasValue)
        {
            _wolf.CalculatePathTo(WolfAgent.LastKnownHunterPosition.Value);
        }
    }

    public override void Update()
    {
        if (TryChaseHunter()) return;

        if (HasPathToFollow())
        {
            FollowPath();
        }
        else
        {
            Sniff();
        }
    }

    private bool TryChaseHunter()
    {
        if (!_wolf.DetectHunter(out Vector3 hunterPos)) return false;

        _wolf.Howl(hunterPos);
        _wolf.FSM.ChangeState(_wolf.ChaseState);
        return true;
    }

    private bool HasPathToFollow()
    {
        return _pathIndex < _wolf.CurrentActivePath.Count;
    }

    private void FollowPath()
    {
        AgentMovement.MoveAlongPath(_wolf.transform, _wolf.CurrentActivePath, ref _pathIndex, _wolf.Speed, NodeArrivalDistance);
    }

    private void Sniff()
    {
        if (!_isSniffing)
        {
            StartSniffing();
        }

        _sniffTimer += Time.deltaTime;
        if (_sniffTimer < _sniffDuration) return;

        Debug.Log("Lobo: Fin del olfateo sin éxito. Retomando patrullaje.");
        WolfAgent.ResetAlert();
        _wolf.FSM.ChangeState(_wolf.PatrolState);
    }

    private void StartSniffing()
    {
        _isSniffing = true;
        _sniffTimer = 0f;
        Debug.Log("Lobo: Llegó al punto de alerta. Olfateando el área...");
        _wolf.ClearPath();
    }

    public override void Exit()
    {
        Debug.Log("Lobo: Saliendo de INVESTIGATE.");
        _wolf.ClearPath();
    }
}