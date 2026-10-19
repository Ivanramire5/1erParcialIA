using System.Collections.Generic;
using UnityEngine;

// Estructura de coordenadas que utiliza tu Graph.cs para mapear los vecinos
public struct Coordinates
{
    public int X;
    public int Y;

    public Coordinates(int x, int y)
    {
        X = x;
        Y = y;
    }

    // Direcciones adyacentes (Arriba, Abajo, Izquierda, Derecha)
    public IEnumerable<Coordinates> GetAllAdyacentDirections
    {
        get
        {
            yield return new Coordinates(X, Y + 1);
            yield return new Coordinates(X, Y - 1);
            yield return new Coordinates(X + 1, Y);
            yield return new Coordinates(X - 1, Y);
        }
    }
}

public class Node : MonoBehaviour
{
    public Coordinates Coordinates { get; private set; }
    
    [Header("Configuración del Nodo")]
    public bool Blocked = false; // Define si es transitable o un muro
    public int Cost = 1;         // Costo de movimiento para el A*

    private Renderer _renderer;

    private void Awake()
    {
        _renderer = GetComponent<Renderer>();
    }

    // Tu script Graph.cs llama a esta función al generar la grilla
    public void Setup(Coordinates coords)
    {
        Coordinates = coords;
    }

    // Utilizado por tu PathfindVisualizer para pintar el camino
    public void ChangeColor(Color color)
    {
        if (_renderer != null)
        {
            _renderer.material.color = color;
        }
    }
}