using System.Collections.Generic;
using UnityEngine;

public class WolfChaseState : State
{
    private readonly WolfAgent _wolf;

    private float _howlInterval = 5f;
    private float _howlTimer = 0f;

    public WolfChaseState(WolfAgent wolf)
    {
        _wolf = wolf;
    }

    public override void Enter()
    {
        Debug.Log("Lobo: Entrando a CHASE. ¡Persiguiendo al cazador!");
        _howlTimer = _howlInterval;
        _wolf.CambiarColorSemaforo(Color.red);
    }

    public override void Update()
    {
        _howlTimer += Time.deltaTime;

        if (_wolf.DetectHunter(out Vector3 hunterPos))
        {
            if (_howlTimer >= _howlInterval)
            {
                _wolf.Howl(hunterPos);
                _howlTimer = 0f;
            }

            PursueHunter(hunterPos);
        }
        else
        {
            // REGLA: Si el cazador entró a una Zona Segura, descartamos la última posición y volvemos a patrullar[cite: 8]
            if (_wolf.IsHunterInSafeZone())
            {
                Debug.Log("Lobo: El cazador ingresó a una Zona Segura. Perdiendo objetivo y retornando a patrullar.");
                WolfAgent.ResetAlert();
                if (_wolf.CurrentActivePath != null) _wolf.CurrentActivePath.Clear();
                _wolf.FSM.ChangeState(_wolf.PatrolState);
            }
            else
            {
                Debug.Log("Lobo: Cazador perdido de vista. Pasando a investigar.");
                _wolf.FSM.ChangeState(_wolf.InvestigateState);
            }
        }
    }

    private void PursueHunter(Vector3 targetPos)
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
        Debug.Log("Lobo: Saliendo de CHASE.");
    }
}