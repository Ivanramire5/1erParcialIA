using UnityEngine;

public class WolfChaseState : State
{
    private readonly WolfAgent _wolf;

    private readonly float _howlInterval = 1.5f;
    private float _howlTimer = 0f;

    public WolfChaseState(WolfAgent wolf)
    {
        _wolf = wolf;
    }

    public override void Enter()
    {
        Debug.Log("Lobo: Entrando a CHASE. ¡Persiguiendo al cazador!");
        _howlTimer = 0f;
        _wolf.SetColor(Color.red);
    }

    public override void Update()
    {
        if (!_wolf.DetectHunter(out Vector3 hunterPos))
        {
            HandleLostHunter();
            return;
        }

        RepeatAlert(hunterPos);

        if (!_wolf.HasLineOfSightTo(hunterPos)) return;

        _wolf.MoveTowards(hunterPos);
        TryCatchHunter(hunterPos);
    }

    private void HandleLostHunter()
    {
        if (_wolf.IsHunterInSafeZone())
        {
            Debug.Log("Lobo: El cazador ingresó a una Zona Segura. Perdiendo objetivo y retornando a patrullar.");
            ReturnToPatrol();
            return;
        }

        Debug.Log("Lobo: Cazador perdido de vista. Pasando a investigar.");
        _wolf.FSM.ChangeState(_wolf.InvestigateState);
    }

    private void RepeatAlert(Vector3 hunterPos)
    {
        _howlTimer += Time.deltaTime;
        if (_howlTimer < _howlInterval) return;

        _howlTimer = 0f;
        _wolf.Howl(hunterPos);
    }

    private void TryCatchHunter(Vector3 hunterPos)
    {
        if (AgentMovement.FlatDistance(_wolf.transform.position, hunterPos) > _wolf.CaptureRadius) return;
        if (_wolf.HunterTarget == null) return;

        FSMAgent hunter = _wolf.HunterTarget.GetComponentInParent<FSMAgent>();
        if (hunter == null) return;

        hunter.TakeDamage(hunter.maxHealth);

        if (hunter.IsDead)
        {
            Debug.Log("¡EL LOBO ATRAPÓ AL CAZADOR!");

            ReturnToPatrol();
        }
    }

    private void ReturnToPatrol()
    {
        WolfAgent.ResetAlert();
        _wolf.FSM.ChangeState(_wolf.PatrolState);
    }

    public override void Exit()
    {
        Debug.Log("Lobo: Saliendo de CHASE.");
    }
}