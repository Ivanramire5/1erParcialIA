using System.Collections.Generic;
using UnityEngine;

public class WolfAgent : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private float _speed = 3.5f;
    public float Speed => _speed;

    [Header("Percepcion y referencias")]
    public Transform HunterTarget;
    public float viewRadius = 10f;
    public float viewAngle = 90f;
    [SerializeField] private LayerMask wallMask;

    [Header("Patrulla")]
    public List<Transform> waypoints;
    public float waypointCheckDiastance = 1f;

    //Estado compartido para alertar a la manada
    public static Vector3? LastKnownHunterPosition { get; private set; }
    public static bool IsAlerted { get; private set; }

    private readonly FiniteStateMachine _fsm = new();
    public FiniteStateMachine FSM => _fsm;

    //Estados de los lobos
    public WolfPatrolState PatrolState { get; private set; }
    public WolfChaseState ChaseState { get; private set; }
    public WolfInvestigateState InvestigateState { get; private set; }

    private void Start()
    {
        PatrolState = new WolfPatrolState(this);
        ChaseState = new WolfChaseState(this);
        InvestigateState = new WolfInvestigateState(this);

        // Agregamos a la FSM
        _fsm.AddState(PatrolState);
        _fsm.AddState(ChaseState);
        _fsm.AddState(InvestigateState);

        // Arrancamos patrullando
        _fsm.ChangeState(PatrolState);
    }

    private void Update()
    {
        _fsm.Update();
    }

    // Alertar a la manada mediante un aullido
    public void Howl(Vector3 hunterPos)
    {
        LastKnownHunterPosition = hunterPos;
        IsAlerted = true;
        Debug.Log("¡AULLIDO DE LOBO! ¡Cazador detectado en " + hunterPos + "!");
    }

    //Sensor basico de linea de vista para detectar al cazador|FOV del lobo
    public bool DetectHunter(out Vector3 hunterPos)
    {
        hunterPos = default;
        if (HunterTarget == null) return false;

        Vector3 dirToHunter = HunterTarget.position - transform.position;
        float dist = dirToHunter.magnitude;

        if (dist <= viewRadius)
        {
            if (Vector3.Angle(transform.forward, dirToHunter.normalized) <= viewAngle / 2f)
            {
                // Chequear obstáculos con Linecast
                if (!Physics.Linecast(transform.position, HunterTarget.position, wallMask))
                {
                    hunterPos = HunterTarget.position;
                    return true;
                }
            }
        }
        return false;
    }
}
