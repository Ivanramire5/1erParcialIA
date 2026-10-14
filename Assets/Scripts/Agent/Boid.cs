using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ColorController))]
public class Boid : MonoBehaviour
{
    public VisionSensor Sensor { get; private set; }

    [Header("Stats")]
    [SerializeField] private float _maxSpeed = 5f;
    [SerializeField] private float _maxClamp = 10f;

    [SerializeField, Tooltip("Radio de visión matemática para el Flocking")]
    private float _viewRadius = 5f;
    public float ViewRadius => _viewRadius;

    [SerializeField] private float _arriveRadius = 2f;

    [SerializeField] private float _interactRadius = 1.5f;
    public float InteractRadius => _interactRadius;

    [Header("Flocking Values")]
    [SerializeField] private float _separationRadius = 2f;
    public float SeparationRadius => _separationRadius;

    [SerializeField, Range(0f, 3f)] private float _separationWeight = 1.5f;
    [SerializeField, Range(0f, 3f)] private float _cohesionWeight = 1f;
    [SerializeField, Range(0f, 3f)] private float _alignmentWeight = 1f;

    [Header("Obstacle Avoidance")]
    [SerializeField] private float _obstacleViewDistance = 3f;
    [SerializeField] private float _obstacleWeight = 5f;
    [SerializeField] private LayerMask _obstacleLayer;

    [Header("Health & Respawn")]
    public float maxHealth = 100f;
    public float currentHealth;
    public float respawnTime = 3f;

    private Vector3 _velocity;
    public Vector3 Velocity => _velocity;
    public bool IsDead => currentHealth <= 0;

    private ColorController _colorController;
    public static readonly List<Boid> AllAgents = new();

    // --- FSM y Estados ---
    public FiniteStateMachine FSM { get; private set; }
    public FlockState StateFlock { get; private set; }
    public FleeState StateFlee { get; private set; }
    public InteractState StateInteract { get; private set; }
    public DeadState StateDead { get; private set; }

    private void Awake()
    {
        AllAgents.Add(this);
        _colorController = GetComponent<ColorController>();
        Sensor = GetComponentInChildren<VisionSensor>();
    }

    private void Start()
    {
        currentHealth = maxHealth;
        Vector3 randomVector = new(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f));
        _velocity = randomVector.normalized * _maxSpeed;

        // Inicializamos la FSM y los estados
        FSM = new FiniteStateMachine();
        StateFlock = new FlockState(this);
        StateFlee = new FleeState(this);
        StateInteract = new InteractState(this);
        StateDead = new DeadState(this);

        FSM.AddState(StateFlock);
        FSM.AddState(StateFlee);
        FSM.AddState(StateInteract);
        FSM.AddState(StateDead);

        FSM.ChangeState(StateFlock);
    }

    private void Update()
    {
        FSM.Update();
    }

    public void ApplyMovement(Vector3 steering)
    {
        _velocity += steering;
        _velocity.y = 0f;
        _velocity = Vector3.ClampMagnitude(_velocity, _maxSpeed);

        transform.position += _velocity * Time.deltaTime;

        if (_velocity.sqrMagnitude > 0.001f)
        {
            transform.forward = _velocity;
        }
    }

    public void SetColor(Color color)
    {
        _colorController.SetColor(color);
    }

    public void InteractWithObject(Transform targetObject)
    {
        if (Vector3.Distance(transform.position, targetObject.position) <= _interactRadius)
        {
            InterestObject obj = targetObject.GetComponent<InterestObject>();
            if (obj != null)
            {
                TakeDamage(obj.TrapDamage * Time.deltaTime);
                obj.TakeDamage(obj.TrapDamage * Time.deltaTime);
            }
        }
    }

    #region Flocking y Steering Públicos
    public Vector3 CalculateFlocking()
    {
        return CalculateSeparation(AllAgents, _separationRadius) * _separationWeight
             + CalculateAlignment(AllAgents, _viewRadius) * _alignmentWeight
             + CalculateCohesion(AllAgents, _viewRadius) * _cohesionWeight
             + CalculateObstacleAvoidance() * _obstacleWeight;
    }

    public Vector3 CalculateSeparation(IEnumerable<Boid> agents, float radius)
    {
        Vector3 desired = Vector3.zero;
        int count = 0;
        foreach (Boid item in agents)
        {
            if (item == this || item.IsDead) continue;
            if (InRange(item.transform.position, radius))
            {
                Vector3 diff = transform.position - item.transform.position;
                desired += diff;
                count++;
            }
        }
        if (count == 0) return Vector3.zero;
        desired /= count;
        return CalculateSteering(desired.normalized * _maxSpeed);
    }

    private Vector3 CalculateAlignment(IEnumerable<Boid> agents, float radius)
    {
        Vector3 desired = Vector3.zero;
        int count = 0;
        foreach (Boid item in agents)
        {
            if (item == this || item.IsDead) continue;
            if (InRange(item.transform.position, radius))
            {
                Vector3 vel = item.Velocity;
                desired += vel;
                count++;
            }
        }
        if (count == 0) return Vector3.zero;
        desired /= count;
        return CalculateSteering(desired.normalized * _maxSpeed);
    }

    private Vector3 CalculateCohesion(IEnumerable<Boid> agents, float radius)
    {
        Vector3 desiredPosition = Vector3.zero;
        int count = 0;
        foreach (Boid item in agents)
        {
            if (item == this || item.IsDead) continue;
            if (InRange(item.transform.position, radius))
            {
                desiredPosition += item.transform.position;
                count++;
            }
        }
        if (count == 0) return Vector3.zero;
        desiredPosition /= count;
        return CalculateSeek(desiredPosition);
    }

    private Vector3 CalculateObstacleAvoidance()
    {
        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, _obstacleViewDistance, _obstacleLayer))
        {
            Vector3 desired = hit.normal * _maxSpeed;
            return CalculateSteering(desired);
        }
        return Vector3.zero;
    }

    private Vector3 CalculateSeek(Vector3 targetPosition)
    {
        Vector3 dir = (targetPosition - transform.position);
        Vector3 desired = dir.normalized * _maxSpeed;
        return CalculateSteering(desired);
    }

    public Vector3 CalculateFlee(Vector3 targetPosition)
    {
        Vector3 dir = (transform.position - targetPosition);
        Vector3 desired = dir.normalized * _maxSpeed;
        return CalculateSteering(desired);
    }

    public Vector3 CalculateArrive(Vector3 targetPosition)
    {
        Vector3 dir = targetPosition - transform.position;
        float distance = dir.magnitude;
        float speed = _maxSpeed;

        if (distance <= _arriveRadius)
        {
            speed *= (distance / _arriveRadius);
        }

        Vector3 desired = dir.normalized * speed;
        return CalculateSteering(desired);
    }

    private Vector3 CalculateSteering(Vector3 desired)
    {
        Vector3 steering = desired - _velocity;
        return Vector3.ClampMagnitude(steering, _maxClamp) * Time.deltaTime;
    }

    private bool InRange(Vector3 position, float radius)
    {
        Vector3 diff = position - transform.position;
        return diff.sqrMagnitude <= radius * radius;
    }
    #endregion

    #region Vida y Recolección
    public void TakeDamage(float amount)
    {
        if (IsDead) return;
        currentHealth -= amount;

        if (IsDead && FSM != null)
        {
            FSM.ChangeState(StateDead);
        }
    }

    public void PrepareForDeath()
    {
        _velocity = Vector3.zero;
        SetColor(Color.black);

        Collider col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true;

        Debug.Log("Un boid ha muerto. Esperando recolección.");
    }

    private void Respawn()
    {
        transform.position = new Vector3(Random.Range(-10f, 10f), transform.position.y, Random.Range(-10f, 10f));
        currentHealth = maxHealth;
        _velocity = new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f)).normalized * _maxSpeed;

        Sensor.ClearSensors();

        Collider col = GetComponent<Collider>();
        if (col != null) col.isTrigger = false;

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = false;

        gameObject.SetActive(true);
        FSM.ChangeState(StateFlock);
    }

    public void OnCollected()
    {
        gameObject.SetActive(false);
        Invoke(nameof(Respawn), respawnTime);
    }

    private void OnDestroy()
    {
        AllAgents.Remove(this);
    }
    #endregion
}

