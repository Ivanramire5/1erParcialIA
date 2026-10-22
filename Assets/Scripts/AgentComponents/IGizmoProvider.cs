using System.Collections.Generic;
using UnityEngine;

public enum GizmoType
{
    WireSphere,
    VisionCone
}

public struct GizmoData
{
    public GizmoType Type;
    public float Radius;
    public float Angle; // Útil para el cono de visión
    public Color Color;
    public Vector3 Offset;

    // Constructor para Esferas
    public GizmoData(float radius, Color color, Vector3 offset = default)
    {
        Type = GizmoType.WireSphere;
        Radius = radius;
        Angle = 0f;
        Color = color;
        Offset = offset;
    }

    // Constructor para Conos de Visión
    public GizmoData(float radius, float angle, Color color, Vector3 offset = default)
    {
        Type = GizmoType.VisionCone;
        Radius = radius;
        Angle = angle;
        Color = color;
        Offset = offset;
    }
}

public interface IGizmoProvider
{
    IEnumerable<GizmoData> GetGizmosToDraw();
}