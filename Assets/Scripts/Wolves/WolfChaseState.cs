using System.Collections.Generic;
using UnityEngine;

public class WolfChaseState : State
{
    private readonly WolfAgent _wolf;

    public WolfChaseState(WolfAgent wolf)
    {
        _wolf = wolf;
    }

    public override void Enter()
    {
        Debug.Log("Lobo: Entrando a CHASE. ¡Persiguiendo al cazador!");
    }

    public override void Update()
    {
        // 1. SENSADO CONTINUO: ¿El cazador sigue en nuestra línea de visión?
        if (_wolf.DetectHunter(out Vector3 hunterPos))
        {
            // Actualizamos constantemente la última posición para el resto de la manada[cite: 5]
            _wolf.Howl(hunterPos); 
            
            // Persecución física
            PursueHunter(hunterPos);
        }
        else
        {
            // 2. PÉRDIDA DE VISIÓN: Si lo pierde, va a investigar dónde lo vio por última vez[cite: 5]
            Debug.Log("Lobo: Cazador perdido de vista. Pasando a investigar.");
            _wolf.FSM.ChangeState(_wolf.InvestigateState);
        }
    }

    private void PursueHunter(Vector3 targetPos)
    {
        Vector3 dir = (targetPos - _wolf.transform.position).normalized;
        dir.y = 0f; // Mantenemos el movimiento plano

        if (dir.sqrMagnitude > 0.01f)
        {
            // Rotación suavizada (evitamos el efecto trompo)
            Quaternion targetRot = Quaternion.LookRotation(dir);
            _wolf.transform.rotation = Quaternion.Slerp(_wolf.transform.rotation, targetRot, Time.deltaTime * 10f);
        }

        // Se desplaza hacia el objetivo (podés multiplicar la velocidad si querés que corra más rápido al cazar)
        _wolf.transform.position += dir * _wolf.Speed * Time.deltaTime;
    }

    public override void Exit()
    {
        Debug.Log("Lobo: Saliendo de CHASE.");
    }
}
