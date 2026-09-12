using System.Collections.Generic;
using UnityEngine;

public class FSMAgent : MonoBehaviour
{
    [Header("Stats del Cazador")]
    public float Speed = 5f;
    [SerializeField] public float _viewRadius;

    [Header("Ataque Cuerpo a Cuerpo")]
    public float MeleeTBA = 2f;
    public float currentMeleeTBATimer;
    public float MeleeAttackRadius = 2f;
    public float MeleeAttackDamage = 40f;

    [Header("Ataque a Distancia")]
    public float RangeTBA = 5f;
    public float currentRangeTBATimer;
    public float RangeAttackRadius = 7f;
  

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

        _velocity += steering;
        _velocity.y = 0f;
        _velocity = Vector3.ClampMagnitude(_velocity, Speed);

        transform.position += _velocity * Time.deltaTime;

        if (_velocity.sqrMagnitude > 0.001f)
        {
            transform.forward = _velocity.normalized;
        }
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
    }
}