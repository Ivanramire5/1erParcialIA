using UnityEngine;

[RequireComponent(typeof(Boid))]
public class BoidGizmos : MonoBehaviour
{
    private Boid _agent;

    private void Awake()
    {
        _agent = GetComponent<Boid>();
    }

    private void OnDrawGizmosSelected()
    {
        // En modo edición (antes de darle Play), Awake no se ejecuta, por lo que buscamos la referencia si es nula.
        if (_agent == null)
        {
            _agent = GetComponent<Boid>();
        }

        if (_agent == null) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _agent.ViewRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _agent.SeparationRadius);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, _agent.InteractRadius);
    }
}