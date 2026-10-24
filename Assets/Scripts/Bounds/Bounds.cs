using UnityEngine;

public class Bounds : MonoBehaviour
{
    public static Bounds Instance { get; private set; }

    [SerializeField] private float _height = 34f;
    [SerializeField] private float _width = 60f;
    [SerializeField] private bool _drawGizmos;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public Vector3 OutOfBounds(Vector3 position) 
    {
        Vector3 newPosition = position;

        if (position.x > _width / 2) newPosition.x = -_width / 2;
        if (position.x < -_width/2 ) newPosition.x = _width / 2;
        if(position.z > _height / 2) newPosition.z = -_height / 2;
        if( position.z < -_height/2 ) newPosition.z= _height / 2;

        return newPosition;
    }
    public Vector3 CalculateBoundPosition(Vector3 position) 
    {
        Vector3 newPosition = position;
        Vector3 center = transform.position; 

        float halfWidth = _width / 2f;
        float halfHeight = _height / 2f;

      
        if (position.x > center.x + halfWidth) newPosition.x = center.x - halfWidth;
        if (position.x < center.x - halfWidth) newPosition.x = center.x + halfWidth;

        if (position.z > center.z + halfHeight) newPosition.z = center.z - halfHeight;
        if (position.z < center.z - halfHeight) newPosition.z = center.z + halfHeight;

        return newPosition;
    }

    private void OnDrawGizmos()
    {
        if (!_drawGizmos) return;

        Gizmos.color = Color.cyan;
      
        Gizmos.DrawWireCube(transform.position, new Vector3(_width, 0f, _height));
    }
}