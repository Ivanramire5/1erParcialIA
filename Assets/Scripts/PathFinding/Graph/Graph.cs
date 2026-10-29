using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class Graph : MonoBehaviour
{
    public static Graph Current { get; private set; }

    public List<Node> AllNodes => _nodes;

    [Header("Nodos del Grafo")]
    [SerializeField] private List<Node> _nodes = new();

    [Header("Parámetros de Autogeneración")]
    [SerializeField] private float _maxConnectionDistance = 6f; // Distancia máxima para conectar
    [SerializeField] private LayerMask _wallMask; 

    private void Awake()
    {
        if (Current == null)
        {
            Current = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        LoadNodes();
    }

    [ContextMenu("Recargar Nodos")]
    public void LoadNodes()
    {
        _nodes = new List<Node>(GetComponentsInChildren<Node>());
    }

    [ContextMenu("Generar Conexiones Automáticas")]
    public void GenerateConnections()
    {
        LoadNodes();

        foreach (var node in _nodes)
        {
            node.ClearNeighbors();
        }

        int totalConnections = 0;

        for (int i = 0; i < _nodes.Count; i++)
        {
            for (int j = i + 1; j < _nodes.Count; j++)
            {
                Node nodeA = _nodes[i];
                Node nodeB = _nodes[j];

                float distance = Vector3.Distance(nodeA.transform.position, nodeB.transform.position);

                // 1. Validar distancia máxima
                if (distance <= _maxConnectionDistance)
                {
                    // 2. Validar línea de visión
                    Vector3 posA = nodeA.transform.position + Vector3.up * 0.5f;
                    Vector3 posB = nodeB.transform.position + Vector3.up * 0.5f;

                    // Si NO choca contra la WallMask, hay camino libre
                    if (!Physics.Linecast(posA, posB, _wallMask))
                    {
                        nodeA.AddNeighbor(nodeB);
                        nodeB.AddNeighbor(nodeA);
                        totalConnections += 2; // Bidireccional
                    }
                }
            }
        }

        // Marcar los nodos como modificados para que Unity los guarde en el Inspector
#if UNITY_EDITOR
        foreach (var node in _nodes)
        {
            EditorUtility.SetDirty(node);
        }
        EditorUtility.SetDirty(this);
#endif

        Debug.Log($"<color=cyan>¡Grafo generado con éxito! Nodos totales: {_nodes.Count} | Conexiones totales creadas: {totalConnections}</color>");
    }

    public List<Node> GetNeighbors(Node node)
    {
        if (node == null) return new List<Node>();
        return node.Neighbors;
    }

    /// <summary>
    /// Encuentra el nodo transitable más cercano a una posición del mundo.
    /// </summary>
    public Node GetClosestNode(Vector3 position, bool excludeSafeZones = false)
    {
        Node closestNode = null;
        float minSqDistance = Mathf.Infinity;

        foreach (Node node in _nodes)
        {
            if (node == null || node.Blocked) continue;

            // Si el agente no puede entrar a zonas seguras, filtramos el nodo
            if (excludeSafeZones && node.IsSafeZone) continue;

            float sqDist = (node.transform.position - position).sqrMagnitude;
            if (sqDist < minSqDistance)
            {
                minSqDistance = sqDist;
                closestNode = node;
            }
        }

        return closestNode;
    }
}