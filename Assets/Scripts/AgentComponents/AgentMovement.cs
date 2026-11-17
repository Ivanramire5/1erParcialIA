using System.Collections.Generic;
using UnityEngine;

public static class AgentMovement
{
    private const float TurnSpeed = 15f;

    public static void MoveTowards(Transform agent, Vector3 targetPosition, float speed, ObstacleAvoidance avoidance = null)
    {
        Vector3 direction = GetFlatDirection(agent.position, targetPosition);
        if (direction.sqrMagnitude < 0.01f) return;

        if (avoidance != null)
        {
            direction = avoidance.AvoidObstacles(agent.position, direction, FlatDistance(agent.position, targetPosition));
        }

        RotateTowards(agent, direction);
        agent.position += direction * speed * Time.deltaTime;
    }

    public static void MoveAlongPath(Transform agent, List<Node> path, ref int pathIndex, float speed, float arrivalDistance, ObstacleAvoidance avoidance = null)
    {
        Vector3 nodePosition = path[pathIndex].transform.position;

        if (FlatDistance(agent.position, nodePosition) <= arrivalDistance)
        {
            pathIndex++;
            return;
        }

        MoveTowards(agent, nodePosition, speed, avoidance);
    }

    public static void FaceTowards(Transform agent, Vector3 targetPosition)
    {
        Vector3 direction = GetFlatDirection(agent.position, targetPosition);
        if (direction.sqrMagnitude < 0.01f) return;

        RotateTowards(agent, direction);
    }

    public static float FlatDistance(Vector3 from, Vector3 to)
    {
        from.y = 0f;
        to.y = 0f;
        return Vector3.Distance(from, to);
    }

    private static Vector3 GetFlatDirection(Vector3 from, Vector3 to)
    {
        Vector3 direction = to - from;
        direction.y = 0f;
        return direction.normalized;
    }

    private static void RotateTowards(Transform agent, Vector3 direction)
    {
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        agent.rotation = Quaternion.Slerp(agent.rotation, targetRotation, Time.deltaTime * TurnSpeed);
    }
}