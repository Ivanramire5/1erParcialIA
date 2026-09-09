using UnityEngine;

public class InterestObject : MonoBehaviour
{
    public float health = 30f;

    public void TakeDamage(float amount)
    {
        health -= amount;
        if (health <= 0)
        {
            Destroy(gameObject);
        }
    }
}