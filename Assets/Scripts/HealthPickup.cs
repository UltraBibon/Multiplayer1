/*
using Unity.Netcode;
using UnityEngine;

public class HealthPickup : NetworkBehaviour
{
    [SerializeField] private int _healAmount = 40;

    private PickupManager _manager;
    private Vector3 _spawnPosition;
    private bool _isInitialized;

    public void Init(PickupManager manager, Vector3 spawnPosition)
    {
        _manager = manager;
        _spawnPosition = spawnPosition;
        _isInitialized = true;

        Debug.Log($"HealthPickup: Initialized at {spawnPosition}");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer || !_isInitialized) return;

        var player = other.GetComponent<PlayerNetwork>();
        if (player == null) return;

        if (!player.IsAlive.Value)
        {
            Debug.Log("HealthPickup: Player is dead, ignoring");
            return;
        }

        if (player.HP.Value >= 100)
        {
            Debug.Log("HealthPickup: Player HP already full");
            return;
        }

        int oldHp = player.HP.Value;
        player.HP.Value = Mathf.Min(100, player.HP.Value + _healAmount);

        Debug.Log($"HealthPickup: Healed player from {oldHp} to {player.HP.Value}");

        if (_manager != null)
            _manager.OnPickedUp(_spawnPosition);

        NetworkObject.Despawn(true);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 1.5f);
    }
}
*/

/*
using FishNet.Object;
using UnityEngine;

public class HealthPickup : NetworkBehaviour
{
    [SerializeField] private int _healAmount = 40;

    private PickupManager _manager;
    private Vector3 _spawnPosition;
    private bool _isInitialized;

    public void Init(PickupManager manager, Vector3 spawnPosition)
    {
        _manager = manager;
        _spawnPosition = spawnPosition;
        _isInitialized = true;

        Debug.Log($"HealthPickup: Initialized at {spawnPosition}");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!base.IsServerInitialized || !_isInitialized) return;

        var player = other.GetComponent<PlayerNetwork>();
        if (player == null) return;

        if (!player.IsAlive)
        {
            Debug.Log("HealthPickup: Player is dead, ignoring");
            return;
        }

        if (player.HP >= 100)
        {
            Debug.Log("HealthPickup: Player HP already full");
            return;
        }

        int oldHp = player.HP;
        player.HP = Mathf.Min(100, player.HP + _healAmount);

        Debug.Log($"HealthPickup: Healed player from {oldHp} to {player.HP}");

        if (_manager != null)
            _manager.OnPickedUp(_spawnPosition);

        // FishNet: деспаун через ServerManager
        ServerManager.Despawn(gameObject);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 1.5f);
    }
}
*/


using FishNet.Object;
using UnityEngine;

public class HealthPickup : NetworkBehaviour
{
    [SerializeField] private int _healAmount = 40;

    private PickupManager _manager;
    private Vector3 _spawnPosition;
    private bool _isInitialized;

    public void Init(PickupManager manager, Vector3 spawnPosition)
    {
        _manager = manager;
        _spawnPosition = spawnPosition;
        _isInitialized = true;

        Debug.Log($"HealthPickup: Initialized at {spawnPosition}");
    }

    private void OnTriggerEnter(Collider other)
    {
        // В FishNet проверяем IsServerInitialized вместо IsServer
        if (!IsServerInitialized || !_isInitialized) return;

        var player = other.GetComponent<PlayerNetwork>();
        if (player == null) return;

        if (!player.IsAlive.Value)
        {
            Debug.Log("HealthPickup: Player is dead, ignoring");
            return;
        }

        if (player.HP.Value >= 100)
        {
            Debug.Log("HealthPickup: Player HP already full");
            return;
        }

        int oldHp = player.HP.Value;
        player.HP.Value = Mathf.Min(100, player.HP.Value + _healAmount);

        Debug.Log($"HealthPickup: Healed player from {oldHp} to {player.HP.Value}");

        if (_manager != null)
            _manager.OnPickedUp(_spawnPosition);

        // В FishNet деспавн объекта
        base.Despawn();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 1.5f);
    }
}