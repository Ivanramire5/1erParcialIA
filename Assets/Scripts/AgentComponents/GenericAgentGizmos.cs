using System.Collections.Generic;
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

    private void OnDrawGizmos()
    {
        ResolveProvider();
        if (_gizmoProvider == null) return;

        foreach (var gizmo in _gizmoProvider.GetGizmosToDraw())
        {
            Gizmos.color = gizmo.Color;
            Vector3 center = transform.position + gizmo.Offset;

            switch (gizmo.Type)
            {
                case GizmoType.WireSphere:
                    Gizmos.DrawWireSphere(center, gizmo.Radius);
                    break;

                case GizmoType.VisionCone:
                    DrawVisionCone(center, gizmo.Radius, gizmo.Angle);
                    break;

                case GizmoType.PathLines:
                    DrawPathGizmos(gizmo.PathPoints, gizmo.Color);
                    break;
            }
        }
    }

    private void DrawVisionCone(Vector3 position, float radius, float angle)
    {
        Gizmos.DrawWireSphere(position, radius);

        Quaternion leftRayRotation = Quaternion.AngleAxis(-angle * 0.5f, transform.up);
        Quaternion rightRayRotation = Quaternion.AngleAxis(angle * 0.5f, transform.up);

        Vector3 leftRayDirection = leftRayRotation * transform.forward;
        Vector3 rightRayDirection = rightRayRotation * transform.forward;

        Gizmos.DrawRay(position, leftRayDirection * radius);
        Gizmos.DrawRay(position, rightRayDirection * radius);
    }

    private void DrawPathGizmos(List<Vector3> points, Color color)
    {
        if (points == null || points.Count < 2) return;

        Gizmos.color = color;
        for (int i = 0; i < points.Count - 1; i++)
        {
            Gizmos.DrawLine(points[i] + Vector3.up * 0.2f, points[i + 1] + Vector3.up * 0.2f);
            Gizmos.DrawSphere(points[i] + Vector3.up * 0.2f, 0.2f);
        }
        Gizmos.DrawSphere(points[points.Count - 1] + Vector3.up * 0.2f, 0.2f);
    }
}