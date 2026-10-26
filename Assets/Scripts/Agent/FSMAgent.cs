using System.Collections.Generic;
using UnityEngine;

public class FSMAgent : MonoBehaviour
{
    [Header("Stats del Cazador")]
    public float Speed = 5f;
    [SerializeField] public float _viewRadius = 15f;

    [Header("Health System")]
    public float maxHealth = 100f;
    public float currentHealth;
    public bool IsDead => currentHealth <= 0;

    [Header("Variables de Juego")]
    public int RequiredGathers = 3;
    public float RespawnTime = 5f;
    public float SafeZoneWaitTime = 4f;
    public int boidsCollected = 0; // Contador de recolecciones

    [Header("Zonas del Mapa")]
    [Tooltip("Arrastrá acá el Transform del punto de Respawn (Zona Inicial)")]
    public Transform respawnPoint;
    [HideInInspector] public Vector3 initialPosition;

    [Header("Obstacle Avoidance (Corto Alcance)")]
    public float obstacleViewDistance = 3f;
    public float obstacleWeight = 5f;
    public LayerMask obstacleLayer;

    [Header("Ataque Cuerpo a Cuerpo")]
    public float MeleeTBA = 2f;
    public float currentMeleeTBATimer;
    public float MeleeAttackRadius = 2f;
    public float MeleeAttackDamage = 40f;
    public RectTransform HitSignal;

    [Header("Ataque a Distancia")]
    public float RangeTBA = 5f;
    public float currentRangeTBATimer;
    public float RangeAttackRadius = 7f;
    public GameObject bulletPrefab;
    public Transform spawnBulletPoint;

    [Header("Visual Settings")]
    [SerializeField] private Renderer childRenderer;
    private static readonly int ColorPropID = Shader.PropertyToID("_BaseColor");
    private MaterialPropertyBlock _propBlock;

    [Header("Steering Settings")]
    public float maxClamp = 10f;
    private Vector3 _velocity;

    [Header("Trampas Settings")]
    public GameObject interestObjectPrefab;
    public float timeToSpawnObject = 5f;
    [HideInInspector] public float objectSpawnTimer = 0f;
    [HideInInspector] public List<GameObject> activeInterestObjects = new List<GameObject>();

    // Máquina de Estados 
    public FiniteStateMachine FSM { get; private set; }
    
    public HunterSearchState Search { get; private set; }
    public HunterFleeState Flee { get; private set; }
    public HunterExitState Exit { get; private set; }
    public HunterDeadState Dead { get; private set; }
    
    // Estados Clásicos adaptados
    public AttackState Attack { get; private set; }
    public GatherState Gather { get; private set; }

    [Header("Objetivos Actuales")]
    public Transform CurrentTarget; // Presa viva
    public Transform DeadTarget;    // Presa muerta
    public Transform ThreatTarget;  // Lobo detectado

    // --- VARIABLES DE OPTIMIZACIÓN DE RENDIMIENTO ---
    private float _senseTimer = 0f;
    private readonly float _senseInterval = 0.2f;
    private Transform _lastSeenAlive = null;
    private Transform _lastSeenDead = null;
    private Transform _lastSeenWolf = null;

    private void Awake()
    {
        _propBlock = new MaterialPropertyBlock();
        if (childRenderer == null)
        {
            Transform targetChild = transform.Find("Hunter/MTRL_Hunter");
            if (targetChild != null) childRenderer = targetChild.GetComponent<Renderer>();
        }
    }

    public void SetChildColor(Color newColor)
    {
        if (childRenderer == null) return;
        childRenderer.GetPropertyBlock(_propBlock);
        _propBlock.SetColor(ColorPropID, newColor);
        childRenderer.SetPropertyBlock(_propBlock);
    }
    
    #region Apaga o enciende al cazador
    public void SetVisible(bool isVisible)
    {
        if (childRenderer != null)
        {
            childRenderer.enabled = isVisible;
        }
    }
    #endregion

    private void Start()
    {
        initialPosition = transform.position;
        currentHealth = maxHealth;
        FSM = new FiniteStateMachine();

        // Inicializamos los estados
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

        currentMeleeTBATimer = MeleeTBA;
        currentRangeTBATimer = RangeTBA;

        // El estado inicial obligatorio ahora es Search
        FSM.ChangeState(Search);
    }

    private void Update()
    {
        currentMeleeTBATimer += Time.deltaTime;
        currentRangeTBATimer += Time.deltaTime;

        FSM.Update();

        if (HitSignal != null)
        {
            HitSignal.gameObject.SetActive(currentMeleeTBATimer < 1.8f);
        }
    }

    #region Sensores (OPTIMIZADO)
    // Ahora devuelve la presa viva, la muerta y al Lobo más cercano con escaneo optimizado
    public void SenseEnvironment(float viewRadius, ref Transform aliveBoid, ref Transform deadBoid, ref Transform wolf)
    {
        _senseTimer += Time.deltaTime;
        
        // Si no pasó el tiempo de escaneo, devolvemos lo que guardamos en la caché
        if (_senseTimer < _senseInterval)
        {
            aliveBoid = _lastSeenAlive;
            deadBoid = _lastSeenDead;
            wolf = _lastSeenWolf;
            return;
        }

        _senseTimer = 0f;

        Collider[] hits = Physics.OverlapSphere(transform.position, viewRadius);
        float closestAliveDist = Mathf.Infinity;
        float closestDeadDist = Mathf.Infinity;
        float closestWolfDist = Mathf.Infinity;

        // Limpiamos las referencias actuales para actualizarlas
        aliveBoid = null;
        deadBoid = null;
        wolf = null;

        foreach (Collider hit in hits)
        {
            // 1. Detectar Lobos (La máxima prioridad)
            if (hit.CompareTag("Wolf"))
            {
                float dist = Vector3.Distance(transform.position, hit.transform.position);
                if (dist < closestWolfDist)
                {
                    // Comprobamos si hay pared en medio usando tu obstacleLayer
                    if (!Physics.Linecast(transform.position + Vector3.up, hit.transform.position + Vector3.up, obstacleLayer))
                    {
                        closestWolfDist = dist;
                        wolf = hit.transform;
                    }
                }
            }

            // 2. Detectar Boids
            Boid boid = hit.GetComponent<Boid>();
            if (boid != null)
            {
                float dist = Vector3.Distance(transform.position, boid.transform.position);
                
                // Comprobamos línea de visión
                if (!Physics.Linecast(transform.position + Vector3.up, boid.transform.position + Vector3.up, obstacleLayer))
                {
                    if (boid.IsDead)
                    {
                        if (dist < closestDeadDist)
                        {
                            closestDeadDist = dist;
                            deadBoid = boid.transform;
                        }
                    }
                    else
                    {
                        if (dist < closestAliveDist)
                        {
                            closestAliveDist = dist;
                            aliveBoid = boid.transform;
                        }
                    }
                }
            }
        }

        // Guardamos los resultados para el próximo ciclo
        _lastSeenAlive = aliveBoid;
        _lastSeenDead = deadBoid;
        _lastSeenWolf = wolf;
    }
    #endregion

    #region El damage que recibe el cazador 
    public void TakeDamage(float amount)
    {
        if (IsDead) return;
        currentHealth -= amount;
        Debug.Log($"Cazador recibió {amount} de daño. Vida restante: {currentHealth}");
    }
    #endregion

    #region Movimiento Corto Alcance (Steering)
    public void SeekTo(Vector3 targetPosition)
    {
        Vector3 direction = targetPosition - transform.position;
        direction.y = 0f;

        Vector3 desired = direction.normalized * Speed;
        Vector3 seekSteering = desired - _velocity;
        seekSteering.y = 0f;
        seekSteering = Vector3.ClampMagnitude(seekSteering, maxClamp);

        Vector3 obstacleSteering = CalculateObstacleAvoidance() * obstacleWeight;

        Vector3 totalSteering = seekSteering + obstacleSteering;
        totalSteering = Vector3.ClampMagnitude(totalSteering, maxClamp) * Time.deltaTime;

        _velocity += totalSteering;
        _velocity.y = 0f;
        _velocity = Vector3.ClampMagnitude(_velocity, Speed);

        transform.position += _velocity * Time.deltaTime;

        if (_velocity.sqrMagnitude > 0.001f)
        {
            transform.forward = _velocity.normalized;
        }
    }

    private Vector3 CalculateObstacleAvoidance()
    {
        Vector3 rayDir = _velocity.sqrMagnitude > 0.001f ? _velocity.normalized : transform.forward;
        Vector3[] directions = { rayDir, Quaternion.Euler(0, 35, 0) * rayDir, Quaternion.Euler(0, -35, 0) * rayDir };
        Vector3 rayOrigin = transform.position + Vector3.up * 0.5f;

        foreach (Vector3 dir in directions)
        {
            if (Physics.Raycast(rayOrigin, dir, out RaycastHit hit, obstacleViewDistance, obstacleLayer))
            {
                Vector3 avoidanceDir = hit.normal;
                avoidanceDir.y = 0f;
                Vector3 desired = avoidanceDir.normalized * Speed;
                Vector3 steering = desired - _velocity;
                steering.y = 0f;
                return Vector3.ClampMagnitude(steering, maxClamp);
            }
        }
        return Vector3.zero;
    }

    public void StopVelocity()
    {
        _velocity = Vector3.zero;
    }
    #endregion

    #region Objetos de Interés (Trampas)
    public void TrySpawnInterestObject()
    {
        objectSpawnTimer += Time.deltaTime;

        if (objectSpawnTimer >= timeToSpawnObject)
        {
            objectSpawnTimer = 0f;
            activeInterestObjects.RemoveAll(obj => obj == null || !obj.activeInHierarchy);

            if (activeInterestObjects.Count < 5)
            {
                Vector3 spawnPosition = new Vector3(transform.position.x, 0.4f, transform.position.z);
                GameObject newInterestObject = Instantiate(interestObjectPrefab, spawnPosition, Quaternion.identity);
                activeInterestObjects.Add(newInterestObject);
            }
        }
    }
    #endregion
    
    #region Disparo (Ranged Attack)
    public void FireBullet(Transform target)
    {
        if (bulletPrefab != null && spawnBulletPoint != null)
        {
            Vector3 direction = (target.position - spawnBulletPoint.position).normalized;
            GameObject bulletObj = Instantiate(bulletPrefab, spawnBulletPoint.position, Quaternion.identity);
            bulletObj.transform.forward = direction;
        }
        else
        {
            Debug.LogWarning("Falta asignar el prefab de la bala o el spawn point en el FSMAgent.");
        }
    }
    #endregion
}