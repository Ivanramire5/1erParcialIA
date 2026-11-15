using UnityEngine;

public class HunterWeapon : MonoBehaviour
{
    private const float HitSignalDuration = 1.8f;

    [Header("Ataque Cuerpo a Cuerpo")]
    [SerializeField] private float _meleeCooldown = 2f;
    [SerializeField] private float _meleeRadius = 3f;
    [SerializeField] private float _meleeDamage = 40f;
    [SerializeField] private RectTransform _hitSignal;

    [Header("Ataque a Distancia")]
    [SerializeField] private float _rangeCooldown = 8f;
    [SerializeField] private float _rangeRadius = 8f;
    [SerializeField] private GameObject _bulletPrefab;
    [SerializeField] private Transform _bulletSpawnPoint;

    private float _meleeTimer;
    private float _rangeTimer;

    public float MeleeRadius => _meleeRadius;
    public float RangeRadius => _rangeRadius;
    public bool CanMeleeAttack => _meleeTimer >= _meleeCooldown;
    public bool CanRangeAttack => _rangeTimer >= _rangeCooldown;

    private void Awake()
    {
        _meleeTimer = _meleeCooldown;
        _rangeTimer = _rangeCooldown;
    }

    public void Tick(float deltaTime)
    {
        _meleeTimer += deltaTime;
        _rangeTimer += deltaTime;
        UpdateHitSignal();
    }

    public void MeleeAttack(Boid boid)
    {
        _meleeTimer = 0f;
        boid.TakeDamage(_meleeDamage);
    }

    public void ShootAt(Transform target)
    {
        _rangeTimer = 0f;

        if (_bulletPrefab == null || _bulletSpawnPoint == null)
        {
            Debug.LogWarning("Falta asignar el prefab de la bala o el punto de salida en HunterWeapon.");
            return;
        }

        Vector3 direction = (target.position - _bulletSpawnPoint.position).normalized;
        GameObject bullet = Instantiate(_bulletPrefab, _bulletSpawnPoint.position, Quaternion.identity);
        bullet.transform.forward = direction;
    }

    private void UpdateHitSignal()
    {
        if (_hitSignal == null) return;

        _hitSignal.gameObject.SetActive(_meleeTimer < HitSignalDuration);
    }
}
