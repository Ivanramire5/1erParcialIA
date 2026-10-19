using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Graph : MonoBehaviour
{
    private Node[,] _grid;
    
    // Lista plana de todos los nodos para facilitar búsquedas[cite: 2]
    public List<Node> GetAllGridNodes => _grid.Cast<Node>().ToList();

    [Header("Grid setup")]
    [SerializeField] private int _gridWidth = 3;
    [SerializeField] private int _gridHeight = 3;
    [SerializeField] private float _nodeOffset = 1.1f;

    [Header("References")]
    [SerializeField] private Node _nodePrefab;

    // Singleton básico para acceder globalmente[cite: 2]
    public static Graph Current { get; private set; }

    private void Awake()
    {
        if (Current != null) Destroy(gameObject);
        else Current = this;
    }

    void Start()
    {
        GenerateGrid();
    }

    public void GenerateGrid()
    {
        _grid = new Node[_gridWidth, _gridHeight];

        for (int x = 0; x < _grid.GetLength(0); x++)
        {
            for (int y = 0; y < _grid.GetLength(1); y++)
            {
                _grid[x, y] = Instantiate(_nodePrefab);
                _grid[x, y].transform.SetParent(transform);
                // Ubicamos físicamente el nodo[cite: 2]
                _grid[x, y].transform.position = new Vector3(x * _nodeOffset, 0f, y * _nodeOffset);
                // Le asignamos sus coordenadas lógicas[cite: 2]
                _grid[x, y].Setup(new Coordinates(x, y));
            }
        }
    }

    public List<Node> GetNeighbors(Node current)
    {
        List<Node> neighbors = new();
        Coordinates coordinates = current.Coordinates;

        foreach (Coordinates coord in coordinates.GetAllAdyacentDirections)
        {
            // Evitamos salirnos de los límites de la matriz[cite: 2]
            if (coord.X < 0 || coord.X >= _grid.GetLength(0)) continue;
            if (coord.Y < 0 || coord.Y >= _grid.GetLength(1)) continue;

            neighbors.Add(_grid[coord.X, coord.Y]);
        }

        return neighbors;
    }

    // Enlaza la posición física del lobo o del cazador con el nodo matemático más cercano
    public Node GetClosestNode(Vector3 position)
    {
        Node closestNode = null;
        float minDistance = float.MaxValue;

        foreach (Node node in GetAllGridNodes)
        {
            // Ignoramos nodos que son paredes para no generar caminos rotos
            if (node == null || node.Blocked) continue;

            float distance = Vector3.Distance(position, node.transform.position);
            
            if (distance < minDistance)
            {
                minDistance = distance;
                closestNode = node;
            }
        }

        return closestNode;
    }
}