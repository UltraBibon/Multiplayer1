/*
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
*/


/*

using FishNet.Object;
using FishNet.Managing;
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

    public override void OnStartNetwork()
    {
        _currentAmmo = _maxAmmo;
        _playerNetwork = GetComponent<PlayerNetwork>();
        _lastAmmoRegenTime = Time.time;

        Debug.Log($"[PlayerShooting] Spawned with {_currentAmmo} ammo");
    }

    private void Update()
    {
        if (!base.IsOwner) return;

        if (_playerNetwork != null && !_playerNetwork.IsAlive) return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log($"[PlayerShooting] Shoot attempt - Ammo: {_currentAmmo}");
            ShootServerRpc(_firePoint.position, _firePoint.forward);
        }

        if (base.IsServerInitialized)
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
    private void ShootServerRpc(Vector3 pos, Vector3 dir)
    {
        Debug.Log($"[PlayerShooting] ServerRpc received");

        if (_playerNetwork == null) return;
        if (_playerNetwork.HP <= 0) return;
        if (_currentAmmo <= 0) return;
        if (Time.time < _lastShotTime + _cooldown) return;

        _lastShotTime = Time.time;
        _currentAmmo--;
        _lastAmmoRegenTime = Time.time;

        Debug.Log($"[PlayerShooting] Shooting! Ammo left: {_currentAmmo}");

        var go = Instantiate(_projectilePrefab, pos + dir * 1.2f, Quaternion.LookRotation(dir));
        var no = go.GetComponent<NetworkObject>();
        if (no != null)
        {
            // FishNet: спавн через ServerManager
            ServerManager.Spawn(go);
        }
        else
        {
            Debug.LogError("[PlayerShooting] Projectile missing NetworkObject!");
            Destroy(go);
        }
    }

    public void ResetAmmo()
    {
        if (base.IsServerInitialized)
        {
            _currentAmmo = _maxAmmo;
            Debug.Log($"[PlayerShooting] Ammo reset to {_currentAmmo}");
        }
    }
}

*/

using FishNet;
using FishNet.Object;
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

    public override void OnStartServer()
    {
        _currentAmmo = _maxAmmo;
        _playerNetwork = GetComponent<PlayerNetwork>();
        _lastAmmoRegenTime = Time.time;

        Debug.Log($"[PlayerShooting] Server spawned with {_currentAmmo} ammo");
    }

    public override void OnStartClient()
    {
        _playerNetwork = GetComponent<PlayerNetwork>();
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

        // Регенерация патронов на сервере
        if (IsServerInitialized)
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
    private void ShootServerRpc(Vector3 pos, Vector3 dir, FishNet.Connection.NetworkConnection sender = null)
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
            // Спавним снаряд и назначаем владельца
            InstanceFinder.ServerManager.Spawn(go, sender);
        }
        else
        {
            Debug.LogError("[PlayerShooting] Projectile missing NetworkObject!");
            Destroy(go);
        }
    }

    public void ResetAmmo()
    {
        if (IsServerInitialized)
        {
            _currentAmmo = _maxAmmo;
            Debug.Log($"[PlayerShooting] Ammo reset to {_currentAmmo}");
        }
    }
}