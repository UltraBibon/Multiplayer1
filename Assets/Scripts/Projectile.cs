/*
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
*/

/*
using FishNet.Object;
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
        if (!base.IsServerInitialized) return;

        var target = other.GetComponent<PlayerNetwork>();
        if (target == null) return;

        // Проверка на дружественный огонь
        if (target.OwnerId == base.OwnerId) return;

        int newHp = Mathf.Max(0, target.HP - _damage);
        target.HP = newHp;

        // FishNet: деспаун через ServerManager
        ServerManager.Despawn(gameObject);
    }
}
*/

using FishNet.Object;
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
        // Только сервер обрабатывает столкновения
        if (!IsServerInitialized) return;

        var target = other.GetComponent<PlayerNetwork>();
        if (target == null) return;

        if (target.Owner.ClientId == Owner.ClientId) return;

        int newHp = Mathf.Max(0, target.HP.Value - _damage);
        target.HP.Value = newHp;

        // Деспавним снаряд
        base.Despawn();
    }
}