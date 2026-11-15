using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(HunterTrapSpawner))]
[RequireComponent(typeof(HunterWeapon))]
public class FSMAgent : MonoBehaviour, IGizmoProvider
{
    [Header("Stats del Cazador")]
    public float Speed = 5f;

    [Header("Health System")]
    public float maxHealth = 100f;
    public float currentHealth;
    public bool IsDead => currentHealth <= 0;

    [Header("Variables de Juego")]
    public int RequiredGathers = 3;
    public float RespawnTime = 5f;
    public float SafeZoneWaitTime = 4f;
    public int boidsCollected = 0; 

    [Header("Zonas del Mapa")]
    [Tooltip("Arrastrá acá el Transform del punto de Respawn (Zona Inicial)")]
    public Transform respawnPoint;
    [HideInInspector] public Vector3 initialPosition;

    public FiniteStateMachine FSM { get; private set; }
    
    public HunterSearchState Search { get; private set; }
    public HunterFleeState Flee { get; private set; }
    public HunterExitState Exit { get; private set; }
    public HunterDeadState Dead { get; private set; }
    
    public AttackState Attack { get; private set; }
    public GatherState Gather { get; private set; }

    [Header("Objetivos Actuales")]
    public Transform CurrentTarget; 
    public Transform DeadTarget;    

    private VisionSensor _visionSensor;
    private ColorController _colorController;

    public HunterTrapSpawner TrapSpawner { get; private set; }
    public HunterWeapon Weapon { get; private set; }

    private void Awake()
    {
        _colorController = GetComponent<ColorController>();
        TrapSpawner = GetComponent<HunterTrapSpawner>();
        Weapon = GetComponent<HunterWeapon>();
        _visionSensor = GetComponentInChildren<VisionSensor>();
        if (_visionSensor == null) Debug.LogError("El cazador necesita un componente VisionSensor.");
    }

    public void SetColor(Color newColor)
    {
        if (_colorController != null) _colorController.SetColor(newColor);
    }

    public void SetVisible(bool isVisible)
    {
        if (_colorController != null) _colorController.SetVisible(isVisible);
    }

    private void Start()
    {
        initialPosition = new Vector3(transform.position.x, 0f, transform.position.z);
        transform.position = initialPosition;
        currentHealth = maxHealth;

        InicializarFSM();
    }

    private void InicializarFSM()
    {
        FSM = new FiniteStateMachine();

        Search = new HunterSearchState(this);
        Flee = new HunterFleeState(this);
        Exit = new HunterExitState(this);
        Dead = new HunterDeadState(this);
        Attack = new AttackState(this);
        Gather = new GatherState(this);

        FSM.AddState(Search);
        FSM.AddState(Flee);
        FSM.AddState(Exit);
        FSM.AddState(Dead);
        FSM.AddState(Attack);
        FSM.AddState(Gather);

        FSM.ChangeState(Search);
    }

    private void Update()
    {
        FSM.Update();
        Weapon.Tick(Time.deltaTime);
    }

    #region Percepcion
    public Transform GetVisibleWolf()
    {
        return _visionSensor.GetClosestVisibleWolf(transform.position);
    }

    public Transform GetVisibleAliveBoid()
    {
        return _visionSensor.GetClosestVisibleObject(transform.position, IsAliveBoid);
    }

    public Transform GetVisibleDeadBoid()
    {
        return _visionSensor.GetClosestVisibleObject(transform.position, IsDeadBoid);
    }

    public bool CanSee(Transform target)
    {
        return _visionSensor.IsObjectVisible(target);
    }

    public bool TryFleeFromWolf()
    {
        if (GetVisibleWolf() == null) return false;

        FSM.ChangeState(Flee);
        return true;
    }

    private static bool IsAliveBoid(Transform target)
    {
        return target.TryGetComponent(out Boid boid) && !boid.IsDead;
    }

    private static bool IsDeadBoid(Transform target)
    {
        return target.TryGetComponent(out Boid boid) && boid.IsDead;
    }
    #endregion

    #region Ciclo de Caza
    public void ContinueHuntOrExit()
    {
        if (boidsCollected >= RequiredGathers)
        {
            FSM.ChangeState(Exit);
        }
        else
        {
            FSM.ChangeState(Search);
        }
    }
    #endregion

    #region El damage que recibe el cazador 
    public void TakeDamage(float amount)
    {
        if (IsDead) return;
        currentHealth -= amount;
        Debug.Log($"Cazador recibió {amount} de daño. Vida restante: {currentHealth}");

        if (IsDead)
        {
            FSM.ChangeState(Dead);
        }
    }
    #endregion

    #region Reaparicion en la Zona inicial
    public void MoveToRespawn()
    {
        Vector3 spawnPos = respawnPoint != null ? respawnPoint.position : initialPosition;
        spawnPos.y = 0f;
        transform.position = spawnPos;
    }
    #endregion

    #region Gizmos
    public IEnumerable<GizmoData> GetGizmosToDraw()
    {
        if (_visionSensor == null)
        {
            _visionSensor = GetComponentInChildren<VisionSensor>();
        }

        if (_visionSensor != null)
        {
            yield return new GizmoData(_visionSensor.ViewRadius, _visionSensor.ViewAngle, Color.cyan);
        }

        HunterWeapon weapon = GetComponent<HunterWeapon>();
        if (weapon != null)
        {
            yield return new GizmoData(weapon.MeleeRadius, Color.red);
            yield return new GizmoData(weapon.RangeRadius, new Color(1f, 0.5f, 0f));
        }
    }
    #endregion
}