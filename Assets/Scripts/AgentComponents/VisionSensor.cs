using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class VisionSensor : MonoBehaviour
{
    [Header("Field of View Settings")]

    [SerializeField, Range(0f, 360f)] private float _viewAngle = 270f;
    [SerializeField] private float _eyeHeight = 0.5f;

    [Header("Filtros por Layer")]
    [SerializeField] private LayerMask _hunterMask;
    [SerializeField] private LayerMask _objectMask;
    [SerializeField] private LayerMask _obstacleMask;// layer que obstaculiza la vision. 

    private readonly List<Transform> _detectedHunters = new();
    private readonly List<Transform> _detectedObjects = new();

    public bool HasVisibleHunters => GetClosestVisibleHunter(transform.position) != null;
    public bool HasVisibleObjects => GetClosestVisibleObject(transform.position) != null;

    private void Awake()
    {
        SphereCollider sensor = GetComponent<SphereCollider>();
        sensor.isTrigger = true;

        // Buscamos el componente Boid en el objeto padre y le asignamos su radio al Collider
        Boid parentBoid = GetComponentInParent<Boid>();
        if (parentBoid != null)
        {
            sensor.radius = parentBoid.ViewRadius;
        }
        else
        {
            Debug.LogWarning("VisionSensor no encontró un Boid en sus objetos padres.", gameObject);
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
    }

    public Transform GetClosestVisibleHunter(Vector3 currentPosition)
    {
        return GetClosestVisibleFromList(_detectedHunters, currentPosition);
    }

    public Transform GetClosestVisibleObject(Vector3 currentPosition)
    {
        return GetClosestVisibleFromList(_detectedObjects, currentPosition);
    }

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
    }
}