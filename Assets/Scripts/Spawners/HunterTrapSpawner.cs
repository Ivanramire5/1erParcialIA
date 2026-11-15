using System.Collections.Generic;
using UnityEngine;

public class HunterTrapSpawner : MonoBehaviour
{
    private const int MaxActiveTraps = 5;

    [SerializeField] private GameObject _trapPrefab;
    [SerializeField] private float _timeToSpawn = 3f;

    private float _spawnTimer = 0f;
    private readonly List<GameObject> _activeTraps = new List<GameObject>();

    public void TrySpawnTrap()
    {
        _spawnTimer += Time.deltaTime;
        if (_spawnTimer < _timeToSpawn) return;

        _spawnTimer = 0f;
        RemoveDestroyedTraps();

        if (_activeTraps.Count >= MaxActiveTraps) return;

        SpawnTrap();
    }

    private void RemoveDestroyedTraps()
    {
        _activeTraps.RemoveAll(trap => trap == null || !trap.activeInHierarchy);
    }

    private void SpawnTrap()
    {
        Vector3 spawnPosition = new Vector3(transform.position.x, 0.4f, transform.position.z);
        _activeTraps.Add(Instantiate(_trapPrefab, spawnPosition, Quaternion.identity));
    }
}
