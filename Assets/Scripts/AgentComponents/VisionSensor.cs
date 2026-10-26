using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class VisionSensor : MonoBehaviour
{
    [Header("Field of View Settings")]
    [SerializeField, Range(0f, 360f)] private float _viewAngle = 90f;
    public float ViewAngle => _viewAngle;

    [SerializeField] private float _eyeHeight = 0.5f;
    public float EyeHeight => _eyeHeight;

    private SphereCollider _sensorCollider;
    public float ViewRadius => _sensorCollider != null ? _sensorCollider.radius : 0f;

    [Header("Filtros por Layer")]
    [SerializeField] private LayerMask _hunterMask;
    [SerializeField] private LayerMask _objectMask;
    [SerializeField] private LayerMask _obstacleMask;
    [SerializeField] private LayerMask _wolfMask;

    private readonly List<Transform> _detectedHunters = new();
    private readonly List<Transform> _detectedObjects = new();
    private readonly List<Transform> _detectedWolves = new();

    // --- VARIABLES DE OPTIMIZACIÓN ---
    private float _senseTimer = 0.2f; // Arranca en 0.2f para que escanee en el frame 1
    private readonly float _senseInterval = 0.2f;
    private Transform _cachedHunter;
    private Transform _cachedObject;
    private Transform _cachedWolf;

    public bool HasVisibleHunters => _cachedHunter != null;
    public bool HasVisibleObjects => _cachedObject != null;
    public bool HasVisibleWolves => _cachedWolf != null;

    private void Awake()
    {
        _sensorCollider = GetComponent<SphereCollider>();
        _sensorCollider.isTrigger = true;

        Boid parentBoid = GetComponentInParent<Boid>();
        if (parentBoid != null)
        {
            _sensorCollider.radius = parentBoid.ViewRadius;
        }
    }

    // --- NUEVO: LOOP DE OPTIMIZACIÓN ---
    private void Update()
    {
        _senseTimer += Time.deltaTime;
        
        // Ejecutamos los Raycasts y cálculos pesados solo 5 veces por segundo
        if (_senseTimer >= _senseInterval)
        {
            _cachedHunter = GetClosestVisibleFromList(_detectedHunters, transform.position);
            _cachedObject = GetClosestVisibleFromList(_detectedObjects, transform.position);
            _cachedWolf = GetClosestVisibleFromList(_detectedWolves, transform.position);
            
            _senseTimer = 0f;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (IsAValidTarget(other.gameObject.layer, _hunterMask) && !_detectedHunters.Contains(other.transform))
        {
            _detectedHunters.Add(other.transform);
        }
        else if (IsAValidTarget(other.gameObject.layer, _objectMask) && !_detectedObjects.Contains(other.transform))
        {
            _detectedObjects.Add(other.transform);
        }
        else if (IsAValidTarget(other.gameObject.layer, _wolfMask) && !_detectedWolves.Contains(other.transform))
        {
            _detectedWolves.Add(other.transform);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (IsAValidTarget(other.gameObject.layer, _hunterMask))
        {
            _detectedHunters.Remove(other.transform);
        }
        else if (IsAValidTarget(other.gameObject.layer, _objectMask))
        {
            _detectedObjects.Remove(other.transform);
        }
        else if (IsAValidTarget(other.gameObject.layer, _wolfMask))
        {
            _detectedWolves.Remove(other.transform);
        }
    }

    // Ahora simplemente devolvemos la variable guardada para no gastar procesador
    public Transform GetClosestVisibleHunter(Vector3 currentPosition) => _cachedHunter;
    public Transform GetClosestVisibleWolf(Vector3 currentPosition) => _cachedWolf;
    public Transform GetClosestVisibleObject(Vector3 currentPosition) => _cachedObject;

    private Transform GetClosestVisibleFromList(List<Transform> list, Vector3 currentPosition)
    {
        Transform closest = null;
        float minDistance = Mathf.Infinity;

        for (int i = list.Count - 1; i >= 0; i--)
        {
            Transform target = list[i];

            if (target == null || !target.gameObject.activeInHierarchy)
            {
                list.RemoveAt(i);
                continue;
            }

            if (!CanSeeTarget(target)) continue;

            float dist = Vector3.Distance(currentPosition, target.position);
            if (dist < minDistance)
            {
                minDistance = dist;
                closest = target;
            }
        }

        return closest;
    }

    private static bool IsAValidTarget(int layer, LayerMask mask) => (mask.value & (1 << layer)) != 0;

    private bool CanSeeTarget(Transform target)
    {
        if (IsAValidTarget(target.gameObject.layer, _hunterMask))
        {
            if (Graph.Current != null)
            {
                Node closestNode = Graph.Current.GetClosestNode(target.position);
                // Los lobos no pueden ver al cazador si está en Zona Segura[cite: 5, 8]
                if (closestNode != null && closestNode.IsSafeZone)
                {
                    return false;
                }
            }
        }

        Vector3 dirToTarget = (target.position - transform.position).normalized;
        if (Vector3.Angle(transform.forward, dirToTarget) > _viewAngle * 0.5f)
        {
            return false;
        }

        Vector3 eyePos = transform.position + Vector3.up * _eyeHeight;
        Vector3 targetPos = target.position + Vector3.up * _eyeHeight;
        Vector3 direction = targetPos - eyePos;

        if (Physics.Raycast(eyePos, direction.normalized, out RaycastHit hit, direction.magnitude, _obstacleMask))
        {
            return hit.transform == target || hit.transform.IsChildOf(target);
        }

        return true;
    }

    public void ClearSensors()
    {
        _detectedHunters.Clear();
        _detectedObjects.Clear();
        _detectedWolves.Clear();
    }
}