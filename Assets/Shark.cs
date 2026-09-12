using System.Collections.Generic;
using UnityEngine;

public class Shark : MonoBehaviour
{
    [SerializeField] private float _maxSpeed = 5f;
    [SerializeField] private float _maxSteering = 5f;

    [Tooltip("Distancia a la que el tiburón se come al pez")]
    [SerializeField] private float _eatDistance = 0.5f;

    private Transform _transform;
    private Vector3 _velocity;
    private List<Fish> _fishesInRange = new List<Fish>();

    public enum SteeringModes { Seek, Flee, Arrive, Pursuit, Evade }
    public SteeringModes currentSteering;
    private void Awake()
    {
        _transform = transform; // Cacheado de transform
    }

    private void Update()
    {
        float dt = Time.deltaTime; // Cacheamos Time.deltaTime una vez por frame

        Fish targetFish = GetClosestFishAndCleanup(out float closestDistSqr);

        if (targetFish != null)
        {
            if (closestDistSqr <= (_eatDistance * _eatDistance))
            {
                targetFish.GetEaten();
            }
            else
            {
                Seek(targetFish.transform.position, dt);
            }

            _transform.position += _velocity * dt;

            if (_velocity != Vector3.zero)
            {
                _transform.forward = _velocity.normalized;
            }
        }
        else
        {
            // Frena suavemente si no hay presas
            _velocity = Vector3.Lerp(_velocity, Vector3.zero, dt * 5f);
            _transform.position += _velocity * dt;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<Fish>(out Fish fish))
        {
            fish.SetThreat(_transform);

            if (!_fishesInRange.Contains(fish))
            {
                _fishesInRange.Add(fish);
                currentSteering = SteeringModes.Pursuit;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent<Fish>(out Fish fish))
        {
            fish.ClearThreat(_transform); // El pez se calma al salir del rango
            _fishesInRange.Remove(fish);
        }
    }

    // Método unificado : Limpia y busca simultáneamente 
    private Fish GetClosestFishAndCleanup(out float minDistanceSqr)
    {
        Fish closest = null;
        minDistanceSqr = Mathf.Infinity;
        Vector3 currentPos = _transform.position;

        for (int i = _fishesInRange.Count - 1; i >= 0; i--)
        {
            Fish fish = _fishesInRange[i];

            if (fish == null)
            {
                _fishesInRange.RemoveAt(i);
            }
            else
            {
                float distSqr = (fish.transform.position - currentPos).sqrMagnitude;
                if (distSqr < minDistanceSqr)
                {
                    minDistanceSqr = distSqr;
                    closest = fish;
                }
            }
        }

        return closest;
    }

    private void Seek(Vector3 targetPos, float dt)
    {
        var desired = DesiredVector(targetPos);
        _velocity += CalculateSteering(desired, dt);
    }

    private Vector3 CalculateSteering(Vector3 desired, float dt)
    {
        Vector3 steering = desired - _velocity;
        steering = Vector3.ClampMagnitude(steering, _maxSteering * dt);
        return steering;
    }

    private Vector3 DesiredVector(Vector3 targetPos)
    {
        Vector3 desired = (targetPos - _transform.position).normalized;
        desired *= _maxSpeed;
        return desired;
    }

    private void Pursuit(Vector3 targetPos, float dt)
    {

    }
}