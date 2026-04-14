/*

using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(PlayerNetwork))]
public class PlayerShooting : NetworkBehaviour
{
    [Header("Shooting Settings")]
    [SerializeField] private GameObject _projectilePrefab;
    [SerializeField] private Transform _firePoint;
    [SerializeField] private float _fireRange = 50f;
    [SerializeField] private float _cooldown = 0.4f;
    [SerializeField] private int _maxAmmo = 10;
    [SerializeField] private LayerMask _hitMask;

    [Header("References")]
    [SerializeField] private PlayerNetwork _playerNetwork;

    private float _lastShotTime;
    private int _currentAmmo;

    public System.Action<int> OnAmmoChanged;

    private void Awake()
    {
        if (_playerNetwork == null)
            _playerNetwork = GetComponent<PlayerNetwork>();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        _currentAmmo = _maxAmmo;

        if (IsOwner)
            OnAmmoChanged?.Invoke(_currentAmmo);
    }

    private void Update()
    {
        if (!IsOwner) return;

        if (_playerNetwork != null && !_playerNetwork.GetIsAlive())
            return;

        if (Input.GetKeyDown(KeyCode.Mouse0) || Input.GetKeyDown(KeyCode.Space))
        {
            TryShoot();
        }

        if (Input.GetKeyDown(KeyCode.R) && _currentAmmo < _maxAmmo)
        {
            Reload();
        }
    }

    private void TryShoot()
    {
        if (Time.time < _lastShotTime + _cooldown)
            return;

        ShootServerRpc(_firePoint.position, _firePoint.forward);
    }

    [ServerRpc]
    private void ShootServerRpc(Vector3 position, Vector3 direction,
                                ServerRpcParams rpcParams = default)
    {
        if (_playerNetwork == null || !_playerNetwork.GetIsAlive())
            return;

        if (_currentAmmo <= 0)
            return;

        if (Time.time < _lastShotTime + _cooldown)
            return;

        _lastShotTime = Time.time;
        _currentAmmo--;

        if (IsServer && OwnerClientId == rpcParams.Receive.SenderClientId)
            OnAmmoChanged?.Invoke(_currentAmmo);

        if (_projectilePrefab != null)
        {
            Vector3 spawnPos = position + direction * 1.2f;
            Quaternion spawnRot = Quaternion.LookRotation(direction);

            GameObject projectileGO = Instantiate(_projectilePrefab, spawnPos, spawnRot);
            NetworkObject projectileNO = projectileGO.GetComponent<NetworkObject>();

            Projectile projectile = projectileGO.GetComponent<Projectile>();
            if (projectile != null)
            {
                projectile.SetOwnerClientId(rpcParams.Receive.SenderClientId);
                projectile.SetDamage(20); 
            }

            projectileNO.SpawnWithOwnership(rpcParams.Receive.SenderClientId);
        }
    }

    private void Reload()
    {
        _currentAmmo = _maxAmmo;
        OnAmmoChanged?.Invoke(_currentAmmo);
    }

    public int GetCurrentAmmo() => _currentAmmo;
}

*/

/*
using Unity.Netcode;
using UnityEngine;

public class PlayerShooting : NetworkBehaviour
{
    [SerializeField] private GameObject _projectilePrefab;
    [SerializeField] private Transform _firePoint;
    [SerializeField] private float _cooldown = 0.4f;
    [SerializeField] private int _maxAmmo = 10;

    private float _lastShotTime;
    private int _currentAmmo;
    private PlayerNetwork _playerNetwork;

    public override void OnNetworkSpawn()
    {
        _currentAmmo = _maxAmmo;
        _playerNetwork = GetComponent<PlayerNetwork>();
    }

    private void Update()
    {
        if (!IsOwner) return;

        var playerNetwork = GetComponent<PlayerNetwork>();
        if (playerNetwork != null && !playerNetwork.IsAlive.Value) return;

        if (Input.GetKeyDown(KeyCode.Space))
            ShootServerRpc(_firePoint.position, _firePoint.forward);
    }

    [ServerRpc]
    private void ShootServerRpc(Vector3 pos, Vector3 dir, ServerRpcParams rpc = default)
    {
        if (_playerNetwork.HP.Value <= 0) return;
        if (_currentAmmo <= 0) return;
        if (Time.time < _lastShotTime + _cooldown) return;

        _lastShotTime = Time.time;
        _currentAmmo--;

        var go = Instantiate(_projectilePrefab, pos + dir * 1.2f, Quaternion.LookRotation(dir));
        var no = go.GetComponent<NetworkObject>();
        no.SpawnWithOwnership(rpc.Receive.SenderClientId);
    }
}*/


using Unity.Netcode;
using UnityEngine;

public class PlayerShooting : NetworkBehaviour
{
    [SerializeField] private GameObject _projectilePrefab;
    [SerializeField] private Transform _firePoint;
    [SerializeField] private float _cooldown = 0.4f;
    [SerializeField] private int _maxAmmo = 10;
    [SerializeField] private float _ammoRegenTime = 5f;

    private float _lastShotTime;
    private int _currentAmmo;
    private PlayerNetwork _playerNetwork;
    private float _lastAmmoRegenTime;

    public override void OnNetworkSpawn()
    {
        _currentAmmo = _maxAmmo;
        _playerNetwork = GetComponent<PlayerNetwork>();
        _lastAmmoRegenTime = Time.time;

        Debug.Log($"[PlayerShooting] Spawned with {_currentAmmo} ammo");
    }

    private void Update()
    {
        if (!IsOwner) return;

        if (_playerNetwork != null && !_playerNetwork.IsAlive.Value) return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log($"[PlayerShooting] Shoot attempt - Ammo: {_currentAmmo}, IsAlive: {_playerNetwork?.IsAlive.Value}");
            ShootServerRpc(_firePoint.position, _firePoint.forward);
        }

        if (IsServer)
        {
            RegenAmmo();
        }
    }

    private void RegenAmmo()
    {
        if (_currentAmmo < _maxAmmo && Time.time > _lastAmmoRegenTime + _ammoRegenTime)
        {
            _currentAmmo++;
            _lastAmmoRegenTime = Time.time;
            Debug.Log($"[PlayerShooting] Ammo regenerated: {_currentAmmo}/{_maxAmmo}");
        }
    }

    [ServerRpc]
    private void ShootServerRpc(Vector3 pos, Vector3 dir, ServerRpcParams rpc = default)
    {
        Debug.Log($"[PlayerShooting] ServerRpc received - HP: {_playerNetwork?.HP.Value}, Ammo: {_currentAmmo}");

        if (_playerNetwork == null)
        {
            Debug.LogError("[PlayerShooting] PlayerNetwork is null!");
            return;
        }

        if (_playerNetwork.HP.Value <= 0)
        {
            Debug.Log("[PlayerShooting] Rejected: Player dead");
            return;
        }

        if (_currentAmmo <= 0)
        {
            Debug.Log("[PlayerShooting] Rejected: No ammo");
            return;
        }

        if (Time.time < _lastShotTime + _cooldown)
        {
            Debug.Log("[PlayerShooting] Rejected: Cooldown");
            return;
        }

        _lastShotTime = Time.time;
        _currentAmmo--;
        _lastAmmoRegenTime = Time.time;

        Debug.Log($"[PlayerShooting] Shooting! Ammo left: {_currentAmmo}");

        var go = Instantiate(_projectilePrefab, pos + dir * 1.2f, Quaternion.LookRotation(dir));
        var no = go.GetComponent<NetworkObject>();
        if (no != null)
        {
            no.SpawnWithOwnership(rpc.Receive.SenderClientId);
        }
        else
        {
            Debug.LogError("[PlayerShooting] Projectile missing NetworkObject!");
            Destroy(go);
        }
    }

    public void ResetAmmo()
    {
        if (IsServer)
        {
            _currentAmmo = _maxAmmo;
            Debug.Log($"[PlayerShooting] Ammo reset to {_currentAmmo}");
        }
    }
}