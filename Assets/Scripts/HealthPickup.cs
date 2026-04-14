/*

using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class HealthPickup : NetworkBehaviour
{
    [Header("Pickup Settings")]
    [SerializeField] private int _healAmount = 40;
    [SerializeField] private float _pickupRadius = 1.5f;

    private PickupManager _manager;
    private Vector3 _spawnPosition;
    private bool _isCollected = false;

    private void Awake()
    {
        SphereCollider collider = GetComponent<SphereCollider>();
        collider.isTrigger = true;
        collider.radius = _pickupRadius;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        _spawnPosition = transform.position;
        _isCollected = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return;
        if (_isCollected) return;

        PlayerNetwork player = other.GetComponent<PlayerNetwork>();
        if (player == null) return;

        if (!player.GetIsAlive()) return;

        if (player.GetHP() >= 100) return;

        player.Heal(_healAmount);
        _isCollected = true;

        Debug.Log($"[HealthPickup] {player.Nickname.Value} healed for {_healAmount}");

        if (_manager != null)
        {
            _manager.OnPickedUp(_spawnPosition);
        }

        NetworkObject.Despawn(destroy: true);
    }

    public void Init(PickupManager manager, Vector3 spawnPos)
    {
        _manager = manager;
        _spawnPosition = spawnPos;
    }
}
*/

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