using System.Collections.Generic;
using UnityEngine;

public class WolfChaseState : State
{
    private readonly WolfAgent _wolf;
    
    
    private float _howlInterval = 1.5f; 
    private float _howlTimer = 0f;

    public WolfChaseState(WolfAgent wolf)
    {
        _wolf = wolf;
    }

    public override void Enter()
    {
        Debug.Log("Lobo: Entrando a CHASE. ¡Persiguiendo al cazador!");
        _howlTimer = _howlInterval;

        _wolf.SetColor(Color.red);
    }

    public override void Update()
    {        
        if (_wolf.DetectHunter(out Vector3 hunterPos))
        {
            PursueHunter(hunterPos);
                
        } 
        else
        {
            if (_wolf.IsHunterInSafeZone())
            {
                Debug.Log("Lobo: El cazador ingresó a una Zona Segura. Perdiendo objetivo y retornando a patrullar.");
                ReturnToPatrol();
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
        _howlTimer += Time.deltaTime;

        Vector3 dir = (targetPos - _wolf.transform.position).normalized;
        dir.y = 0f;

        if (dir.sqrMagnitude > 0.01f)
        {
            Quaternion targetRot = Quaternion.LookRotation(dir);
            _wolf.transform.rotation = Quaternion.Slerp(_wolf.transform.rotation, targetRot, Time.deltaTime * 10f);
        }

        _wolf.transform.position += dir * _wolf.Speed * Time.deltaTime;

        Attack(targetPos);
    }
  private void Attack(Vector3 hunterPos) 
    {

        Vector3 wolfFlat = new Vector3(_wolf.transform.position.x, 0f, _wolf.transform.position.z);
        Vector3 hunterFlat = new Vector3(hunterPos.x, 0f, hunterPos.z);

        float distancia = Vector3.Distance(wolfFlat, hunterFlat);

        if (distancia <= 2.5f)
        {
            if (_wolf.HunterTarget != null)
            {

                FSMAgent hunter = _wolf.HunterTarget.GetComponentInParent<FSMAgent>();

                if (hunter != null)
                {

                    hunter.TakeDamage(hunter.maxHealth);

                    if (hunter.IsDead)
                    {
                        Debug.Log("¡EL LOBO ATRAPÓ AL CAZADOR!");
                        hunter.FSM.ChangeState(hunter.Dead);
                        WolfAgent.ResetAlert();
                        ReturnToPatrol();
                        return;
                    }
                }

            }
        }

    }
   private void ReturnToPatrol()
    {
       
        WolfAgent.ResetAlert();
        if (_wolf.CurrentActivePath != null) _wolf.CurrentActivePath.Clear();
        _wolf.FSM.ChangeState(_wolf.PatrolState);
    }  
    public override void Exit()
    {
        Debug.Log("Lobo: Saliendo de CHASE.");
    } 
}