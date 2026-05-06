/*
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
*/



/*
using FishNet.Managing;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using FishNet.Object;
using FishNet;

public class PickupManager : MonoBehaviour
{
    [SerializeField] private GameObject _healthPickupPrefab;
    [SerializeField] private Transform[] _spawnPoints;
    [SerializeField] private float _respawnDelay = 10f;
    [SerializeField] private int _initialSpawnCount = 3;

    private NetworkManager _manager;
    private bool _isInitialized;

    private void Awake()
    {
        Debug.Log("[PickupManager] Awake called");
    }

    private void Start()
    {
        Debug.Log("[PickupManager] Start called");

        _manager = InstanceFinder.NetworkManager;

        if (_manager == null)
        {
            Debug.LogError("[PickupManager] NetworkManager is NULL!");
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
        _manager.ServerManager.OnServerStarted += OnServerStarted;
    }

    private void OnServerStarted()
    {
        Debug.Log("[PickupManager] OnServerStarted called");

        if (!_manager.IsServer)
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
        if (networkObj != null && _manager.IsServer)
        {
            // FishNet: спавн через ServerManager
            _manager.ServerManager.Spawn(go);
        }

        Debug.Log($"[PickupManager] Spawned: {go.name}");
    }

    private void OnDestroy()
    {
        if (_manager != null)
            _manager.ServerManager.OnServerStarted -= OnServerStarted;
    }
}
*/


using FishNet.Object;
using FishNet.Managing;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using FishNet.Transporting;
using FishNet;

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

        var networkManager = InstanceFinder.NetworkManager;
        if (networkManager == null)
        {
            Debug.LogError("[PickupManager] NetworkManager is NULL!");
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

        Debug.Log("[PickupManager] Subscribing to server start");
        // В FishNet используем событие OnStartNetwork
        networkManager.ServerManager.OnServerConnectionState += OnServerConnectionState;
    }

    private void OnServerConnectionState(ServerConnectionStateArgs args)
    {
        if (args.ConnectionState == LocalConnectionState.Started)
        {
            Debug.Log("[PickupManager] Server started - initializing pickups");
            Initialize();
        }
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

        // Перемешиваем точки появления
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

        // В FishNet спавним объект через ServerManager
        InstanceFinder.ServerManager.Spawn(go);

        Debug.Log($"[PickupManager] Spawned: {go.name}");
    }

    private void OnDestroy()
    {
        var networkManager = InstanceFinder.NetworkManager;
        if (networkManager != null)
            networkManager.ServerManager.OnServerConnectionState -= OnServerConnectionState;
    }
}