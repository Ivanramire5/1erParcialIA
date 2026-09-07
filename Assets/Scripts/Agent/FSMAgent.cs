using System.Collections.Generic; 
using UnityEngine;

public class FSMAgent : MonoBehaviour 
{ 
    [Header("Stats Base")]
    [SerializeField] private float _speed = 3f; 
    public float Speed => _speed;

    [SerializeField] private PatrolData _patrolData;

    [Header("Variables Obligatorias Parcial")]

    public float TBA = 3f; //Tiempo mínimo entre ataques
    public float RangeAttackRadius = 10f; //Distancia máxima para realizar ataques a distancia.
    public float MeleeAttackRadius = 2f;  //Distancia máxima para realizar ataques cuerpo a cuerpo

    [Header("Objetivos del Cazador")]

    public Transform CurrentTarget; 
    public Transform DeadTarget;

    // Temporizador para controlar el TBA
    public float currentTBATimer = 0f; 

    private readonly FiniteStateMachine _fsm = new();
    public FiniteStateMachine FSM => _fsm;

    // Declaración de todos los estados
    public IdleState Idle { get; private set; }
    public PatrolState Patrol { get; private set; }
    public AttackState Attack { get; private set; }
    public GatherState Gather { get; private set; }

    private void Start()
    {
        //Inicializamos todos los estados
        Idle = new IdleState(this);
        Patrol = new PatrolState(_patrolData, this);
        Attack = new AttackState(this);
        Gather = new GatherState(this);

        //Agregamos los estados a la FSM
        _fsm.AddState(Idle);
        _fsm.AddState(Patrol);
        _fsm.AddState(Attack);
        _fsm.AddState(Gather);

        //Iniciamos la máquina de estados
        _fsm.ChangeState(Idle);
    }

    private void Update()
    {
        //Actualizamos el temporizador del ataque
        if (currentTBATimer < TBA)
        {
            currentTBATimer += Time.deltaTime;
        }

        //Ejecutamos el Update del estado actual
        _fsm.Update();
    }
}