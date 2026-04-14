/*

using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(SphereCollider))]
public class Projectile : NetworkBehaviour
{
    [Header("Projectile Settings")]
    [SerializeField] private float _speed = 20f;
    [SerializeField] private float _lifeTime = 3f;
    [SerializeField] private float _damageRadius = 0.5f;

    private int _damage = 20;
    private ulong _ownerClientId;
    private Rigidbody _rb;
    private float _spawnTime;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.isKinematic = false;
        _rb.useGravity = false;

        SphereCollider collider = GetComponent<SphereCollider>();
        collider.isTrigger = true;
        collider.radius = _damageRadius;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        _spawnTime = Time.time;

        _rb.linearVelocity = transform.forward * _speed;
    }

    private void Update()
    {
        if (IsServer && Time.time > _spawnTime + _lifeTime)
        {
            NetworkObject.Despawn(destroy: true);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return;

        PlayerNetwork target = other.GetComponent<PlayerNetwork>();
        if (target == null) return;

        if (target.OwnerClientId == _ownerClientId)
            return;

        target.TakeDamage(_damage);

        Debug.Log($"[Projectile] Hit {target.Nickname.Value} for {_damage} dmg");

        NetworkObject.Despawn(destroy: true);
    }

    public void SetOwnerClientId(ulong clientId)
    {
        _ownerClientId = clientId;
    }

    public void SetDamage(int damage)
    {
        _damage = damage;
    }
}
*/


using Unity.Netcode;
using UnityEngine;

public class Projectile : NetworkBehaviour
{
    [SerializeField] private float _speed = 18f;
    [SerializeField] private int _damage = 20;

    private void Update()
    {
        transform.Translate(Vector3.forward * _speed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return;

        var target = other.GetComponent<PlayerNetwork>();
        if (target == null) return;

        if (target.OwnerClientId == OwnerClientId) return;

        int newHp = Mathf.Max(0, target.HP.Value - _damage);
        target.HP.Value = newHp;

        NetworkObject.Despawn(true);
    }
}