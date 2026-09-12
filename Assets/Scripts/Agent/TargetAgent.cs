using System.Collections.Generic;
using UnityEngine;

public class TargetAgent : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private float _maxSpeed = 5f;
    [SerializeField] private float _maxClamp = 10f;
    [SerializeField] private float _viewRadius = 5f;
    [SerializeField] private float _arriveRadius = 2f;
    [SerializeField] private float _interactRadius = 1.5f;

    [Header("Flocking Values")]
    [SerializeField] private float _separationRadius = 2f;
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

    private static readonly List<TargetAgent> _allAgents = new();

    private void Awake()
    {
        _allAgents.Add(this);
    }

    private void Start()
    {
        currentHealth = maxHealth;
        Vector3 randomVector = new(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f));
        _velocity = randomVector.normalized * _maxSpeed;
    }

    private void Update()
    {
        if (IsDead)
        {
            _velocity = Vector3.zero;
            return;
        }

        Transform hunter = null;
        Transform closestObject = null;
        SenseEnvironment(ref hunter, ref closestObject);

        Vector3 steering = Vector3.zero;

        if (hunter != null)
        {
            steering += CalculateFlee(hunter.position) + CalculateSeparation(_allAgents, _separationRadius) * _separationWeight;
        }
        else if (closestObject != null)
        {
            steering += CalculateArrive(closestObject.position) + CalculateSeparation(_allAgents, _separationRadius) * _separationWeight;
            InteractWithObject(closestObject);
        }
        else
        {
            steering += CalculateFlocking();
        }

        _velocity += steering;
        _velocity.y = 0f;
        _velocity = Vector3.ClampMagnitude(_velocity, _maxSpeed);

        transform.position += _velocity * Time.deltaTime;

        if (_velocity.sqrMagnitude > 0.001f)
        {
            transform.forward = _velocity;
        }

        transform.position = Bounds.Instance.CalculateBoundPosition(transform.position);
    }

    private void SenseEnvironment(ref Transform hunter, ref Transform closestObject)
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, _viewRadius);
        float closestDist = Mathf.Infinity;

        foreach (Collider hit in hits)
        {
            if (hit.CompareTag("Hunter"))
            {
                hunter = hit.transform;
                return;
            }
            else if (hit.CompareTag("InterestObject"))
            {
                float dist = Vector3.Distance(transform.position, hit.transform.position);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closestObject = hit.transform;
                }
            }
        }
    }

    private void InteractWithObject(Transform targetObject)
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

    #region Flocking y Movimiento
    private Vector3 CalculateFlocking()
    {
        Vector3 force = CalculateSeparation(_allAgents, _separationRadius) * _separationWeight
                      + CalculateAlignment(_allAgents, _viewRadius) * _alignmentWeight
                      + CalculateCohesion(_allAgents, _viewRadius) * _cohesionWeight
                      + CalculateObstacleAvoidance() * _obstacleWeight;

        return force;
    }

    private Vector3 CalculateCohesion(IEnumerable<TargetAgent> agents, float radius)
    {
        Vector3 desiredPosition = Vector3.zero;
        int count = 0;
        foreach (TargetAgent item in agents)
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

    private Vector3 CalculateSeparation(IEnumerable<TargetAgent> agents, float radius)
    {
        Vector3 desired = Vector3.zero;
        int count = 0;
        foreach (TargetAgent item in agents)
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

    private Vector3 CalculateAlignment(IEnumerable<TargetAgent> agents, float radius)
    {
        Vector3 desired = Vector3.zero;
        int count = 0;
        foreach (TargetAgent item in agents)
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

    private Vector3 CalculateSeek(Vector3 targetPosition)
    {
        Vector3 dir = (targetPosition - transform.position);
        Vector3 desired = dir.normalized * _maxSpeed;
        return CalculateSteering(desired);
    }

    private Vector3 CalculateFlee(Vector3 targetPosition)
    {
        Vector3 dir = (transform.position - targetPosition);
        Vector3 desired = dir.normalized * _maxSpeed;
        return CalculateSteering(desired);
    }

    private Vector3 CalculateArrive(Vector3 targetPosition)
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

    private Vector3 CalculateObstacleAvoidance()
    {
        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, _obstacleViewDistance, _obstacleLayer))
        {
            Vector3 desired = hit.normal * _maxSpeed;
            return CalculateSteering(desired);
        }
        return Vector3.zero;
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

        if (IsDead)
        {
            OnDie();
        }
    }

    

  

    public void OnDie()
    {
        // El boid se detiene
        _velocity = Vector3.zero;

        // Convertimos el collider en Trigger para que el cazador no lo empuje al acercarse
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = true;
        }

        Debug.Log("Un boid ha muerto. Esperando recolección.");
    }

    private void Respawn()
    {
        transform.position = new Vector3(Random.Range(-10f, 10f), transform.position.y, Random.Range(-10f, 10f));
        currentHealth = maxHealth;
        _velocity = new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f)).normalized * _maxSpeed;

        // Volvemos a hacer el collider sólido cuando resucita
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = false;
        }

        gameObject.SetActive(true);
    }
    public void OnCollected()
    {
        gameObject.SetActive(false);
        Invoke(nameof(Respawn), respawnTime);
    }

    private void OnDestroy()
    {
        _allAgents.Remove(this);
    }
    #endregion
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _viewRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _separationRadius);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position,_interactRadius);
    }
}