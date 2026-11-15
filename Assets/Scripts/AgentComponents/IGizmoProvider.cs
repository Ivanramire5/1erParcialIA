using System.Collections.Generic;
using UnityEngine;

public enum GizmoType
{
    WireSphere,
    VisionCone,
    PathLines
}

public struct GizmoData
{
    public GizmoType Type;
    public float Radius;
    public float Angle;
    public Color Color;
    public Vector3 Offset;
    public List<Vector3> PathPoints;

    public GizmoData(float radius, Color color, Vector3 offset = default)
    {
        Type = GizmoType.WireSphere;
        Radius = radius;
        Angle = 0f;
        Color = color;
        Offset = offset;
        PathPoints = null;
    }

    public GizmoData(float radius, float angle, Color color, Vector3 offset = default)
    {
        Type = GizmoType.VisionCone;
        Radius = radius;
        Angle = angle;
        Color = color;
        Offset = offset;
        PathPoints = null;
    }

    public GizmoData(List<Vector3> pathPoints, Color color)
    {
        Type = GizmoType.PathLines;
        Radius = 0f;
        Angle = 0f;
        Color = color;
        Offset = default;
        PathPoints = pathPoints;
    }
}

public interface IGizmoProvider
{
    IEnumerable<GizmoData> GetGizmosToDraw();
}