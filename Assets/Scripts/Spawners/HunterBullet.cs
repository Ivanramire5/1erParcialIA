using UnityEngine;

public class HunterBullet : MonoBehaviour
{
    public float speed = 40f;
   
    public float damage = 70f;
    public float lifetime = 3f; 

    private void Start()
    {
       
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
      
        transform.position += transform.forward * speed * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
       
        TargetAgent boid = other.GetComponent<TargetAgent>();
        if (boid != null && !boid.IsDead)
        {
           
            boid.TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}