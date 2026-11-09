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
    public int boidsCollected = 0; 

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
    
    public AttackState Attack { get; private set; }
    public GatherState Gather { get; private set; }

    [Header("Objetivos Actuales")]
    public Transform CurrentTarget; 
    public Transform DeadTarget;    
    public Transform ThreatTarget; 

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
        initialPosition = new Vector3(transform.position.x, 0f, transform.position.z);
        transform.position = initialPosition;
        currentHealth = maxHealth;

        currentMeleeTBATimer = MeleeTBA;
        currentRangeTBATimer = RangeTBA;

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
        currentMeleeTBATimer += Time.deltaTime;
        currentRangeTBATimer += Time.deltaTime;
        
        _senseTimer += Time.deltaTime;

        FSM.Update();

        if (HitSignal != null)
        {
            HitSignal.gameObject.SetActive(currentMeleeTBATimer < 1.8f);
        }
    }

    #region Sensores (OPTIMIZADO Y BLINDADO)
    public void SenseEnvironment(float viewRadius, ref Transform aliveBoid, ref Transform deadBoid, ref Transform wolf)
    {
        if (_senseTimer < _senseInterval)
        {
            aliveBoid = _lastSeenAlive;
            deadBoid = _lastSeenDead;
            wolf = _lastSeenWolf;
            return;
        }
        
        _senseTimer = 0f;
        
        Collider[] hits = Physics.OverlapSphere(transform.position, viewRadius, Physics.AllLayers, QueryTriggerInteraction.Ignore);
        
        float closestAliveDist = Mathf.Infinity;
        float closestDeadDist = Mathf.Infinity;
        float closestWolfDist = Mathf.Infinity;

        aliveBoid = null;
        deadBoid = null;
        wolf = null;

        Vector3 eyePos = transform.position + Vector3.up;

        foreach (Collider hit in hits)
        {
            if (hit.transform == transform) continue; 

            Vector3 targetPos = hit.transform.position + Vector3.up;
            
            if (Physics.Linecast(eyePos, targetPos, obstacleLayer))
            {
                continue; 
            }

            if (hit.CompareTag("Wolf"))
            {
                float dist = Vector3.Distance(transform.position, hit.transform.position);
                if (dist < closestWolfDist)
                {
                    closestWolfDist = dist;
                    wolf = hit.transform;
                }
            }

            Boid boid = hit.GetComponent<Boid>();
            if (boid != null)
            {
                float dist = Vector3.Distance(transform.position, boid.transform.position);
                
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
    #region Reseteo de Cooldowns de Ataque
    public void ResetMeleeTimer()
    {
        currentMeleeTBATimer = 0f;
    }

    public void ResetRangeTimer()
    {
        currentRangeTBATimer = 0f;
    }
    #endregion
}