using UnityEngine;

[DisallowMultipleComponent]
public class GenericAgentGizmos : MonoBehaviour
{
    private IGizmoProvider _gizmoProvider;

    private void Awake()
    {
        ResolveProvider();
    }

    private void ResolveProvider()
    {
        if (_gizmoProvider == null)
        {
            _gizmoProvider = GetComponent<IGizmoProvider>();
        }
    }

    private void OnDrawGizmosSelected()
    {
        ResolveProvider();
        if (_gizmoProvider == null) return;

        foreach (var gizmo in _gizmoProvider.GetGizmosToDraw())
        {
            Gizmos.color = gizmo.Color;
            Vector3 center = transform.position + gizmo.Offset;

            if (gizmo.Type == GizmoType.WireSphere)
            {
                Gizmos.DrawWireSphere(center, gizmo.Radius);
            }
            else if (gizmo.Type == GizmoType.VisionCone)
            {
                DrawVisionCone(center, gizmo.Radius, gizmo.Angle);
            }
        }
    }

    private void DrawVisionCone(Vector3 position, float radius, float angle)
    {
        // Dibuja el radio de visión base
        Gizmos.DrawWireSphere(position, radius);

        // Dibuja los límites del ángulo de visión
        Quaternion leftRayRotation = Quaternion.AngleAxis(-angle * 0.5f, transform.up);
        Quaternion rightRayRotation = Quaternion.AngleAxis(angle * 0.5f, transform.up);

        Vector3 leftRayDirection = leftRayRotation * transform.forward;
        Vector3 rightRayDirection = rightRayRotation * transform.forward;

        Gizmos.DrawRay(position, leftRayDirection * radius);
        Gizmos.DrawRay(position, rightRayDirection * radius);
    }
}