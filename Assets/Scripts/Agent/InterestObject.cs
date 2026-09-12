using UnityEngine;

public class InterestObject : MonoBehaviour
{
    [SerializeField] private float _trapDamage = 40f;
    public float TrapDamage => _trapDamage;
    public float health = 100f;

    public void TakeDamage(float amount)
    {
        health -= amount;
        if (health <= 0)
        {
            Destroy(gameObject);
        }
    }
}