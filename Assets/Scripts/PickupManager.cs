/*
using Unity.Netcode;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PickupManager : MonoBehaviour
{
    [Header("Pickup Settings")]
    [SerializeField] private GameObject _healthPickupPrefab;
    [SerializeField] private Transform[] _spawnPoints;
    [SerializeField] private float _respawnDelay = 10f;

    [Header("Debug")]
    [SerializeField] private bool _showGizmos = true;

    private Dictionary<Vector3, bool> _spawnPointOccupied;
    private List<Vector3> _availablePositions;

    private void Start()
    {
        if (!NetworkManager.Singleton || !NetworkManager.Singleton.IsServer)
        {
            enabled = false;
            return;
        }

        InitializeSpawnPoints();
        SpawnAllPickups();

        Debug.Log($"[PickupManager] Initialized with {_spawnPoints.Length} spawn points");
    }

    private void InitializeSpawnPoints()
    {
        _spawnPointOccupied = new Dictionary<Vector3, bool>();
        _availablePositions = new List<Vector3>();

        foreach (var point in _spawnPoints)
        {
            if (point != null)
            {
                Vector3 pos = point.position;
                _spawnPointOccupied[pos] = false;
                _availablePositions.Add(pos);
            }
        }
    }

    private void SpawnAllPickups()
    {
        foreach (var point in _spawnPoints)
        {
            if (point != null)
            {
                SpawnPickup(point.position);
            }
        }
    }

    private void SpawnPickup(Vector3 position)
    {
        if (_healthPickupPrefab == null) return;

        GameObject pickupGO = Instantiate(_healthPickupPrefab, position, Quaternion.identity);
        NetworkObject pickupNO = pickupGO.GetComponent<NetworkObject>();
        HealthPickup pickup = pickupGO.GetComponent<HealthPickup>();

        if (pickup != null)
        {
            pickup.Init(this, position);
        }

        if (pickupNO != null)
        {
            pickupNO.Spawn();
            _spawnPointOccupied[position] = true;

            Debug.Log($"[PickupManager] Spawned pickup at {position}");
        }
    }

    public void OnPickedUp(Vector3 position)
    {
        StartCoroutine(RespawnAfterDelay(position));
    }

    private IEnumerator RespawnAfterDelay(Vector3 position)
    {
        yield return new WaitForSeconds(_respawnDelay);

        SpawnPickup(position);
    }

    private void OnDrawGizmos()
    {
        if (!_showGizmos) return;

        Gizmos.color = Color.green;

        foreach (var point in _spawnPoints)
        {
            if (point != null)
            {
                Gizmos.DrawWireSphere(point.position, 1f);
                Gizmos.DrawLine(point.position, point.position + Vector3.up * 2f);
            }
        }
    }
}
*/

using Unity.Netcode;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PickupManager : MonoBehaviour
{
    [SerializeField] private GameObject _healthPickupPrefab;
    [SerializeField] private Transform[] _spawnPoints;
    [SerializeField] private float _respawnDelay = 10f;
    [SerializeField] private int _initialSpawnCount = 3;

    private bool _isInitialized;

    private void Awake()
    {
        Debug.Log("[PickupManager] Awake called");
    }

    private void Start()
    {
        Debug.Log("[PickupManager] Start called");

        if (NetworkManager.Singleton == null)
        {
            Debug.LogError("[PickupManager] NetworkManager.Singleton is NULL!");
            enabled = false;
            return;
        }

        if (_healthPickupPrefab == null)
        {
            Debug.LogError("[PickupManager] HealthPickup prefab NOT assigned!");
            enabled = false;
            return;
        }

        if (_spawnPoints == null || _spawnPoints.Length == 0)
        {
            Debug.LogError("[PickupManager] Spawn points NOT assigned!");
            enabled = false;
            return;
        }

        Debug.Log("[PickupManager] Subscribing to OnServerStarted");
        NetworkManager.Singleton.OnServerStarted += OnServerStarted;
    }

    private void OnServerStarted()
    {
        Debug.Log("[PickupManager] OnServerStarted called");
        Debug.Log($"[PickupManager] IsServer now: {NetworkManager.Singleton.IsServer}");

        if (!NetworkManager.Singleton.IsServer)
        {
            Debug.Log("[PickupManager] Not server, skipping initialization");
            return;
        }

        Initialize();
    }

    private void Initialize()
    {
        Debug.Log("[PickupManager] Initialize called");

        if (_isInitialized)
        {
            Debug.LogWarning("[PickupManager] Already initialized!");
            return;
        }

        _isInitialized = true;

        Debug.Log($"[PickupManager] Spawning {_initialSpawnCount} health pickups");

        int count = Mathf.Min(_initialSpawnCount, _spawnPoints.Length);

        var shuffled = new List<Transform>(_spawnPoints);

        for (int i = 0; i < shuffled.Count; i++)
        {
            int j = Random.Range(i, shuffled.Count);
            var temp = shuffled[i];
            shuffled[i] = shuffled[j];
            shuffled[j] = temp;
        }

        for (int i = 0; i < count; i++)
        {
            Debug.Log($"[PickupManager] Spawning pickup {i + 1} at {shuffled[i].position}");
            SpawnPickup(shuffled[i].position);
        }
    }

    public void OnPickedUp(Vector3 position)
    {
        Debug.Log($"[PickupManager] OnPickedUp at {position}");
        StartCoroutine(RespawnAfterDelay(position));
    }

    private IEnumerator RespawnAfterDelay(Vector3 position)
    {
        yield return new WaitForSeconds(_respawnDelay);
        Debug.Log($"[PickupManager] Respawning at {position}");
        SpawnPickup(position);
    }

    private void SpawnPickup(Vector3 position)
    {
        Debug.Log($"[PickupManager] SpawnPickup at {position}");

        var go = Instantiate(_healthPickupPrefab, position, Quaternion.identity);

        var pickup = go.GetComponent<HealthPickup>();
        if (pickup != null)
            pickup.Init(this, position);

        var networkObj = go.GetComponent<NetworkObject>();
        if (networkObj != null)
            networkObj.Spawn();

        Debug.Log($"[PickupManager] Spawned: {go.name}");
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
            NetworkManager.Singleton.OnServerStarted -= OnServerStarted;
    }
}