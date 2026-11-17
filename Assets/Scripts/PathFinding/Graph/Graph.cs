using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class Graph : MonoBehaviour
{
    private static readonly Vector3 CastOffset = Vector3.up * 0.5f;

    public static Graph Current { get; private set; }

    public List<Node> AllNodes => _nodes;

    [Header("Nodos del Grafo")]
    [SerializeField] private List<Node> _nodes = new();

    [Header("Parámetros de Autogeneración")]
    [SerializeField] private float _maxConnectionDistance = 6f; 
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

                
                if (distance <= _maxConnectionDistance && HasClearPath(nodeA.transform.position, nodeB.transform.position))
                {
                    nodeA.AddNeighbor(nodeB);
                    nodeB.AddNeighbor(nodeA);
                    totalConnections += 2;
                }
            }
        }

      
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

    public Node GetClosestNode(Vector3 position, bool excludeSafeZones = false, bool mustBeVisible = false)
    {
        Node closestNode = null;
        float minSqDistance = Mathf.Infinity;

        foreach (Node node in _nodes)
        {
            if (node == null || node.Blocked) continue;
            if (excludeSafeZones && node.IsSafeZone) continue;

            float sqDist = (node.transform.position - position).sqrMagnitude;
            if (sqDist >= minSqDistance) continue;
            if (mustBeVisible && !HasClearPath(position, node.transform.position)) continue;

            minSqDistance = sqDist;
            closestNode = node;
        }

        return closestNode;
    }

    public Node GetClosestVisibleNode(Vector3 position, bool excludeSafeZones = false)
    {
        Node visibleNode = GetClosestNode(position, excludeSafeZones, mustBeVisible: true);
        return visibleNode != null ? visibleNode : GetClosestNode(position, excludeSafeZones);
    }

    public bool IsInSafeZone(Vector3 position)
    {
        Node closestNode = GetClosestNode(position);
        return closestNode != null && closestNode.IsSafeZone;
    }

    public bool HasClearPath(Vector3 from, Vector3 to)
    {
        return !Physics.Linecast(from + CastOffset, to + CastOffset, _wallMask);
    }

    public bool IsWallAhead(Vector3 origin, Vector3 direction, float radius, float distance, out RaycastHit hit)
    {
        return Physics.SphereCast(origin + CastOffset, radius, direction, out hit, distance, _wallMask);
    }
}