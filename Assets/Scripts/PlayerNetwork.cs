/*
using FishNet.Object;
using FishNet.Object.Synchronizing;
using FishNet.Managing;
using UnityEngine;
using System.Collections;
using System.Linq;

[RequireComponent(typeof(NetworkObject))]
public class PlayerNetwork : NetworkBehaviour
{
    [SyncVar(OnChange = nameof(OnNicknameChanged))]
    public string Nickname = "Player";

    [SyncVar(OnChange = nameof(OnHpChanged))]
    public int HP = 100;

    [SyncVar(OnChange = nameof(OnIsAliveChanged))]
    public bool IsAlive = true;

    [SerializeField] private GameObject _playerModel;

    private Transform[] _spawnPoints;
    private Coroutine _respawnCoroutine;

    public override void OnStartNetwork()
    {
        Debug.Log($"[Spawn] Owner: {base.OwnerId} | Server: {base.IsServerInitialized} | Client: {base.IsClientInitialized}");
        FindSpawnPoints();

        if (base.IsOwner)
            SubmitNicknameServerRpc(ConnectionUI.PlayerNickname);

        // Сервер устанавливает начальную позицию
        if (base.IsServerInitialized && _spawnPoints != null && _spawnPoints.Length > 0)
        {
            ApplyPositionObserversRpc(_spawnPoints[0].position);
            Debug.Log($"[Server] Initial pos RPC sent: {_spawnPoints[0].position}");
        }
    }

    private void FindSpawnPoints()
    {
        GameObject[] spawns = GameObject.FindGameObjectsWithTag("SpawnPoint");
        _spawnPoints = spawns.Select(go => go.transform).ToArray();

        if (_spawnPoints.Length == 0)
        {
            Debug.LogWarning("No 'SpawnPoint' tags found! Using fallback.");
            _spawnPoints = new Transform[] { transform };
        }
        else
        {
            Debug.Log($"Found {_spawnPoints.Length} spawn points.");
        }
    }

    public override void OnStopNetwork()
    {
        if (_respawnCoroutine != null)
        {
            StopCoroutine(_respawnCoroutine);
            _respawnCoroutine = null;
        }
    }

    [ServerRpc]
    private void SubmitNicknameServerRpc(string nickname)
    {
        string safe = string.IsNullOrWhiteSpace(nickname) ? $"Player_{base.OwnerId}" : nickname.Trim();
        Nickname = safe;
    }

    private void OnHpChanged(int prev, int next, bool asServer)
    {
        Debug.Log($"[SyncVar] HP: {prev} -> {next} | IsServer: {base.IsServerInitialized}");

        if (next <= 0 && IsAlive)
        {
            if (base.IsServerInitialized)
            {
                Debug.Log("[Server] Player died - starting respawn");
                Die();
            }
        }
    }

    private void OnNicknameChanged(string oldVal, string newVal, bool asServer)
    {
        Debug.Log($"[SyncVar] Nickname: {oldVal} -> {newVal}");
    }

    private void OnIsAliveChanged(bool oldVal, bool newVal, bool asServer)
    {
        Debug.Log($"[SyncVar] IsAlive: {oldVal} -> {newVal}");
        if (_playerModel) _playerModel.SetActive(newVal);

        var cc = GetComponent<CharacterController>();
        if (cc) cc.enabled = newVal;

        var movement = GetComponent<PlayerMovement>();
        if (movement) movement.enabled = newVal;

        var shooting = GetComponent<PlayerShooting>();
        if (shooting) shooting.enabled = newVal;
    }

    private void Die()
    {
        IsAlive = false;
        if (_respawnCoroutine != null) StopCoroutine(_respawnCoroutine);
        _respawnCoroutine = StartCoroutine(RespawnRoutine());
    }

    private IEnumerator RespawnRoutine()
    {
        Debug.Log("[Routine] Waiting 3s for respawn...");
        yield return new WaitForSeconds(3f);

        Vector3 spawnPos = Vector3.zero;
        if (_spawnPoints != null && _spawnPoints.Length > 0)
            spawnPos = _spawnPoints[Random.Range(0, _spawnPoints.Length)].position;

        ApplyPositionObserversRpc(spawnPos);
        HP = 100;
        IsAlive = true;

        Debug.Log($"[Server] Direct respawn at {spawnPos}");
        _respawnCoroutine = null;
    }

    [ObserversRpc]
    private void ApplyPositionObserversRpc(Vector3 pos)
    {
        if (!base.IsOwner) return;

        transform.position = pos;
        transform.rotation = Quaternion.identity;
        Debug.Log($"[Client] APPLIED RESPAWN POSITION: {pos}");

        var shooting = GetComponent<PlayerShooting>();
        if (shooting != null) shooting.ResetAmmo();
    }

    private void Update()
    {
        if (base.IsOwner && Input.GetKeyDown(KeyCode.K))
        {
            Debug.Log("[Input] K pressed -> Kill");
            if (base.IsServerInitialized) HP = 0;
            else TestKillServerRpc();
        }
    }

    [ServerRpc]
    private void TestKillServerRpc() => HP = 0;
}
*/

using FishNet.Object;
using FishNet.Object.Synchronizing;
using FishNet.Connection;
using UnityEngine;
using System.Collections;
using System.Linq;

[RequireComponent(typeof(NetworkObject))]
public class PlayerNetwork : NetworkBehaviour
{
    // В FishNet используем SyncVar<string> вместо NetworkVariable<FixedString32Bytes>
    public readonly SyncVar<string> Nickname = new SyncVar<string>();
    public readonly SyncVar<int> HP = new SyncVar<int>(100);
    public readonly SyncVar<bool> IsAlive = new SyncVar<bool>(true);

    [SerializeField] private GameObject _playerModel;

    private Transform[] _spawnPoints;
    private Coroutine _respawnCoroutine;

    // Вместо OnNetworkSpawn используем OnStartServer и OnStartClient
    public override void OnStartServer()
    {
        // Код, который должен выполняться на сервере при инициализации
        FindSpawnPoints();
        // Подписка на изменения значений
        HP.OnChange += OnHpChanged;
        IsAlive.OnChange += OnIsAliveChanged;
    }

    public override void OnStartClient()
    {
        // Код, который должен выполняться на клиенте при инициализации
        FindSpawnPoints();
        // Подписка на изменения значений
        HP.OnChange += OnHpChanged;
        IsAlive.OnChange += OnIsAliveChanged;

        if (IsOwner)
        {
            SubmitNicknameServerRpc(ConnectionUI.PlayerNickname);
            // Если сервер, немедленно устанавливаем позицию
            if (IsServerInitialized && _spawnPoints != null && _spawnPoints.Length > 0)
            {
                transform.position = _spawnPoints[0].position;
            }
        }
    }

    private void FindSpawnPoints()
    {
        GameObject[] spawns = GameObject.FindGameObjectsWithTag("SpawnPoint");
        _spawnPoints = spawns.Select(go => go.transform).ToArray();

        if (_spawnPoints.Length == 0)
        {
            Debug.LogWarning("No 'SpawnPoint' tags found! Using fallback.");
            _spawnPoints = new Transform[] { transform };
        }
        else
        {
            Debug.Log($"Found {_spawnPoints.Length} spawn points.");
        }
    }

    public override void OnStopNetwork()
    {
        HP.OnChange -= OnHpChanged;
        IsAlive.OnChange -= OnIsAliveChanged;
    }

    [ServerRpc(RequireOwnership = false)]
    private void SubmitNicknameServerRpc(string nickname)
    {
        string safe = string.IsNullOrWhiteSpace(nickname) ? $"Player_{Owner.ClientId}" : nickname.Trim();
        Nickname.Value = safe;
    }

    private void OnHpChanged(int prev, int next, bool asServer)
    {
        Debug.Log($"[NetVar] HP: {prev} -> {next} | IsServer: {asServer} | IsAlive: {IsAlive.Value}");

        // Только сервер обрабатывает смерть
        if (next <= 0 && IsAlive.Value && asServer)
        {
            Debug.Log("[Server] Player died - starting respawn");
            Die();
        }
    }

    private void Die()
    {
        IsAlive.Value = false;
        if (_respawnCoroutine != null) StopCoroutine(_respawnCoroutine);
        _respawnCoroutine = StartCoroutine(RespawnRoutine());
    }

    private IEnumerator RespawnRoutine()
    {
        Debug.Log("[Routine] Waiting 3s for respawn...");
        yield return new WaitForSeconds(3f);

        Debug.Log("[Routine] Time's up - respawning directly");

        Vector3 spawnPos = Vector3.zero;
        if (_spawnPoints != null && _spawnPoints.Length > 0)
            spawnPos = _spawnPoints[Random.Range(0, _spawnPoints.Length)].position;

        // Применяем позицию через ObserversRpc
        ApplyPositionObserversRpc(spawnPos);

        // Обновляем статы
        HP.Value = 100;
        IsAlive.Value = true;

        Debug.Log($"[Server] Direct respawn at {spawnPos}");
        _respawnCoroutine = null;
    }

    [ObserversRpc]
    private void ApplyPositionObserversRpc(Vector3 pos)
    {
        if (!IsOwner) return;

        transform.position = pos;
        transform.rotation = Quaternion.identity;
        Debug.Log($"[Client] APPLIED RESPAWN POSITION: {pos}");

        var shooting = GetComponent<PlayerShooting>();
        if (shooting != null) shooting.ResetAmmo();
    }

    private void OnIsAliveChanged(bool prev, bool next, bool asServer)
    {
        Debug.Log($"[Client] IsAlive: {prev} -> {next}");
        if (_playerModel) _playerModel.SetActive(next);

        var cc = GetComponent<CharacterController>();
        if (cc) cc.enabled = next;

        var movement = GetComponent<PlayerMovement>();
        if (movement) movement.enabled = next;

        var shooting = GetComponent<PlayerShooting>();
        if (shooting) shooting.enabled = next;
    }

    private void Update()
    {
        if (IsOwner && Input.GetKeyDown(KeyCode.K))
        {
            Debug.Log("[Input] K pressed -> Kill");
            if (IsServerInitialized) HP.Value = 0;
            else TestKillServerRpc();
        }
    }

    [ServerRpc]
    private void TestKillServerRpc() => HP.Value = 0;
}