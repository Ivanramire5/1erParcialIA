using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour
{
    [Header("Configuración del Nodo")]
    [SerializeField] private int _cost = 1;
    public int Cost => _cost;

    [SerializeField] private bool _blocked = false;
    public bool Blocked
    {
        get => _blocked;
        set => _blocked = value;
    }

    [Header("Zona Segura (Restricción de Lobos)")]
    [SerializeField] private bool _isSafeZone = false;
    public bool IsSafeZone => _isSafeZone;

    [Header("Conexiones")]
    [SerializeField] private List<Node> _neighbors = new();
    public List<Node> Neighbors => _neighbors;

    public void AddNeighbor(Node neighbor)
    {
        if (neighbor != null && !_neighbors.Contains(neighbor) && neighbor != this)
        {
            _neighbors.Add(neighbor);
        }
    }

    public void ClearNeighbors()
    {
        _neighbors.Clear();
    }

    private void OnDrawGizmos()
    {
        // Color según estado (Magenta = Zona Segura, Rojo = Bloqueado, Amarillo = Normal)
        Gizmos.color = _isSafeZone ? Color.magenta : (_blocked ? Color.red : Color.yellow);
        Gizmos.DrawSphere(transform.position, 0.3f);

        // Dibujar líneas hacia los vecinos
        Gizmos.color = Color.green;
        foreach (var neighbor in _neighbors)
        {
            if (neighbor != null)
            {
                Gizmos.DrawLine(transform.position + Vector3.up * 0.1f, neighbor.transform.position + Vector3.up * 0.1f);
            }
        }
    }
}