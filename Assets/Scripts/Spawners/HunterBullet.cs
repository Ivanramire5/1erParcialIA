using UnityEngine;

public class HunterBullet : MonoBehaviour
{
    public float speed = 20f;
    public float damage = 50f;
    public float lifetime = 3f; // Tiempo antes de destruirse si no choca con nada

    private void Start()
    {
        // Destruir la bala después de un tiempo para no llenar la memoria
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        // Mover la bala hacia adelante
        transform.position += transform.forward * speed * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Verificamos si chocamos contra un Boid
        TargetAgent boid = other.GetComponent<TargetAgent>();
        if (boid != null && !boid.IsDead)
        {
            // Aplicamos el daño y destruimos la bala
            boid.TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}