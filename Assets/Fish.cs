using UnityEngine;

public class Fish : MonoBehaviour
{
    [Header("Base Fish Settings")]
    [SerializeField] protected float _maxSpeed = 3f;
    [SerializeField] protected float _maxSteering = 3f;

    protected Transform _transform;
    protected Vector3 _velocity;
    protected Transform _threat;

    protected virtual void Awake()
    {
        _transform = transform; // Cacheamos el transform por rendimiento
    }

    protected virtual void Update()
    {
        float dt = Time.deltaTime; 

        if (_threat != null)
        {
            FleeFromThreat(dt);
        }
        else { _velocity = Vector3.zero; }

        // Aplicar movimiento usando dt
        _transform.position += _velocity * dt;

        if (_velocity != Vector3.zero)
        {
            _transform.forward = _velocity.normalized;
        }
    }

    
    protected virtual void FleeFromThreat(float dt)
    {
        Vector3 desired = (_transform.position - _threat.position).normalized * _maxSpeed;
        Vector3 steering = desired - _velocity;
        steering = Vector3.ClampMagnitude(steering, _maxSteering * dt);

        _velocity += steering;
    }

    public virtual void SetThreat(Transform threat)
    {
        _threat = threat;

        // Evita generar basura con strings en producción usando directivas de preprocesador
#if UNITY_EDITOR
        Debug.Log($"Pez {_transform.name} siendo amenazado por {threat.name}");
#endif
    }

    // Método para limpiar la amenaza cuando el tiburón sale de rango
    public virtual void ClearThreat(Transform threat)
    {
        if (_threat == threat)
        {
            _threat = null;
          
        }
   
    }

    public virtual void GetEaten()
    {
#if UNITY_EDITOR
        Debug.Log($" se comieron al pez  {_transform.name}");
#endif
        Destroy(gameObject);
    }
}