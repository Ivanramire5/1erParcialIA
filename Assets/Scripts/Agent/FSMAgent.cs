using System.Collections.Generic;
using UnityEngine;

public class FSMAgent : MonoBehaviour
{
    [Header("Stats del Cazador")]
    public float Speed = 5f;
    [SerializeField] public float _viewRadius;

    [Header("Obstacle Avoidance")]
    public float obstacleViewDistance = 3f;
    public float obstacleWeight = 5f;
    public LayerMask obstacleLayer;

    [Header("Ataque Cuerpo a Cuerpo")]
    public float MeleeTBA = 2f;
    public float currentMeleeTBATimer;
    public float MeleeAttackRadius = 2f;
    public float MeleeAttackDamage = 40f;

    [Header("Ataque a Distancia")]
    public float RangeTBA = 5f;
    public float currentRangeTBATimer;
    public float RangeAttackRadius = 7f;

    [Header("Visual Settings")]
    [SerializeField] private Renderer childRenderer;

    private static readonly int ColorPropID = Shader.PropertyToID("_BaseColor"); 
    private MaterialPropertyBlock _propBlock;

    [Header("Disparo Settings")]
    public GameObject bulletPrefab;
    public Transform spawnBulletPoint;

    [Header("Steering Settings")]
    public float maxClamp = 10f;
    private Vector3 _velocity;

    [Header("Interest Objects / Traps Settings")]
    public GameObject interestObjectPrefab;
    [SerializeField, Tooltip("Tiempo en segundos entre cada aparición de un objeto de interés")]
    private float timeToSpawnObject = 5f;
    private float _objectSpawnTimer = 0f;
    private List<GameObject> _activeInterestObjects = new List<GameObject>();

    [Header("Patrol Data")]
    public PatrolData patrolData;

    // Máquina de Estados y Estados
    public FiniteStateMachine FSM { get; private set; }
    public PatrolState Patrol { get; private set; }
    public AttackState Attack { get; private set; }
    public GatherState Gather { get; private set; }
    public IdleState Idle { get; private set; }

    // Objetivos
    public Transform CurrentTarget;
    public Transform DeadTarget;
    private void Awake()
    {
        _propBlock = new MaterialPropertyBlock();

        // Si no se asignó en el Inspector, busca la ruta exacta desde la raíz
        if (childRenderer == null)
        {
            Transform targetChild = transform.Find("Hunter/MTRL_Hunter");
            if (targetChild != null)
            {
                childRenderer = targetChild.GetComponent<Renderer>();
            }
            else
            {
                Debug.LogError("No se encontró el objeto hijo en la ruta Hunter/MTRL_Hunter");
            }
        }
    }
    public void SetChildColor(Color newColor)
    {
        if (childRenderer == null) return;

        childRenderer.GetPropertyBlock(_propBlock);
        _propBlock.SetColor(ColorPropID, newColor);
        childRenderer.SetPropertyBlock(_propBlock);
    }
    private void Start()
    {
        FSM = new FiniteStateMachine();

        Patrol = new PatrolState(patrolData, this);
        Attack = new AttackState(this);
        Gather = new GatherState(this);
        Idle = new IdleState(this);

        FSM.AddState(Patrol);
        FSM.AddState(Attack);
        FSM.AddState(Gather);
        FSM.AddState(Idle);

        currentMeleeTBATimer = MeleeTBA;
        currentRangeTBATimer = RangeTBA;

        FSM.ChangeState(Patrol);
    }

    private void Update()
    {
        currentMeleeTBATimer += Time.deltaTime;
        currentRangeTBATimer += Time.deltaTime;

        FSM.Update();
    }

    #region Disparo (Ranged Attack)
    public void FireBullet(Transform target)
    {
        if (bulletPrefab != null && spawnBulletPoint != null)
        {
            // Apuntamos la dirección hacia el target
            Vector3 direction = (target.position - spawnBulletPoint.position).normalized;

            // Instanciamos la bala
            GameObject bulletObj = Instantiate(bulletPrefab, spawnBulletPoint.position, Quaternion.identity);
            
            // Hacemos que mire hacia donde tiene que ir
            bulletObj.transform.forward = direction;


        }
        else
        {
            Debug.LogWarning("Falta asignar el prefab de la bala o el spawn point en el FSMAgent.");
        }
    }
    #endregion

    #region Movimiento (Steering)
    public void SeekTo(Vector3 targetPosition)
    {
        Vector3 direction = targetPosition - transform.position;
        direction.y = 0f;

        Vector3 desired = direction.normalized * Speed;

        Vector3 steering = desired - _velocity;
        steering.y = 0f;
        steering = Vector3.ClampMagnitude(steering, maxClamp) * Time.deltaTime;

        // Sumamos la fuerza de esquivar obstáculos al steering principal
        steering += CalculateObstacleAvoidance() * obstacleWeight;

        _velocity += steering;
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

        Vector3[] directions = new Vector3[]
        {
        rayDir,
        Quaternion.Euler(0, 35, 0) * rayDir,
        Quaternion.Euler(0, -35, 0) * rayDir
        };

        foreach (Vector3 dir in directions)
        {
            if (Physics.Raycast(transform.position, dir, out RaycastHit hit, obstacleViewDistance, obstacleLayer))
            {
                Vector3 desired = hit.normal * Speed;
                Vector3 steering = desired - _velocity;
                return Vector3.ClampMagnitude(steering, maxClamp) * Time.deltaTime;
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
        _objectSpawnTimer += Time.deltaTime;

        if (_objectSpawnTimer >= timeToSpawnObject)
        {
            _objectSpawnTimer = 0f;
            _activeInterestObjects.RemoveAll(obj => obj == null || !obj.activeInHierarchy);

            if (_activeInterestObjects.Count < 5)
            {
            
                Vector3 spawnPosition = new Vector3(transform.position.x, 0.4f, transform.position.z);

                GameObject newInterestObject = Instantiate(interestObjectPrefab, spawnPosition, Quaternion.identity);
                _activeInterestObjects.Add(newInterestObject);
            }
        }
    }
    #endregion

    #region Sensores
    public void SenseEnvironment(float viewRadius, ref Transform aliveBoid, ref Transform deadBoid)
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, viewRadius);
        float closestAliveDist = Mathf.Infinity;
        float closestDeadDist = Mathf.Infinity;

        foreach (Collider hit in hits)
        {
            TargetAgent boid = hit.GetComponent<TargetAgent>();
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
    }
    #endregion

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _viewRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, MeleeAttackRadius);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, RangeAttackRadius);



        Vector3 rayDir = _velocity.sqrMagnitude > 0.001f ? _velocity.normalized : transform.forward;
        Vector3[] directions = new Vector3[]
        {
        rayDir,
        Quaternion.Euler(0, 35, 0) * rayDir,
        Quaternion.Euler(0, -35, 0) * rayDir
        };

        foreach (Vector3 dir in directions)
        {
            if (Physics.Raycast(transform.position, dir, out RaycastHit hit, obstacleViewDistance, obstacleLayer))
            {
                // Rojo: Rayo bloqueado por un obstáculo y punto de impacto
                Gizmos.color = Color.red;
                Gizmos.DrawLine(transform.position, hit.point);
                Gizmos.DrawWireSphere(hit.point, 0.15f);

                // Dibuja la normal del impacto (dirección del rebote/fuerza)
                Gizmos.color = Color.yellow;
                Gizmos.DrawRay(hit.point, hit.normal * 1.5f);
            }
            else
            {
                // Verde: Camino despejado hasta la distancia máxima de visión
                Gizmos.color = Color.green;
                Gizmos.DrawRay(transform.position, dir * obstacleViewDistance);
            }
        }
    }
}