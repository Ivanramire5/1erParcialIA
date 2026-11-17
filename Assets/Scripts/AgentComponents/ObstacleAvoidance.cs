using UnityEngine;

[System.Serializable]
public class ObstacleAvoidance
{
    [SerializeField] private float _lookAheadDistance = 2f;
    [SerializeField] private float _bodyRadius = 0.6f;
    [SerializeField, Range(0f, 0.95f)] private float _avoidanceWeight = 0.5f;

    public Vector3 AvoidObstacles(Vector3 position, Vector3 moveDirection, float distanceToTarget)
    {
        float lookAhead = Mathf.Min(_lookAheadDistance, distanceToTarget);

        if (!Graph.Current.IsWallAhead(position, moveDirection, _bodyRadius, lookAhead, out RaycastHit hit))
        {
            return moveDirection;
        }

        Vector3 avoidance = hit.normal;
        avoidance.y = 0f;

        return (moveDirection + avoidance * _avoidanceWeight).normalized;
    }
}
