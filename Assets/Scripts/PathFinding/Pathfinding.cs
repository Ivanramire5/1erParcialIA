using System.Collections.Generic; 
using UnityEngine; 

public static class Pathfinding 
{ 
    
    public static float GetHeuristic(Vector3 posA, Vector3 posB) 
    { 
        return Vector3.Distance(posA, posB); 
    }

   
    private static float GetStepCost(Node from, Node to)
    {
        return Vector3.Distance(from.transform.position, to.transform.position) * to.Cost;
    }

    public static List<Node> AStar(Node startingNode, Node endNode, System.Func<Node, bool> nodeFilter = null)
    {
        if (startingNode == null || endNode == null)
        {
            Debug.LogError("Invalid node parameters");
            return new();
        }

        PriorityQueue<Node> frontier = new();
        frontier.Enqueue(startingNode, 0);

        Dictionary<Node, Node> cameFrom = new();
        cameFrom.Add(startingNode, null);

        Dictionary<Node, float> costSoFar = new();
        costSoFar.Add(startingNode, 0f);

        Node current;
        Vector3 endPosition = endNode.transform.position;

        while (frontier.Count != 0)
        {
            current = frontier.Dequeue();

            if (current == endNode)
            {
                List<Node> path = new();
                while (current != null)
                {
                    path.Add(current);
                    current = cameFrom[current];
                }
                path.Reverse();
                return path;
            }

            foreach (Node neighbor in Graph.Current.GetNeighbors(current))
            {
                if (neighbor.Blocked) continue;
                if (nodeFilter != null && !nodeFilter(neighbor)) continue;

                float newCost = costSoFar[current] + GetStepCost(current, neighbor);
                
                if (cameFrom.ContainsKey(neighbor) && newCost >= costSoFar[neighbor]) continue;
                
                frontier.Enqueue(neighbor, newCost + GetHeuristic(neighbor.transform.position, endPosition));
                cameFrom[neighbor] = current;
                costSoFar[neighbor] = newCost;
            }
        }

        return new();
    }
}