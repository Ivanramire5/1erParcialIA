using System.Collections.Generic;
using UnityEngine;

public class TargetAgent : MonoBehaviour
{
    private enum SteeringModes { Seek, Flee, Arrive, Evade, Flocking }

    [Header("Stats")]
    [SerializeField]
    private float _maxSpeed = 5f;
    [SerializeField]
    private float _maxClamp = 10f;
    [SerializeField]
    private float _viewRadius = 5f;
    [SerializeField]
    private SteeringModes _currentMode;
    [SerializeField]
    private float _arriveRadius = 3f;

    private Vector3 _velocity;
    public Vector3 Velocity => _velocity;


    [Header("Flocking values")]
    [SerializeField]
    private float _separationRadius = 2f;
    [SerializeField, Range(0f, 3f)]
    private float _separationWeight = 1f;
    [SerializeField, Range(0f, 3f)]
    private float _cohesionWeight = 1f;
    [SerializeField, Range(0f, 3f)]
    private float _alignmentWeight = 1f;


    [Header("References")]
    [SerializeField]
    private Transform target;
   
    private TargetAgent targetAgent;

    private static readonly List<TargetAgent> _allAgents = new();


    private void Awake()
    {
        _allAgents.Add(this);
    }

    private void Start()
    {
        Vector3 randomVector = new(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f));
        _velocity += randomVector.normalized * _maxSpeed;
    }

    private void Update()
    {
        if (_currentMode == SteeringModes.Flocking) CalculateFlocking();
        else if (_currentMode == SteeringModes.Evade) _velocity += CalculateEvade(targetAgent);
        else _velocity += GetCurrentSteeringMode();

        transform.position += _velocity * Time.deltaTime;
        transform.forward = _velocity;
        transform.position = Bounds.Instance.CalculateBoundPosition(transform.position);
    }
    private Vector3 GetCurrentSteeringMode() => GetSteering(_currentMode, target.position);
    private Vector3 GetSteering(SteeringModes mode, Vector3 targetPosition)
    {
        return mode switch
        {
            SteeringModes.Seek => CalculateSeek(targetPosition),
            SteeringModes.Flee => CalculateFlee(targetPosition),
            SteeringModes.Arrive => CalculateArrive(targetPosition),
            _ => Vector3.zero,
        };
    }

    private Vector3 CalculateSteering(Vector3 desired)
    {
        Vector3 steering = desired - _velocity;
        steering = Vector3.ClampMagnitude(steering, _maxClamp);
        return steering * Time.deltaTime;
    }
    private Vector3 GetFuturePosition(TargetAgent target)
    {
        float distanceToTarget = (target.transform.position - transform.position).magnitude;
        float predictedTime = distanceToTarget / (_maxSpeed + target.Velocity.magnitude);
        return target.transform.position + target.Velocity * predictedTime;
    }
    private Vector3 CalculateEvade(TargetAgent target) => CalculateFlee(GetFuturePosition(target));
    private Vector3 CalculateFlee(Vector3 targetPosition)
    {
        Vector3 desired = (targetPosition - transform.position).normalized * _maxSpeed;
        return CalculateSteering(-desired);
    }
    private Vector3 CalculateSeek(Vector3 targetPosition)
    {
        Vector3 desired = (targetPosition - transform.position).normalized * _maxSpeed;
        return CalculateSteering(desired);
    }
    private Vector3 CalculateArrive(Vector3 targetPosition)
    {
        Vector3 dir = (targetPosition - transform.position);
        float speed = _maxSpeed;
        float distance = dir.magnitude;

        if (distance <= _arriveRadius)
        {
            float percentDistance = distance / _arriveRadius;
            speed *= percentDistance;
        }

        Vector3 desired = dir.normalized * speed;
        return CalculateSteering(desired);
    }
    private void CalculateFlocking()
    {
        _velocity += CalculateSeparation(_allAgents, _separationRadius) * _separationWeight
                    + CalculateAlignment(_allAgents, _viewRadius) * _alignmentWeight
                    + CalculateCohesion(_allAgents, _viewRadius) * _cohesionWeight;
    
    }
    private Vector3 CalculateCohesion(IEnumerable<TargetAgent> agents, float radius)
    {
        Vector3 desiredPosition = default;
        int count = 0;

        foreach (TargetAgent item in agents)
        {
            if (item == this) continue;
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
        Vector3 desired = default;
        int count = 0;

        foreach (TargetAgent item in agents)
        {
            if (item == this) continue;
            if (InRange(item.transform.position, radius))
            {
                desired += (item.transform.position - transform.position);
                count++;
            }
        }

        if (count == 0) return Vector3.zero;

        desired /= count;

        return CalculateSteering(-desired.normalized * _maxSpeed);
    }
    private Vector3 CalculateAlignment(IEnumerable<TargetAgent> agents, float radius)
    {
        Vector3 desired = default;
        int count = 0;

        foreach (TargetAgent item in agents)
        {
            if (item == this) continue;
            if (InRange(item.transform.position, radius))
            {
                desired += item.Velocity;
                count++;
            }
        }
        if (count == 0) return Vector3.zero;
        desired /= count;

        return CalculateSteering(desired.normalized * _maxSpeed);
    }
    private bool InRange(Vector3 position, float radius) => (position - transform.position).sqrMagnitude <= radius * radius;
}